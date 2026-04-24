namespace eOrderTouchApp.Models.ReportsModel
{
    public class AccountWiseBalanceReportModel
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public decimal Credit { get; set; }
        public decimal Debit { get; set; }
        public decimal Balance { get; set; }
    }
}