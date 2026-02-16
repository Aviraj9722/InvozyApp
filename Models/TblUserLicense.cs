using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace eOrderTouchApp.Models;
[Table("tblUserLicense")]
public partial class TblUserLicense
{
    public int Id { get; set; } = 0;

    public int? BusinessId { get; set; }

    public string? LicenseKey { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? CreatedOn { get; set; }
 
}
