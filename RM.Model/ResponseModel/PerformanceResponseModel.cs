using System;

namespace RM.Model.ResponseModel
{
    public class PerformanceResponseModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime SentAt { get; set; }
        public string Symbol { get; set; }
        public string TradingSymbol { get; set; }
        public string Cmp { get; set; }
        public string Duration { get; set; }
        public string InvestmentMessage { get; set; }
        public decimal? Roi { get; set; }
        public string Status { get; set; }
        public decimal? EntryPrice { get; set; }
        public decimal? ExitPrice { get; set; }
        public decimal? Ltp { get; set; }
    }
}
