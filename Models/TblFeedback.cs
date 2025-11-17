using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace eOrderTouchApp.Models;
[Table("tblFeedback")]

public partial class TblFeedback
{
    public int Id { get; set; }

    public string? CustomerName { get; set; }

    public string? MobileNo { get; set; }

    public string? Feedback { get; set; }

    public int? Ratings { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? BuisnessId { get; set; }

    public virtual TblBusiness? Buisness { get; set; }
}
