namespace eOrderTouchApp.Models.ReportsModel
{
    public class CustomerCreditSaleReport
    {
        public int? OrderId { get; set; }
        public string? CustomerName { get; set; }
        public DateTime? DateOfOrder { get; set; }
        public decimal? TotalSale { get; set; }
    }


}
