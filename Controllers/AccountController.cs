using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace eOrderTouchApp.Controllers
{ 
    public class AccountController : Controller
    {
        private readonly eOrderTouchContext _context;

        public AccountController(eOrderTouchContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.TblUsers
                .FirstOrDefaultAsync(u => u.UserName == username && u.Password == password);

            if (user != null)
            {

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                   
                    new Claim("UserId",user.Id.ToString()),
                    new Claim(ClaimTypes.Role,user.Role),
                     new Claim("OrgId", user.BussinessId.ToString()??"0")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);


                return RedirectToAction("Dashboard", "Home");
            }

            ViewBag.Message = "Invalid username or password";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View("AccessDenied");
        }
    }
}
