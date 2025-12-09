using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eOrderTouchApp.Controllers
{
    public class CustomerController : Controller
    {
        private readonly eOrderTouchContext _context;

        public CustomerController(eOrderTouchContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            var customers = await _context.TblCustomers
                .Where(w => w.BusinessId == businessId)
                .ToListAsync();

            return View(customers);
        }

        [HttpPost]
        public async Task<JsonResult> Create([FromBody] TblCustomer customer)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            if (string.IsNullOrWhiteSpace(customer.Name))
                return Json(new { success = false, message = "Customer name is required." });

            customer.BusinessId = businessId;
            _context.TblCustomers.Add(customer);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<JsonResult> Edit([FromBody] TblCustomer customer)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            var cust = await _context.TblCustomers
                .Where(w => w.Id == customer.Id && w.BusinessId == businessId)
                .FirstOrDefaultAsync();

            if (cust == null)
                return Json(new { success = false, message = "Customer not found." });

            cust.Name = customer.Name;
            cust.Address = customer.Address;
            cust.MobileNo = customer.MobileNo;
            cust.EmailId = customer.EmailId;
            cust.GSTN = customer.GSTN;
            cust.Location = customer.Location;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            var customer = await _context.TblCustomers
                .Where(w => w.Id == id && w.BusinessId == businessId)
                .FirstOrDefaultAsync();

            if (customer == null)
                return Json(new { success = false, message = "Customer not found." });

            _context.TblCustomers.Remove(customer);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
