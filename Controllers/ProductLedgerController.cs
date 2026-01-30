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
        public IActionResult Index(int selectedbusinessId = 0)
        {
            int userId = Convert.ToInt32(User.FindFirst("UserId")?.Value);

            int businessId;

            if (User.IsInRole("HeadOfficer"))
            {
                // Load HO units (for dropdown / tabs)
                var units = _context.TblHOUnits
                    .Where(x => x.UserId == userId)
                    .Include(x => x.Business)
                    .Select(x => new
                    {
                        BusinessId = x.Business.Id,
                        x.Business.BusinessName,
                        x.Business.Address
                    })
                    .ToList();

                ViewBag.Units = units;

                businessId = selectedbusinessId > 0
                    ? selectedbusinessId
                    : units.FirstOrDefault()?.BusinessId ?? 0;
            }
            else
            {
                businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);
            }

            if (businessId == 0)
                return RedirectToAction("Login", "Account");

            var vm = new ProductLedgerFilterVM
            {
                BusinessId = businessId
            };

            LoadProducts(vm);

            return View(vm);
        }

        // AJAX: get ledger report data
        [HttpGet]
        public IActionResult GetReportData(int productId,DateTime fromDate,DateTime toDate, int? selectedbusinessId)
        {
            int businessId;

            if (User.IsInRole("HeadOfficer"))
            {
                businessId = selectedbusinessId ?? 0;
            }
            else
            {
                businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);
            }

            if (businessId == 0)
                return BadRequest("Business not found.");

            try
            {
                var parameters = new[]
                {
            new SqlParameter("@ProductId", productId),
            new SqlParameter("@BusinessId", businessId),
            new SqlParameter("@FromDate", fromDate),
            new SqlParameter("@ToDate", toDate)
        };

                var data = _context.Set<ProductLedgerVM>()
                    .FromSqlRaw(
                        "EXEC Pro_GetItemDailyStockSummary1 @ProductId, @BusinessId, @FromDate, @ToDate",
                        parameters)
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

        [HttpGet]
        public IActionResult GetProductsByBusiness(int businessId)
        {
            var products = _context.TblProducts
                .Where(x => x.BusinessId == businessId)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                }).ToList();

            return Json(products);
        }
    }
}
