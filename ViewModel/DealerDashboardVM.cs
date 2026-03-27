namespace eOrderTouchApp.ViewModel
{
    public class DealerDashboardVM
    {
        public int TotalPurchased { get; set; }
        public int SoldLicenses { get; set; }
        public int RemainingLicenses { get; set; }
        public int ActiveLicenses { get; set; }
        public int ExpiringSoon { get; set; }
        public int ExpiredLicenses { get; set; }

        public List<DealerCustomerVM> Customers { get; set; } = new();
        public List<DealerCustomerVM> ExpiringSoonCustomers { get; set; } = new();
    }

    public class DealerCustomerVM
    {
        public int BusinessId { get; set; }
        public string CustomerName { get; set; }
        public string OwnerName { get; set; }
        public string Mobile { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; }
        
    }
}
