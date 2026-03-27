namespace eOrderTouchApp.Models
{
    public class TblDealerLicenseTransaction
    {
        public int Id { get; set; }

        public int? DealerId { get; set; }

        public int? PurchaseQty { get; set; }

        public decimal? TotalPrice { get; set; }

        public bool? PaymentReceived { get; set; }

        public DateTime? CreatedOn { get; set; }

        // Navigation property
        public virtual TblDealer Dealer { get; set; }
    }
}
