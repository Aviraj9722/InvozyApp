namespace eOrderTouchApp.Models.ReportsModel
{
    public class TransactionReportModel
    {
        public String TransactionDate { get; set; }
        public string AccountName { get; set; }
        public string PaymentMode { get; set; }
        public char TypeOfTransaction { get; set; }
        public decimal Amount { get; set; }
        public string Narration { get; set; }
    }
}