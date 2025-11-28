using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace eOrderTouchApp.Models;
[Table("tblEnquiry")]
public partial class TblEnquiry
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? EmailId { get; set; }

    public string? MobileNo { get; set; }

    public string? Comments { get; set; }

    public string? Status { get; set; }

    public string? FollowUpOne { get; set; }

    public string? FollowUpTwo { get; set; }

    public string? FollowUpThree { get; set; }

    public string? FollowUpFour { get; set; }

    public string? BusinessType { get; set; }

    public int? NoOfTables { get; set; }
}
