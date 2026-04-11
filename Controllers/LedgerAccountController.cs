using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace eOrderTouchApp.Controllers
{
    public class LedgerAccountController : Controller
    {
        private readonly eOrderTouchContext _context;

        public LedgerAccountController(eOrderTouchContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // ✅ GET ALL
        public JsonResult GetAccounts()
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            var data = _context.TblLedgerAccounts
                .Where(a => a.BusinessId == businessId && a.Type == "Expense")
                .Select(a => new
                {
                    a.Id,
                    a.Name,
                    a.Description,
                    a.Status
                }).ToList();

            return Json(data);
        }

        // ✅ SAVE (CREATE + UPDATE)
        [HttpPost]
        public JsonResult Save(TblLedgerAccount model)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            if (model.Id == 0)
            {
                model.BusinessId = businessId;
                model.Type = "Expense";
                model.CreatedOn = DateTime.Now;

                _context.TblLedgerAccounts.Add(model);
            }
            else
            {
                var acc = _context.TblLedgerAccounts.Find(model.Id);

                acc.Name = model.Name;
                acc.Description = model.Description;
                acc.Status = model.Status;
            }

            _context.SaveChanges();
            return Json(new { success = true });
        }

        // ✅ DELETE (Soft Delete)
        public JsonResult Delete(int id)
        {
            var acc = _context.TblLedgerAccounts.Find(id);

            if (acc != null)
            {
                acc.Status = "Inactive";
                _context.SaveChanges();
            }

            return Json(new { success = true });
        }
    }
}
