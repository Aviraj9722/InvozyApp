using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eOrderTouchApp.Controllers
{
    [AuthorizeToRoles("HeadOfficer")]
    public class HOActivityController : Controller
    {
        private readonly eOrderTouchContext _context;

        public HOActivityController(eOrderTouchContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            return View();
        }
    }
}
