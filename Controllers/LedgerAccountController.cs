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
            EnsureDefaultAccounts();
            return View();
        }

        // AUTO CREATE DEFAULT ACCOUNTS
        private void EnsureDefaultAccounts()
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            bool cashExists = _context.TblLedgerAccounts
                .Any(x => x.BusinessId == businessId && x.Name == "Cash In Hand");

            if (!cashExists)
            {
                _context.TblLedgerAccounts.Add(new TblLedgerAccount
                {
                    BusinessId = businessId,
                    Name = "Cash In Hand",
                    Type = "Cash",
                    Description = "Cash Account",
                    Status = "Active",
                    CreatedOn = DateTime.Now
                });
            }

            bool bankExists = _context.TblLedgerAccounts
                .Any(x => x.BusinessId == businessId && x.Name == "Cash In Bank");

            if (!bankExists)
            {
                _context.TblLedgerAccounts.Add(new TblLedgerAccount
                {
                    BusinessId = businessId,
                    Name = "Cash In Bank",
                    Type = "Bank",
                    Description = "Bank Account",
                    Status = "Active",
                    CreatedOn = DateTime.Now
                });
            }

            _context.SaveChanges();
        }

        // GET ALL
        public JsonResult GetAccounts()
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            var data = _context.TblLedgerAccounts
                .Where(a => a.BusinessId == businessId && a.Status != "Deleted")
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
                x.Id != model.Id);

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

                // 🔒 Prevent editing system accounts
                if (acc.Name == "Cash In Hand" || acc.Name == "Cash In Bank")
                {
                    acc.Description = model.Description;
                    acc.Status = model.Status;
                }
                else
                {
                    acc.Name = model.Name;
                    acc.Description = model.Description;
                    acc.Type = model.Type;
                    acc.Status = model.Status;
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

            acc.Status = "Inactive";

            _context.SaveChanges();

            return Json(new { success = true });
        }
    }
}