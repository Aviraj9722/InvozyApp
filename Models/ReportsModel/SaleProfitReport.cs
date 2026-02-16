namespace eOrderTouchApp.Models.ReportsModel
{
    public class SaleProfitReport
    {
        public string? DateOfOrder { get; set; }
        public decimal? TotalSale { get; set; }
        public decimal? TotalCost { get; set; }
        public decimal? Profit { get; set; }
    }
}
