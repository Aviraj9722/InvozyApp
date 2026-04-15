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

        // GET ALL
        public JsonResult GetAccounts()
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            var data = _context.TblLedgerAccounts
                .Where(a => a.BusinessId == businessId)
                .OrderBy(a => a.Name)
                .Select(a => new
                {
                    a.Id,
                    a.Name,
                    a.Description,
                    a.Type,
                    a.Status
                }).ToList();

            return Json(data);
        }

        // SAVE
        [HttpPost]
        public JsonResult Save(TblLedgerAccount model)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            bool exists = _context.TblLedgerAccounts.Any(x =>
                x.BusinessId == businessId &&
                x.Name == model.Name &&
                x.Id != model.Id &&
                x.Status != "IsDeleted");

            if (exists)
            {
                return Json(new
                {
                    success = false,
                    message = "GL Account already exists"
                });
            }

            if (model.Id == 0)
            {
                model.BusinessId = businessId;
                model.CreatedOn = DateTime.Now;
                model.Status = "Active";

                // 🔒 Only allow Income / Expense from UI
                if (model.Type != "Income" && model.Type != "Expense")
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid account type"
                    });
                }

                _context.TblLedgerAccounts.Add(model);
            }
            else
            {
                var acc = _context.TblLedgerAccounts.Find(model.Id);

                if (acc == null)
                    return Json(new { success = false });

                // ✅ restore if deleted
                acc.Status = "Active";

                // 🔒 Prevent editing system accounts
                if (acc.Name == "Cash In Hand" || acc.Name == "Cash In Bank")
                {
                    acc.Description = model.Description;
                }
                else
                {
                    acc.Name = model.Name;
                    acc.Description = model.Description;
                    acc.Type = model.Type;
                }
            }

            _context.SaveChanges();

            return Json(new { success = true });
        }


        // DELETE (SOFT DELETE)
        public JsonResult Delete(int id)
        {
            var acc = _context.TblLedgerAccounts.Find(id);

            if (acc == null)
                return Json(new { success = false });

            // Prevent system delete
            if (acc.Name == "Cash In Hand" || acc.Name == "Cash In Bank")
            {
                return Json(new
                {
                    success = false,
                    message = "System GL account cannot delete"
                });
            }

            // ✅ Soft delete
            acc.Status = "IsDeleted";

            _context.SaveChanges();

            return Json(new { success = true });
        }
    }
}