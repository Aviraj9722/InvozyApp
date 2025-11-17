using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace eOrderTouchApp.Models;
[Table("tblUser")]
public partial class TblUser
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public string? Role { get; set; }

    public string? MobileNumber { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? BussinessId { get; set; }

    public string? EmailId { get; set; }

    public virtual TblBusiness? Bussiness { get; set; }

    public virtual ICollection<TblOrderMaster> TblOrderMasters { get; set; } = new List<TblOrderMaster>();

    public virtual ICollection<TblProduct> TblProducts { get; set; } = new List<TblProduct>();

    public virtual ICollection<TblUserLicense> TblUserLicenses { get; set; } = new List<TblUserLicense>();
}
