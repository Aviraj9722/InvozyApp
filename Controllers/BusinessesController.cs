using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.ProjectModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace eOrderTouchApp.Controllers
{
   
    public class BusinessesController : Controller
    {
        private readonly eOrderTouchContext _context;
        private readonly IWebHostEnvironment _env;

        public BusinessesController(eOrderTouchContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ========================
        // INDEX (Grid + Pagination)
        // ========================
        [AuthorizeToRoles("Admin","Dealer")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            List<TblBusiness> data;

            if (role == "Dealer")
            {
                int dealerId = Convert.ToInt32(User.FindFirst("DealerId")?.Value);

                data = await (from b in _context.TblBusinesses
                              join l in _context.TblUserLicenses
                              on b.Id equals l.BusinessId
                              where l.DealerId == dealerId
                              orderby b.Id descending
                              select b).ToListAsync();
                // ======================================
                // ✅ ADD THIS BLOCK (NO DISTURB)
                // ======================================
                int totalPurchased = await _context.TblDealerLicenseTransactions
                    .Where(x => x.DealerId == dealerId)
                    .SumAsync(x => (int?)x.PurchaseQty) ?? 0;

                int usedLicenses = await _context.TblUserLicenses
                    .Where(x => x.DealerId == dealerId)
                    .CountAsync();

                ViewBag.RemainingLicenses = totalPurchased - usedLicenses;
            }
            else
            {
                data = await _context.TblBusinesses
                            .OrderByDescending(x => x.Id)
                            .ToListAsync();
            }

            // Drop-down list data
            ViewBag.BusinessTypes = await _context.TblBusinessTypes.ToListAsync();
            ViewBag.PrinterSizes = await _context.TblPrinterSizes.ToListAsync();

            return View(data);
        }

        [AuthorizeToRoles("Admin", "Dealer")]      
        [HttpPost]
        public async Task<IActionResult> Create(TblBusiness business, IFormFile? LogoFile)
        {
            // -----------------------------
            // MODELSTATE CLEAN
            // -----------------------------
            ModelState.Remove("Id");
            ModelState.Remove("IsActive");
            ModelState.Remove("HideCustomerField");
            ModelState.Remove("HideTableDropDown");
            ModelState.Remove("IsKOTEnabled");
            ModelState.Remove("IsCustomerMandetory");
            ModelState.Remove("BarcodeEnabled");
            ModelState.Remove("IsMultilengual");
            ModelState.Remove("IsTableNoRequired");
            ModelState.Remove("IsReceiptReprint");

            if (!ModelState.IsValid)
            {
                var allErrors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new
                    {
                        Field = x.Key,
                        Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    }).ToList();

                return BadRequest(allErrors);
            }

            // =========================================
            // ✅ LICENSE CHECK RIGHT HERE
            // =========================================
            var dealerClaim = User.FindFirst("DealerId");

            if (dealerClaim != null)
            {
                int dealerId = Convert.ToInt32(dealerClaim.Value);

                int totalPurchased = await _context.TblDealerLicenseTransactions
                    .Where(x => x.DealerId == dealerId)
                    .SumAsync(x => (int?)x.PurchaseQty) ?? 0;

                int usedLicenses = await _context.TblUserLicenses
                    .Where(x => x.DealerId == dealerId)
                    .CountAsync();

                int remaining = totalPurchased - usedLicenses;

                if (remaining <= 0)
                {
                    return BadRequest(new
                    {
                        message = "❌ Dealer license stock finished. Please purchase more licenses."
                    });
                }
            }

            // -----------------------------
            // SAVE BUSINESS
            // -----------------------------
            business.CreatedOn = DateTime.Now;

            if (LogoFile != null)
            {
                if (LogoFile.Length > 100 * 1024)
                    return BadRequest("Logo size must be less than 100 KB");

                var allowedTypes = new[] { "image/jpeg", "image/png" };
                if (!allowedTypes.Contains(LogoFile.ContentType))
                    return BadRequest("Only PNG or JPG images are allowed");

                if (!string.IsNullOrEmpty(business.Logo))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, "Uploads", business.Logo);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                business.Logo = await SaveCompressedLogo(LogoFile);
            }

            _context.TblBusinesses.Add(business);
            await _context.SaveChangesAsync();

            // ✅ CREATE DEFAULT LEDGER IF FINANCE ENABLED
            if (business.EnableFinance == true)
            {
                bool cashExists = _context.TblLedgerAccounts
                    .Any(x => x.BusinessId == business.Id && x.Name == "Cash In Hand");

                if (!cashExists)
                {
                    _context.TblLedgerAccounts.Add(new TblLedgerAccount
                    {
                        BusinessId = business.Id,
                        Name = "Cash In Hand",
                        Type = "Cash",
                        Description = "Cash Account",
                        CreatedOn = DateTime.Now
                    });
                }

                bool bankExists = _context.TblLedgerAccounts
                    .Any(x => x.BusinessId == business.Id && x.Name == "Cash In Bank");

                if (!bankExists)
                {
                    _context.TblLedgerAccounts.Add(new TblLedgerAccount
                    {
                        BusinessId = business.Id,
                        Name = "Cash In Bank",
                        Type = "Bank",
                        Description = "Bank Account",
                        CreatedOn = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();
            }


            // -----------------------------
            // CREATE LICENSE
            // -----------------------------
            if (dealerClaim != null)
            {
                int dealerId = Convert.ToInt32(dealerClaim.Value);

                var license = new TblUserLicense
                {
                    BusinessId = business.Id,
                    DealerId = dealerId,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddYears(1),
                    LicenseKey = Guid.NewGuid().ToString(),
                    CreatedOn = DateTime.Now
                };

                _context.TblUserLicenses.Add(license);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, message = "Business saved successfully!" });
        }

        // ==========
        // EDIT (GET)
        // ==========
        [AuthorizeToRoles("Admin", "Dealer")]
        public async Task<IActionResult> Edit(int id)
        {
            var b = await _context.TblBusinesses.FindAsync(id);

            if (b == null)
                return NotFound();

            // 🔒 Restrict Dealer to only their own businesses
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (role == "Dealer")
            {
                int dealerId = Convert.ToInt32(User.FindFirst("DealerId")?.Value);

                bool isAllowed = await _context.TblUserLicenses
                    .AnyAsync(x => x.BusinessId == id && x.DealerId == dealerId);

                if (!isAllowed)
                {
                    return Unauthorized("❌ You are not allowed to view/edit this business");
                }
            }

            return Json(new
            {
                id = b.Id,
                businessName = b.BusinessName,
                businessTypeId = b.BusinessTypeId,
                ownerName = b.OwnerName,
                gstin = b.Gstin,
                email = b.Email,
                address = b.Address,
                city = b.City,
                printerSizeId = b.PrinterSizeId,
                isGstApplicable = b.IsGstapplicable,
                hideCustomerField = b.HideCustomerField,
                hideTableDropDown = b.HideTableDropDown,
                isKOTEnabled = b.IsKOTEnabled,
                barcodeEnabled = b.BarcodeEnabled,
                IsCustomerMandetory = b.IsCustomerMandetory,
                kichenPrinterName = b.KichenPrinterName,
                counterPrinterName = b.CounterPrinterName,
                isMultilengual = b.IsMultilengual,
                isTableNoRequired = b.IsTableNoRequired,
                isReceiptReprint = b.IsReceiptReprint,
                isActive = b.IsActive,
                logo = b.Logo,
                mobileNo = b.MobileNo,
                qrCode = b.Qrcode,
                discountType = b.DiscountType,
                reportData = b.ReportData,
                enableFinance = b.EnableFinance,
            });
        }

        // =============
        // UPDATE (POST)
        // =============
        [HttpPost]
        [AuthorizeToRoles("Admin","Dealer")]
        // public async Task<IActionResult> Update(TblBusiness business)//, IFormFile LogoFile, IFormFile QRCodeFile)
        public async Task<IActionResult> Update(TblBusiness business, IFormFile? LogoFile)
        {
            ModelState.Remove("Id");
            ModelState.Remove("IsActive");
            ModelState.Remove("HideCustomerField");
            ModelState.Remove("HideTableDropDown");
            ModelState.Remove("IsKOTEnabled");
            ModelState.Remove("IsCustomerMandetory");
            ModelState.Remove("BarcodeEnabled");
            ModelState.Remove("IsMultilengual");
            ModelState.Remove("IsTableNoRequired");
            ModelState.Remove("IsReceiptReprint");

            if (!ModelState.IsValid)
            {
                var allErrors = ModelState
          .Where(x => x.Value.Errors.Count > 0)
          .Select(x => new
          {
              Field = x.Key,
              Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToList()
          }).ToList();

                return BadRequest(allErrors);
            }

            var existing = await _context.TblBusinesses.FindAsync(business.Id);

            if (existing == null)
                return NotFound();

            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (role == "Dealer")
            {
                int dealerId = Convert.ToInt32(User.FindFirst("DealerId")?.Value);

                bool isAllowed = await _context.TblUserLicenses
                    .AnyAsync(x => x.BusinessId == business.Id && x.DealerId == dealerId);

                if (!isAllowed)
                {
                    return Unauthorized("❌ You are not allowed to edit this business");
                }
            }
            // Logo upload
            if (LogoFile != null)
            {
                //// 🔐 Size validation (100 KB)
                //if (LogoFile.Length > 100 * 1024)
                //    return BadRequest("Logo size must be less than 100 KB");

                // 🔐 Type validation
                var allowedTypes = new[] { "image/jpeg", "image/png" };
                if (!allowedTypes.Contains(LogoFile.ContentType))
                    return BadRequest("Only PNG or JPG images are allowed");

                // Delete old logo (optional)
                if (!string.IsNullOrEmpty(existing.Logo))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, "Uploads", existing.Logo);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                existing.Logo = await SaveCompressedLogo(LogoFile);
            }

            // Update fields
            existing.BusinessName = business.BusinessName;
            existing.BusinessTypeId = business.BusinessTypeId;
            existing.OwnerName = business.OwnerName;
            existing.Gstin = business.Gstin;
            existing.Email = business.Email;
            existing.MobileNo = business.MobileNo;
            existing.Address = business.Address;
            existing.City = business.City;
            existing.PrinterSizeId = business.PrinterSizeId;
            existing.DiscountType = business.DiscountType;
            existing.IsGstapplicable = business.IsGstapplicable;
            existing.HideCustomerField = business.HideCustomerField;
            existing.HideTableDropDown = business.HideTableDropDown;
            existing.IsKOTEnabled = business.IsKOTEnabled;
            existing.IsActive = business.IsActive;
            existing.IsMultilengual= business.IsMultilengual;
            existing.BarcodeEnabled = business.BarcodeEnabled;
            existing.IsCustomerMandetory = business.IsCustomerMandetory;
            existing.KichenPrinterName = business.KichenPrinterName;
            existing.CounterPrinterName = business.CounterPrinterName;
            existing.IsTableNoRequired = business.IsTableNoRequired;
            existing.IsReceiptReprint = business.IsReceiptReprint;
            existing.Qrcode = business.Qrcode;
            existing.ReportData = business.ReportData;
            //// Replace logo if file selected
            //if (LogoFile != null)
            //{
            //    existing.Logo = await SaveFile(LogoFile);
            //}

            //// Replace QR Code if file selected
            //if (QRCodeFile != null)
            //{
            //    existing.Qrcode = await SaveFile(QRCodeFile);
            //}

            // ✅ CHECK IF JUST ENABLED
            bool financeJustEnabled =
                 existing.EnableFinance.GetValueOrDefault()
                 && business.EnableFinance.GetValueOrDefault();

            existing.EnableFinance = business.EnableFinance;

            if (financeJustEnabled)
            {
                bool cashExists = _context.TblLedgerAccounts
                    .Any(x => x.BusinessId == existing.Id && x.Name == "Cash In Hand");

                if (!cashExists)
                {
                    _context.TblLedgerAccounts.Add(new TblLedgerAccount
                    {
                        BusinessId = existing.Id,
                        Name = "Cash In Hand",
                        Type = "Cash",
                        Description = "Cash Account",
                        CreatedOn = DateTime.Now
                    });
                }

                bool bankExists = _context.TblLedgerAccounts
                    .Any(x => x.BusinessId == existing.Id && x.Name == "Cash In Bank");

                if (!bankExists)
                {
                    _context.TblLedgerAccounts.Add(new TblLedgerAccount
                    {
                        BusinessId = existing.Id,
                        Name = "Cash In Bank",
                        Type = "Bank",
                        Description = "Bank Account",
                        CreatedOn = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [AuthorizeToRoles("Owner")]
        public async Task<IActionResult> Setting()
        {
            ViewBag.BusinessTypes = await _context.TblBusinessTypes.ToListAsync();
            ViewBag.PrinterSizes = await _context.TblPrinterSizes.ToListAsync();
            ViewBag.YesNo =new List<ModelYesNo>(){ 
                new ModelYesNo(){key="true" , value="Yes" },
                new ModelYesNo(){ key="false" , value="No" },
            };
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            var b = await _context.TblBusinesses.FindAsync(businessId);

            if (b == null)
                return NotFound();

            return View(b);
        }
        [AuthorizeToRoles("Owner")]
        [HttpPost]
        public async Task<IActionResult> SaveSettings([FromForm]TblBusiness business)
        {
            ModelState.Remove("Id");
            ModelState.Remove("IsActive");
            ModelState.Remove("HideCustomerField");
            ModelState.Remove("HideTableDropDown");
            ModelState.Remove("IsKOTEnabled");
            ModelState.Remove("IsCustomerMandetory");
            ModelState.Remove("BarcodeEnabled");
            ModelState.Remove("IsMultilengual");
            if (!ModelState.IsValid)
            {
                var allErrors = ModelState
          .Where(x => x.Value.Errors.Count > 0)
          .Select(x => new
          {
              Field = x.Key,
              Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToList()
          }).ToList();

                return BadRequest(allErrors);
            }

            var existing = await _context.TblBusinesses.FindAsync(business.Id);

            if (existing == null)
                return NotFound();

            // Update fields
            existing.BusinessName = business.BusinessName;
            existing.BusinessTypeId = business.BusinessTypeId;
            existing.OwnerName = business.OwnerName;
            existing.Gstin = business.Gstin;
            existing.Email = business.Email;
            existing.MobileNo = business.MobileNo;
            existing.Address = business.Address;
            existing.City = business.City;
            existing.PrinterSizeId = business.PrinterSizeId;
            existing.IsGstapplicable = business.IsGstapplicable;
            existing.HideCustomerField = business.HideCustomerField;
            existing.HideTableDropDown = business.HideTableDropDown;
           
            await _context.SaveChangesAsync();
            return RedirectToAction("Setting");
        }
        // ==========
        // DELETE
        // ==========
        [AuthorizeToRoles("Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.TblBusinesses.FindAsync(id);

            if (data == null)
                return NotFound();

            _context.TblBusinesses.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private async Task<string> SaveFile(IFormFile file)
        {
            string folder = Path.Combine(_env.WebRootPath, "Uploads");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string path = Path.Combine(folder, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }
        [HttpPost]
        public async Task<IActionResult> DeleteLogo(int id)
        {
            var business = await _context.TblBusinesses.FindAsync(id);

            if (business == null || string.IsNullOrEmpty(business.Logo))
                return BadRequest("No logo found");

            string folder = Path.Combine(_env.WebRootPath, "Uploads");
            string path = Path.Combine(folder, business.Logo);

            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            business.Logo = null;
            await _context.SaveChangesAsync();

            return Ok();
        }

        private async Task<string> SaveCompressedLogo(IFormFile file)
        {
            using var image = await SixLabors.ImageSharp.Image.LoadAsync(file.OpenReadStream());

            // Resize (optional but helps with compression)
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(120, 0) // limit width to 120px
            }));

            using var ms = new MemoryStream();

            // First encoding attempt
            var encoder = new JpegEncoder
            {
                Quality = 50 // init-only
            };

            await image.SaveAsJpegAsync(ms, encoder);

            // If still too large, do a second pass
            if (ms.Length > 10 * 1024) // >10 KB
            {
                ms.SetLength(0);

                encoder = new JpegEncoder
                {
                    Quality = 30 // more aggressive
                };

                await image.SaveAsJpegAsync(ms, encoder);
            }

            string folder = Path.Combine(_env.WebRootPath, "Uploads");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = Guid.NewGuid() + ".jpg";
            string path = Path.Combine(folder, fileName);

            await System.IO.File.WriteAllBytesAsync(path, ms.ToArray());

            return fileName;
        }

    }
}

