namespace eOrderTouchApp.Models.ReportsModel
{
    public class DateWiseSaleProfitReportModel
    {
        public DateTime DateOfOrder { get; set; }
        public decimal TotalSale { get; set; }
        public decimal TotalCost { get; set; }
        public decimal Profit { get; set; }
    }
}
