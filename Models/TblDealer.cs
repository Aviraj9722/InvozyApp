using System.ComponentModel.DataAnnotations.Schema;

namespace eOrderTouchApp.Models
{
    [Table("tblDealer")]
    public class TblDealer
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? MobileNo { get; set; }
        public string? EmailId { get; set; }
        public string? GSTN { get; set; }
        public string? Location { get; set; }
        public string? DealerCode { get; set; }
    }
}
