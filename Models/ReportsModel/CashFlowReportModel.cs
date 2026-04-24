namespace eOrderTouchApp.Models.ReportsModel
{
    public class CashFlowReportModel
    {
        public String TransactionDate { get; set; }
        public string Name { get; set; }
        public string PaymentMode { get; set; }
        public string TypeOfTransaction { get; set; }
        public decimal Amount { get; set; }

        public decimal CashIn { get; set; }
        public decimal CashOut { get; set; }
    }
}