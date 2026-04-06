using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eOrderTouchApp.Controllers
{
    
    public class FeedbackController : Controller
    {
        private readonly eOrderTouchContext _context;

        public FeedbackController(eOrderTouchContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult Create(int BID = 0)
        {
            ViewBag.BusinessId = BID;

            // 🔹 Fetch business name for this BID
            var businessName = _context.TblBusinesses
                .Where(b => b.Id == BID)
                .Select(b => b.BusinessName)
                .FirstOrDefault();

            ViewBag.BusinessName = businessName ?? "Our Business";

            return View();
        }

        [AuthorizeToRoles("Owner", "HeadOfficer")]
        public IActionResult List(int selectedbusinessId = 0)
        {
            int userId = Convert.ToInt32(User.FindFirst("UserId")?.Value);

            // 🔹 HO Units Dropdown
            if (User.IsInRole("HeadOfficer"))
            {
                var units = _context.TblHOUnits
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

            int businessId;
            if (User.IsInRole("HeadOfficer"))
            {
                businessId = selectedbusinessId;
            }
            else
            {
                businessId = Convert.ToInt32(User.FindFirst("OrgId")!.Value);
            }

            ViewBag.SelectedBusinessId = businessId;

            var feedbacks = _context.TblFeedbacks
                .Where(x => x.BuisnessId == businessId)
                .OrderByDescending(x => x.CreatedOn)
                .ToList();

            return View(feedbacks);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult SubmitFeedback(TblFeedback model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.CreatedOn = DateTime.Now;

            var orgClaim = User.FindFirst("OrgId");

            if (orgClaim != null)
            {
                // Owner submitting feedback
                model.BuisnessId = int.Parse(orgClaim.Value);
            }
            else
            {
                // Customer submitting feedback
                if (model.BuisnessId == 0)
                    return BadRequest("BusinessId missing");
            }

            _context.TblFeedbacks.Add(model);
            _context.SaveChanges();

            return Ok(new { message = "Feedback submitted successfully!" });
        }



    }
}
