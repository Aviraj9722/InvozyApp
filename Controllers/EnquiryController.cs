using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using eOrderTouchApp.Services;

namespace eOrderTouchApp.Controllers
{
 
    public class EnquiryController : Controller
    {
        private readonly eOrderTouchContext _context;
        private readonly IEmailService _emailService;  

        public EnquiryController(eOrderTouchContext context,IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> Submit(TblEnquiry model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                model.Status = "Pending";

                _context.TblEnquiries.Add(model);
                _context.SaveChanges();
                string subject = "New Enquiry Received - Invozy";
                string body = $@"
                            <h3>New Enquiry Details</h3>
                            <p><strong>Name:</strong> {model.Name}</p>
                            <p><strong>Email:</strong> {model.EmailId}</p>
                            <p><strong>Mobile:</strong> {model.MobileNo}</p>
                            <p><strong>Business Type:</strong> {model.BusinessType}</p>
                            <p><strong>No of Tables:</strong> {model.NoOfTables}</p>
                            <br/>
                            <p>Status: Pending</p>
                        ";

                var emailList = new List<string>
                {
                    "avirajkarad@gmail.com",
                    "rvnitin5@gmail.com"
                };

                foreach (var email in emailList)
                {
                    await _emailService.SendEmailAsync(email, subject, body);
                }
                return Ok(new { message = "Enquiry submitted successfully!" });
            }
            catch (Exception er)
            {

                throw;
            }
          

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
