using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace eOrderTouchApp.Models;
[Table("tblBusinessType")]
public partial class TblBusinessType
{
    public int Id { get; set; }

    public string? BusinessTypeName { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual ICollection<TblBusiness> TblBusinesses { get; set; } = new List<TblBusiness>();
}
