using Microsoft.EntityFrameworkCore;

namespace eOrderTouchApp.Models.ReportsModel
{
    [Keyless]
    public class TodayDashboardVM
    {
        public int TotalOrders { get; set; }
        public decimal TotalSale { get; set; }
        public decimal Cash { get; set; }
        public decimal Online { get; set; }
        public decimal Credit { get; set; }
        public decimal Profit { get; set; }
    }

}
