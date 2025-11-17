using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace eOrderTouchApp.Models;
[Table("tblCategories")]
public partial class TblCategory
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? BusinessId { get; set; }

    public virtual TblBusiness? Business { get; set; }

    public virtual ICollection<TblProduct> TblProducts { get; set; } = new List<TblProduct>();
}
