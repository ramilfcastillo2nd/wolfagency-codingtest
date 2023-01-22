using Infrastructure.Data.Repositories.Interfaces;
using Infrastructure.Entities;
using Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key; // encryption/decryption only done in server
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly ITokenRepository _tokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        public TokenService(UserManager<AppUser> userManager, IConfiguration config, IUnitOfWork unitOfWork, ITokenRepository tokenRepository, IUserRepository userRepository)
        {
            _userManager = userManager;
            _config = config;
            _key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["JWTSetting:Secret"]));
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _tokenRepository = tokenRepository;
        }
        public async Task UpdateRefreshToken(Guid userId, int refreshTokenId)
        {
            var tokens = _tokenRepository.GetRefreshTokens();
            var refreshToken = tokens.FirstOrDefault(x => x.id == refreshTokenId);
            if (refreshToken != null)
            {
                refreshToken.ExpiredOn = DateTime.UtcNow.AddMinutes(double.Parse(_config["JWTSetting:RefreshExpiration"]));
                _unitOfWork.Repository<AppUserToken>().Update(refreshToken);
                await _unitOfWork.Complete();
            }
        }
        public AppUserToken GetRefreshToken(Guid userid)
        {
            var tokens = _tokenRepository.GetRefreshTokens();
            var userRefreshToken = tokens.Where(u => u.UserId == userid).OrderByDescending(u => u.id).FirstOrDefault();

            return userRefreshToken;
        }
        public AppUserToken GetRefreshToken(Guid userId, string refreshToken = "")
        {
            var tokens = _tokenRepository.GetRefreshTokens();
            return tokens.FirstOrDefault(x => x.ExpiredOn > DateTime.UtcNow && x.RefreshToken == refreshToken);
        }
        public async Task<(string Token, string RefreshToken, List<Claim>)> CreateToken(AppUser user, string timeZone = "", string location = "")
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_config["JWTSetting:AccessExpiration"])),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                tokenDescriptor.Subject.AddClaim(new Claim(ClaimTypes.Role, role));
            }
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            //Get Latest Current RefreshToken
            var latestRefreshToken = _userRepository.GetLatestRefreshTokenByUserId(user.Id);
            var now = DateTime.UtcNow;
            if (latestRefreshToken != null)
            {
                if (latestRefreshToken.ExpiredOn < DateTime.Now)
                {
                    var refreshToken = GenerateRefreshToken();
                    //await SaveRefreshToken(user.Id, refreshToken, timeZone, location);
                    latestRefreshToken.RefreshToken = refreshToken;
                    latestRefreshToken.TimeZone = timeZone;
                    latestRefreshToken.ExpiredOn = now.AddMinutes(int.Parse(_config["JWTSetting:RefreshExpiration"]));
                    _userRepository.UpdateRefreshToken(latestRefreshToken);
                    return (tokenString, refreshToken, claims);
                }
                else
                {
                    //Update current refresh token record with the latest token
                    return (tokenString, latestRefreshToken.RefreshToken,claims);
                }
            }
            else
            {
                var refreshToken = GenerateRefreshToken();
                await SaveRefreshToken(user.Id, refreshToken, timeZone, location);
                return (tokenString, refreshToken, claims);
            }
        }
        public async Task SaveRefreshToken(Guid userId, string newRefreshToken, string timeZone = "", string location = "")
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var timeZoneId = timeZone;

            timeZone = GetTimeZone(timeZoneId);
            if (user != null)
            {
                var now = DateTime.UtcNow;
                var refreshToken = new AppUserToken
                {
                    RefreshToken = newRefreshToken,
                    CreatedOn = now,
                    ExpiredOn = now.AddMinutes(int.Parse(_config["JWTSetting:RefreshExpiration"])),
                    TimeZone = timeZone,
                    TimeZoneId = timeZoneId,
                    UserId = user.Id
                };
                _userRepository.AddRefreshToken(refreshToken);
            }
        }
        public string GetTimeZone(string timeZone)
        {
            try
            {
                return timeZone.Contains("/") ? timeZone : TimeZoneConverter.TZConvert.WindowsToIana(timeZone);
            }
            catch
            {
                return "Asia/Manila";
            }
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
