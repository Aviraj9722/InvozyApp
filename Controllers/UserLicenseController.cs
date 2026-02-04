using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eOrderTouchApp.Controllers
{
    [AuthorizeToRoles("Admin")]
    public class UserLicenseController : Controller
    {
        private readonly eOrderTouchContext _context;

        public UserLicenseController(eOrderTouchContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int OrgId=0)
        {

            TempData["OrgId"] = OrgId;
            ViewBag.Business = await _context.TblUserLicenses.Where(w=>w.BusinessId== OrgId).OrderByDescending(o=>o.BusinessId).ToListAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            int OrgId = Convert.ToInt32(TempData.Peek("OrgId"));
            var data = await _context.TblUserLicenses
                .Where(w => w.BusinessId == OrgId)
                .Include(x => x.Business)
                .Select(x => new {
                    id = x.Id,
                    licenseKey = x.LicenseKey,
                    startDate = x.StartDate,
                    endDate = x.EndDate,
                    businessName = x.Business.BusinessName
                })
                .ToListAsync();

            return Json(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TblUserLicense model)
        {
            try
            {
                int orgId = Convert.ToInt32(TempData.Peek("OrgId"));

                if (string.IsNullOrWhiteSpace(model.LicenseKey))
                    return Json(new { success = false, message = "License Key is required" });

                if (model.StartDate == null)
                    return Json(new { success = false, message = "Start Date is required" });

                if (model.EndDate == null)
                    return Json(new { success = false, message = "End Date is required" });

                model.BusinessId = orgId;
                model.CreatedOn = DateTime.Now;

                _context.TblUserLicenses.Add(model);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Created successfully" });
            }
            catch (Exception er)
            {
                return Json(new { success = false, message = "Unable to create" });
            }


        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] TblUserLicense model)
        {
            if (model.Id <= 0)
                return Json(new { success = false, message = "Invalid license Id" });

            int orgId = Convert.ToInt32(TempData.Peek("OrgId"));

            var existing = await _context.TblUserLicenses
                .Where(x => x.Id == model.Id && x.BusinessId == orgId)
                .FirstOrDefaultAsync();

            if (existing == null)
                return Json(new { success = false, message = "License not found" });

            // Update fields
            existing.LicenseKey = model.LicenseKey?.Trim();
            existing.StartDate = model.StartDate;
            existing.EndDate = model.EndDate;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Updated successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            int OrgId = Convert.ToInt32(TempData.Peek("OrgId"));
            var obj = await _context.TblUserLicenses
                .Where(w => w.Id == id && w.BusinessId == OrgId)
                .FirstOrDefaultAsync();

            if (obj == null)
                return Json(new { success = false, message = "License not found" });

            _context.TblUserLicenses.Remove(obj);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
