using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace eOrderTouchApp.Models;
[Table("tblOrderMaster")]
public partial class TblOrderMaster
{
    public int Id { get; set; }

    public string? CustomerName { get; set; }

    public DateTime? DateOfOrder { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? GrandTotal { get; set; }

    public decimal? Gsttotal { get; set; }

    public string? PaymentMode { get; set; }

    public bool? PaymentStatus { get; set; }

    public bool? Printed { get; set; }

    public int? UserId { get; set; }

    public int? BuisnessId { get; set; }

    public bool? IsCanceled { get; set; }

    public string? CancelNote { get; set; }
    public float? DiscountPercent { get; set; }

    public float? DiscountedPrice { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? CustomerMobNo { get; set; }
    public string? TableDetails { get; set; }

    public virtual TblBusiness? Buisness { get; set; }

    public virtual ICollection<TblOrderDetail> TblOrderDetails { get; set; } = new List<TblOrderDetail>();
    public virtual ICollection<TblKOTDetail> TblKOTDetails { get; set; }
    public virtual TblUser? User { get; set; }
}
