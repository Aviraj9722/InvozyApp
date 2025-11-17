using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace eOrderTouchApp.Controllers
{
    [Authorize]
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
            var products = await _context.TblProducts.Where(w=>w.BusinessId == businessId).Include(p => p.Category).OrderByDescending(o=>o.Id).ToListAsync();
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> Save(TblProduct product, IFormFile? photo)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);


            if (photo != null && photo.Length > 0)
            {
                string uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);

                string fileName = Guid.NewGuid() + Path.GetExtension(photo.FileName);
                string filePath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                product.Photo = "/uploads/" + fileName;
            }
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
                if (product.Photo != null) existing.Photo = product.Photo;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            var product = await _context.TblProducts.Where(w=>w.Id==id && w.BusinessId == businessId).FirstOrDefaultAsync();
            if (product != null)
            {
                _context.TblProducts.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
