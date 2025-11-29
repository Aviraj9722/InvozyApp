using eOrderTouchApp.Models;
using eOrderTouchApp.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgetPassword(ForgetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check if email exists in DB
            var user = _context.TblUsers.FirstOrDefault(x => x.EmailId == model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Email not found.");
                return View(model);
            }

            // Redirect to ResetPassword Page – pass Email
            return RedirectToAction("ResetPassword", new { email = model.Email});
        }

        public IActionResult ResetPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("ForgetPassword");

            var vm = new ResetPasswordViewModel
            {
                Email = email
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View(model);
            }

            // Find user
            var user = _context.TblUsers.FirstOrDefault(x => x.EmailId == model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View(model);
            }

            // Save new password
            user.Password = model.Password; // Hash later if required
            _context.SaveChanges();

            TempData["Success"] = "Password updated successfully.";
            return RedirectToAction("Login");
        }


    }
}
