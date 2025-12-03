using eOrderTouchApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Transactions;

[AuthorizeToRoles("User", "Owner")]
public class OrderController : Controller
{
    private readonly eOrderTouchContext _context;

    public OrderController(eOrderTouchContext context)
    {
        _context = context;
    }

    // ---------- DTOs used by frontend ----------
    public class OrderItemDto
    {
        public int productId { get; set; }
        public decimal qty { get; set; }
        public decimal price { get; set; }
    }

    public class OrderDto
    {
        public int? editOrderId { get; set; } // if present, update existing order
        public string customerName { get; set; }
        public string tableDetail { get; set; }
        public string paymentMode { get; set; }
        public bool isPaymentDone { get; set; }
        public bool isPrinted { get; set; }

        public List<OrderItemDto> items { get; set; }
    }

    // ---------- Reporting actions (unchanged, kept for reference) ----------
    public async Task<IActionResult> Report()
    {
        int businessId = Convert.ToInt32(User.FindFirst(System.Security.Claims.ClaimTypes.UserData)?.Value);

        var Orders = await _context.TblOrderMasters
            .Where(w => w.BuisnessId == businessId)
            .Include(I => I.TblOrderDetails)
            .Include(u => u.User)
            .OrderByDescending(o => o.Id)
            .ToListAsync();

        ViewBag.Materials = _context.TblProducts.ToList();

        return View(Orders);
    }

    [HttpPost]
    public async Task<IActionResult> Report2(DateTime fromDT, DateTime toDT)
    {
        int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

        var Orders = await _context.TblOrderMasters
            .Where(w => w.BuisnessId == businessId)
            .Include(I => I.TblOrderDetails)
            .Include(u=> u.User)
            .Where(w => w.DateOfOrder >= fromDT && w.DateOfOrder <= toDT)
            .OrderByDescending(o => o.Id)
            .ToListAsync();

        ViewBag.TotalCash = Orders.Where(w => w.PaymentMode == "Cash").Sum(w => w.GrandTotal);
        ViewBag.Online = Orders.Where(w => w.PaymentMode == "Online").Sum(s => s.GrandTotal);
        ViewBag.Free = Orders.Where(w => w.PaymentMode == "Free").Sum(s => s.GrandTotal);
        ViewBag.Credit = Orders.Where(w => w.PaymentMode == "Credit").Sum(s => s.GrandTotal);
        ViewBag.Materials = _context.TblProducts.ToList();
        //To keep the dates after posting the data//
        ViewBag.FromDate = fromDT.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDT.ToString("yyyy-MM-dd");

        return View("Report", Orders);
    }

