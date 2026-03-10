namespace eOrderTouchApp.Models.ReportsModel
{
    public class DateWiseSaleDiscountModel
    {
        public DateTime DateOfOrder { get; set; }
        public decimal Total { get; set; }
        public decimal Discount { get; set; }
        public decimal GrandTotal { get; set; }
    }
}
