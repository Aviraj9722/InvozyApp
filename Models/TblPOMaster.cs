using System.ComponentModel.DataAnnotations.Schema;

namespace eOrderTouchApp.Models
{
    [Table("tblPOMaster")]
    public class TblPOMaster
    {
        public int Id { get; set; }
        public int? BusinessId { get; set; }
        public DateTime? DateOfPurchase { get; set; }
        public int? VendorId { get; set; }
        public decimal? GrandTotal { get; set; }

        public string? InvoiceNo { get; set; }

        public TblBusiness? Business { get; set; }
        public TblVendor? Vendor { get; set; }

        public ICollection<TblPODetails>? PODetails { get; set; }
    }
}
