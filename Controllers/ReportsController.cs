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

        public async Task<IActionResult> PrintGstInvoice(int orderId, int selectedbusinessId = 0)
        {
            try
            {
                int businessId = selectedbusinessId > 0
                    ? selectedbusinessId
                    : Convert.ToInt32(User.FindFirst("OrgId")?.Value);

                var vm = new GstInvoiceReportsVM
                {
                    Items = new List<GstItemVM>(),
                    GstGrouping = new List<GstTaxGroupingVM>()
                };

                using (var conn = _context.Database.GetDbConnection())
                {
                    await conn.OpenAsync();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "sp_GetGstTaxInvoice";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter("@OrderId", orderId));
                        cmd.Parameters.Add(new SqlParameter("@BusinessId", businessId));

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            // 1️⃣ HEADER
                            if (await reader.ReadAsync())
                            {
                                vm.BusinessName = reader["BusinessName"]?.ToString();
                                vm.BusinessAddress = reader["BusinessAddress"]?.ToString();
                                vm.BusinessGSTIN = reader["BusinessGSTIN"]?.ToString();
                                vm.BusinessMobNo = reader["BusinessMobNo"]?.ToString();
                                vm.ReportData = reader["ReportData"]?.ToString();
                                vm.InvoiceNo = reader["InvoiceNo"]?.ToString();
                                vm.OrderDate = Convert.ToDateTime(reader["DateOfOrder"]);
                                vm.CustomerName = reader["CustomerName"]?.ToString();
                                vm.CustomerMobNo = reader["CustomerMobNo"]?.ToString();

                                // ✅ NEW
                                vm.TotalAmount = Convert.ToDecimal(reader["TotalAmount"]);
                                vm.Discount = Convert.ToDecimal(reader["Discount"]);
                                vm.NetAmount = Convert.ToDecimal(reader["TaxableAfterDiscount"]);
                            }

                            // 2️⃣ ITEMS
                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    vm.Items.Add(new GstItemVM
                                    {
                                        ItemName = reader["ItemName"]?.ToString(),
                                        Quantity = Convert.ToDecimal(reader["Quantity"]),
                                        UOM = reader["UOM"]?.ToString(),
                                        Price = Convert.ToDecimal(reader["Price"]),
                                        GstPercent = Convert.ToDecimal(reader["GstPercent"]),

                                        // ✅ USE DISCOUNTED TOTAL
                                        TotalAmount = Convert.ToDecimal(reader["DiscountedTotal"])
                                    });
                                }
                            }

                            // 3️⃣ GST GROUPING
                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var tax = new GstTaxGroupingVM
                                    {
                                        GstPercentage = Convert.ToDecimal(reader["GstPercentage"]),
                                        TaxableAmount = Convert.ToDecimal(reader["TaxableAmount"]),
                                        CGST = Convert.ToDecimal(reader["CGST"]),
                                        SGST = Convert.ToDecimal(reader["SGST"]),
                                        TotalTax = Convert.ToDecimal(reader["TotalTax"])
                                    };

                                    vm.GstGrouping.Add(tax);

                                    vm.TotalTaxable += tax.TaxableAmount;
                                    vm.TotalCGST += tax.CGST;
                                    vm.TotalSGST += tax.SGST;
                                    vm.GrandTax += tax.TotalTax;
                                }
                            }
                            // FINAL TOTALS
                            if (await reader.NextResultAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    vm.TotalTaxable = Convert.ToDecimal(reader["TotalTaxable"]);
                                    vm.TotalCGST = Convert.ToDecimal(reader["TotalCGST"]);
                                    vm.TotalSGST = Convert.ToDecimal(reader["TotalSGST"]);
                                    vm.TotalGST = Convert.ToDecimal(reader["TotalGST"]);
                                    vm.GrandTotal = Convert.ToDecimal(reader["GrandTotal"]);
                                }
                            }
                        }
                    }
                }

                return View("GstInvoicePrint", vm);
            }
            catch (Exception ex)
            {
                return BadRequest("Server Error : " + ex.Message);
            }
        }
    }
}
