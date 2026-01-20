namespace eOrderTouchApp.Models.ReportsModel
{
    public class DateWiseSaleDiscountModel
    {
        public DateTime DateOfOrder { get; set; }
        public decimal TotalSale { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal NetSale { get; set; }
    }
}
