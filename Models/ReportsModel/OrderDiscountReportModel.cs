namespace eOrderTouchApp.Models.ReportsModel
{
    public class OrderDiscountReportModel
    {
        public int? OrderId { get; set; }
        public string? DateOfOrder { get; set; }
        public decimal? TotalOrder { get; set; }
        public double? DiscountPercent { get; set; }
        public double? DiscountPrice { get; set; }
    }
}
