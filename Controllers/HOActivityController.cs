using eOrderTouchApp.Models;
using eOrderTouchApp.Models.ReportsModel;
using eOrderTouchApp.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace eOrderTouchApp.Controllers
{
    [AuthorizeToRoles("HeadOfficer")]
    public class HOActivityController : Controller
    {
        private readonly eOrderTouchContext _context;

        public HOActivityController(eOrderTouchContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            int hoUserId = Convert.ToInt32(User.FindFirst("UserId").Value);

            var branches = await _context.HOBranchDashboardVMs.FromSqlRaw("EXEC sp_HO_BranchSaleProfit @HOUserId", new SqlParameter("@HOUserId", hoUserId)).AsNoTracking().ToListAsync();


            var model = new HODashboardVM
            {
                TotalSale = branches.Sum(x => x.TotalSale),
                TotalProfit = branches.Sum(x => x.Profit),
                Branches = branches
            };

            return View(model);
        }

        public IActionResult HOGraph()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetHOBusinessUnits()
        {
            int hoUserId = Convert.ToInt32(User.FindFirst("UserId")!.Value);

            var units = await _context.TblHOUnits
                .Where(x => x.UserId == hoUserId)
                .Join(_context.TblBusinesses,
                      ho => ho.BusinessId,
                      b => b.Id,
                      (ho, b) => new
                      {
                          id = b.Id,
                          name = b.BusinessName
                      })
                .Distinct()
                .ToListAsync();

            return Json(units);
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> BranchSaleProfitChart(DateTime fromDate, DateTime toDate, int? businessId)
        {
            int hoUserId = Convert.ToInt32(User.FindFirst("UserId")!.Value);

            // If business selected → ONLY that
            List<int> branchIds;

            if (businessId.HasValue)
            {
                branchIds = new List<int> { businessId.Value };
            }
            else
            {
                branchIds = await _context.TblHOUnits
                    .Where(x => x.UserId == hoUserId)
                    .Select(x => x.BusinessId!.Value)
                    .ToListAsync();
            }

            var finalResult = new List<HODateWiseSaleProfitDto>();

            foreach (var branchId in branchIds)
            {
                // SALES
                var sales = await _context.DateWiseSaleReportModels
                    .FromSqlRaw(
                        "EXEC Pro_GenerateReport @ReportName,@BusinessId,@FromDate,@ToDate",
                        new SqlParameter("@ReportName", "Date-Wise Sale Reports"),
                        new SqlParameter("@BusinessId", branchId),
                        new SqlParameter("@FromDate", fromDate),
                        new SqlParameter("@ToDate", toDate)
                    )
                    .AsNoTracking()
                    .ToListAsync();

                // PROFIT (DTO BASED - SAFE)
                List<HOProfitReportDto> profits;

                try
                {
                    profits = await _context
                        .Set<HOProfitReportDto>()
                        .FromSqlRaw(
                            "EXEC Pro_GenerateReport @ReportName,@BusinessId,@FromDate,@ToDate",
                            new SqlParameter("@ReportName", "Date-Wise Sale Profit Reports"),
                            new SqlParameter("@BusinessId", branchId),
                            new SqlParameter("@FromDate", fromDate),
                            new SqlParameter("@ToDate", toDate)
                        )
                        .AsNoTracking()
                        .ToListAsync();


                }
                catch (Exception ex)
                {
                    return BadRequest("PROFIT ERROR: " + ex.Message);
                }


                var profitLookup = profits
                .Where(x => x.DateOfOrder != null)
                .GroupBy(x => x.DateOfOrder!.Value.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x =>
                        (x.TotalSale ?? 0) - (x.TotalCost ?? 0)
                    )
                );


                foreach (var s in sales)
                {
                    if (s.DateOfOrder == null)
                        continue;

                    var date = s.DateOfOrder.Value.Date;

                    finalResult.Add(new HODateWiseSaleProfitDto
                    {
                        Date = date,
                        Sale = s.TotalSale ?? 0,
                        Profit = profitLookup.ContainsKey(date)
                                    ? profitLookup[date]
                                    : 0
                    });
                }

            }

            var grouped = finalResult
                .GroupBy(x => x.Date)
                .Select(g => new
                {
                    date = g.Key.ToString("dd-MM-yyyy"),
                    sale = g.Sum(x => x.Sale),
                    profit = g.Sum(x => x.Profit)
                })
                .OrderBy(x => DateTime.ParseExact(x.date, "dd-MM-yyyy", null))
                .ToList();

            return Json(grouped);
        }

        [HttpGet]
        public IActionResult HOItemWiseSaleBar(DateTime fromDate, DateTime toDate, int? businessUnitId)
        {
            var query =
                from od in _context.TblOrderDetails
                join om in _context.TblOrderMasters
                    on od.Oid equals om.Id
                join p in _context.TblProducts
                    on od.ProductId equals p.Id
                where om.DateOfOrder >= fromDate
                      && om.DateOfOrder <= toDate
                select new
                {
                    om.BuisnessId,
                    p.Name,
                    od.Qty
                };

            // 🔹 Business-wise filter
            if (businessUnitId.HasValue && businessUnitId.Value > 0)
            {
                query = query.Where(x => x.BuisnessId == businessUnitId.Value);
            }

            var data = query
                .GroupBy(x => x.Name)
                .Select(g => new
                {
                    label = g.Key,
                    value = g.Sum(x => x.Qty)
                })
                .OrderByDescending(x => x.value)
                .Take(15)
                .ToList();

            return Json(data);
        }




    }

    public class HODateWiseSaleProfitDto
    {
        public DateTime Date { get; set; }
        public decimal Sale { get; set; }
        public decimal Profit { get; set; }
    }

    public class HOItemSaleDto
    {
        public string ItemName { get; set; }
        public decimal TotalSale { get; set; }
    }


}
