using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 20;

            var data = await _context.TblBusinesses
                        .OrderByDescending(x => x.Id)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

            // Drop-down list data
            ViewBag.BusinessTypes = await _context.TblBusinessTypes.ToListAsync();
            ViewBag.PrinterSizes = await _context.TblPrinterSizes.ToListAsync();

            return View(data);
        }

        // ===============
        // CREATE BUSINESS
        // ===============
        [HttpPost]
        public async Task<IActionResult> Create(TblBusiness business)
        {
            ModelState.Remove("Id");
            ModelState.Remove("IsActive");
            ModelState.Remove("HideCustomerField");
            ModelState.Remove("HideTableDropDown");
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

            _context.TblBusinesses.Add(business);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // ==========
        // EDIT (GET)
        // ==========
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
        public async Task<IActionResult> Update(TblBusiness business)//, IFormFile LogoFile, IFormFile QRCodeFile)
        {
            ModelState.Remove("Id");
            ModelState.Remove("IsActive");
            ModelState.Remove("HideCustomerField");
            ModelState.Remove("HideTableDropDown");
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
            existing.IsActive = business.IsActive;

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

        // ==========
        // DELETE
        // ==========
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

        // =====================
        // SAVE FILE (Helper)
        // =====================
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

