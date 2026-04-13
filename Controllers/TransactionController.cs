using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace eOrderTouchApp.Controllers
{
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
            return View();
        }

        // ✅ GET ALL TRANSACTIONS
        public JsonResult GetTransactions()
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            var data = (from t in _context.TblTransactions
                        join a in _context.TblLedgerAccounts on t.AccountId equals a.Id
                        where t.BusinessId == businessId
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
                .Where(a => a.BusinessId == businessId && a.Type == "Expense" && a.Status == "Active")
                .Select(a => new { a.Id, a.Name })
                .ToList();

            return Json(data);
        }

        // ✅ SAVE TRANSACTION
        [HttpPost]
        public JsonResult Save(TblTransaction model)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            model.BusinessId = businessId;
            model.CreatedOn = DateTime.Now;

            _context.TblTransactions.Add(model);

            // 🔥 AUTO CASH/BANK IMPACT
            int cashAccountId = _context.TblLedgerAccounts
                .Where(a => a.BusinessId == businessId && a.Type == "Cash")
                .Select(a => a.Id).FirstOrDefault();

            int bankAccountId = _context.TblLedgerAccounts
                .Where(a => a.BusinessId == businessId && a.Type == "Bank")
                .Select(a => a.Id).FirstOrDefault();

            int targetAccount = model.PaymentMode == "Cash" ? cashAccountId : bankAccountId;

            _context.TblTransactions.Add(new TblTransaction
            {
                BusinessId = businessId,
                AccountId = targetAccount,
                Amount = model.Amount,
                PaymentMode = model.PaymentMode,
                Narration = "Auto Entry",
                TypeOfTransaction = 'D', 
                TransactionDate = model.TransactionDate,
                CreatedOn = DateTime.Now
            });

            _context.SaveChanges();

            return Json(new { success = true });
        }

        

    }
}
