using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using eOrderTouchApp.Models;
using Microsoft.EntityFrameworkCore;

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
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("Role", user.Role?.ToString()??""),
                    new Claim("OrgId", user.BussinessId.ToString()??"0")
                };

                var claimsIdentity = new ClaimsIdentity(claims, "UserCookie");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync("UserCookie", claimsPrincipal);

                return RedirectToAction("Dashboard", "Home");
            }

            ViewBag.Message = "Invalid username or password";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("UserCookie");
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View("AccessDenied");
        }
    }
}
