using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eOrderTouchApp.Controllers
{
    [AuthorizeToRoles("Owner")]
    public class TransactionController : Controller
    {
        private readonly eOrderTouchContext _context;

        public TransactionController(eOrderTouchContext context)
        {
            _context = context;
        }

        // ✅ CREATE FORM
        public IActionResult Index()
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            int cashAccountId = _context.TblLedgerAccounts
                .Where(a => a.BusinessId == businessId && a.Type == "Cash")
                .Select(a => a.Id)
                .FirstOrDefault();

            int bankAccountId = _context.TblLedgerAccounts
                .Where(a => a.BusinessId == businessId && a.Type == "Bank")
                .Select(a => a.Id)
                .FirstOrDefault();

            // 🔥 CALL FUNCTION (like fn_getStock)
            var cashBalance = _context.Database
                .SqlQueryRaw<LedgerBalanceVM>(
                    $"SELECT dbo.fn_GetLedgerBalance({businessId}, {cashAccountId}) AS Balance")
                .FirstOrDefault()?.Balance ?? 0;

            var bankBalance = _context.Database
                .SqlQueryRaw<LedgerBalanceVM>(
                    $"SELECT dbo.fn_GetLedgerBalance({businessId}, {bankAccountId}) AS Balance")
                .FirstOrDefault()?.Balance ?? 0;

            ViewBag.CashInHand = cashBalance;
            ViewBag.BankBalance = bankBalance;

            return View();
        }

        // ✅ GET ALL TRANSACTIONS
        public JsonResult GetTransactions()
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            var data = (from t in _context.TblTransactions
                        join a in _context.TblLedgerAccounts on t.AccountId equals a.Id
                        where t.BusinessId == businessId && a.Name !="Cash in Hand" && a.Name != "Cash in Bank"
                        orderby t.TransactionDate descending
                        select new
                        {
                            AccountId = t.AccountId,
                            AccountName = a.Name,
                            t.Amount,
                            t.PaymentMode,
                            t.Narration,
                            t.TypeOfTransaction,
                            t.TransactionDate}).ToList();

            return Json(data);
        }

        // ✅ GET ACCOUNTS (Expense only)
        public JsonResult GetAccounts()
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            var data = _context.TblLedgerAccounts
                .Where(a => a.BusinessId == businessId && a.Type == "Expense")
                .Select(a => new { a.Id, a.Name })
                .ToList();

            return Json(data);
        }

        // ✅ SAVE TRANSACTION
        [HttpPost]
        public JsonResult Save(TblTransaction model)
        {
            try
            {
                int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

                model.BusinessId = businessId;
                model.CreatedOn = DateTime.Now;

                int cashAccountId = _context.TblLedgerAccounts
                    .Where(a => a.BusinessId == businessId && a.Type == "Cash")
                    .Select(a => a.Id).FirstOrDefault();

                int bankAccountId = _context.TblLedgerAccounts
                    .Where(a => a.BusinessId == businessId && a.Type == "Bank")
                    .Select(a => a.Id).FirstOrDefault();

                if (cashAccountId == 0 || bankAccountId == 0)
                {
                    return Json(new { success = false, message = "Cash/Bank missing" });
                }

                // 🔥 BALANCE CHECK
                var cashBalance = _context.Database.SqlQueryRaw<LedgerBalanceVM>($"SELECT dbo.fn_GetLedgerBalance({businessId}, {cashAccountId}) AS Balance").FirstOrDefault()?.Balance ?? 0;

                var bankBalance = _context.Database.SqlQueryRaw<LedgerBalanceVM>($"SELECT dbo.fn_GetLedgerBalance({businessId}, {bankAccountId}) AS Balance").FirstOrDefault()?.Balance ?? 0;

                if (model.TypeOfTransaction == 'C') // Payment
                {
                    if (model.PaymentMode == "Cash" && model.Amount > cashBalance)
                        return Json(new { success = false, message = "Not enough Cash Balance" });

                    if ((model.PaymentMode == "Bank" || model.PaymentMode == "Online") && model.Amount > bankBalance)
                        return Json(new { success = false, message = "Not enough Bank Balance" });
                }
                if (model.AccountId == 0)
                {
                    return Json(new { success = false, message = "Please select account" });
                }
                if (model.Amount <= 0)
                {
                    return Json(new { success = false, message = "Amount must be greater than 0" });
                }
                // ✅ 1. PERSON ENTRY
                _context.TblTransactions.Add(model);

                // ✅ 2. CASH/BANK ENTRY (NO REVERSE)
                int targetAccount = model.PaymentMode == "Cash" ? cashAccountId : bankAccountId;

                _context.TblTransactions.Add(new TblTransaction
                {
                    BusinessId = businessId,
                    AccountId = targetAccount,
                    Amount = model.Amount,
                    PaymentMode = model.PaymentMode,
                    Narration = "Auto Entry",
                    TypeOfTransaction = model.TypeOfTransaction == 'D' ? 'C' : 'D', // ✅ SAME TYPE
                    TransactionDate = model.TransactionDate,
                    CreatedOn = DateTime.Now
                });

                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
            
        }


    }

    public class LedgerBalanceVM
    {
        public decimal Balance { get; set; }
    }
}
