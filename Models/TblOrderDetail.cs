using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace eOrderTouchApp.Models;
[Table("tblOrderDetails")]
public partial class TblOrderDetail
{
    public int Id { get; set; }

    public int? Oid { get; set; }

    public int? ProductId { get; set; }

    public decimal? Qty { get; set; }

    public decimal? Price { get; set; }

    public decimal? Total { get; set; }

    public decimal? Gstpercentage { get; set; }

    public decimal? Gstamount { get; set; }

    public decimal? SGST { get; set; }
    public decimal? CGST { get; set; }
    public decimal? IGST { get; set; }


    public virtual TblOrderMaster? OidNavigation { get; set; }

    public virtual TblProduct? Product { get; set; }
    public bool? IsKOTPrinted { get; set; }
}
