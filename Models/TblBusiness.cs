using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace eOrderTouchApp.Models;
[Table("tblBusinesses")]
public partial class TblBusiness
{
    public int Id { get; set; }

    public string? BusinessName { get; set; }

    public int? BusinessTypeId { get; set; }

    public string? OwnerName { get; set; }

    public string? Gstin { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Logo { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? PrinterSizeId { get; set; }

    public bool? HideCustomerField { get; set; }

    public bool? HideTableDropDown { get; set; }

    public bool? IsGstapplicable { get; set; }

    public string? Qrcode { get; set; }


    public string? MobileNo { get; set; }
    public virtual TblBusinessType? BusinessType { get; set; }

    public virtual TblPrinterSize? PrinterSize { get; set; }

    public virtual ICollection<TblCategory> TblCategories { get; set; } = new List<TblCategory>();

    public virtual ICollection<TblFeedback> TblFeedbacks { get; set; } = new List<TblFeedback>();

    public virtual ICollection<TblOrderMaster> TblOrderMasters { get; set; } = new List<TblOrderMaster>();

    public virtual ICollection<TblProduct> TblProducts { get; set; } = new List<TblProduct>();

    public virtual ICollection<TblPurchaseOrderStock> TblPurchaseOrderStocks { get; set; } = new List<TblPurchaseOrderStock>();

    public virtual ICollection<TblUom> TblUoms { get; set; } = new List<TblUom>();

    public virtual ICollection<TblUser> TblUsers { get; set; } = new List<TblUser>();

    public ICollection<TblPOMaster>? POMasters { get; set; }
}
