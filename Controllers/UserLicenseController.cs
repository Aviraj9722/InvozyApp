using eOrderTouchApp.Models;
using eOrderTouchApp.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            ViewBag.Dealers = await _context.TblDealer
                                        .Select(d => new SelectListItem
                                        {
                                            Value = d.Id.ToString(),
                                            Text = d.Name
                                        }).ToListAsync();

            ViewBag.OrgName = await _context.TblBusinesses
                                   .Where(x => x.Id == OrgId)
                                   .Select(x => x.BusinessName)
                                   .FirstOrDefaultAsync();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            int OrgId = Convert.ToInt32(TempData.Peek("OrgId"));
            var data = await _context.TblUserLicenses
                .Where(w => w.BusinessId == OrgId)
                .Select(x => new {
                    id = x.Id,
                    licenseKey = x.LicenseKey,
                    startDate = x.StartDate,
                    endDate = x.EndDate,
                    dealerId = x.DealerId,
                    dealerName = _context.TblDealer
                                .Where(d => d.Id == x.DealerId)
                                .Select(d => d.Name)
                                .FirstOrDefault()
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
            existing.DealerId = model.DealerId;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Updated successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] DeleteLicenseVM model)
        {
            int OrgId = Convert.ToInt32(TempData.Peek("OrgId"));

            var obj = await _context.TblUserLicenses
                .FirstOrDefaultAsync(w => w.Id == model.Id && w.BusinessId == OrgId);

            if (obj == null)
                return Json(new { success = false, message = "License not found" });

            _context.TblUserLicenses.Remove(obj);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Deleted successfully" });
        }

        public async Task<IActionResult> LicenseStatus()
        {
            var today = DateTime.Today;

            var data = await _context.TblBusinesses
                .Select(b => new BusinessLicenseStatusVM
                {
                    BusinessId = b.Id,
                    BusinessName = b.BusinessName,
                    
                    DealerName = _context.TblUserLicenses
                    .Where(l => l.BusinessId == b.Id)
                    .OrderByDescending(l => l.EndDate)
                    .Join(_context.TblDealer,
                          l => l.DealerId,
                          u => u.Id,
                          (l, u) => u.Name) 
                    .FirstOrDefault(),

                    LicenseKey = _context.TblUserLicenses
                        .Where(l => l.BusinessId == b.Id)
                        .OrderByDescending(l => l.EndDate)
                        .Select(l => l.LicenseKey)
                        .FirstOrDefault(),

                    LicenseStartDate = _context.TblUserLicenses
                        .Where(l => l.BusinessId == b.Id)
                        .OrderByDescending(l => l.EndDate)
                        .Select(l => l.StartDate)
                        .FirstOrDefault(),

                    LicenseEndDate = _context.TblUserLicenses
                        .Where(l => l.BusinessId == b.Id)
                        .OrderByDescending(l => l.EndDate)
                        .Select(l => l.EndDate)
                        .FirstOrDefault()
                })
                .ToListAsync();

            // ✅ STATUS LOGIC
            foreach (var item in data)
            {
                if (item.LicenseEndDate == null)
                {
                    item.Status = "No License";
                }
                else if (item.LicenseEndDate < today)
                {
                    item.Status = "Expired";
                }
                else if (item.LicenseEndDate <= today.AddDays(15))
                {
                    item.Status = "Expiring Soon";
                }
                else
                {
                    item.Status = "Active";
                }
            }

            // Order by EndDate descending
            data = data
                .OrderByDescending(x => x.LicenseEndDate)
                .ToList();

            return View(data);
        }
    }
}
