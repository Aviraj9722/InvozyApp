namespace eOrderTouchApp.Models.ReportsModel
{
    public class DailySaleReportModel
    {
        public int? OrderId { get; set; }
        public DateTime? DateOfOrder { get; set; }
        public decimal? TotalQty { get; set; }
        public decimal? TotalSale { get; set; }



    }

}
