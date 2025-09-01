namespace RM.Model.Models
{
    public class CallPerformancemSpResult
    {
        public string Symbol { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? CurrentMarketValue { get; set; }
        public decimal? Roi { get; set; }
        public decimal? EntryPrice { get; set; }
        public string Status { get; set; }
        public int Duration { get; set; }
        public string Cmp { get; set; }
    }
}
