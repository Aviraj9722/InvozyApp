using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Authorization;
[Authorize]
public class OrderController : Controller
{
    private readonly eOrderTouchContext _context;

    public OrderController(eOrderTouchContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Report()
    {
        int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

        var Orders = await _context.TblOrderMasters.Where(w=>w.BuisnessId == businessId).Include(I=>I.TblOrderDetails).OrderByDescending(o=>o.Id).ToListAsync();

        ViewBag.Materials = _context.TblProducts.ToList();

        return View(Orders);
    }
    [HttpPost]
    public async Task<IActionResult> Report2(DateTime fromDT, DateTime toDT)
    {
        int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

        var Orders = await _context.TblOrderMasters.Where(w => w.BuisnessId == businessId).Include(I => I.TblOrderDetails).Where(w=>w.DateOfOrder>= fromDT && w.DateOfOrder<=toDT) .OrderByDescending(o => o.Id).ToListAsync();

        ViewBag.TotalCash = Orders.Where(w => w.PaymentMode == "Cash").Sum(w => w.GrandTotal);
        ViewBag.Online = Orders.Where(w => w.PaymentMode == "Online").Sum(s => s.GrandTotal);
        ViewBag.Free = Orders.Where(w => w.PaymentMode == "Free").Sum(s => s.GrandTotal);
        ViewBag.Materials = _context.TblProducts.ToList();

        return View("Report",Orders);
    }

    public async Task<IActionResult> Create()
    {
        int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);
        int UserId = Convert.ToInt32(User.FindFirst("UserId")?.Value);


        var categories = await _context.TblCategories.Where(w => w.BusinessId == businessId).ToListAsync();

        var products = await _context.TblProducts.Where(w => w.BusinessId == businessId)
            .Select(p => new
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId,
                Photo = string.IsNullOrEmpty(p.Photo) ? "" : p.Photo  // 👈 adjust if needed
                ,businessId
            })
            .ToListAsync();

        ViewBag.Categories = categories;
        ViewBag.ProductsJson = System.Text.Json.JsonSerializer.Serialize(products);
        ViewBag.Orgnization = await _context.TblBusinesses.Where(w=>w.Id== businessId).FirstOrDefaultAsync();
        return View();
    }


    [HttpPost]
    public async Task<IActionResult> SaveOrder([FromBody] OrderDto orderDto)
    {

        try
        {
            int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);
            int UserId = Convert.ToInt32(User.FindFirst("UserId")?.Value);


            if (orderDto == null || orderDto.Items == null || !orderDto.Items.Any())
            return Json(new { success = false, message = "Invalid order data" });

        decimal grandTotal = orderDto.Items.Sum(x => x.Price * x.Qty);

        var master = new TblOrderMaster
        {
            CustomerName = orderDto.CustomerName,
            DateOfOrder = DateTime.Now,
            GrandTotal = grandTotal,
            PaymentStatus = false,
            Printed = false,
            UserId = UserId, // Replace with logged-in user
            BuisnessId = businessId,
            PaymentMode = orderDto.paymentMode,
            TableDetails =orderDto.tableDetail,
            CreatedOn= DateTime.Now,
            TblOrderDetails = orderDto.Items.Select(x => new TblOrderDetail
            {
                ProductId = x.ProductId,
                Qty = x.Qty,
                Price = x.Price,
                Total = x.Price * x.Qty
            }).ToList()
        };

        _context.TblOrderMasters.Add(master);
        await _context.SaveChangesAsync();

        return Json(new { success = true, orderId = master.Id });
        }
        catch (Exception er)
        {

            throw;
        }
    }

}
