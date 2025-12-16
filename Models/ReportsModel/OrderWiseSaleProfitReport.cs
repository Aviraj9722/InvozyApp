namespace eOrderTouchApp.Models.ReportsModel
{
    public class OrderWiseSaleProfitReport
    {
        public int? OrderId { get; set; }
        public string? CustomerName { get; set; }
        public decimal? TotalSale { get; set; }
        public decimal? TotalCost { get; set; }
        public decimal? Profit { get; set; }
    }

}
