namespace eOrderTouchApp.Models.ReportsModel
{
    public class DateWiseSaleProfitReport
    {
        public DateTime? SaleDate { get; set; }
        public decimal? TotalSale { get; set; }
        public decimal? TotalCost { get; set; }
        public decimal? Profit { get; set; }
    }

}
