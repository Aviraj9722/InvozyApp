using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace eOrderTouchApp.Models;
[Table("tblProduct")]
public partial class TblProduct
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? RegionalName { get; set; }

    public string? Code { get; set; }

    public decimal? Gstpercentage { get; set; }

    public decimal? Gstamount { get; set; }

    public decimal? Price { get; set; }

    public int? CategoryId { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? UserId { get; set; }

    public string? Photo { get; set; }

    public int? BusinessId { get; set; }

    public int? UoMid { get; set; }
    public virtual TblUom? Uom{ get; set; }

    public bool? IsActive { get; set; }

    public virtual TblBusiness? Business { get; set; }

    public virtual TblCategory? Category { get; set; }

    public virtual ICollection<TblOrderDetail> TblOrderDetails { get; set; } = new List<TblOrderDetail>();

    public virtual TblUser? User { get; set; }

    public ICollection<TblPODetails>? PODetails { get; set; }

    public string? HSNCode { get; set; }

    public decimal? PurchasePrice { get; set; }
    public int? KitchenCounterId { get; set; }   
    public string? FoodType { get; set; }
    public virtual TblKitchenCounter? KitchenCounter { get; set; }
}