    // ---------- Create page (GET) ----------
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
                Photo = string.IsNullOrEmpty(p.Photo) ? "" : p.Photo,
                businessId
            })
            .ToListAsync();

        ViewBag.Categories = categories;
        ViewBag.ProductsJson = System.Text.Json.JsonSerializer.Serialize(products);
        ViewBag.Orgnization = await _context.TblBusinesses.Where(w => w.Id == businessId).FirstOrDefaultAsync();
        return View();
    }

    // ---------- New: get last 5 orders (for history modal) ----------
    [HttpGet]
    public async Task<IActionResult> GetLastOrders()
    {
        int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

        var orders = await _context.TblOrderMasters
            .Where(w => w.BuisnessId == businessId && w.PaymentStatus!=true)
            .OrderByDescending(o => o.Id)
            .Select(o => new
            {
                o.Id,
                CustomerName = o.CustomerName,
                TableDetails = o.TableDetails,
                GrandTotal = o.GrandTotal,
                DateOfOrder = o.DateOfOrder
            })
           // .Take(5)
            .ToListAsync();

        return Json(new { success = true, data = orders });
    }

    // ---------- New: get a single order by id including details (for editing) ----------
    [HttpGet]
    public async Task<IActionResult> GetOrder(int id)
    {
        int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

        var order = await _context.TblOrderMasters
            .Where(w => w.BuisnessId == businessId && w.Id == id)
            .Include(m => m.TblOrderDetails)
            .FirstOrDefaultAsync();

        if (order == null) return Json(new { success = false, message = "Order not found" });

        var dto = new
        {
            order.Id,
            CustomerName = order.CustomerName,
            TableDetails = order.TableDetails,
            PaymentMode = order.PaymentMode,
            GrandTotal = order.GrandTotal,
            DateOfOrder = order.DateOfOrder,
            Items = order.TblOrderDetails.Select(d => new
            {
                productId = d.ProductId,
                qty = d.Qty,
                price = d.Price
            }).ToList()
        };

        return Json(new { success = true, data = dto });
    }

    // ---------- SaveOrder: INSERT or UPDATE based on incoming DTO ----------
    [HttpPost]
    public async Task<IActionResult> SaveOrder([FromBody] OrderDto orderDto)
    {
        if (orderDto == null || orderDto.items == null || !orderDto.items.Any())
            return Json(new { success = false, message = "Invalid order data" });

        int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);
        int UserId = Convert.ToInt32(User.FindFirst("UserId")?.Value);

        decimal grandTotal = orderDto.items.Sum(x => x.price * x.qty);

        // If editOrderId is present -> Update existing (Option A)
        if (orderDto.editOrderId.HasValue && orderDto.editOrderId.Value > 0)
        {
            var editId = orderDto.editOrderId.Value;
            // Ensure the order exists and belongs to this business
            var existingMaster = await _context.TblOrderMasters
                .Include(m => m.TblOrderDetails)
                .FirstOrDefaultAsync(m => m.Id == editId && m.BuisnessId == businessId);

            if (existingMaster == null)
                return Json(new { success = false, message = "Order not found or not allowed" });

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Update master fields
                    existingMaster.CustomerName = orderDto.customerName;
                    existingMaster.TableDetails = orderDto.tableDetail;
                    existingMaster.PaymentMode = orderDto.paymentMode;
                    existingMaster.PaymentStatus = orderDto.isPaymentDone;
                    existingMaster.Printed = orderDto.isPrinted;
                    existingMaster.GrandTotal = grandTotal;
                    existingMaster.TotalAmount = grandTotal; // you can change this logic if needed
                    existingMaster.Gsttotal = 0; // keep as 0 unless you want to calculate
                    existingMaster.DateOfOrder = DateTime.Now;
                    existingMaster.UserId = UserId;
                    existingMaster.CreatedOn = DateTime.Now; // optionally update created/modified timestamps

                    // Remove old details
                    var oldDetails = _context.TblOrderDetails.Where(d => d.Oid == existingMaster.Id);
                    _context.TblOrderDetails.RemoveRange(oldDetails);
                    await _context.SaveChangesAsync();

                    // Add new details
                    var newDetails = orderDto.items.Select(x => new TblOrderDetail
                    {
                        Oid = existingMaster.Id,
                        ProductId = x.productId,
                        Qty = x.qty,
                        Price = x.price,
                        Total = x.price * x.qty,
                        Gstpercentage = 0,
                        Gstamount = 0
                    }).ToList();

                    await _context.TblOrderDetails.AddRangeAsync(newDetails);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return Json(new { success = true, orderId = existingMaster.Id, message = "Order updated" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    // log or rethrow as appropriate
                    return Json(new { success = false, message = "Update failed: " + ex.Message });
                }
            }
        }
        else
        {
            // Create new master + details (existing code behavior)
            var master = new TblOrderMaster
            {
                CustomerName = orderDto.customerName,
                DateOfOrder = DateTime.Now,
                GrandTotal = grandTotal,
                TotalAmount = grandTotal,
                Gsttotal = 0,
                PaymentStatus = orderDto.isPaymentDone,
                Printed = orderDto.isPrinted,
                UserId = UserId,
                BuisnessId = businessId,             
                PaymentMode = orderDto.paymentMode,
                TableDetails = orderDto.tableDetail,
                CreatedOn = DateTime.Now,
                TblOrderDetails = orderDto.items.Select(x => new TblOrderDetail
                {
                    ProductId = x.productId,
                    Qty = x.qty,
                    Price = x.price,
                    Total = x.price * x.qty,
                    Gstpercentage = 0,
                    Gstamount = 0
                }).ToList()
            };

            _context.TblOrderMasters.Add(master);
            await _context.SaveChangesAsync();

            return Json(new { success = true, orderId = master.Id });
        }
    }
}
