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
        int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value ?? "0");

        DateTime todayStart = DateTime.Today;
        DateTime todayEnd = DateTime.Today.AddDays(1).AddTicks(-1);

        var Orders = await _context.TblOrderMasters
            .Where(w => w.BuisnessId == businessId
                && w.DateOfOrder.HasValue
                && w.DateOfOrder.Value.Date == DateTime.Today)   
            .Include(I => I.TblOrderDetails)
            .Include(u => u.User)
            .OrderByDescending(o => o.Id)
            .ToListAsync();


        ViewBag.Materials = _context.TblProducts.ToList();

        ViewBag.TotalCash = Orders.Where(w => w.PaymentMode == "Cash").Sum(w => w.GrandTotal);
        ViewBag.Online = Orders.Where(w => w.PaymentMode == "Online").Sum(s => s.GrandTotal);
        ViewBag.Free = Orders.Where(w => w.PaymentMode == "Free").Sum(s => s.GrandTotal);
        ViewBag.Credit = Orders.Where(w => w.PaymentMode == "Credit").Sum(s => s.GrandTotal);

        ViewBag.FromDate = todayStart.ToString("yyyy-MM-dd");
        ViewBag.ToDate = todayStart.ToString("yyyy-MM-dd");

        return View(Orders);
    }

    [HttpPost]
    public async Task<IActionResult> Report2(DateTime fromDT, DateTime toDT)
    {
        int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);
        DateTime startDate = fromDT.Date;
        DateTime endDate = toDT.Date.AddDays(1).AddTicks(-1);

        var Orders = await _context.TblOrderMasters
        .Where(w => w.BuisnessId == businessId
            && w.DateOfOrder.HasValue
            && w.DateOfOrder.Value.Date >= fromDT.Date
            && w.DateOfOrder.Value.Date <= toDT.Date)
        .Include(I => I.TblOrderDetails)
        .Include(u => u.User)
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

        if (!string.IsNullOrEmpty(orderDto.tableDetail))
        {
            // ---------- VALIDATION: Prevent multiple pending orders for same table ----------
            var existingPendingOrder = await _context.TblOrderMasters
                .Where(o => o.TableDetails == orderDto.tableDetail
                        && o.PaymentStatus == false
                        && o.BuisnessId == businessId)
                .FirstOrDefaultAsync();

            if (existingPendingOrder != null && !orderDto.editOrderId.HasValue)
            {
                return Json(new
                {
                    success = false,
                    message = $"Order already exists for this {orderDto.tableDetail} and is still pending."
                });
            }
        }
      

        decimal grandTotal = orderDto.items.Sum(x => x.price * x.qty);

        // ----------------------------------------------------------
        // CASE 1: UPDATE EXISTING ORDER
        // ----------------------------------------------------------
        if (orderDto.editOrderId.HasValue && orderDto.editOrderId.Value > 0)
        {
            var editId = orderDto.editOrderId.Value;

            var existingMaster = await _context.TblOrderMasters
                .Include(m => m.TblOrderDetails)
                .FirstOrDefaultAsync(m => m.Id == editId && m.BuisnessId == businessId);

            if (existingMaster == null)
                return Json(new { success = false, message = "Order not found or not allowed" });

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // ---------- Update master ----------
                    existingMaster.CustomerName = orderDto.customerName;
                    existingMaster.TableDetails = orderDto.tableDetail;
                    existingMaster.PaymentMode = orderDto.paymentMode;
                    existingMaster.PaymentStatus = orderDto.isPaymentDone;
                    existingMaster.Printed = orderDto.isPrinted;
                    existingMaster.GrandTotal = grandTotal;
                    existingMaster.TotalAmount = grandTotal;
                    existingMaster.Gsttotal = 0;
                    existingMaster.DateOfOrder = DateTime.Now;
                    existingMaster.UserId = UserId;
                    existingMaster.CreatedOn = DateTime.Now;

                    // ----------------------------------------------------------
                    // ---------- NEW KOT LOGIC (DETECT NEW OR INCREASED ITEMS) ----------
                    // ----------------------------------------------------------
                    List<TblOrderDetail> kotItems = new List<TblOrderDetail>();

                    foreach (var dtoItem in orderDto.items)
                    {
                        var existingItem = existingMaster.TblOrderDetails
                            .FirstOrDefault(d => d.ProductId == dtoItem.productId);

                        if (existingItem == null)
                        {
                            // NEW ITEM => Add to KOT list
                            kotItems.Add(new TblOrderDetail
                            {
                                Oid = existingMaster.Id,
                                ProductId = dtoItem.productId,
                                Qty = dtoItem.qty,
                                Price = dtoItem.price,
                                Total = dtoItem.qty * dtoItem.price,
                                Gstpercentage = 0,
                                Gstamount = 0,
                                IsKOTPrinted = false
                            });
                        }
                        else if (dtoItem.qty > existingItem.Qty)
                        {
                            // QTY INCREASE → Only send difference
                            decimal newQty = dtoItem.qty - Convert.ToDecimal(existingItem.Qty);

                            kotItems.Add(new TblOrderDetail
                            {
                                Oid = existingMaster.Id,
                                ProductId = dtoItem.productId,
                                Qty = newQty,
                                Price = existingItem.Price,
                                Total = existingItem.Price * newQty,
                                Gstpercentage = 0,
                                Gstamount = 0,
                                IsKOTPrinted = false
                            });

                            // Update old item qty
                            existingItem.Qty = dtoItem.qty;
                            existingItem.Total = existingItem.Price * dtoItem.qty;
                        }
                        else
                        {
                            // BLOCK QTY DECREASE if item KOT already printed
                            if (dtoItem.qty < existingItem.Qty)
                            {
                                return Json(new
                                {
                                    success = false,
                                    message = "Cannot reduce quantity because KOT already printed."
                                });
                            }

                            // normal update
                            existingItem.Qty = dtoItem.qty;
                            existingItem.Price = dtoItem.price;
                            existingItem.Total = dtoItem.qty * dtoItem.price;
                        }
                    }

                    // ---------- Save updated master ----------
                    await _context.SaveChangesAsync();

                    // ---------- Add new KOT items as separate rows ----------
                    if (kotItems.Count > 0)
                    {
                        await _context.TblOrderDetails.AddRangeAsync(kotItems);
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    // ---------- Return KOT items to frontend ----------
                    return Json(new
                    {
                        success = true,
                        orderId = existingMaster.Id,
                        kotItems = kotItems.Select(k => new
                        {
                            k.ProductId,
                            k.Qty,
                            k.Price
                        }).ToList(),
                        message = "Order updated"
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Update failed: " + ex.Message });
                }
            }
        }

        // ----------------------------------------------------------
        // CASE 2: INSERT NEW ORDER
        // ----------------------------------------------------------
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
                Gstamount = 0,
                IsKOTPrinted = false    // NEW ORDER = send all to KOT
            }).ToList()
        };

        _context.TblOrderMasters.Add(master);
        await _context.SaveChangesAsync();

        return Json(new
        {
            success = true,
            orderId = master.Id,
            kotItems = master.TblOrderDetails.Select(k => new
            {
                k.ProductId,
                k.Qty,
                k.Price
            })
        });
    }



    [HttpGet]
    public async Task<IActionResult> PrintKOT(int orderId, bool reprint = false)
    {
        var order = await _context.TblOrderMasters
            .Include(o => o.TblOrderDetails)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return Content("Order not found");

        var kotItems = order.TblOrderDetails
            .Where(d => d.IsKOTPrinted == false)
            .ToList();

        if (kotItems.Count == 0)
            return Content("No new items for KOT");

        // Mark as printed
        foreach (var item in kotItems)
            item.IsKOTPrinted = true;

        await _context.SaveChangesAsync();

        // Build 58mm text
        string kotText = "";
        kotText += "***** KOT *****\n";
        kotText += "Table : " + order.TableDetails + "\n";
        kotText += "Time  : " + DateTime.Now.ToString("hh:mm tt") + "\n";
        kotText += "--------------------------\n";

        foreach (var i in kotItems)
        {
            var product = await _context.TblProducts.FindAsync(i.ProductId);
            kotText += product.Name + "\n";
            kotText += "Qty: " + i.Qty + "\n";
            kotText += "--------------------------\n";
        }

        return Content(kotText, "text/plain");
    }

    // ---------- SaveOrder: INSERT or UPDATE based on incoming DTO ----------
    //[HttpPost]
    //public async Task<IActionResult> SaveOrder([FromBody] OrderDto orderDto)
    //{


    //    if (orderDto == null || orderDto.items == null || !orderDto.items.Any())
    //        return Json(new { success = false, message = "Invalid order data" });

    //    int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);
    //    int UserId = Convert.ToInt32(User.FindFirst("UserId")?.Value);

    //    decimal grandTotal = orderDto.items.Sum(x => x.price * x.qty);

    //    // If editOrderId is present -> Update existing (Option A)
    //    if (orderDto.editOrderId.HasValue && orderDto.editOrderId.Value > 0)
    //    {
    //        var editId = orderDto.editOrderId.Value;
    //        // Ensure the order exists and belongs to this business
    //        var existingMaster = await _context.TblOrderMasters
    //            .Include(m => m.TblOrderDetails)
    //            .FirstOrDefaultAsync(m => m.Id == editId && m.BuisnessId == businessId);

    //        if (existingMaster == null)
    //            return Json(new { success = false, message = "Order not found or not allowed" });

    //        using (var transaction = await _context.Database.BeginTransactionAsync())
    //        {
    //            try
    //            {
    //                // Update master fields
    //                existingMaster.CustomerName = orderDto.customerName;
    //                existingMaster.TableDetails = orderDto.tableDetail;
    //                existingMaster.PaymentMode = orderDto.paymentMode;
    //                existingMaster.PaymentStatus = orderDto.isPaymentDone;
    //                existingMaster.Printed = orderDto.isPrinted;
    //                existingMaster.GrandTotal = grandTotal;
    //                existingMaster.TotalAmount = grandTotal; // you can change this logic if needed
    //                existingMaster.Gsttotal = 0; // keep as 0 unless you want to calculate
    //                existingMaster.DateOfOrder = DateTime.Now;
    //                existingMaster.UserId = UserId;
    //                existingMaster.CreatedOn = DateTime.Now; // optionally update created/modified timestamps

    //                // Remove old details
    //                var oldDetails = _context.TblOrderDetails.Where(d => d.Oid == existingMaster.Id);
    //                _context.TblOrderDetails.RemoveRange(oldDetails);
    //                await _context.SaveChangesAsync();

    //                // Add new details
    //                var newDetails = orderDto.items.Select(x => new TblOrderDetail
    //                {
    //                    Oid = existingMaster.Id,
    //                    ProductId = x.productId,
    //                    Qty = x.qty,
    //                    Price = x.price,
    //                    Total = x.price * x.qty,
    //                    Gstpercentage = 0,
    //                    Gstamount = 0
    //                }).ToList();

    //                await _context.TblOrderDetails.AddRangeAsync(newDetails);
    //                await _context.SaveChangesAsync();

    //                await transaction.CommitAsync();

    //                return Json(new { success = true, orderId = existingMaster.Id, message = "Order updated" });
    //            }
    //            catch (Exception ex)
    //            {
    //                await transaction.RollbackAsync();
    //                // log or rethrow as appropriate
    //                return Json(new { success = false, message = "Update failed: " + ex.Message });
    //            }
    //        }
    //    }
    //    else
    //    {
    //        // Create new master + details (existing code behavior)
    //        var master = new TblOrderMaster
    //        {
    //            CustomerName = orderDto.customerName,
    //            DateOfOrder = DateTime.Now,
    //            GrandTotal = grandTotal,
    //            TotalAmount = grandTotal,
    //            Gsttotal = 0,
    //            PaymentStatus = orderDto.isPaymentDone,
    //            Printed = orderDto.isPrinted,
    //            UserId = UserId,
    //            BuisnessId = businessId,             
    //            PaymentMode = orderDto.paymentMode,
    //            TableDetails = orderDto.tableDetail,
    //            CreatedOn = DateTime.Now,
    //            TblOrderDetails = orderDto.items.Select(x => new TblOrderDetail
    //            {
    //                ProductId = x.productId,
    //                Qty = x.qty,
    //                Price = x.price,
    //                Total = x.price * x.qty,
    //                Gstpercentage = 0,
    //                Gstamount = 0
    //            }).ToList()
    //        };

    //        _context.TblOrderMasters.Add(master);
    //        await _context.SaveChangesAsync();

    //        return Json(new { success = true, orderId = master.Id });
    //    }
//}
}
