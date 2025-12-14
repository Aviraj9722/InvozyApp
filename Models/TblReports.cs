using System.ComponentModel.DataAnnotations.Schema;

namespace eOrderTouchApp.Models
{
    [Table("tblReports")]
    public class TblReports
    {
        public int Id { get; set; }

        public string? Name { get; set; }
    }
}
