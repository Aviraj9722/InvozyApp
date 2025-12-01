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

        public async Task<IActionResult> Index(int id=0)
        {

            TempData["UserId"] = id;
            ViewBag.Users = await _context.TblUserLicenses.Where(w=>w.UserId==id).OrderByDescending(o=>o.Id).ToListAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            int uid = Convert.ToInt32(TempData.Peek("UserId"));
            var data = await _context.TblUserLicenses.Where(w => w.UserId == uid)
                .Include(x => x.User)
                .Select(x => new {
                    x.Id,
                    x.LicenseKey,
                    x.StartDate,
                    x.EndDate,
                    UserName = x.User.UserName
                }).ToListAsync();

            return Json(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TblUserLicense model)
        {
            model.UserId = Convert.ToInt32(TempData.Peek("UserId"));
            model.CreatedOn = DateTime.Now;

            await _context.TblUserLicenses.AddAsync(model);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Saved successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> Update( TblUserLicense model)
        {
            var existing = await _context.TblUserLicenses.FindAsync(model.Id);
            if (existing == null)
                return Json(new { success = false, message = "Not found" });
            
            model.UserId = Convert.ToInt32(TempData.Peek("UserId"));
            existing.UserId = model.UserId;
            existing.LicenseKey = model.LicenseKey;
            existing.StartDate = model.StartDate;
            existing.EndDate = model.EndDate;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Updated successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            int UserId = Convert.ToInt32(TempData.Peek("UserId"));
            var obj = await _context.TblUserLicenses.Where(w=>w.UserId == UserId).FirstOrDefaultAsync();
            if (obj == null)
                return Json(new { success = false });

            _context.TblUserLicenses.Remove(obj);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
