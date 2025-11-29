using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eOrderTouchApp.Controllers
{
 
    public class EnquiryController : Controller
    {
        private readonly eOrderTouchContext _context;

        public EnquiryController(eOrderTouchContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Submit(TblEnquiry model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.Status = "Pending";   

            _context.TblEnquiries.Add(model);
            _context.SaveChanges();

            return Ok(new { message = "Enquiry submitted successfully!" });
        }

        
        public IActionResult AdminList()
        {
            var data = _context.TblEnquiries.OrderByDescending(x => x.Id).ToList();
            return View(data); 
        }

        
        public IActionResult GetEnquiry(int id)
        {
            var item = _context.TblEnquiries.Find(id);
            if (item == null) return NotFound();
            return Json(item);
        }

        
        [HttpPost]
        public IActionResult UpdateFollowUp(TblEnquiry model)
        {
            var item = _context.TblEnquiries.Find(model.Id);
            if (item == null) return Json(new { success = false, message = "Not found" });

            item.Status = model.Status;
            item.FollowUpOne = model.FollowUpOne;
            item.FollowUpTwo = model.FollowUpTwo;
            item.FollowUpThree = model.FollowUpThree;
            item.FollowUpFour = model.FollowUpFour;

            _context.SaveChanges();
            return Json(new { success = true, message = "Updated" });
        }


    }
}
