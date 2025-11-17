using Microsoft.AspNetCore.Mvc;
using eOrderTouchApp.Models;

namespace eOrderTouchApp.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly eOrderTouchContext _context;

        public FeedbackController(eOrderTouchContext context)
        {
            _context = context;
        }

        // PUBLIC PAGE TO SHOW FEEDBACK BUTTON & MODAL
        public IActionResult Index()
        {
            return View();
        }

        // SAVE FEEDBACK (POST)
        [HttpPost]
        public IActionResult SubmitFeedback(TblFeedback model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.CreatedOn = DateTime.Now;

            // If business is selected from modal (hidden field)
            model.BuisnessId = model.BuisnessId == 0 ? null : model.BuisnessId;

            _context.TblFeedbacks.Add(model);
            _context.SaveChanges();

            return Ok(new { message = "Feedback submitted successfully!" });
        }
    }
}
