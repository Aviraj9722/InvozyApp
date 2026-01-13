using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace eOrderTouchApp.Controllers
{
    [AuthorizeToRoles("Owner")]
    public class ProductController : Controller
    {
        private readonly eOrderTouchContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(eOrderTouchContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            ViewBag.Categories = await _context.TblCategories.Where(w => w.BusinessId == businessId).ToListAsync();
            ViewBag.GST = await _context.TblGsts
            .Where(w => w.BusinessId == businessId)
            .ToListAsync();

            ViewBag.UOMList = await _context.TblUoms
            .Where(x => x.BusinessId == businessId)
            .ToListAsync();

            // ✅ ADD THIS
            ViewBag.KitchenCounters = await _context.TblKitchenCounters
                .Where(k => k.BusinessId == businessId)
                .ToListAsync();

            var products = await _context.TblProducts.Where(w => w.BusinessId == businessId).Include(p => p.Category).OrderByDescending(o => o.Id).ToListAsync();
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> Save(TblProduct product, IFormFile? photo)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            //Checks BarCode 
            if (!string.IsNullOrWhiteSpace(product.Code))
            {
                bool exists = await _context.TblProducts
                    .AnyAsync(p =>
                        p.BusinessId == businessId &&
                        p.Code == product.Code &&
                        p.Id != product.Id
                    );

                if (exists)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Barcode already exists in your organization. Please enter a unique barcode."
                    });
                }
            }

            if (!string.IsNullOrEmpty(Request.Form["croppedImageData"]))
            {
                string base64 = Request.Form["croppedImageData"];
                var bytes = Convert.FromBase64String(base64.Replace("data:image/jpeg;base64,", ""));

                if (bytes.Length > 50000)
                    return Json(new { success = false, message = "Image size must be less than 50KB." });

                string uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);

                string fileName = Guid.NewGuid() + ".jpg";
                string filePath = Path.Combine(uploads, fileName);

                System.IO.File.WriteAllBytes(filePath, bytes);
                product.Photo = "/uploads/" + fileName;
            }
            decimal price = product.Price ?? 0;
            decimal gstPercent = product.Gstpercentage ?? 0;

            product.Gstamount = Math.Round(
                (price * gstPercent) / 100,
                2
            );
            product.BusinessId = businessId;
            if (product.Id == 0)
            {
                product.CreatedOn = DateTime.Now;
                _context.TblProducts.Add(product);
            }
            else
            {
                var existing = await _context.TblProducts.FindAsync(product.Id);
                if (existing == null) return NotFound();

                existing.Name = product.Name;
                existing.Code = product.Code;
                existing.RegionalName = product.RegionalName;
                existing.Price = product.Price;
                existing.CategoryId = product.CategoryId;
                existing.Gstpercentage = product.Gstpercentage;
                existing.Gstamount = product.Gstamount;
                existing.PurchasePrice = product.PurchasePrice;
                existing.UoMid = product.UoMid;
                existing.HSNCode = product.HSNCode;
                existing.KitchenCounterId = product.KitchenCounterId;
                if (product.Photo != null) existing.Photo = product.Photo;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Product saved successfully" });
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            var product = await _context.TblProducts
                .FirstOrDefaultAsync(w => w.Id == id && w.BusinessId == businessId);

            if (product != null)
            {
                _context.TblProducts.Remove(product);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }
    }
}
