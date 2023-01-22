using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infrastructure.Data
{
    public class WolfAgencyCodingTestContext : IdentityDbContext<AppUser, AppRole, Guid>
    {
        public WolfAgencyCodingTestContext(DbContextOptions<WolfAgencyCodingTestContext> options) : base(options)
        {
            
        }
        public virtual DbSet<AppUserToken> AppUserTokens { get; set; }
        public virtual DbSet<UserProfile> UserProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
