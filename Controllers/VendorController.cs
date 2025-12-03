using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eOrderTouchApp.Controllers
{
    public class VendorController : Controller
    {
        private readonly eOrderTouchContext _context;

        public VendorController(eOrderTouchContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vendors = await _context.TblVendors.ToListAsync();
            return View(vendors);
        }

        // CREATE OR UPDATE
        [HttpPost]
        public async Task<IActionResult> SaveVendor(TblVendor vendor)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid data");

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
