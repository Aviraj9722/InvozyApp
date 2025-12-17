using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.ProjectModel;

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
        [AuthorizeToRoles("Admin")]
        public async Task<IActionResult> Index(int page = 1)
        {
            //int pageSize = 20;

            var data = await _context.TblBusinesses
                        .OrderByDescending(x => x.Id)
                        //.Skip((page - 1) * pageSize)
                        //.Take(pageSize)
                        .ToListAsync();

            // Drop-down list data
            ViewBag.BusinessTypes = await _context.TblBusinessTypes.ToListAsync();
            ViewBag.PrinterSizes = await _context.TblPrinterSizes.ToListAsync();

            return View(data);
        }
        [AuthorizeToRoles("Admin")]
        // ===============
        // CREATE BUSINESS
        // ===============
        [HttpPost]
        public async Task<IActionResult> Create(TblBusiness business, IFormFile? LogoFile)
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

            business.CreatedOn = DateTime.Now;

            // Logo upload
            if (LogoFile != null)
            {
                // 🔐 Size validation (100 KB)
                if (LogoFile.Length > 100 * 1024)
                    return BadRequest("Logo size must be less than 100 KB");

                // 🔐 Type validation
                var allowedTypes = new[] { "image/jpeg", "image/png" };
                if (!allowedTypes.Contains(LogoFile.ContentType))
                    return BadRequest("Only PNG or JPG images are allowed");

                // Delete old logo (optional)
                if (!string.IsNullOrEmpty(business.Logo))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, "Uploads", business.Logo);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                business.Logo = await SaveFile(LogoFile);
            }


            _context.TblBusinesses.Add(business);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // ==========
        // EDIT (GET)
        // ==========
        [AuthorizeToRoles("Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var b = await _context.TblBusinesses.FindAsync(id);

            if (b == null)
                return NotFound();

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
                isActive = b.IsActive,
                logo = b.Logo,
                mobileNo = b.MobileNo,
                qrCode = b.Qrcode
            });
        }



        // =============
        // UPDATE (POST)
        // =============
        [HttpPost]
        [AuthorizeToRoles("Admin")]
        // public async Task<IActionResult> Update(TblBusiness business)//, IFormFile LogoFile, IFormFile QRCodeFile)
        public async Task<IActionResult> Update(
    TblBusiness business,
    IFormFile? LogoFile
)
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

            // Logo upload
            if (LogoFile != null)
            {
                // 🔐 Size validation (100 KB)
                if (LogoFile.Length > 100 * 1024)
                    return BadRequest("Logo size must be less than 100 KB");

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

                existing.Logo = await SaveFile(LogoFile);
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
            existing.Qrcode = business.Qrcode;
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
        [AuthorizeToRoles("Owner")]
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

    }
}

