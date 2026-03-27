using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace eOrderTouchApp.Controllers
{

    public class UserController : Controller
    {
        private readonly eOrderTouchContext _context;

        public UserController(eOrderTouchContext context)
        {
            _context = context;
        }

        // GET: User List
        [AuthorizeToRoles("Owner")]
        public async Task<IActionResult> UserList()
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            TempData["OrgId"] = businessId;
            var users = await _context.TblUsers.Where(w => w.BussinessId == businessId).ToListAsync();

            return View(users);
        }

        [AuthorizeToRoles("Admin", "Dealer")]
        public async Task<IActionResult> Index(int OrgId = 0)
        {
            int businessId = 0;

            // 👉 If Admin → use passed OrgId
            if (User.IsInRole("Admin"))
            {
                businessId = OrgId == 0
                    ? Convert.ToInt32(User.FindFirst("OrgId")?.Value)
                    : OrgId;
            }

            // 👉 If Dealer → always use their OrgId
            if (User.IsInRole("Dealer"))
            {
                businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);
            }
            if (OrgId>0)
            {
                businessId = OrgId;
            }
            TempData["OrgId"] = businessId;

            var users = await _context.TblUsers
                .Where(w => w.BussinessId == businessId)
                .ToListAsync();

            var org = await _context.TblBusinesses
                .FirstOrDefaultAsync(w => w.Id == businessId);

            ViewBag.OrgName = org?.BusinessName;
            ViewBag.Roles = Roles.GetRoles();

            return View(users);
        }

        [AuthorizeToRoles("Admin","Dealer")]
        [HttpPost]
        public async Task<JsonResult> Create([FromBody] TblUser user)
        {
            int orgId = Convert.ToInt32(TempData.Peek("OrgId"));
            // ✅ STEP 1: CHECK USER COUNT FOR THIS BUSINESS
            int userCount = await _context.TblUsers
                .CountAsync(u => u.BussinessId == orgId);

            if (userCount >= 5)
            {
                return Json(new
                {
                    success = false,
                    message = "User limit reached (Max 5 users allowed per business)."
                });
            }

            // === DUPLICATE CHECK ===
            bool emailExists = await _context.TblUsers
            .AnyAsync(u => u.BussinessId == orgId && u.EmailId == user.EmailId);

            if (emailExists)
                return Json(new { success = false, message = "Email already exists in this organisation." });

            bool mobileExists = await _context.TblUsers
                .AnyAsync(u => u.BussinessId == orgId && u.MobileNumber == user.MobileNumber);

            if (mobileExists)
                return Json(new { success = false, message = "Mobile number already exists in this organisation." });

            bool usernameExists = await _context.TblUsers
                .AnyAsync(u => u.BussinessId == orgId && u.UserName == user.UserName);

            if (usernameExists)
                return Json(new { success = false, message = "Username already exists in this organisation." });


            if (ModelState.IsValid)
            {
                user.BussinessId = orgId;
                user.CreatedOn = DateTime.Now;

                _context.TblUsers.Add(user);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Invalid data" });
        }

        [AuthorizeToRoles("Admin","Dealer")]
        [HttpPost]
        public async Task<JsonResult> Edit([FromBody] TblUser user)
        {
            int orgId = Convert.ToInt32(TempData.Peek("OrgId"));

            // === DUPLICATE CHECK WHILE EDIT ===
            bool emailExists = await _context.TblUsers
            .AnyAsync(u => u.BussinessId == orgId
                    && u.EmailId == user.EmailId
                    && u.Id != user.Id);

            if (emailExists)
                return Json(new { success = false, message = "Email already exists in this organisation." });

            bool mobileExists = await _context.TblUsers
                .AnyAsync(u => u.BussinessId == orgId
                            && u.MobileNumber == user.MobileNumber
                            && u.Id != user.Id);

            if (mobileExists)
                return Json(new { success = false, message = "Mobile number already exists in this organisation." });

            bool usernameExists = await _context.TblUsers
                .AnyAsync(u => u.BussinessId == orgId
                            && u.UserName == user.UserName
                            && u.Id != user.Id);

            if (usernameExists)
                return Json(new { success = false, message = "Username already exists in this organisation." });


            if (ModelState.IsValid)
            {
                user.BussinessId = orgId;
                user.CreatedOn = DateTime.Now;

                _context.TblUsers.Update(user);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Invalid data" });

        }

        [AuthorizeToRoles("Admin","Dealer")]
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
