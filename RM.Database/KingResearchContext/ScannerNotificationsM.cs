using System;

namespace RM.Database.ResearchMantraContext
{
    public class ScannerPerformanceM
    {
        public int ID { get; set; }
        public decimal? Ltp { get; set; }
        public string Message { get; set; }
        public decimal? NetChange { get; set; }
        public decimal? Cmp { get; set; }
        public decimal? PercentChange { get; set; }
        public string TradingSymbol { get; set; }
        public string ViewChart { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime SentAt { get; set; }
        public string Topic { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDelete { get; set; }
    }
}
