using Infrastructure.Data.Repositories.Interfaces;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    public class UserRepository: IUserRepository
    {
        private readonly WolfAgencyCodingTestContext _context;
        public UserRepository(WolfAgencyCodingTestContext context)
        {
            _context = context;
        }

        public async Task<(int, IEnumerable<AppUser>)> GetAllUsers(int skip, int take, string search)
        {
            var users = await _context.Users
                .Where(x => (string.IsNullOrEmpty(search) || x.UserName.ToLower().Contains(search)))
                .ToListAsync();
            var totalCount = users.Count();
            users = users
                .Skip(skip)
                .Take(take).ToList();
            return (totalCount, users);
        }

        public void UpdateRefreshToken(AppUserToken refreshToken)
        {
            try
            {
                //var user = _context.Users.Include(s => s.RefreshTokens).SingleOrDefault(s => s.Id == refreshToken.UserId);
                //user.RefreshTokens.Add(refreshToken);
                _context.AppUserTokens.Update(refreshToken);
                _context.SaveChanges();
            }
            catch (Exception x)
            {
                throw new Exception(x.Message);
            }
        }

        public AppUserToken GetLatestRefreshTokenByUserId(Guid userId)
        {
            try
            {
                var user = _context.Users.Include(s => s.RefreshTokens).SingleOrDefault(s => s.Id == userId);
                var refreshToken = user.RefreshTokens.OrderByDescending(s => s.CreatedOn).FirstOrDefault();
                return refreshToken;
            }
            catch (Exception x)
            {
                throw new Exception(x.Message);
            }
        }

        public void AddRefreshToken(AppUserToken refreshToken)
        {
            try
            {
                var user = _context.Users.Include(s => s.RefreshTokens).SingleOrDefault(s => s.Id == refreshToken.UserId);
                user.RefreshTokens.Add(refreshToken);
                _context.SaveChanges();
            }
            catch (Exception x)
            {
                throw new Exception(x.Message);
            }
        }
        public AppUser GetUserWithDetail(Guid userId)
        {
            return _context.Users
                    //.Include(s => s.UserProfile)
                    .Include(s => s.RefreshTokens)
                    .Where(s => s.Id == userId).FirstOrDefault();
        }
        //public void UpdateUser(User user)
        //{
        //    try
        //    {
        //        if (user != null)
        //        {
        //            _context.Attach(user).State = EntityState.Modified;
        //            _context.SaveChanges();
        //        }
        //    }
        //    catch (Exception x)
        //    {
        //        throw new Exception(x.Message);
        //    }
        //}

        //public User GetUserByAppId(int id)
        //{
        //    var appUser = _context.Users
        //            .SingleOrDefault(s => s.Id == id);
        //    if (appUser == null)
        //        throw new Exception("User is not existing");
        //    else
        //        return appUser.UserProfile;
        //}

        public async Task<string> GetCurrentTimezoneById(Guid id)
        {
            try
            {
                var user = await _context.Users
                    .Include(s => s.RefreshTokens)
                    .Where(s => s.Id == id).FirstOrDefaultAsync();
                var timeZone = (user.RefreshTokens.OrderByDescending(s => s.CreatedOn).First()).TimeZone;
                return timeZone;
            }
            catch (Exception x)
            {
                throw new Exception(x.Message);
            }
        }

        public async Task<AppUser> GetUserWithDetailsById(Guid id)
        {
            return await _context.Users
                //.Include(u => u.UserProfile)
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        //public void AddRefreshToken(AppUserToken refreshToken)
        //{
        //    try
        //    {
        //        var user = _context.Users
        //            //.Include(s => s.RefreshTokens)
        //            .SingleOrDefault(s => s.Id == refreshToken.UserId);
        //        user.RefreshTokens.Add(refreshToken);
        //        _context.SaveChanges();
        //    }
        //    catch (Exception x)
        //    {
        //        throw new Exception(x.Message);
        //    }
        //}
    }
}
