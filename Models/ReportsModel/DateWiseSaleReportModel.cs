namespace eOrderTouchApp.Models.ReportsModel
{
    public class DateWiseSaleReportModel
    {
        public DateTime? DateOfOrder { get; set; }
        public int? TotalOrders { get; set; }
        public decimal? Cash { get; set; }
        public decimal? Online { get; set; }
        public decimal? Credit { get; set; }
        public decimal? TotalSale { get; set; }

        
    }
}
