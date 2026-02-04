using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace eOrderTouchApp.Controllers
{

    public class HeadOfficeController : Controller
    {
        private readonly eOrderTouchContext _context;

        public HeadOfficeController(eOrderTouchContext context)
        {
            _context = context;
        }

        // GET: User List
        [AuthorizeToRoles("Admin")]
        public async Task<IActionResult> UserList()
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            TempData["OrgId"] = businessId;
            var users = await _context.TblUsers.Where(w => w.BussinessId == businessId).ToListAsync();

            return View(users);
        }

        [AuthorizeToRoles("Admin")]
        public async Task<IActionResult> Index()
        {
            var users = await _context.TblUsers
           .Where(u => u.Role == "HeadOfficer")
           .OrderByDescending(u => u.Id)
           .ToListAsync();

                ViewBag.AllBusinesses = await _context.TblBusinesses.ToListAsync();

            return View(users);
        }

        [AuthorizeToRoles("Admin")]
        [HttpPost]
        public async Task<JsonResult> Create([FromBody] JsonElement data)
        {
            try
            {

                var user = JsonSerializer.Deserialize<TblUser>(
                data.GetProperty("user").GetRawText());


                var businessIds = JsonSerializer.Deserialize<List<int>>(
                data.GetProperty("businessIds").GetRawText());

                if (user == null)
                    return Json(new { success = false, message = "Invalid user data" });

                user.Role = "HeadOfficer";
                user.CreatedOn = DateTime.Now;

                _context.TblUsers.Add(user);
                await _context.SaveChangesAsync();

                if (businessIds != null && businessIds.Any())
                {
                    foreach (var bizId in businessIds)
                    {
                        _context.TblHOUnits.Add(new TblHOUnit
                        {
                            UserId = user.Id,
                            BusinessId = bizId
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                
                return Json(new { success = false, message = ex.Message });
            }
        }


        [AuthorizeToRoles("Admin")]
        [HttpPost]
        public async Task<JsonResult> Edit([FromBody] JsonElement data)
        {
            try
            {
                var user = JsonSerializer.Deserialize<TblUser>(
                    data.GetProperty("user").GetRawText()
                );

                var businessIds = JsonSerializer.Deserialize<List<int>>(
                    data.GetProperty("businessIds").GetRawText()
                );

                if (user == null)
                    return Json(new { success = false, message = "Invalid user data" });

                // ===== UPDATE USER =====
                var dbUser = await _context.TblUsers.FindAsync(user.Id);
                if (dbUser == null)
                    return Json(new { success = false, message = "User not found" });

                dbUser.Name = user.Name;
                dbUser.EmailId = user.EmailId;
                dbUser.MobileNumber = user.MobileNumber;
                dbUser.Role = "HeadOfficer";
                dbUser.IsActive = user.IsActive;
                dbUser.UserName = user.UserName;
                dbUser.Password = user.Password;

                _context.TblUsers.Update(dbUser);

                // ===== UPDATE HO UNIT MAPPING =====
                var oldMappings = _context.TblHOUnits
                    .Where(x => x.UserId == user.Id);

                _context.TblHOUnits.RemoveRange(oldMappings);

                if (businessIds != null && businessIds.Any())
                {
                    foreach (var bizId in businessIds)
                    {
                        _context.TblHOUnits.Add(new TblHOUnit
                        {
                            UserId = user.Id,
                            BusinessId = bizId
                        });
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetUserBusinesses(int id)
        {
            var bizIds = await _context.TblHOUnits
                .Where(x => x.UserId == id)
                .Select(x => x.BusinessId)
                .ToListAsync();

            return Json(bizIds);
        }

        [AuthorizeToRoles("Admin")]
        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            try
            {
                var user = await _context.TblUsers.FindAsync(id);
                if (user == null)
                    return Json(new { success = false, message = "User not found" });

                // 🔥 STEP 1: Remove HO mappings FIRST
                var hoUnits = await _context.TblHOUnits
                    .Where(x => x.UserId == id)
                    .ToListAsync();

                if (hoUnits.Any())
                {
                    _context.TblHOUnits.RemoveRange(hoUnits);
                }

                // 🔥 STEP 2: Remove User
                _context.TblUsers.Remove(user);

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }

        }


    }
}
