using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eOrderTouchApp.Models
{
    [Table("TblKOTDetail")]
    public class TblKOTDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Qty { get; set; }

        [Required]
        [StringLength(20)]
        public string KotType { get; set; } = string.Empty;  // New, Update, Cancel

        public DateTime CreatedAt { get; set; }

        public int? CreatedBy { get; set; }

        [ForeignKey("OrderId")]
        public virtual TblOrderMaster? Order { get; set; }
        
    }
}
