using Infrastructure.Entities;

namespace Infrastructure.Data.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<(int, IEnumerable<AppUser>)> GetAllUsers(int skip, int take, string search);
        Task<string> GetCurrentTimezoneById(Guid id);
        Task<AppUser> GetUserWithDetailsById(Guid id);
        AppUser GetUserWithDetail(Guid userId);
        void AddRefreshToken(AppUserToken refreshToken);
        AppUserToken GetLatestRefreshTokenByUserId(Guid userId);
        void UpdateRefreshToken(AppUserToken refreshToken);
    }
}
