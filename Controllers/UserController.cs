using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace eOrderTouchApp.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly eOrderTouchContext _context;

        public UserController(eOrderTouchContext context)
        {
            _context = context;
        }

        // GET: User List

        public async Task<IActionResult> UserList()
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            TempData["OrgId"] = businessId;
            var users = await _context.TblUsers.Include(I=>I.TblUserLicenses).Where(w => w.BussinessId == businessId).ToListAsync();
           
            return View(users);
        }
        public async Task<IActionResult> Index(int OrgId=0)
        {
            if (User.FindFirst("OrgId")!=null)
            {
                OrgId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);
            }
            TempData["OrgId"] = OrgId;
            var users = await _context.TblUsers.Where(w=>w.BussinessId == OrgId).ToListAsync();
            var org = await _context.TblBusinesses.Where(w=>w.Id == OrgId).FirstOrDefaultAsync();
            ViewBag.OrgName = org.BusinessName;
            return View(users);
        }

        [HttpPost]
        public async Task<JsonResult> Create([FromBody] TblUser user)
        {
            if (ModelState.IsValid)
            {
                user.BussinessId = Convert.ToInt32(TempData.Peek("OrgId"));
                user.CreatedOn = DateTime.Now;
                _context.TblUsers.Add(user);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Invalid data" });
        }

        [HttpPost]
        public async Task<JsonResult> Edit([FromBody] TblUser user)
        {
            if (ModelState.IsValid)
            {
                user.BussinessId = Convert.ToInt32(TempData.Peek("OrgId"));
                user.CreatedOn = DateTime.Now;
                _context.TblUsers.Update(user);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Invalid data" });
        }

        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            var user = await _context.TblUsers.FindAsync(id);
            if (user != null)
            {
                _context.TblUsers.Remove(user);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "User not found" });
        }

      
    }
}
