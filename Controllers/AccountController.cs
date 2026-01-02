using eOrderTouchApp.Models;
using eOrderTouchApp.Services;
using eOrderTouchApp.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Security.Claims;

namespace eOrderTouchApp.Controllers
{ 
    public class AccountController : Controller
    {
        private readonly eOrderTouchContext _context;
        private readonly IEmailService _emailService;
        private readonly IDataProtector _protector;

        public AccountController(eOrderTouchContext context, IEmailService emailService, IDataProtectionProvider provider)
        {
            _context = context;
            _emailService = emailService;
            _protector = provider.CreateProtector("ResetPasswordProtector");
        }

        public IActionResult Test()
        {
            return View();
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

                if (user.Role == "HeadOfficer")
                {
                    return RedirectToAction("Dashboard", "HOActivity");
                }

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

        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid email format." });

            var user = await _context.TblUsers
                .FirstOrDefaultAsync(x => x.EmailId == model.Email);

            if (user == null)
                return BadRequest(new { message = "Incorrect Email ID" });

            // Encrypt email + time
            var tokenData = $"{user.EmailId}|{DateTime.UtcNow}";
            var token = _protector.Protect(tokenData);

            var resetLink = Url.Action(
                "ResetPassword",
                "Account",
                new { token },
                Request.Scheme);

            var body = $@"
                <h3>Password Reset Link</h3>
                <p>Click below to reset your password for Invozy Account:</p>
                <a href='{resetLink}'>Reset Password</a>
                <p>This link expires in 30 minutes.</p>";

              await _emailService.SendEmailAsync(
                    user.EmailId,
                    "Reset Your Invozy Password",
                    body);

            return Ok(new
            {
                message = "Reset password link has been sent to your registered email."
            });
        }

        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("ForgetPassword");

            try
            {
                var decrypted = _protector.Unprotect(token);
                var parts = decrypted.Split('|');

                var email = parts[0];
                var sentTime = DateTime.Parse(parts[1]);

                if ((DateTime.UtcNow - sentTime).TotalMinutes > 30)
                {
                    TempData["Error"] = "Reset link expired.";
                    return RedirectToAction("ForgetPassword");
                }

                return View(new ResetPasswordViewModel
                {
                    Email = email,
                    Token = token
                });
            }
            catch
            {
                TempData["Error"] = "Invalid reset link.";
                return RedirectToAction("ForgetPassword");
            }
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

            try
            {
                var decrypted = _protector.Unprotect(model.Token);
                var email = decrypted.Split('|')[0];

                var user = _context.TblUsers.FirstOrDefault(x => x.EmailId == email);
                if (user == null)
                {
                    ModelState.AddModelError("", "User not found.");
                    return View(model);
                }

                user.Password = model.Password; // hash later
                _context.SaveChanges();

                ViewBag.Success = "Password reset successfully.";
                ViewBag.Redirect = true;
                return View(model);
            }
            catch
            {
                ModelState.AddModelError("", "Invalid or expired token.");
                return View(model);
            }
        }



    }
}
