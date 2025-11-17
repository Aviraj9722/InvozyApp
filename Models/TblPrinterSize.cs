using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace eOrderTouchApp.Models;
[Table("tblPrinterSize")]
public partial class TblPrinterSize
{
    public int Id { get; set; }

    public string? PrinterSize { get; set; }

    public virtual ICollection<TblBusiness> TblBusinesses { get; set; } = new List<TblBusiness>();
}
