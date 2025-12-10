using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eOrderTouchApp.Controllers
{
    [AuthorizeToRoles("Owner")]
    public class VendorController : Controller
    {
        private readonly eOrderTouchContext _context;

        public VendorController(eOrderTouchContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);
            var vendors = await _context.TblVendors.Where(w=>w.BusinessId == businessId).ToListAsync();
            return View(vendors);
        }

        // CREATE OR UPDATE
        [HttpPost]
        public async Task<IActionResult> SaveVendor(TblVendor vendor)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);
            if (!ModelState.IsValid)
                return BadRequest("Invalid data");
            vendor.BusinessId = businessId;
            if (vendor.Id == 0)
            {
                _context.TblVendors.Add(vendor);
            }
            else
            {
                _context.TblVendors.Update(vendor);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = vendor.Id == 0 ? "Vendor created!" : "Vendor updated!" });
        }

        // GET BY ID (AJAX)
        public async Task<IActionResult> GetVendor(int id)
        {


            var vendor = await _context.TblVendors.FindAsync(id);
            if (vendor == null) return NotFound();

            return Json(vendor);
        }

        // DELETE
        public async Task<IActionResult> Delete(int id)
        {
            var vendor = await _context.TblVendors.FindAsync(id);
            if (vendor == null) return NotFound();

            _context.TblVendors.Remove(vendor);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Vendor deleted!" });
        }
    }
}
