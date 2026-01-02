using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eOrderTouchApp.Controllers
{
   
    public class FeedbackController : Controller
    {
        private readonly eOrderTouchContext _context;

        public FeedbackController(eOrderTouchContext context)
        {
            _context = context;
        }

        // CUSTOMER VIEW (NO GRID)
        [AllowAnonymous] // customers don't need login
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


        [AuthorizeToRoles("Owner")]
        public IActionResult List()
        {
            var orgClaim = User.FindFirst("OrgId");
            if (orgClaim == null)
                return Unauthorized();

            int businessId = int.Parse(orgClaim.Value);

            ViewBag.BusinessId = businessId;   // ✅ ADD THIS

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
