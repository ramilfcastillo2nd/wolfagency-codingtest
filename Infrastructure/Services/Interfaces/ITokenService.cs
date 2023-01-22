using Infrastructure.Entities;
using System.Security.Claims;

namespace Infrastructure.Services.Interfaces
{
    public interface ITokenService
    {
        Task<(string Token, string RefreshToken, List<Claim>)> CreateToken(AppUser user, string timeZone = "", string location = "");
        AppUserToken GetRefreshToken(Guid userId, string refreshToken);
        AppUserToken GetRefreshToken(Guid userid);
        Task UpdateRefreshToken(Guid userId, int refreshTokenId);
    }
}
