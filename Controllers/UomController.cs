using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eOrderTouchApp.Models;

namespace eOrderTouchApp.Controllers
{
    public class UomController : Controller
    {
        private readonly eOrderTouchContext _context;

        public UomController(eOrderTouchContext context)
        {
            _context = context;
        }

        // -----------------------
        // INDEX (returns full view)
        // -----------------------
        public async Task<IActionResult> Index()
        {
            // We will let the view fetch data by AJAX (GetBusinesses / GetPaged).
            // But return the view so route /Uom/Index works.
            return View();
        }

        // -----------------------------------------
        // GET: /Uom/GetBusinesses
        // returns list of businesses for dropdown
        // -----------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetBusinesses()
        {
            var list = await _context.TblBusinesses
                .OrderBy(b => b.BusinessName)
                .Select(b => new { id = b.Id, name = b.BusinessName })
                .ToListAsync();

            return Json(list);
        }

        // -----------------------------------------
        // GET: /Uom/GetPaged?page=&pageSize=&businessId=&search=
        // returns paged result for grid
        // -----------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetPaged(int page = 1, int pageSize = 10,  string search = "")
        {
           int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            if (businessId <= 0)
            {
                return Json(new { page = 1, totalPages = 1, items = Array.Empty<object>() });
            }

            var query = _context.TblUoms
                .Where(u => u.BusinessId == businessId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(u => (u.UnitName ?? "").ToLower().Contains(search));
            }

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var items = await query
                .OrderBy(u => u.UnitName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new
                {
                    id = u.Id,
                    unitName = u.UnitName,
                    businessId = u.BusinessId
                })
                .ToListAsync();

            return Json(new { page, totalPages, items });
        }

        // -----------------------------------------
        // GET: /Uom/Get/{id}
        // returns single record for edit
        // -----------------------------------------
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

            var u = await _context.TblUoms.Where(w=>w.BusinessId== businessId && w.Id==id).FirstOrDefaultAsync();
            if (u == null) return NotFound();
            return Json(new { id = u.Id, unitName = u.UnitName, businessId = u.BusinessId });
        }

        // -----------------------------------------
        // POST: /Uom/Create
        // expects multipart/form-data (FormData)
        // -----------------------------------------
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] TblUom model)
        {
            // remove server-side validation if you want to ignore data annotations:
            // ModelState.Clear();
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);
            model.BusinessId = businessId;
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToArray() })
                    .ToArray();

                return BadRequest(errors);
            }

            var entity = new TblUom
            {
                BusinessId = model.BusinessId,
                UnitName = model.UnitName
            };

            _context.TblUoms.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, id = entity.Id });
        }

        // -----------------------------------------
        // POST: /Uom/Update
        // expects multipart/form-data
        // -----------------------------------------
        [HttpPost]
        public async Task<IActionResult> Update([FromForm] TblUom model)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);
            model.BusinessId = businessId;
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToArray() })
                    .ToArray();

                return BadRequest(errors);
            }

            var existing = await _context.TblUoms.FindAsync(model.Id);
            if (existing == null) return NotFound();

            existing.UnitName = model.UnitName;
            existing.BusinessId = model.BusinessId;

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // -----------------------------------------
        // POST: /Uom/Delete/{id}
        // -----------------------------------------
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);
            
            var existing = await _context.TblUoms.Where(w=>w.Id==id && w.BusinessId == businessId).FirstOrDefaultAsync();
            if (existing == null) return NotFound();

            _context.TblUoms.Remove(existing);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
    }
}
