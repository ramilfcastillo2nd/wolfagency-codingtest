using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace wolfagency_codingtest.Helpers
{
    public interface IDatabaseInitializer
    {
        Task SeedData();
    }
    public class DatabaseInitializer : IDatabaseInitializer
    {

        private readonly WolfAgencyCodingTestContext _context;
        private readonly IAccountService _accountService;
        private IHostingEnvironment _env;
        private IConfiguration _config;

        public DatabaseInitializer(WolfAgencyCodingTestContext context, IAccountService accountService, IHostingEnvironment env, IConfiguration config)
        {
            _accountService = accountService;
            _context = context;
            _env = env;
            _config = config;
        }

        public async Task SeedData()
        {
            await _context.Database.MigrateAsync().ConfigureAwait(false);
            await InitNewRoles();
        }

        private async Task InitNewRoles()
        {
            string administrator = _config.GetSection("Roles:0").Value;
            string user = _config.GetSection("Roles:1").Value;

            await EnsureRoleAsync(administrator, "Administrator", new string[] { });
            await EnsureRoleAsync(user, "User", new string[] { });

            //Create Super Admin User
            var userExists = await _accountService.CheckUserExists("test@gmail.com");
            if (!userExists)
            {
                var password = "";
                if (_env.IsDevelopment())
                    password = "P@ssw0rd";
                else if (_env.IsStaging())
                    password = "P@ssw0rd";
                else
                    password = "P@ssw0rd";

                await CreateUserAsync("test@gmail.com", password, "Default", "Admin", "test@gmail.com", "", new string[] { administrator });
            }

            ////Create Super Admin User
            //var user2Exists = await _accountService.CheckUserExists("philip@growthlever.io");
            //if (!user2Exists)
            //{
            //    var password = "";
            //    if (_env.IsDevelopment())
            //        password = "ypmd@LihvhLbYRBFcJ3A";
            //    else if (_env.IsStaging())
            //        password = "L8BYu*9EuKzgqNiNgAfB";
            //    else
            //        password = "hTfpwbs!D7p6eZbjNUk!";

            //    await CreateUserAsync("philip@growthlever.io", password, "Default", "Admin", "philip@growthlever.io", "", new string[] { superAdmin });
            //}

            ////Create Super Admin User
            //var user3Exists = await _accountService.CheckUserExists("hazelle@growthlever.io");
            //if (!user3Exists)
            //{
            //    var password = "";
            //    if (_env.IsDevelopment())
            //        password = "ypmd@LihvhLbYRBFcJ3A";
            //    else if (_env.IsStaging())
            //        password = "L8BYu*9EuKzgqNiNgAfB";
            //    else
            //        password = "hTfpwbs!D7p6eZbjNUk!";

            //    await CreateUserAsync("hazelle@growthlever.io", password, "Default", "Admin", "hazelle@growthlever.io", "", new string[] { superAdmin });
            //}

        }

        private async Task EnsureRoleAsync(string roleName, string description, string[] claims)
        {
            if ((await _accountService.GetRoleByNameAsync(roleName)) == null)
            {
                AppRole applicationRole = new AppRole(roleName, description);

                var result = await _accountService.CreateRoleAsync(applicationRole, claims);
            }
        }

        private async Task<AppUser> CreateUserAsync(string userName, string password, string firstName, string lastName, string email, string phoneNumber, string[] roles)
        {

            var applicationUser = new AppUser
            {
                FirstName = firstName,
                LastName = lastName,
                UserName = userName,
                Email = email,
                PhoneNumber = phoneNumber,
                EmailConfirmed = true
            };

            var result = await _accountService.CreateUserAsync(applicationUser, roles, password);

            return result.Item1;
        }
    }
}
