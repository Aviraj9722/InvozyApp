using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace eOrderTouchApp.Controllers
{
   
    public class ProductLedgerController : Controller
    {
        private readonly eOrderTouchContext _context;

        public ProductLedgerController(eOrderTouchContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            int orgId = GetOrgIdFromClaims();
            if (orgId == 0)
            {
                return RedirectToAction("Login", "Account"); // or show error message
            }

            var vm = new ProductLedgerFilterVM
            {
                BusinessId = orgId
            };

            LoadProducts(vm);

            return View(vm);
        }

        // AJAX: get ledger report data
        [HttpGet]
        public IActionResult GetReportData(int productId, DateTime fromDate, DateTime toDate)
        {
            int orgId = GetOrgIdFromClaims();
            if (orgId == 0)
                return BadRequest("Organization ID not found. Please login again.");

            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@ProductId", productId),
                    new SqlParameter("@BusinessId", orgId),
                    new SqlParameter("@FromDate", fromDate),
                    new SqlParameter("@ToDate", toDate)
                };

                // Execute stored procedure and map to ProductLedgerVM
                var data = _context.Set<ProductLedgerVM>()
                                   .FromSqlRaw("EXEC Pro_GetItemDailyStockSummary1 @ProductId, @BusinessId, @FromDate, @ToDate", parameters)
                                   .ToList();

                return Json(data);
            }
            catch (Exception ex)
            {
                return BadRequest("SERVER ERROR: " + ex.Message);
            }
        }

        // Load products for dropdown
        private void LoadProducts(ProductLedgerFilterVM vm)
        {
            vm.Products = _context.TblProducts
                                  .Where(x => x.BusinessId == vm.BusinessId)
                                  .Select(x => new SelectListItem
                                  {
                                      Value = x.Id.ToString(),
                                      Text = x.Name
                                  })
                                  .ToList();
        }

        // Helper: get OrgId safely from user claims
        private int GetOrgIdFromClaims()
        {
            var claim = User.FindFirst("OrgId")?.Value;
            if (!string.IsNullOrEmpty(claim) && int.TryParse(claim, out int orgId))
                return orgId;
            return 0;
        }
    }
}
