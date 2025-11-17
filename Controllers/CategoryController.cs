using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace eOrderTouchApp.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly eOrderTouchContext _context;

        public CategoryController(eOrderTouchContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            var categories = await _context.TblCategories.Where(w=>w.BusinessId == businessId).ToListAsync();
            return View(categories);
        }

        [HttpPost]
        public async Task<JsonResult> Create([FromBody] TblCategory category)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            if (string.IsNullOrWhiteSpace(category.Name))
                return Json(new { success = false, message = "Category name is required." });
            category.BusinessId = businessId;
            _context.TblCategories.Add(category);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<JsonResult> Edit([FromBody] TblCategory category)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            var dbCategory = await _context.TblCategories.FindAsync(category.Id);
            if (dbCategory == null)
                return Json(new { success = false, message = "Category not found." });
            category.BusinessId = businessId;
            dbCategory.Name = category.Name;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            var category = await _context.TblCategories.Where(w=>w.BusinessId==businessId).FirstOrDefaultAsync();
            if (category == null)
                return Json(new { success = false });

            _context.TblCategories.Remove(category);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}
