using Microsoft.EntityFrameworkCore;

namespace eOrderTouchApp.ViewModel
{
    [Keyless]
    public class HOBranchDashboardVM
    {
        public int BusinessId { get; set; }
        public string BranchName { get; set; }
        public string Location { get; set; }
        public decimal TotalSale { get; set; }
        public decimal Profit { get; set; }
    }

    public class HODashboardVM
    {
        public decimal TotalSale { get; set; }
        public decimal TotalProfit { get; set; }
        public List<HOBranchDashboardVM> Branches { get; set; }
    }

}
