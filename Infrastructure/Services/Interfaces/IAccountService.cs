using Infrastructure.Entities;

namespace Infrastructure.Services.Interfaces
{
    public interface IAccountService
    {
        Task<(bool, string[])> CreateRoleAsync(AppRole role, IEnumerable<string> claims);
        Task<bool> CheckUserExists(string email);
        Task<string> CreateConfirmTokenAsync(AppUser user);
        Task<string> GetUserTimezoneById(Guid userId);
        Task<AppRole> GetRoleByNameAsync(string roleName);
        Task<(AppUser, string[])> CreateUserAsync(AppUser user, IEnumerable<string> roles, string password, bool requireConfirmation = true);
        Task<(string NewToken, string NewRefreshToken, string TimeZone, string userId)> RefreshJWTokenAsync(string token, string refreshToken, string timezone = "", string location = "");
    }
}
