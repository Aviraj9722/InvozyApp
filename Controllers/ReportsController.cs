using eOrderTouchApp.Models;
using eOrderTouchApp.Models.ReportsModel;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace eOrderTouchApp.Controllers
{
    public class ReportsController : Controller
    {
        private readonly eOrderTouchContext _context;

        public ReportsController(eOrderTouchContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
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
        public async Task<IActionResult> GenerateReport(int reportId, DateTime? fromDate, DateTime? toDate)
        {
            try
            {

                int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

                if (fromDate == null || toDate == null)
                    return BadRequest("Dates cannot be empty!");

                var data = await _context.ExecuteReport(
                                reportId,
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
        
    }
}
