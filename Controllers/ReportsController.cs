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
        
    }
}
