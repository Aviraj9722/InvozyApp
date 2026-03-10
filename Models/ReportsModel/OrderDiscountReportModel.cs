namespace eOrderTouchApp.Models.ReportsModel
{
    public class OrderDiscountReportModel
    {
        public int? OrderId { get; set; }
        public string? DateOfOrder { get; set; }
        public decimal? Total { get; set; }
        public double? Discount { get; set; }
        public double? GrandTotal { get; set; }
    }
}
