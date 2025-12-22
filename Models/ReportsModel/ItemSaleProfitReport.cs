namespace eOrderTouchApp.Models.ReportsModel
{
    public class ItemSaleProfitReport
    {
        public string? ItemName { get; set; }
        public decimal? TotalQty { get; set; }
        public decimal? TotalSale { get; set; }
        public decimal? TotalCost { get; set; }
        public decimal? Profit { get; set; }
    }

}
