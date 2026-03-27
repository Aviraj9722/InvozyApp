using eOrderTouchApp.Models;
using eOrderTouchApp.Services;
using eOrderTouchApp.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace eOrderTouchApp.Controllers
{
    
    public class DealerController : Controller
    {
        private readonly eOrderTouchContext _context;
        private readonly IEmailService _emailService;
        private readonly IDataProtector _protector;

        public DealerController(
            eOrderTouchContext context,
            IEmailService emailService,
            IDataProtectionProvider provider)
        {
            _context = context;
            _emailService = emailService;
            _protector = provider.CreateProtector("DealerResetPasswordProtector");
        }

        [AuthorizeToRoles("Admin")]
        public async Task<IActionResult> Index()
        {
            var dealers = await _context.TblDealer.ToListAsync(); 
            return View(dealers);
        }

        [AuthorizeToRoles("Admin")]
        [HttpPost]
        public async Task<IActionResult> SaveDealer(TblDealer dealer)
        {
            try
            {
                bool isNew = dealer.Id == 0;

                // ✅ DUPLICATE CHECKS

                // Dealer Code
                bool codeExists = await _context.TblDealer
                    .AnyAsync(x => x.DealerCode == dealer.DealerCode && x.Id != dealer.Id);

                if (codeExists)
                    return BadRequest(new { message = "Dealer Code already exists." });

                // Email
                bool emailExists = await _context.TblDealer
                    .AnyAsync(x => x.EmailId == dealer.EmailId && x.Id != dealer.Id);

                if (emailExists)
                    return BadRequest(new { message = "Email already exists." });

                // Mobile
                bool mobileExists = await _context.TblDealer
                    .AnyAsync(x => x.MobileNo == dealer.MobileNo && x.Id != dealer.Id);

                if (mobileExists)
                    return BadRequest(new { message = "Mobile number already exists." });

                // ✅ SAVE / UPDATE
                if (isNew)
                {
                    _context.TblDealer.Add(dealer);
                }
                else
                {
                    var existingDealer = await _context.TblDealer.FindAsync(dealer.Id);

                    if (existingDealer == null)
                        return NotFound();

                    existingDealer.Name = dealer.Name;
                    existingDealer.DealerCode = dealer.DealerCode;
                    existingDealer.EmailId = dealer.EmailId;
                    existingDealer.MobileNo = dealer.MobileNo;
                    existingDealer.GSTN = dealer.GSTN;
                    existingDealer.Address = dealer.Address;
                    existingDealer.Location = dealer.Location;
                    existingDealer.Password = dealer.Password;
                    existingDealer.IsActive = dealer.IsActive;
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = isNew ? "Dealer saved successfully" : "Dealer updated successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [AuthorizeToRoles("Admin")]
        public async Task<IActionResult> GetDealer(int id)
        {
            var dealer = await _context.TblDealer.FindAsync(id);
            if (dealer == null) return NotFound();

            return Json(dealer);
        }

        [AuthorizeToRoles("Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var dealer = await _context.TblDealer.FindAsync(id);
                if (dealer == null) return NotFound();

                _context.TblDealer.Remove(dealer);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Dealer deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string dealerCode, string password)
        {

            var dealer = await _context.TblDealer
                  .FirstOrDefaultAsync(x => x.DealerCode == dealerCode
                              && x.Password == password);

            if (dealer == null)
            {
                ViewBag.Error = "Invalid Dealer Code or Password";
                return View();
            }

            //// ✅ CHECK ACTIVE STATUS
            if (dealer.IsActive != true)
            {
                ViewBag.Error = "Your account is deactivated. Contact admin.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, dealer.Name ?? ""),
                new Claim("DealerId", dealer.Id.ToString()),
                new Claim("DealerCode", dealer.DealerCode ?? ""),
                new Claim(ClaimTypes.Role, "Dealer")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            return RedirectToAction("DealerDashboard");
        }

        [Authorize(Roles = "Dealer")]
        public async Task<IActionResult> DealerDashboard()
        {
            int dealerId = Convert.ToInt32(User.FindFirst("DealerId")?.Value);

            // ----------------------------
            // CHECK DEALER LICENSE STOCK
            // ----------------------------
            int totalPurchased = await _context.TblDealerLicenseTransactions
                .Where(x => x.DealerId == dealerId)
                .SumAsync(x => (int?)x.PurchaseQty) ?? 0;

            int soldLicenses = await _context.TblUserLicenses
                .Where(x => x.DealerId == dealerId)
                .CountAsync();

            int remaining = totalPurchased - soldLicenses;

            if (remaining <= 5)
            {
                TempData["Error"] = "⚠ License stock is running low. Please purchase more licenses.";
            }

            DealerDashboardVM vm = new DealerDashboardVM();

            // ----------------------------
            // CUSTOMER LICENSE DATA
            // ----------------------------
            var data = await (from l in _context.TblUserLicenses
                              join b in _context.TblBusinesses
                              on l.BusinessId equals b.Id
                              where l.DealerId == dealerId
                              select new
                              {
                                  b.Id,
                                  b.BusinessName,
                                  b.OwnerName,
                                  b.MobileNo,
                                  b.City,
                                  b.Address,
                                  LicenseEndDate = l.EndDate
                              }).ToListAsync();

            vm.Customers = data.Select(x => new DealerCustomerVM
            {
                BusinessId = x.Id,
                CustomerName = x.BusinessName,
                OwnerName = x.OwnerName,
                Mobile = x.MobileNo,
                City = x.City,
                Address = x.Address,
                ExpiryDate = x.LicenseEndDate,
                Status = x.LicenseEndDate == null
                            ? "No License"
                            : x.LicenseEndDate < DateTime.Now
                                ? "Expired"
                                : "Active"
            }).ToList();

            // ----------------------------
            // DASHBOARD CARDS
            // ----------------------------
            var today = DateTime.Today;
            vm.TotalPurchased = totalPurchased;

            vm.SoldLicenses = soldLicenses;

            vm.RemainingLicenses = remaining;

            vm.ActiveLicenses = data.Count(x => x.LicenseEndDate >= DateTime.Now);

            vm.ExpiredLicenses = data.Count(x => x.LicenseEndDate < DateTime.Now);

            vm.ExpiringSoon = data.Count(x => x.LicenseEndDate >= today &&
                                             x.LicenseEndDate <= today.AddDays(15));

            // ----------------------------
            // EXPIRING SOON CUSTOMER LIST
            // ----------------------------
            vm.ExpiringSoonCustomers = vm.Customers
                .Where(x => x.ExpiryDate >= today &&
                            x.ExpiryDate <= today.AddDays(15))
                .ToList();

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> RenewLicense(int businessId)
        {
            try
            {
                int dealerId = Convert.ToInt32(User.FindFirst("DealerId")?.Value);

                var license = await _context.TblUserLicenses
                    .Where(x => x.BusinessId == businessId && x.DealerId == dealerId)
                    .OrderByDescending(x => x.EndDate)
                    .FirstOrDefaultAsync();

                if (license == null)
                    return NotFound(new { message = "License not found." });

                // ✅ CHECK: Only renew if expired
                if (license.EndDate >= DateTime.Today)
                {
                    return BadRequest(new
                    {
                        message = "License is still active. Cannot renew before expiry."
                    });
                }

                // ✅ Renew
                license.StartDate = DateTime.Today;
                license.EndDate = DateTime.Today.AddYears(1);

                await _context.SaveChangesAsync();

                return Ok(new { message = "License renewed successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [AllowAnonymous]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid email format." });

            var dealer = await _context.TblDealer
                .FirstOrDefaultAsync(x => x.EmailId == model.Email);

            if (dealer == null)
                return BadRequest(new { message = "Dealer with this email does not exist." });

            // create encrypted token (email + timestamp)
            var tokenData = $"{dealer.EmailId}|{DateTime.UtcNow}";
            var token = _protector.Protect(tokenData);

            var resetLink = Url.Action(
            "ResetPassword",
            "Dealer",
            new { token },
            protocol: Request.Scheme,
            host: Request.Host.Value);

            var body = $@"
                <div style='font-family: Arial, sans-serif;'>
                    <h3>Dealer Password Reset</h3>

                    <p>Click the button below to reset your password:</p>

                    <p>
                        <a href='{resetLink}'
                           style='background:#0078d7;color:white;padding:10px 15px;
                                  text-decoration:none;border-radius:5px;'>
                           Reset Password
                        </a>
                    </p>

                    <p><b>This link expires in 30 minutes.</b></p>

                    <hr/>
                    <p style='font-size:12px;color:gray;'>
                        Powered by <b>Invozy</b>
                    </p>
                </div>";

            await _emailService.SendEmailAsync(
                dealer.EmailId,
                "Reset your Dealer account password",
                body);

            return Ok(new
            {
                message = "Password reset link has been sent to your registered email."
            });
        }

        [AllowAnonymous]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            try
            {
                var tokenData = _protector.Unprotect(token);

                var parts = tokenData.Split('|');
                var email = parts[0];
                var time = DateTime.Parse(parts[1]);

                if (DateTime.UtcNow - time > TimeSpan.FromMinutes(30))
                {
                    TempData["Error"] = "Reset password link expired.";
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
                TempData["Error"] = "Invalid reset password link.";
                return RedirectToAction("Login");
            }
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
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
                var tokenData = _protector.Unprotect(model.Token);

                var parts = tokenData.Split('|');
                var email = parts[0];
                var time = DateTime.Parse(parts[1]);

                if (DateTime.UtcNow - time > TimeSpan.FromMinutes(30))
                {
                    ModelState.AddModelError("", "Reset link expired.");
                    return View(model);
                }

                var dealer = await _context.TblDealer
                    .FirstOrDefaultAsync(x => x.EmailId == email);

                if (dealer == null)
                {
                    ModelState.AddModelError("", "Dealer not found.");
                    return View(model);
                }

                dealer.Password = model.Password;

                await _context.SaveChangesAsync();

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
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        
    }
}
