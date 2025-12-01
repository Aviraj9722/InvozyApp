using System.ComponentModel.DataAnnotations.Schema;

namespace eOrderTouchApp.Models
{
    [Table("tblStock")]
    public class TblStock
    {
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public int? VendorId { get; set; }
        public int? Quantity { get; set; }
        public DateTime? DateOfPurchase { get; set; }

        public TblProduct? Product { get; set; }
        public TblVendor? Vendor { get; set; }
    }
}
