using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eOrderTouchApp.Controllers
{
    [AuthorizeToRoles("Admin")]
    public class DealerController : Controller
    {
        private readonly eOrderTouchContext _context;

        public DealerController(eOrderTouchContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var dealers = await _context.TblDealer.ToListAsync();
            return View(dealers);
        }

        [HttpPost]
        public async Task<IActionResult> SaveDealer(TblDealer dealer)
        {
            try
            {
                bool isNew = dealer.Id == 0;   // capture before EF changes it

                if (isNew)
                    _context.TblDealer.Add(dealer);
                else
                    _context.TblDealer.Update(dealer);

                await _context.SaveChangesAsync();

                return Ok(new { message = isNew ? "Dealer saved successfully" : "Dealer updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        public async Task<IActionResult> GetDealer(int id)
        {
            var dealer = await _context.TblDealer.FindAsync(id);
            if (dealer == null) return NotFound();

            return Json(dealer);
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var dealer = await _context.TblDealer.FindAsync(id);
                if (dealer == null) return NotFound();

                _context.TblDealer.Remove(dealer);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Dealer deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }
    }
}
