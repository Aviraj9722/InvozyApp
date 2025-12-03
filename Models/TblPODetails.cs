using System.ComponentModel.DataAnnotations.Schema;

namespace eOrderTouchApp.Models
{
    [Table("tblPODetails")]
    public class TblPODetails
    {
        public int Id { get; set; }
        public int? POMasterId { get; set; }
        public int? ProductId { get; set; }
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal? Total { get; set; }

        public TblPOMaster? POMaster { get; set; }
        public TblProduct? Product { get; set; }
    }
}
