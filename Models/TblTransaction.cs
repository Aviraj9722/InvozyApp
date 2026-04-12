namespace eOrderTouchApp.Models
{
    public class TblTransaction
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMode { get; set; }
        public string Narration { get; set; }
        public char TypeOfTransaction { get; set; } // C / D
        public DateTime CreatedOn { get; set; }
        public DateTime TransactionDate { get; set; }
        public string BillNo { get; set; }
        public bool IsRefund { get; set; }

        public TblLedgerAccount Account { get; set; }
    }
}
