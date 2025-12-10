using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eOrderTouchApp.Controllers
{
    public class GSTController : Controller
    {
        private readonly eOrderTouchContext _context;

        public GSTController(eOrderTouchContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            var list = await _context.TblGsts
                .Where(x => x.BusinessId == businessId)
                .ToListAsync();

            return View(list);
        }

        [HttpPost]
        public async Task<JsonResult> Create([FromBody] TblGST gst)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            if (gst.GSTValue == null)
                return Json(new { success = false, message = "GST value required" });

            gst.BusinessId = businessId;
            _context.TblGsts.Add(gst);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<JsonResult> Edit([FromBody] TblGST gst)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            var entity = await _context.TblGsts
                .FirstOrDefaultAsync(x => x.Id == gst.Id && x.BusinessId == businessId);

            if (entity == null)
                return Json(new { success = false, message = "GST not found" });

            entity.GSTValue = gst.GSTValue;
            entity.DisplayName = gst.DisplayName;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            var entity = await _context.TblGsts
                .FirstOrDefaultAsync(x => x.Id == id && x.BusinessId == businessId);

            if (entity == null)
                return Json(new { success = false, message = "Not found" });

            _context.TblGsts.Remove(entity);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

    }
}
