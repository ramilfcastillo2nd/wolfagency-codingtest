using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Core.DTO;
using Core.Enums;
using Microsoft.AspNetCore.Identity;
using Infrastructure.Entities;
using Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace wolfagency_codingtest.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IAccountService _accountService;
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IAccountService accountService)
        {

            _signInManager = signInManager;
            _userManager = userManager;
            _tokenService = tokenService;
            _accountService = accountService;
        }
        [AllowAnonymous]
        public IActionResult Logout()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string ReturnUrl)
        {

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return Unauthorized();

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);

            //// Ensure the user is not already locked out.
            //if (result.IsLockedOut)
            //    return BadRequest("The specified user account has been suspended.");

            //// Reject the token request if two-factor authentication has been enabled by the user.
            //if (result.RequiresTwoFactor)
            //    return BadRequest("Invalid login procedure.");

            //if (!result.Succeeded) return Unauthorized();
            var tokenResponse = await _tokenService.CreateToken(user, "Asia/Manila");
            var claims = tokenResponse.Item3;
            claims.Add(new Claim("DotNetMania", "Code"));
            if (result.Succeeded)
            {
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                return LocalRedirect(ReturnUrl == null ? "/Home/Index" : ReturnUrl);
            }
            else
                return Unauthorized();
        }
    }
}
    