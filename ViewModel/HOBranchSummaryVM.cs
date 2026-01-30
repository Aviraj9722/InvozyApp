namespace eOrderTouchApp.ViewModel
{
    public class HOBranchSummaryVM
    {
        public int BusinessId { get; set; }
        public string BranchName { get; set; }
        public string Location { get; set; }

        public decimal TodaySale { get; set; }
        public decimal TodayProfit { get; set; }

        public decimal FYSale { get; set; }
        public decimal FYProfit { get; set; }
    }
}
