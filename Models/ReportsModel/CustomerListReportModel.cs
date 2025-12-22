using Microsoft.EntityFrameworkCore;

namespace eOrderTouchApp.Models.ReportsModel
{
    [Keyless]
    public class CustomerListReportModel
    {
        public string? CustomerName { get; set; }
        public string? MobileNo { get; set; }
        public string? Location { get; set; }
    }
}
