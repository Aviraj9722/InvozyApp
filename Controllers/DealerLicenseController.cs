using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eOrderTouchApp.Models;

namespace eOrderTouchApp.Controllers
{
    public class DealerLicenseController : Controller
    {
        private readonly eOrderTouchContext _context;

        public DealerLicenseController(eOrderTouchContext context)
        {
            _context = context;
        }

        // ===============================
        // Dealer License Page
        // ===============================
        public async Task<IActionResult> Index(int dealerId)
        {
            var dealer = await _context.TblDealer
                .FirstOrDefaultAsync(x => x.Id == dealerId);

            ViewBag.DealerId = dealerId;
            ViewBag.DealerName = dealer?.Name;

            var data = await _context.TblDealerLicenseTransactions
                .Where(x => x.DealerId == dealerId)
                .OrderByDescending(x => x.CreatedOn)
                .ToListAsync();

            return View(data);
        }

        // ===============================
        // CREATE
        // ===============================
        [HttpPost]
        public IActionResult Create(TblDealerLicenseTransaction model)
        {
            if (model == null)
                return BadRequest();

            model.CreatedOn = DateTime.Now;

            _context.TblDealerLicenseTransactions.Add(model);
            _context.SaveChanges();

            return Ok();
        }

        // ===============================
        // EDIT (Load data to form)
        // ===============================
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var transaction = _context.TblDealerLicenseTransactions
                .FirstOrDefault(x => x.Id == id);

            if (transaction == null)
                return NotFound();

            return Json(transaction);
        }

        // ===============================
        // UPDATE
        // ===============================
        [HttpPost]
        public IActionResult Update(TblDealerLicenseTransaction model)
        {
            var transaction = _context.TblDealerLicenseTransactions
                .FirstOrDefault(x => x.Id == model.Id);

            if (transaction == null)
                return NotFound();

            transaction.PurchaseQty = model.PurchaseQty;
            transaction.TotalPrice = model.TotalPrice;
            transaction.PaymentReceived = model.PaymentReceived;

            _context.SaveChanges();

            return Ok();
        }

        // ===============================
        // DELETE
        // ===============================
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var transaction = _context.TblDealerLicenseTransactions
                .FirstOrDefault(x => x.Id == id);

            if (transaction == null)
                return NotFound();

            _context.TblDealerLicenseTransactions.Remove(transaction);
            _context.SaveChanges();

            return Ok();
        }
    }
}