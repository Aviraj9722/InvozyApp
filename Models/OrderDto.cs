namespace eOrderTouchApp.Models
{
    public class OrderDto
    {
        public string CustomerName { get; set; }
        public string tableDetail { get; set; }
        public string paymentMode { get; set; }

        public List<OrderItemDto> Items { get; set; }
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
    }
}
