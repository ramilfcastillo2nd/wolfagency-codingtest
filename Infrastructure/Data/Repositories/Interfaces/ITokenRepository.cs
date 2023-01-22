using Infrastructure.Entities;

namespace Infrastructure.Data.Repositories.Interfaces
{
    public interface ITokenRepository
    {
        IQueryable<AppUserToken> GetRefreshTokens();
        Task UpdateAsync(AppUserToken appUserToken);
    }
}
