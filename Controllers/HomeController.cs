using eOrderTouchApp.Models;
using eOrderTouchApp.Models.ReportsModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Cryptography;

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

    public IActionResult Dashboard()
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
    public async Task<IActionResult> SaleProfitChart(DateTime fromDate, DateTime toDate)
    {
        int OrgId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

        // SALES
        var sales = await _context.DateWiseSaleReportModels
            .FromSqlRaw(
                "EXEC Pro_GenerateReport @ReportName,@BusinessId,@FromDate,@ToDate",
                new SqlParameter("@ReportName", "Date-Wise Sale Reports"),
                new SqlParameter("@BusinessId", OrgId),
                new SqlParameter("@FromDate", fromDate),
                new SqlParameter("@ToDate", toDate)
            )
            .AsNoTracking()
            .ToListAsync();

        // PROFIT
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

        // MERGE BY DATE
        var result = sales
            .GroupJoin(
                profits,
                s => s.DateOfOrder?.Date,
                p => p.DateOfOrder.Date,
                (s, p) => new DateWiseSaleProfitDto
                {
                    Date = s.DateOfOrder?.ToString("dd-MM-yyyy"),
                    Sale = s.TotalSale ?? 0,
                    Profit = p.FirstOrDefault()?.Profit ?? 0
                })
            .OrderBy(x => DateTime.ParseExact(x.Date, "dd-MM-yyyy", null))
            .ToList();

        return Json(result);
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


}
