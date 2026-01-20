using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eOrderTouchApp.Controllers
{
    public class KitchenCounterController : Controller
    {
        private readonly eOrderTouchContext _context;

        public KitchenCounterController(eOrderTouchContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            var list = await _context.TblKitchenCounters
                .Where(x => x.BusinessId == businessId)
                .OrderByDescending(x => x.Id) 
                .ToListAsync();

            return View(list);
        }

        [HttpPost]
        public async Task<JsonResult> Create([FromBody] TblKitchenCounter counter)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            if (string.IsNullOrWhiteSpace(counter.Name))
                return Json(new { success = false, message = "Counter name required" });

            counter.BusinessId = businessId;

            _context.TblKitchenCounters.Add(counter);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<JsonResult> Edit([FromBody] TblKitchenCounter counter)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            var entity = await _context.TblKitchenCounters
                .FirstOrDefaultAsync(x => x.Id == counter.Id && x.BusinessId == businessId);

            if (entity == null)
                return Json(new { success = false, message = "Kitchen counter not found" });

            entity.Name = counter.Name;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            var entity = await _context.TblKitchenCounters
                .FirstOrDefaultAsync(x => x.Id == id && x.BusinessId == businessId);

            if (entity == null)
                return Json(new { success = false, message = "Not found" });

            _context.TblKitchenCounters.Remove(entity);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
