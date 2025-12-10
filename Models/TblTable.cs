using eOrderTouchApp.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eOrderTouchApp.Models
{
    [Table("tblTable")]
    public class TblTable
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public int? BusinessId { get; set; }

        public virtual TblBusiness? Business { get; set; }
    }
}

