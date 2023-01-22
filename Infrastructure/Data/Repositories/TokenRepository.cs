using Infrastructure.Data.Repositories.Interfaces;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    public class TokenRepository : ITokenRepository
    {
        private readonly WolfAgencyCodingTestContext _context;
        public TokenRepository(WolfAgencyCodingTestContext context)
        {
            _context = context;
        }
        public IQueryable<AppUserToken> GetRefreshTokens()
        {
            return _context.Users
                    .Include(s => s.RefreshTokens)
                    .SelectMany(f => f.RefreshTokens);
        }

        public async Task UpdateAsync(AppUserToken appUserToken)
        {
            _context.Attach(appUserToken).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
