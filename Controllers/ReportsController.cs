using eOrderTouchApp.Models;
using eOrderTouchApp.Models.ReportsModel;
using eOrderTouchApp.ViewModel;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace eOrderTouchApp.Controllers
{
    [AuthorizeToRoles("Owner")]
    public class ReportsController : Controller
    {
        private readonly eOrderTouchContext _context;

        public ReportsController(eOrderTouchContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            int userId = Convert.ToInt32(User.FindFirst("UserId")?.Value);

            if (_context.TblUsers.Find(userId).Role=="HeadOfficer")
            {
                var units =  _context.TblHOUnits
               .Where(x => x.UserId == userId)
               .Include(x => x.Business)
               .Select(x => new
               {
                   x.Business.Id,
                   x.Business.BusinessName,
                   x.Business.Address
               })
               .ToList();

                ViewBag.Units = units;
            }

            ViewBag.Reports = _context.TblReports.ToList();
            return View();
        }

        public IActionResult LoadReport(int id)
        {
            var report = _context.TblReports.FirstOrDefault(x => x.Id == id);

            ViewBag.ReportId = id;
            ViewBag.ReportName = report?.Name;

            return PartialView("_ReportContent");
        }

        [HttpPost]
        public async Task<IActionResult> GenerateReport(string reportName, DateTime? fromDate, DateTime? toDate, int selectedbusinessId=0)
        {
            try
            {

                int businessId = selectedbusinessId >0? selectedbusinessId :Convert.ToInt32(User.FindFirst("OrgId")?.Value);

                if (fromDate == null || toDate == null)
                    return BadRequest("Dates cannot be empty!");

                var data =
                await _context.ExecuteReport(
                                reportName,
                                businessId,
                                fromDate.Value,
                                toDate.Value);

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest("SERVER ERROR: " + ex.Message);
            }
        }

        public async Task<IActionResult> GstSaleReportPrint(int orderId)
        {
            try
            {
                if (orderId == null)
                {
                    return RedirectToAction("Report", "Order"); // Go to order list
                }
               
                int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value ?? "0");

                // 1️⃣ Get Order ONLY from this business
                var order = await _context.TblOrderMasters
                    .FirstOrDefaultAsync(x => x.Id == orderId
                                           && x.BuisnessId == businessId);

                if (order == null)
                    return NotFound(); // Prevent access to other business data

                // 2️⃣ Get Order Details
                var orderDetails = await _context.TblOrderDetails
                    .Where(x => x.Oid == orderId)
                    .ToListAsync();

                if (!orderDetails.Any())
                    return NotFound("No order items found.");

                // 3️⃣ Get Business Info
                var business = await _context.TblBusinesses
                    .FirstOrDefaultAsync(x => x.Id == businessId);

                // 4️⃣ Fetch Products in ONE query (Optimized)
                var productIds = orderDetails.Select(x => x.ProductId).Distinct().ToList();

                var products = await _context.TblProducts
                    .Where(p => productIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id, p => p.Name);

                // 5️⃣ Prepare Item List
                var items = orderDetails.Select(x => new GstItemVM
                {
                    ItemName = x.ProductId != null && products.ContainsKey(x.ProductId.Value)
                ? products[x.ProductId.Value]
                : "Unknown Item",

                    Quantity = x.Qty ?? 0,
                    Price = x.Price ?? 0,
                    GstPercent = x.Gstpercentage ?? 0, 
                    UOM = "",
                    TotalAmount = x.Total ?? 0
                }).ToList();

                // 6️⃣ GST Grouping
                var gstGrouping = orderDetails
                    .GroupBy(x => x.Gstpercentage)
                    .Select(g => new GstTaxGroupingVM
                    {
                        GstPercentage = g.Key ?? 0,
                        TaxableAmount = g.Sum(x => x.Total ?? 0),
                        CGST = g.Sum(x => x.CGST ?? 0),
                        SGST = g.Sum(x => x.SGST ?? 0),
                        TotalTax = g.Sum(x =>
                            (x.CGST ?? 0) +
                            (x.SGST ?? 0) +
                            (x.IGST ?? 0))
                    }).ToList();

                // 7️⃣ Prepare ViewModel
                var vm = new GstSaleReportsVM
                {
                    BusinessName = business?.BusinessName,

                    OrderNo = order.Id.ToString(),
                    OrderDate = order.DateOfOrder ?? DateTime.Now,
                    InvoiceNo = order.Id.ToString(),

                    CustomerName = order.CustomerName,
                    CustomerGST = "",
                    CustomerAddress = "",

                    Items = items,
                    GstGrouping = gstGrouping,

                    TotalTaxable = gstGrouping.Sum(x => x.TaxableAmount),
                    TotalCGST = gstGrouping.Sum(x => x.CGST),
                    TotalSGST = gstGrouping.Sum(x => x.SGST),
                    GrandTax = gstGrouping.Sum(x => x.TotalTax)
                };

                return View("GstSaleReportPrint", vm);
            }
            catch (Exception ex)
            {
               
                return StatusCode(500, "Something went wrong while generating GST Sale Report.");
            }
        }
    }
}
