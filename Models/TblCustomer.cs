using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eOrderTouchApp.Models
{
    [Table("tblCustomer")]
    public class TblCustomer
    {
        public int Id { get; set; }

        public int? BusinessId { get; set; }

        public string? Name { get; set; }

        public string? Address { get; set; }

        public string? MobileNo { get; set; }

        public string? EmailId { get; set; }

        public string? GSTN { get; set; }

        public string? Location { get; set; }

        public virtual TblBusiness? Business { get; set; }

    }
}
