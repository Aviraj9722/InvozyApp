namespace eOrderTouchApp.ViewModel
{
    public class StockPurchaseVM
    {
        public StockPurchaseVM()
        {
            Products = new List<ProductStockLine>();
        }

        public string InvoiceNo { get; set; }
        public int? VendorId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal GrandTotal { get; set; }

        public List<ProductStockLine> Products { get; set; }
    }

    public class ProductStockLine
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int AvailableStock { get; set; }
        public int NewQuantity { get; set; } 
    }
}
