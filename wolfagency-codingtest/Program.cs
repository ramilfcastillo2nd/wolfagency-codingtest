using Core.Helpers;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Data.Repositories.Interfaces;
using Infrastructure.Entities;
using Infrastructure.Services;
using Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using wolfagency_codingtest.Helpers;

var builder = WebApplication.CreateBuilder(args);
var configBuilder = new ConfigurationBuilder();


// add values from a json file
configBuilder.AddJsonFile("appsettings.json");

// create the IConfigurationRoot instance
IConfigurationRoot config = configBuilder.Build();

// Add services to the container.
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), (typeof(GenericRepository<>)));
builder.Services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var errors = actionContext.ModelState
            .Where(e => e.Value.Errors.Count > 0)
            .SelectMany(x => x.Value.Errors)
            .Select(x => x.ErrorMessage).ToArray();

        //var errorResponse = new ApiValidationErrorResponse
        //{
        //    Errors = errors
        //};

        return new BadRequestObjectResult(null);//errorResponse
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsList", builder =>
    {
        //App:CorsOrigins in appsettings.json can contain more than one address with splitted by comma.
        builder
            .WithOrigins(
                // App:CorsOrigins in appsettings.json can contain more than one address separated by comma.
                config.GetSection("App:CorsOrigins").Value
                    .Split(",", StringSplitOptions.RemoveEmptyEntries)
                    .ToArray()
            )
            .SetIsOriginAllowedToAllowWildcardSubdomains()
            .SetIsOriginAllowed(origin => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<WolfAgencyCodingTestContext>(options =>
{
    options.UseSqlServer(config.GetSection("ConnectionStrings:DefaultConnection").Value, b => b.MigrationsAssembly("Infrastructure"));
}, ServiceLifetime.Scoped);

builder.Services.AddIdentity<AppUser, AppRole>(options => {
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(1);
    options.Lockout.MaxFailedAccessAttempts = 5;
}).AddEntityFrameworkStores<WolfAgencyCodingTestContext>()
.AddTokenProvider<CustomDataProtectorTokenProvider<AppUser>>("Default");


builder.Services.Configure<IdentityOptions>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
});


builder.Services.Configure<JWTSetting>(config.GetSection(nameof(JWTSetting)));
var token = config.GetSection(nameof(JWTSetting)).Get<JWTSetting>();
var secret = Encoding.ASCII.GetBytes(token.Secret);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
               .AddCookie(options =>
               {
                   options.LoginPath = "/Account/Login";
                   options.LogoutPath = "/Account/Logout";
                   options.Cookie.Name = "DotNetManiaCookie";
               });

builder.Services.AddControllersWithViews();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

SeedDatabase();
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseCookiePolicy();

app.UseEndpoints(endpoints => {
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});


app.Run();


  
void SeedDatabase() //can be placed at the very bottom under app.Run()
{
    using (var scope = app.Services.CreateScope())
    {
        var databaseInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
        databaseInitializer.SeedData().Wait();
    }
}