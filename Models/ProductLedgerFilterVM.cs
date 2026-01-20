using Microsoft.AspNetCore.Mvc.Rendering;

namespace eOrderTouchApp.Models
{
    public class ProductLedgerFilterVM
    {
        public int BusinessId { get; set; }
        public int ProductId { get; set; }
        public DateTime FromDate { get; set; } = DateTime.Today;
        public DateTime ToDate { get; set; } = DateTime.Today;

        public List<SelectListItem> Products { get; set; } = new();
        public List<ProductLedgerVM> ReportData { get; set; } = new();
    }
    public class ProductLedgerVM
    {
        public DateTime Dt { get; set; }
        public decimal Opening { get; set; }
        public decimal ReceivedItem { get; set; }
        public decimal TotalStock { get; set; }
        public decimal SoldItem { get; set; }
        public decimal FinalStock { get; set; }
    }
}
