using eOrderTouchApp.Models;
using eOrderTouchApp.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System.Threading.Channels;

[AuthorizeToRoles("Owner", "HeadOfficer")]
public class StockController : Controller
{
    private readonly eOrderTouchContext _context;

    public StockController(eOrderTouchContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

        var products =  _context.TblProducts.Where(w => w.BusinessId == businessId).ToList();
        
        LoadVendors();

        var vm = new StockPurchaseVM
        {
            PurchaseDate = DateTime.Now,
            Products = _context.TblProducts
            .Where(p => p.BusinessId == businessId)
            .Select(p => new ProductStockLine
            {
                ProductId = p.Id,
                ProductName = p.Name,
                AvailableStock =
                    (_context.TblPODetails.Where(x => x.ProductId == p.Id).Sum(x => (int?)x.Quantity) ?? 0)
                    -
                    (_context.TblOrderDetails.Where(x => x.ProductId == p.Id).Sum(x => (int?)x.Qty) ?? 0)
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    public IActionResult Save(StockPurchaseVM model)
    {
        LoadVendors();
        int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

        // 1️⃣ Create PO Master for this invoice
        var po = new TblPOMaster
        {
            InvoiceNo = model.InvoiceNo,
            VendorId = model.VendorId,
            DateOfPurchase = model.PurchaseDate,
            GrandTotal = model.GrandTotal,
            BusinessId = businessId
        };
        _context.TblPOMaster.Add(po);
        _context.SaveChanges();

        // 2️⃣ Iterate over products and update/add quantities
        foreach (var item in model.Products)
        {
            if (item.NewQuantity <= 0) continue; // skip empty inputs

            var existingDetail = _context.TblPODetails
                .Where(x => x.ProductId == item.ProductId)
                .OrderByDescending(x => x.Id)
                .FirstOrDefault();

            if (existingDetail != null)
            {
                // Add new quantity to existing
                existingDetail.Quantity = (existingDetail.Quantity ?? 0) + item.NewQuantity;
            }
            else
            {
                // Create new PODetail if none exists
                var detail = new TblPODetails
                {
                    POMasterId = po.Id,
                    ProductId = item.ProductId,
                    Quantity = item.NewQuantity,
                    Price = 0,
                    Total = 0
                };
                _context.TblPODetails.Add(detail);
            }
        }

        _context.SaveChanges();

        TempData["Success"] = "Stock updated successfully!";
        return RedirectToAction("Index");
    }

    private void ReloadProducts(StockPurchaseVM model)
    {
        model.Products = _context.TblProducts
           .Select(p => new ProductStockLine
           {
               ProductId = p.Id,
               ProductName = p.Name,
               AvailableStock =
                   (_context.TblPODetails.Where(x => x.ProductId == p.Id).Sum(x => (int?)x.Quantity) ?? 0)
                   -
                   (_context.TblOrderDetails.Where(x => x.ProductId == p.Id).Sum(x => (int?)x.Qty) ?? 0),
               NewQuantity = 0 // initialize
           }).ToList();
    }



    private void LoadVendors()
    {
        int businessId = Convert.ToInt32(User.FindFirst("OrgId")?.Value);

        ViewBag.Vendors = new SelectList(_context.TblVendors.Where(w => w.BusinessId == businessId).ToList(), "Id", "Name");


    }


}
