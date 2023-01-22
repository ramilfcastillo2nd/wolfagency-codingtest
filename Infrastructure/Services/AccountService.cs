using Core.Enums;
using Infrastructure.Data.Repositories.Interfaces;
using Infrastructure.Entities;
using Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IConfiguration _config;
        private readonly ITokenService _tokenService;
        private readonly IUserProfileService _userProfileService;
        public AccountService(
            ITokenService tokenService,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
             IConfiguration config,
             IUserRepository userRepository,
             IUserProfileService userProfileService
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
            _userRepository = userRepository;
            _tokenService = tokenService;
            _userProfileService = userProfileService;
        }
        public async Task<(bool, string[])> CreateRoleAsync(AppRole role, IEnumerable<string> claims)
        {

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
                return (false, result.Errors.Select(e => e.Description).ToArray());

            return (true, new string[] { });
        }
        public async Task<bool> CheckUserExists(string email)
        {
            var user = await _userManager.FindByEmailAsync(email.Trim());
            return user != null ? true : false;
        }

        public async Task<AppRole> GetRoleByNameAsync(string roleName)
        {
            return await _roleManager.FindByNameAsync(roleName);
        }

        public async Task<AppUser> GetUserByIdAsync(Guid userId, bool includeProfile = false)
        {
            AppUser user;
            if (includeProfile)
                user = await _userRepository.GetUserWithDetailsById(userId);
            else
            {
                var Id = userId.ToString();
                user = await _userManager.FindByIdAsync(Id);
            }

            return user;
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            try
            {

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["JWTSetting:Secret"])),
                    ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                SecurityToken securityToken;

                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
                var jwtSecurityToken = securityToken as JwtSecurityToken;
                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
                    return null;

                return principal;
            }
            catch (Exception x)
            {
                throw new Exception(x.Message);
            }
        }
        public async Task<IList<string>> GetUserRolesAsync(AppUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }
        public async Task<(string Token, string RefreshToken)> GenerateJWTokenAsync(AppUser user, string timezone = "", string location = "")
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["JWTSetting:Secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    //new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                }),
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_config["JWTSetting:AccessExpiration"])),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };

            var userRoles = await GetUserRolesAsync(user);
            foreach (var role in userRoles)
            {
                tokenDescriptor.Subject.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            var refreshToken = _tokenService.GetRefreshToken(user.Id);

            return (tokenString, refreshToken.RefreshToken);
        }

        public async Task<(string NewToken, string NewRefreshToken, string TimeZone, string userId)> RefreshJWTokenAsync(string token, string refreshToken, string timezone = "", string location = "")
        {
            var principal = GetPrincipalFromExpiredToken(token);
            if (principal == null)
                return (null, null, null, null);

            var userId = Guid.Parse(principal.Identity.Name);

            var savedRefreshToken = _tokenService.GetRefreshToken(userId, refreshToken);
            if (savedRefreshToken != null)
            {
                if (savedRefreshToken.RefreshToken == refreshToken)
                {
                    var user = await GetUserByIdAsync(userId);
                    var newJwtData = await GenerateJWTokenAsync(user, timezone, location);

                    await _tokenService.UpdateRefreshToken(user.Id, savedRefreshToken.id);
                    return (newJwtData.Token, newJwtData.RefreshToken, await GetUserTimezoneById(userId), userId.ToString());
                }
            }

            return (null, null, null, null);
        }

        
        public async Task<string> CreateConfirmTokenAsync(AppUser user)
        {
            var confirmationToken = string.Empty;
            confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            //Save Last Two Digit Confirmation Code
            confirmationToken = await ModifySaveGeneratedCode(user, confirmationToken, TransactionType.register);
            return confirmationToken;
        }

        public async Task<string> ModifySaveGeneratedCode(AppUser user, string code, TransactionType transactionType)
        {
            var twoDigit = code.Substring(4, 2);

            if (transactionType == TransactionType.register)
                user.LastTwoDigitRegisterConfirmation = int.Parse(twoDigit);
            else
                user.LastTwoDigitForgotPasswordConfirmation = int.Parse(twoDigit);

            await _userManager.UpdateAsync(user);

            code = code.Substring(0, 4);
            return code;
        }
           public async Task<string> GetUserTimezoneById(Guid userId)
        {
            try
            {
                var timeZone = await _userRepository.GetCurrentTimezoneById(userId);
                return string.IsNullOrEmpty(timeZone) ? string.Empty : timeZone;
            }
            catch (Exception x)
            {
                throw new Exception(x.Message);
            }
        }
      
        public async Task<(AppUser, string[])> CreateUserAsync(AppUser user, IEnumerable<string> roles, string password, bool requireConfirmation = true)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return (null, result.Errors.Select(e => e.Description).ToArray());

            user = await _userManager.FindByNameAsync(user.UserName);

            try
            {
                //Create User Profile
                var userProfileRequest = new UserProfile
                {
                    UserId = user.Id,
                    Email = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    CreatedBy = string.Empty,
                    CreatedDate = DateTime.UtcNow
                };

                await _userProfileService.CreateUserProfile(userProfileRequest);

                result = await this._userManager.AddToRolesAsync(user, roles.Distinct());
            }
            catch
            {
                await DeleteUserAsync(user);
                throw;
            }

            if (!result.Succeeded)
            {
                await DeleteUserAsync(user);
                return (null, result.Errors.Select(e => e.Description).ToArray());
            }

            // Enable account confirmation and password recovery
            var confirmationToken = string.Empty;
            if (requireConfirmation)
                confirmationToken = await CreateConfirmTokenAsync(user);

            return (user, new string[] { confirmationToken });

        }
        public async Task<(bool, string[])> DeleteUserAsync(AppUser user)
        {
            var result = await _userManager.DeleteAsync(user);
            return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
        }
    }
}
