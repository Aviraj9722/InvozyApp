namespace eOrderTouchApp.Models.ReportsModel
{
    public class BankFlowReportModel
    {
        public String TransactionDate { get; set; }
        public string Name { get; set; }
        public string PaymentMode { get; set; }
        public string TypeOfTransaction { get; set; }
        public decimal Amount { get; set; }

        public decimal BankIn { get; set; }
        public decimal BankOut { get; set; }
    }
}