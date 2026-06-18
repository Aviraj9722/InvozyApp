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

        //[HttpPost]
        //public async Task<IActionResult> Login(string username, string password)
        //{
        //    var user = await _context.TblUsers
        //        .FirstOrDefaultAsync(u => u.UserName == username && u.Password == password);

        //    if (user != null)
        //    {
        //        var claims = new List<Claim>
        //        {
        //            new Claim(ClaimTypes.Name, user.UserName),

        //            new Claim("UserId",user.Id.ToString()),
        //            new Claim(ClaimTypes.Role,user.Role),
        //             new Claim("OrgId", user.BussinessId.ToString()??"0")
        //        };

        //        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        //        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        //        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

        //        if (user.Role == "HeadOfficer")
        //        {
        //            return RedirectToAction("Dashboard", "HOActivity");
        //        }

        //        return RedirectToAction("Dashboard", "Home");
        //    }


        //    ViewBag.Message = "Invalid username or password";
        //    return View();
        //}

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.TblUsers
                .FirstOrDefaultAsync(u => u.UserName == username && u.Password == password);

            if (user == null)
            {
                ViewBag.Message = "Invalid username or password";
                return View();
            }

            var today = DateTime.UtcNow.Date;

            // Admin & Owner bypass license
            bool requireLicense = !(user.Role == "Admin");

            TblUserLicense license = null;

            if (requireLicense)
            {
                // Fetch last license for this Business
                license = await _context.TblUserLicenses
                    .Where(x => x.BusinessId == user.BussinessId)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                if (license == null)
                {
                    ViewBag.Message = "No license found for this business.";
                    return View(); // Block login
                }

                // Check FromDate & ToDate validity
                if (!license.StartDate.HasValue || !license.EndDate.HasValue)
                {
                    ViewBag.Message = "License date is not valid.";
                    return View(); // Block login
                }

                if (today < license.StartDate.Value.Date)
                {
                    ViewBag.Message = $"License not active yet. Starts on {license.StartDate.Value:dd MMM yyyy}.";
                    return View(); // Block login
                }

                if (today > license.EndDate.Value.Date)
                {
                    ViewBag.Message = $"License expired on {license.EndDate.Value:dd MMM yyyy}.";
                    return View(); // Block login
                }
            }

            // If here → License valid OR Admin/Owner
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("OrgId", user.BussinessId?.ToString() ?? "0")
            };

            if (license != null)
            {
                claims.Add(new Claim("LicenseStart", license.StartDate.Value.ToString("yyyy-MM-dd")));
                claims.Add(new Claim("LicenseEnd", license.EndDate.Value.ToString("yyyy-MM-dd")));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            if (user.Role == "HeadOfficer")
                return RedirectToAction("Dashboard", "HOActivity");

            return RedirectToAction("Dashboard", "Home");
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
                <div style='font-family: Arial, sans-serif;'>
                    <h3>Password Reset Link</h3>

                    <p>Click the button below to reset your password for your <b>Invozy</b> account:</p>

                    <p>
                        <a href='{resetLink}'
                           style='background:#4CAF50;color:white;padding:10px 15px;
                                  text-decoration:none;border-radius:5px;'>
                           Reset Password
                        </a>
                    </p>

                    <p><b>This link expires in 30 minutes.</b></p>

                    <hr />
                    <p style='font-size:12px;color:gray;'>
                        Powered by <b>Invozy</b>
                    </p>
                </div>";


            await _emailService.SendEmailAsync(
                    user.EmailId,
                    "Link for resetting the password for your Invozy account",
                    body);

            return Ok(new
            {
                message = "Password reset link has been sent to your registered email."
            });
        }

        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            try
            {
                var tokenData = _protector.Unprotect(token);

                var parts = tokenData.Split('|');

                if (parts.Length != 2)
                    throw new Exception("Invalid token");

                string email = parts[0];
                DateTime generatedTime = DateTime.Parse(parts[1]);

                if (DateTime.UtcNow > generatedTime.AddMinutes(30))
                {
                    TempData["Error"] = "This reset password link has expired.";
                    return RedirectToAction("Login");
                }

                return View(new ResetPasswordViewModel
                {
                    Email = email,
                    Token = token
                });
            }
            catch
            {
                TempData["Error"] = "This reset password link has expired or is invalid.";
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var tokenData = _protector.Unprotect(model.Token);

                var parts = tokenData.Split('|');

                if (parts.Length != 2)
                    throw new Exception("Invalid token");

                string email = parts[0];
                DateTime generatedTime = DateTime.Parse(parts[1]);

                if (DateTime.UtcNow > generatedTime.AddMinutes(30))
                {
                    ModelState.AddModelError("", "Reset link has expired.");
                    return View(model);
                }

                var user = _context.TblUsers
                    .FirstOrDefault(x => x.EmailId == email);

                if (user == null)
                {
                    ModelState.AddModelError("", "User not found.");
                    return View(model);
                }

                user.Password = model.Password;

                _context.SaveChanges();

                ViewBag.Success = "Password reset successfully.";
                ViewBag.Redirect = true;

                return View(model);
            }
            catch
            {
                ModelState.AddModelError("", "Invalid reset link.");
                return View(model);
            }
        }




    }
}
