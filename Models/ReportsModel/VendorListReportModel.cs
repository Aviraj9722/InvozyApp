using Microsoft.EntityFrameworkCore;

namespace eOrderTouchApp.Models.ReportsModel
{
    [Keyless]
    public class VendorListReportModel
    {
        public string? VendorName { get; set; }
        public string? MobileNo { get; set; }
        public string? Location { get; set; }
    }
}
