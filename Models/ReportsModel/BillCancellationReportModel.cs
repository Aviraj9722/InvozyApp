namespace eOrderTouchApp.Models.ReportsModel
{
    public class BillCancellationReportModel
    {
        public int? OrderId { get; set; }
        public string? CustomerName { get; set; }
        public decimal? OrderTotal { get; set; }
        public string? CancelNote { get; set; }
        public string? CancelledByUser { get; set; }
    }
}
