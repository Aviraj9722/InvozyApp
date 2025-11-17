using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace eOrderTouchApp.Models;
[Table("tblPurchaseOrder_Stock")]
public partial class TblPurchaseOrderStock
{
    public int Id { get; set; }

    public int? BusinessId { get; set; }

    public int? ProductId { get; set; }

    public decimal? Quantity { get; set; }

    public DateTime? DateOfPurchase { get; set; }

    public int? CreatedBy { get; set; }

    public virtual TblBusiness? Business { get; set; }

    public virtual TblProduct? Product { get; set; }
}
