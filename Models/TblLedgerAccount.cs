namespace eOrderTouchApp.Models
{
    public class TblLedgerAccount
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; } // Expense, Cash, Bank
        public string Status { get; set; }
        public DateTime CreatedOn { get; set; }

        public ICollection<TblTransaction> Transactions { get; set; }
    }
}
