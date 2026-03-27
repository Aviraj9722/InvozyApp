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

    public bool? IsReceiptReprint { get; set; }

    public string? Qrcode { get; set; }

    public bool? IsKOTEnabled { get; set; }
    public string? MobileNo { get; set; }
    public virtual TblBusinessType? BusinessType { get; set; }

    public virtual TblPrinterSize? PrinterSize { get; set; }

    public virtual ICollection<TblCategory> TblCategories { get; set; } = new List<TblCategory>();

    public virtual ICollection<TblFeedback> TblFeedbacks { get; set; } = new List<TblFeedback>();

    public virtual ICollection<TblOrderMaster> TblOrderMasters { get; set; } = new List<TblOrderMaster>();

    public virtual ICollection<TblProduct> TblProducts { get; set; } = new List<TblProduct>();

    public virtual ICollection<TblUom> TblUoms { get; set; } = new List<TblUom>();

    public virtual ICollection<TblUser> TblUsers { get; set; } = new List<TblUser>(); 

    public ICollection<TblPOMaster>? POMasters { get; set; }
    public ICollection<TblVendor>? Vendors { get; set; }
    public bool? IsCustomerMandetory { get;  set; }
    public bool? BarcodeEnabled { get;  set; }
    public bool? IsMultilengual { get;  set; }
    public string? KichenPrinterName { get;  set; }
    public string? CounterPrinterName { get;  set; }
    public bool? IsTableNoRequired { get; set; }
    public string? DiscountType { get; set; }
    public string? ReportData { get; set; }

    public virtual ICollection<TblGST> TblGsts { get; set; } = new List<TblGST>();
   
    public virtual ICollection<TblTable> TblTables { get; set; } = new List<TblTable>();
    public virtual ICollection<TblKitchenCounter> TblKitchenCounters { get; set; }
      = new List<TblKitchenCounter>();
}
