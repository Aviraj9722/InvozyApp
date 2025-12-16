namespace eOrderTouchApp.Models.ReportsModel
{
    public class MaterialSaleReportModel
    {
        public string? MaterialName { get; set; }
        public string? Category { get; set; }
        public string? HSNCode { get; set; }
        public string? Barcode { get; set; }

        public decimal? TotalQty { get; set; }
        public decimal? TotalSale { get; set; }
    }
}
