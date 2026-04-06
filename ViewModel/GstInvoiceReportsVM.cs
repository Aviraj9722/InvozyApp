namespace eOrderTouchApp.ViewModel
{
    public class GstInvoiceReportsVM
    {
        public string BusinessName { get; set; }
        public string BusinessAddress { get; set; }
        public string BusinessGSTIN { get; set; }
        public string BusinessMobNo { get; set; }
        public string ReportData { get; set; } 
        public string OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public string InvoiceNo { get; set; }

        public string CustomerName { get; set; }
        public string CustomerMobNo { get; set; }

        public List<GstItemVM> Items { get; set; }
        public List<GstTaxGroupingVM> GstGrouping { get; set; }

        public decimal TotalTaxable { get; set; }
        public decimal TotalCGST { get; set; }
        public decimal TotalSGST { get; set; }
        public decimal GrandTax { get; set; }

        // ✅ NEW FIELDS
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal GSTAmount { get; set; }
        public decimal GrandTotal { get; set; }
    }

    public class GstItemVM
    {
        public string ItemName { get; set; }
        public decimal Quantity { get; set; }
        public string UOM { get; set; }
        public decimal Price { get; set; }      
        public decimal GstPercent { get; set; }     
        public decimal TotalAmount { get; set; }
    }

    public class GstTaxGroupingVM
    {
        public decimal GstPercentage { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal TotalTax { get; set; }
    }
}
