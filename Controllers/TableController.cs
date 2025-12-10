using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eOrderTouchApp.Controllers
{
    public class TableController : Controller
    {
        private readonly eOrderTouchContext _context;

        public TableController(eOrderTouchContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            var list = await _context.TblTables
                .Where(x => x.BusinessId == businessId)
                .ToListAsync();

            return View(list);
        }

        [HttpPost]
        public async Task<JsonResult> Create([FromBody] TblTable table)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            if (string.IsNullOrWhiteSpace(table.Name))
                return Json(new { success = false, message = "Name is required" });

            table.BusinessId = businessId;
            _context.TblTables.Add(table);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<JsonResult> Edit([FromBody] TblTable table)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            var entity = await _context.TblTables
                .FirstOrDefaultAsync(x => x.Id == table.Id && x.BusinessId == businessId);

            if (entity == null)
                return Json(new { success = false });

            entity.Name = table.Name;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            var entity = await _context.TblTables
                .FirstOrDefaultAsync(x => x.Id == id && x.BusinessId == businessId);

            if (entity == null)
                return Json(new { success = false });

            _context.TblTables.Remove(entity);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
