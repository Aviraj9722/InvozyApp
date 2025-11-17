using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace eOrderTouchApp.Models;

[Table("tblUOM")]
public partial class TblUom
{
    public int Id { get; set; }

    public int? BusinessId { get; set; } = 0;

    public string? UnitName { get; set; }

    public virtual TblBusiness? Business { get; set; }
}
