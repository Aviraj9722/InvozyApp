namespace eOrderTouchApp.Models.ReportsModel
{
    public class SaleReportModel
    {
        public int? OrderId { get; set; }
        public string? DateOfOrder { get; set; }
        public decimal? TotalItems { get; set; }

        public decimal? Cash { get; set; }
        public decimal? Online { get; set; }
        public decimal? Credit { get; set; }

        public decimal? TotalSale { get; set; }

    }

}
