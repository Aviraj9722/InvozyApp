using eOrderTouchApp.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eOrderTouchApp.Models
{
    [Table("tblGST")]
    public class TblGST
    {
        public int Id { get; set; }

        public int? GSTValue { get; set; }

        public string? DisplayName { get; set; }

        public int? BusinessId { get; set; }

        public virtual TblBusiness? Business { get; set; }
    }
}

