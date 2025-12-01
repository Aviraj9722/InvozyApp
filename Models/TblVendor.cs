using System.ComponentModel.DataAnnotations.Schema;

namespace eOrderTouchApp.Models
{
    [Table("tblVendor")]
    public class TblVendor
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? EmailId { get; set; }
        public string? MobileNo { get; set; }
        public string? GSTN { get; set; }
        public string? Address { get; set; }
        public string? Location { get; set; }

        public ICollection<TblPOMaster>? POMasters { get; set; }
        public ICollection<TblStock>? Stocks { get; set; }
    }

   
}
