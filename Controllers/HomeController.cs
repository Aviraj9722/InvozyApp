using eOrderTouchApp.Models;
using eOrderTouchApp.Models.ReportsModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Globalization;

namespace eOrderTouchApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly eOrderTouchContext _context;

    public HomeController(ILogger<HomeController> logger, eOrderTouchContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        ViewBag.BusinessTypes = new List<string>
            {
                "Restaurant",
                "Hotel",
                "Cafe",
                "Bar",
                "Fast Food",
                "Food Truck",
                "Canteen / Mess",
                "Bakery",
                "Dhaba",
                "Other"
            };

        return View(); 
    }

    public IActionResult Dashboard1()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult Graph()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> SaleProfitDiscountChart(DateTime fromDate, DateTime toDate)
    {
        try
        {
            int OrgId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            // SALES + PROFIT
            var profits = await _context.DateWiseSaleProfitReports
                .FromSqlRaw(
                    "EXEC Pro_GenerateReport @ReportName,@BusinessId,@FromDate,@ToDate",
                    new SqlParameter("@ReportName", "Date-Wise Sale Profit Reports"),
                    new SqlParameter("@BusinessId", OrgId),
                    new SqlParameter("@FromDate", fromDate),
                    new SqlParameter("@ToDate", toDate)
                )
                .AsNoTracking()
                .ToListAsync();

            // DISCOUNT
            var discounts = await _context.DateWiseSaleDiscountModels
                .FromSqlRaw(
                    "EXEC Pro_GenerateReport @ReportName,@BusinessId,@FromDate,@ToDate",
                    new SqlParameter("@ReportName", "Date-Wise Discount Reports"),
                    new SqlParameter("@BusinessId", OrgId),
                    new SqlParameter("@FromDate", fromDate),
                    new SqlParameter("@ToDate", toDate)
                )
                .AsNoTracking()
                .ToListAsync();

            var result = profits
                .GroupJoin(discounts,
                    p => p.DateOfOrder.Date,
                    d => d.DateOfOrder.Date,
                    (p, d) => new DateWiseSaleProfitDto
                    {
                        Date = p.DateOfOrder.ToString("dd-MM-yyyy"),
                        Sale = p.TotalSale,
                        Profit = p.Profit,
                        Discount = d.FirstOrDefault()?.TotalDiscount ?? 0
                    })
                .OrderBy(x => DateTime.ParseExact(x.Date!, "dd-MM-yyyy", CultureInfo.InvariantCulture))
                .ToList();

            return Json(result);
        }
        catch (Exception ex)
        {
            Response.StatusCode = 500;
            return Json(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> ItemWiseSaleBar(DateTime fromDate, DateTime toDate)
    {
        int OrgId = int.Parse(User.FindFirst("OrgId")!.Value);

        var data = await _context.ItemReportResults
            .FromSqlRaw(
                "EXEC Pro_GenerateReport @ReportName,@BusinessId,@FromDate,@ToDate",
                new SqlParameter("@ReportName", "Item Sale Reports"),
                new SqlParameter("@BusinessId", OrgId),
                new SqlParameter("@FromDate", fromDate),
                new SqlParameter("@ToDate", toDate)
            )
            .AsNoTracking()
            .ToListAsync();

        var result = data
            .OrderByDescending(x => x.TotalSale ?? 0)
            .Take(20) // ⭐ TOP 20 items only
            .Select(x => new
            {
                label = x.ItemName,
                value = x.TotalSale ?? 0m
            });

        return Json(result);
    }

    //public async Task<IActionResult> Dashboard()
    //{
    //    var claimValue = User.FindFirstValue("OrgId");

    //    if (!int.TryParse(claimValue, out int orgId))
    //    {
    //        return RedirectToAction("Login", "Account");
    //    }

    //    //int orgId = int.Parse(User.FindFirst("OrgId")!.Value);
    //    DateTime today = DateTime.Today;

    //    // 1️⃣ Get today's sale (Date-Wise Sale Reports)
    //    var saleList = await _context.DateWiseSaleReportModels
    //        .FromSqlRaw(
    //            "EXEC Pro_GenerateReport @ReportName, @BusinessId, @FromDate, @ToDate",
    //            new SqlParameter("@ReportName", "Date-Wise Sale Reports"),
    //            new SqlParameter("@BusinessId", orgId),
    //            new SqlParameter("@FromDate", today),
    //            new SqlParameter("@ToDate", today)
    //        )
    //        .AsNoTracking()
    //        .ToListAsync();

    //    // 2️⃣ Get today's profit (Date-Wise Sale Profit Reports)
    //    var profitList = await _context.DateWiseSaleProfitReports
    //        .FromSqlRaw(
    //            "EXEC Pro_GenerateReport @ReportName, @BusinessId, @FromDate, @ToDate",
    //            new SqlParameter("@ReportName", "Date-Wise Sale Profit Reports"),
    //            new SqlParameter("@BusinessId", orgId),
    //            new SqlParameter("@FromDate", today),
    //            new SqlParameter("@ToDate", today)
    //        )
    //        .AsNoTracking()
    //        .ToListAsync();

    //    // 3️⃣ Merge into a single KPI object
    //    var dashboard = new TodayDashboardVM
    //    {
    //        TotalOrders = saleList.Sum(x => (int?)x.TotalOrders) ?? 0,
    //        TotalSale = saleList.Sum(x => (decimal?)x.TotalSale) ?? 0,
    //        Cash = saleList.Sum(x => (decimal?)x.Cash) ?? 0,
    //        Online = saleList.Sum(x => (decimal?)x.Online) ?? 0,
    //        Credit = saleList.Sum(x => (decimal?)x.Credit) ?? 0,
    //        Profit = profitList.Sum(x => (decimal?)x.Profit) ?? 0
    //    };

    //    return View(dashboard);
    //}
    public async Task<IActionResult> Dashboard()
    {
        var claimValue = User.FindFirstValue("OrgId");

        if (!int.TryParse(claimValue, out int orgId))
        {
            return RedirectToAction("Login", "Account");
        }

        DateTime today = DateTime.Today;

        // ============================
        // 🔔 LICENSE WARNING LOGIC
        // ============================
        var licenseEndClaim = User.FindFirst("LicenseEnd");

        if (licenseEndClaim != null)
        {
            DateTime licenseEndDate = DateTime.Parse(licenseEndClaim.Value);
            int daysLeft = (licenseEndDate.Date - today).Days;

            if (daysLeft < 0)
            {
                // ❌ License expired (this month or earlier)
                ViewBag.LicenseMessage =
                    $"❌ Your license expired on {licenseEndDate:dd MMM yyyy}. Please renew to continue using the system.";
                ViewBag.LicenseType = "expired";
            }
            else if (daysLeft <= 30)
            {
                // ⚠ Expiring within 1 month
                ViewBag.LicenseMessage =
                    $"⚠ Your license will expire in {daysLeft} day(s) (on {licenseEndDate:dd MMM yyyy}). Please renew soon.";
                ViewBag.LicenseType = "warning";
            }
        }

        // ============================
        // 📊 EXISTING DASHBOARD DATA
        // ============================

        var saleList = await _context.DateWiseSaleReportModels
            .FromSqlRaw(
                "EXEC Pro_GenerateReport @ReportName, @BusinessId, @FromDate, @ToDate",
                new SqlParameter("@ReportName", "Date-Wise Sale Reports"),
                new SqlParameter("@BusinessId", orgId),
                new SqlParameter("@FromDate", today),
                new SqlParameter("@ToDate", today)
            )
            .AsNoTracking()
            .ToListAsync();

        var profitList = await _context.DateWiseSaleProfitReports
            .FromSqlRaw(
                "EXEC Pro_GenerateReport @ReportName, @BusinessId, @FromDate, @ToDate",
                new SqlParameter("@ReportName", "Date-Wise Sale Profit Reports"),
                new SqlParameter("@BusinessId", orgId),
                new SqlParameter("@FromDate", today),
                new SqlParameter("@ToDate", today)
            )
            .AsNoTracking()
            .ToListAsync();

        var dashboard = new TodayDashboardVM
        {
            TotalOrders = saleList.Sum(x => (int?)x.TotalOrders) ?? 0,
            TotalSale = saleList.Sum(x => (decimal?)x.TotalSale) ?? 0,
            Cash = saleList.Sum(x => (decimal?)x.Cash) ?? 0,
            Online = saleList.Sum(x => (decimal?)x.Online) ?? 0,
            Credit = saleList.Sum(x => (decimal?)x.Credit) ?? 0,
            Profit = profitList.Sum(x => (decimal?)x.Profit) ?? 0
        };

        return View(dashboard);
    }

}
