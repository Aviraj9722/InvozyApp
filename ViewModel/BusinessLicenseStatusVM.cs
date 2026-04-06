namespace eOrderTouchApp.ViewModel
{
    public class BusinessLicenseStatusVM
    {
        public int BusinessId { get; set; }
        public string BusinessName { get; set; }
        public string DealerName { get; set; }
        public string LicenseKey { get; set; }
        public DateTime? LicenseStartDate { get; set; }
        public DateTime? LicenseEndDate { get; set; }
        public string Status { get; set; }

        public string DealerMobNo { get; set; }
    }
}
