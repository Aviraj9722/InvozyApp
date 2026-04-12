using eOrderTouchApp.Models;
using eOrderTouchApp.Models.ReportsModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Transactions;

[AuthorizeToRoles("User", "Owner", "HeadOfficer")]
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

        public decimal discountPercent { get; set; }   // NEW
        public decimal discountedPrice { get; set; }   // NEW

        public List<OrderItemDto> items { get; set; }
    }

    // ---------- Reporting actions (unchanged, kept for reference) ----------
    //public async Task<IActionResult> Report()
    //{

    //    var istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
    //    DateTime istNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

    //    int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value ?? "0");

    //    DateTime todayStart = istNow;
    //    DateTime todayEnd = istNow.AddDays(1).AddTicks(-1);

    //    ViewBag.Materials = null;

    //    ViewBag.TotalCash = 0;
    //    ViewBag.Online = 0;
    //    ViewBag.Free = 0;
    //    ViewBag.Credit = 0;

    //    ViewBag.FromDate = todayStart.ToString("yyyy-MM-dd");
    //    ViewBag.ToDate = todayStart.ToString("yyyy-MM-dd");

    //    return View(new List<TblOrderMaster>());
    //}


    //[HttpPost]
    //public async Task<IActionResult> Report2(DateTime fromDT, DateTime toDT)
    //{
    //    int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);
    //    DateTime startDate = fromDT.Date;
    //    DateTime endDate = toDT.Date.AddDays(1).AddTicks(-1);

    //    var Orders = await _context.TblOrderMasters
    //    .Where(w => w.BuisnessId == businessId
    //        && w.DateOfOrder.HasValue
    //        && w.PaymentStatus == true 
    //        && w.IsCanceled == false
    //        && w.DateOfOrder.Value.Date >= fromDT.Date
    //        && w.DateOfOrder.Value.Date <= toDT.Date)
    //    .Include(I => I.TblOrderDetails)
    //    .Include(u => u.User)
    //    .OrderByDescending(o => o.Id)
    //    .ToListAsync();

    //    ViewBag.TotalCash = Orders.Where(w => w.PaymentMode == "Cash").Sum(w => w.GrandTotal);
    //    ViewBag.Online = Orders.Where(w => w.PaymentMode == "Online").Sum(s => s.GrandTotal);
    //    ViewBag.Free = Orders.Where(w => w.PaymentMode == "Free").Sum(s => s.GrandTotal);
    //    ViewBag.Credit = Orders.Where(w => w.PaymentMode == "Credit").Sum(s => s.GrandTotal);
    //    ViewBag.Materials = _context.TblProducts.Where(w => w.BusinessId == businessId).ToList();
    //    ViewBag.TotalDiscountedPrice = Orders.Sum(s => s.DiscountedPrice);
    //    ViewBag.TotalDiscount = Math.Round((double)Orders.Sum(w => w.TotalAmount),2) - Math.Round((double)Orders.Sum(s => s.DiscountedPrice),2);
    //    ViewBag.GrandTotal = (double)Orders.Sum(w => w.GrandTotal) + (double)ViewBag.TotalDiscount;

    //        ViewBag.TotalDiscount = Math.Round(Convert.ToDouble(ViewBag.TotalDiscount), 2);
    //    ViewBag.GrandTotal = Math.Round(Convert.ToDouble(ViewBag.GrandTotal), 2);
    //    //To keep the dates after posting the data//
    //    ViewBag.FromDate = fromDT.ToString("yyyy-MM-dd");
    //    ViewBag.ToDate = toDT.ToString("yyyy-MM-dd");

    //    return View("Report", Orders);
    //}
    public async Task<IActionResult> Report(int selectedbusinessId = 0)
    {
        var istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
        DateTime istNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

        int userId = Convert.ToInt32(User.FindFirst("UserId")?.Value);

        // 🔹 HO dropdown
        if (User.IsInRole("HeadOfficer"))
        {
            var units = _context.TblHOUnits
                .Where(x => x.UserId == userId)
                .Include(x => x.Business)
                .Select(x => new
                {
                    x.Business.Id,
                    x.Business.BusinessName,
                    x.Business.Address
                })
                .ToList();

            ViewBag.Units = units;
        }

        int businessId;
        if (User.IsInRole("HeadOfficer"))
        {
            businessId = selectedbusinessId;   // dropdown driven
        }
        else
        {
            businessId = Convert.ToInt32(User.FindFirst("OrgId")!.Value);
        }

        ViewBag.SelectedBusinessId = businessId;

        ViewBag.Materials = null;
        ViewBag.TotalCash = 0;
        ViewBag.Online = 0;
        ViewBag.Free = 0;
        ViewBag.Credit = 0;

        ViewBag.FromDate = istNow.ToString("yyyy-MM-dd");
        ViewBag.ToDate = istNow.ToString("yyyy-MM-dd");

        var business = _context.TblBusinesses
    .FirstOrDefault(x => x.Id == businessId);

        ViewBag.GstApplicable = business?.IsGstapplicable ?? false;

        return View(new List<TblOrderMaster>());
    }


    [HttpPost]
    public async Task<IActionResult> Report2(DateTime fromDT,DateTime toDT,int selectedbusinessId = 0)
    {
        int userId = Convert.ToInt32(User.FindFirst("UserId")?.Value);

        // 🔹 HO dropdown
        if (User.IsInRole("HeadOfficer"))
        {
            var units = _context.TblHOUnits
                .Where(x => x.UserId == userId)
                .Include(x => x.Business)
                .Select(x => new
                {
                    x.Business.Id,
                    x.Business.BusinessName,
                    x.Business.Address
                })
                .ToList();

            ViewBag.Units = units;
        }

        int businessId;
        if (User.IsInRole("HeadOfficer"))
        {
            businessId = selectedbusinessId;
        }
        else
        {
            businessId = Convert.ToInt32(User.FindFirst("OrgId")!.Value);
        }

        ViewBag.SelectedBusinessId = businessId;

        DateTime startDate = fromDT.Date;
        DateTime endDate = toDT.Date.AddDays(1).AddTicks(-1);

        var Orders = await _context.TblOrderMasters
            .Where(w =>
                w.BuisnessId == businessId
                && w.DateOfOrder.HasValue
                && w.DateOfOrder.Value.Date >= startDate.Date
                && w.DateOfOrder.Value.Date <= endDate.Date
            )
            .Include(o => o.TblOrderDetails)
            .Include(o => o.User)
            .OrderByDescending(o => o.Id)
            .ToListAsync();
        var business = _context.TblBusinesses
   .FirstOrDefault(x => x.Id == businessId);

        ViewBag.GstApplicable = business?.IsGstapplicable ?? false;
        ViewBag.TotalCash = Orders
        .Where(w => w.PaymentMode == "Cash"
                 && (w.PaymentStatus ?? false)
                 && w.IsCanceled == false)
        .Sum(w => w.GrandTotal);

        ViewBag.Online = Orders
        .Where(w => w.PaymentMode == "Online"
                 && (w.PaymentStatus ?? false)
                 && w.IsCanceled == false)
        .Sum(w => w.GrandTotal);

        ViewBag.Free = Orders
        .Where(w => w.PaymentMode == "Free"
                 && (w.PaymentStatus ?? false)
                 && w.IsCanceled == false)
        .Sum(w => w.GrandTotal);

        ViewBag.Credit = Orders
         .Where(w => w.PaymentMode == "Credit"
                  && (w.PaymentStatus ?? false)
                  && w.IsCanceled == false)
         .Sum(w => w.GrandTotal);

        ViewBag.TotalDiscount = Orders
         .Where(w => (w.PaymentStatus ?? false) && w.IsCanceled == false)
         .Sum(w => Convert.ToDecimal(w.TotalAmount ?? 0)
                 - Convert.ToDecimal(w.DiscountedPrice ?? 0));

        // 🔹 Grand Total (Final Payable Total)
        ViewBag.GrandTotal = Orders
            .Where(w => (w.PaymentStatus ?? false) && w.IsCanceled == false)
            .Sum(w => Convert.ToDecimal(w.GrandTotal ?? 0));

        // Round values
        ViewBag.TotalDiscount = Math.Round(Convert.ToDecimal(ViewBag.TotalDiscount), 2);
        ViewBag.GrandTotal = Math.Round(Convert.ToDecimal(ViewBag.GrandTotal), 2);

        ViewBag.Materials = _context.TblProducts
            .Where(w => w.BusinessId == businessId)
            .ToList();

        ViewBag.FromDate = fromDT.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDT.ToString("yyyy-MM-dd");

        return View("Report", Orders);
    }

    // ---------- Create page (GET) ----------
    public async Task<IActionResult> Create()
    {
        int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);
        int UserId = Convert.ToInt32(User.FindFirst("UserId")?.Value);

        ViewBag.Tables = await _context.TblTables
    .Where(t => t.BusinessId == businessId)
    .OrderBy(t => t.Id)
    .ToListAsync();

        var categories = await _context.TblCategories.Where(w => w.BusinessId == businessId).ToListAsync();

        var products = await _context.TblProducts.Where(w => w.BusinessId == businessId)
            .Select(p => new
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId,
                Photo = string.IsNullOrEmpty(p.Photo) ? "" : p.Photo,
                businessId,
                p.Code
               
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
            .Where(w => w.BuisnessId == businessId && w.PaymentStatus != true && (w.IsCanceled == null || w.IsCanceled == false))
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

    
    [HttpPost]
    public async Task<IActionResult> CancelOrder([FromBody] CancelOrderDto dto)
    {
        if (dto == null || dto.orderId <= 0 || string.IsNullOrWhiteSpace(dto.note))
            return Json(new { success = false, message = "Invalid cancellation data." });

        int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

        var order = await _context.TblOrderMasters
            .FirstOrDefaultAsync(o => o.Id == dto.orderId && o.BuisnessId == businessId);

        if (order == null)
            return Json(new { success = false, message = "Order not found." });

        //if (order.PaymentStatus == true)
        //    return Json(new { success = false, message = "Order already completed. Cannot cancel." });

        // Mark as cancelled
        //order.PaymentStatus = false;
        order.IsCanceled = true;
        order.CancelNote = $"{order.CustomerName} (Cancelled: {dto.note})";

        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Order cancelled successfully." });
    }
    [HttpGet]
    public async Task<IActionResult> GetProductByBarcode(string code)
    {
        int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

        var product = await _context.TblProducts
            .Where(p => p.BusinessId == businessId && p.Code == code)
            .Select(p => new {
                p.Id,
                p.Name,
                p.Price,
                p.Photo,
                p.Code
            })
            .FirstOrDefaultAsync();

        if (product == null)
            return Json(new { success = false, message = "Product not found" });

        return Json(new { success = true, data = product });
    }

    [HttpGet]
    public async Task<IActionResult> PrintKOT(int orderId, bool reprint = false)
    {
        var istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
        DateTime istNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);
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
        kotText += "Time  : " + istNow.ToString("dd-MMM-yyyy hh:mm tt") + "\n";
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




    [HttpGet]
    public async Task<IActionResult> GetCustomers(string search)
    {
        int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);


        var query = _context.TblCustomers
                    .Where(c => c.BusinessId == businessId);

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(c =>
                   c.Name.ToLower().Contains(search) ||
                   c.MobileNo.Contains(search) ||
                   c.Location.ToLower().Contains(search));
        }

        var customers = await query
            .OrderBy(c => c.Name)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Location,
                c.MobileNo
            })
            .ToListAsync();

        return Json(customers);
    }


    [HttpPost]
    public async Task<IActionResult> SaveOrder([FromBody] OrderDto orderDto)
    {
        var istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
        DateTime istNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

        if (orderDto == null || orderDto.items == null || !orderDto.items.Any())
            return Json(new { success = false, message = "Invalid order data" });

        int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);
        var business = await _context.TblBusinesses.FindAsync(businessId);
        int UserId = Convert.ToInt32(User.FindFirst("UserId")?.Value);

        // Prevent duplicate pending order on same table
        if (!string.IsNullOrEmpty(orderDto.tableDetail))
        {
            var existingPendingOrder = await _context.TblOrderMasters
                .Where(o => o.TableDetails == orderDto.tableDetail
                            && o.PaymentStatus == false
                            && o.IsCanceled == false
                            && o.BuisnessId == businessId)
                .FirstOrDefaultAsync();

            if (existingPendingOrder != null && !orderDto.editOrderId.HasValue)
            {
                return Json(new
                {
                    success = false,
                    message = $"Order already exists for {orderDto.tableDetail}."
                });
            }
        }

        // decimal grandTotal = orderDto.items.Sum(x => x.price * x.qty);

        decimal discountPercent = orderDto.discountPercent;
        decimal discountAmount = 0;
        decimal discountedPrice = 0;

        decimal grandTotal = orderDto.items.Sum(x => x.price * x.qty);

        if (business.DiscountType?.Trim().ToLower()=="amount")
        {
            var Amout = discountPercent;
            // Flat discount amount
            discountAmount = discountPercent;

            // Safety: discount should not exceed total
            if (discountAmount > grandTotal)
                discountAmount = grandTotal;

            discountedPrice = grandTotal - discountAmount;
        }
        else
        {
            var percent= discountPercent;
             discountAmount = (grandTotal * percent) / 100;
             discountedPrice = grandTotal - discountAmount;
         
        }


        // ----------------------------------------------------------
        // UPDATE EXISTING ORDER
        // ----------------------------------------------------------
        if (orderDto.editOrderId.HasValue && orderDto.editOrderId.Value > 0)
        {
            var editId = orderDto.editOrderId.Value;

            var existingMaster = await _context.TblOrderMasters
                .Include(m => m.TblOrderDetails)
                .FirstOrDefaultAsync(m => m.Id == editId && m.BuisnessId == businessId);

            if (existingMaster == null)
                return Json(new { success = false, message = "Order not found" });

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // -------- Update master ----------
                    existingMaster.CustomerName = orderDto.customerName;
                    existingMaster.TableDetails = orderDto.tableDetail;
                    existingMaster.PaymentMode = orderDto.paymentMode;
                    existingMaster.PaymentStatus = orderDto.isPaymentDone;
                    existingMaster.Printed = orderDto.isPrinted;
                  
                   
               
                    existingMaster.Gsttotal = existingMaster.TblOrderDetails
                                                .Sum(x => x.Gstamount ?? 0);
                    existingMaster.DateOfOrder = istNow;
                    existingMaster.UserId = UserId;
                    existingMaster.CreatedOn = istNow;

                  
                    existingMaster.TotalAmount = grandTotal;      // ORIGINAL total
                    existingMaster.DiscountPercent = (float)discountPercent;
                    existingMaster.DiscountedPrice = (float)discountedPrice;
                    existingMaster.GrandTotal = discountedPrice + existingMaster.Gsttotal;  // PAYABLE amount

                    // NEW KOT LIST (TblKOTDetails)
                    List<TblKOTDetail> kotList = new List<TblKOTDetail>();

                    foreach (var dtoItem in orderDto.items)
                    {

                        var existingItem = existingMaster.TblOrderDetails
                            .FirstOrDefault(d => d.ProductId == dtoItem.productId);

                        // -----------------------------------
                        // CASE 1: NEW ITEM ADDED
                        // -----------------------------------
                        if (existingItem == null)
                        {
                            #region GST calculation for item
                            var prod =  _context.TblProducts.Find(dtoItem.productId);
                            var gst = prod.Gstpercentage== null? 0: (decimal)prod?.Gstpercentage;
                            decimal itemGstAmount = 0;
                            decimal itemCGST = 0;
                            decimal itemSGST = 0;

                            if (gst>0)
                            {
                                decimal itemTotal = dtoItem.qty * dtoItem.price;

                               
                                if (business.IsGstapplicable == true && gst > 0)
                                {
                                    itemGstAmount = (itemTotal * gst) / 100;
                                    itemCGST = itemGstAmount / 2;
                                    itemSGST = itemGstAmount / 2;
                                }

                            }
                            #endregion
                            var mainRow = new TblOrderDetail
                            {
                                Oid = existingMaster.Id,
                                ProductId = dtoItem.productId,
                                Qty = dtoItem.qty,
                                Price = dtoItem.price,
                                Total = dtoItem.qty * dtoItem.price,
                                Gstpercentage = gst,
                                Gstamount = itemGstAmount,
                                CGST = itemCGST,
                                SGST = itemSGST,
                                IsKOTPrinted = false
                            };

                           

                            existingMaster.TblOrderDetails.Add(mainRow);

                            if (business.IsKOTEnabled == true)
                            {
                                kotList.Add(new TblKOTDetail
                                {
                                    OrderId = existingMaster.Id,
                                    ProductId = dtoItem.productId,
                                    Qty = (int)dtoItem.qty,
                                    // Price = dtoItem.price,
                                    KotType = "NEW",
                                    CreatedBy = UserId,
                                    CreatedAt = istNow
                                });
                            }

                            continue;
                        }

                        // -----------------------------------
                        // CASE 2: QTY INCREASED
                        // -----------------------------------
                        if (dtoItem.qty > existingItem.Qty)
                        {
                            int diffQty = (int)dtoItem.qty - (int)existingItem.Qty;

                            if (business.IsKOTEnabled == true)
                            {
                                kotList.Add(new TblKOTDetail
                                {
                                    OrderId = existingMaster.Id,
                                    ProductId = dtoItem.productId,
                                    Qty = diffQty,
                                    // Price = existingItem.Price,
                                    KotType = "UPDATE",
                                    CreatedBy = UserId,
                                    CreatedAt = istNow
                                });
                            }

                            existingItem.Qty = dtoItem.qty;
                            existingItem.Total = dtoItem.qty * existingItem.Price;
                            continue;
                        }

                        // -----------------------------------
                        // CASE 3: QTY DECREASED
                        // -----------------------------------
                        if (dtoItem.qty < existingItem.Qty)
                        {
                            if (existingItem.IsKOTPrinted == true && orderDto.isPaymentDone != true)
                            {
                                return Json(new { success = false, message = "Cannot reduce quantity, KOT already printed." });
                            }

                            int diffQty = (int)existingItem.Qty - (int)dtoItem.qty;

                            if (business.IsKOTEnabled == true)
                            {
                                kotList.Add(new TblKOTDetail
                                {
                                    OrderId = existingMaster.Id,
                                    ProductId = dtoItem.productId,
                                    Qty = diffQty,
                                    //Price = existingItem.Price,
                                    KotType = "CANCEL",
                                    CreatedBy = UserId,
                                    CreatedAt = istNow
                                });
                            }

                            existingItem.Qty = dtoItem.qty;
                            existingItem.Total = dtoItem.qty * existingItem.Price;
                            continue;
                        }

                        // -----------------------------------
                        // CASE 4: SAME QTY – update price only
                        // -----------------------------------
                        existingItem.Price = dtoItem.price;
                        existingItem.Total = dtoItem.qty * dtoItem.price;
                    }

                    await _context.SaveChangesAsync();

                    // ---------------- Insert KOT rows ----------------
                    if (business.IsKOTEnabled == true && kotList.Count > 0)
                    {
                        await _context.TblKOTDetails.AddRangeAsync(kotList);
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    var returnKotItems = (bool)business.IsKOTEnabled ?
                        kotList.Select(k => new { k.ProductId, k.Qty }) : null;

                    return Json(new
                    {
                        success = true,
                        orderId = existingMaster.Id,
                        kotItems = returnKotItems,
                        totalAmount = existingMaster.TotalAmount,
                        grandtotal = existingMaster.GrandTotal,
                        message = "Order updated successfully"
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
        // INSERT NEW ORDER
        // ----------------------------------------------------------
        var orderDetails = new List<TblOrderDetail>();
        decimal totalGst = 0;

        foreach (var x in orderDto.items)
        {
            var prod = _context.TblProducts.Find(x.productId);
            decimal gst = prod?.Gstpercentage ?? 0;

            decimal itemTotal = x.qty * x.price;
            decimal itemGstAmount = 0;
            decimal itemCGST = 0;
            decimal itemSGST = 0;

            if (business.IsGstapplicable == true && gst > 0)
            {
                itemGstAmount = (itemTotal * gst) / 100;
                itemCGST = itemGstAmount / 2;
                itemSGST = itemGstAmount / 2;
            }

            totalGst += itemGstAmount;

            orderDetails.Add(new TblOrderDetail
            {
                ProductId = x.productId,
                Qty = x.qty,
                Price = x.price,
                Total = itemTotal,
                Gstpercentage = gst,
                Gstamount = itemGstAmount,
                CGST = itemCGST,
                SGST = itemSGST,
                IsKOTPrinted = false
            });
        }
        var master = new TblOrderMaster
        {
            CustomerName = orderDto.customerName,
            DateOfOrder = istNow,
            TotalAmount = grandTotal,
            DiscountPercent = (float)discountPercent,
            DiscountedPrice = (float)discountedPrice,
            GrandTotal = discountedPrice + totalGst,   // IMPORTANT
            Gsttotal = totalGst,                       // IMPORTANT
            PaymentStatus = orderDto.isPaymentDone,
            Printed = orderDto.isPrinted,
            UserId = UserId,
            BuisnessId = businessId,
            PaymentMode = orderDto.paymentMode,
            TableDetails = orderDto.tableDetail,
            IsCanceled = false,
            CreatedOn = istNow,
            TblOrderDetails = orderDetails
        };
        //var master = new TblOrderMaster
        //{
        //    CustomerName = orderDto.customerName,
        //    DateOfOrder = istNow,

        //    TotalAmount = grandTotal,        // BEFORE discount
        //    DiscountPercent = (float)discountPercent,
        //    DiscountedPrice = (float)discountedPrice,
        //    GrandTotal = discountedPrice,     // AFTER discount

        //    Gsttotal = 0,
        //    PaymentStatus = orderDto.isPaymentDone,
        //    Printed = orderDto.isPrinted,
        //    UserId = UserId,
        //    BuisnessId = businessId,
        //    PaymentMode = orderDto.paymentMode,
        //    TableDetails = orderDto.tableDetail,
        //    IsCanceled = false,
        //    CreatedOn = istNow,
        //    //TblOrderDetails = orderDto.items.Select(x => new TblOrderDetail
        //    //{
        //    //    ProductId = x.productId,
        //    //    Qty = x.qty,
        //    //    Price = x.price,
        //    //    Total = x.qty * x.price,
        //    //    Gstpercentage = 0,
        //    //    Gstamount = 0,
        //    //    IsKOTPrinted = false
        //    //}).ToList()

        //};

        _context.TblOrderMasters.Add(master);
        await _context.SaveChangesAsync();

        return Json(new
        {
            success = true,
            orderId = master.Id,
            kotItems = (bool)business.IsKOTEnabled
                ? master.TblOrderDetails.Select(k => new { k.ProductId, k.Qty, k.Price })
                : null,
            totalAmount = master.TotalAmount,
            grandtotal = master.GrandTotal
        });
    }


    [HttpGet]
    public async Task<IActionResult> GetOrderGstSummary(int orderId)
    {
        try
        {
            var gstSummary = _context.Database.SqlQueryRaw<GSTSummary>($"Select GSTPercentage, Sum( Price) as Price, Sum (CGST) as CGST, Sum(SGST) as SGST from tblOrderDetails Where GSTAmount > 0 AND OID = {orderId} Group by GSTPercentage").ToListAsync();


            return Ok(new
            {
                success = true,
                data = gstSummary
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }


}
