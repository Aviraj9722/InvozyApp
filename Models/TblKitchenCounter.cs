using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eOrderTouchApp.Models
{
    [Table("tblKitchenCounter")]
    public class TblKitchenCounter
    {
        public int Id { get; set; }    
        public string? Name { get; set; }
        public int? BusinessId { get; set; }
        public virtual TblBusiness? Business { get; set; }
    }
}
