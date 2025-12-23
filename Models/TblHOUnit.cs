using System.ComponentModel.DataAnnotations.Schema;

namespace eOrderTouchApp.Models
{
    [Table("tblHOUnit")]
    public class TblHOUnit
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        public int? BusinessId { get; set; }

        public virtual TblUser? User { get; set; }
        
        public virtual TblBusiness? Business { get; set; }

    }
}
