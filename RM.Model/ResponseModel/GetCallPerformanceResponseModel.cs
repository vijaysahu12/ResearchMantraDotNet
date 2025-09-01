using System;

namespace RM.Model.ResponseModel
{
    public class GetCallPerformanceResponseModel
    {
        public int Id { get; set; }
        public string TradeType { get; set; }
        public DateTime CallDate { get; set; }
        public string StrategyName { get; set; }
        public string StockName { get; set; }
        public int LotSize { get; set; }
        //public string SegmentName { get; set; }
        //public string OptionValue { get; set; }
        public decimal EntryPrice { get; set; }
        public decimal? StopLossPrice { get; set; }
        public decimal Target1Price { get; set; }
        public decimal Target2Price { get; set; }
        public decimal Target3Price { get; set; }
        public string CallStatus { get; set; }
        public decimal ExitPrice { get; set; }
        public decimal ResultHigh { get; set; }
        public decimal Pnl { get; set; }
        public string Raiting { get; set; }
        public string Remarks { get; set; }
        public string CreatedBy { get; set; }
        public bool IsPublic { get; set; }
        public string? ResultTypeKey { get; set; }
        public string StrategyKey { get; set; }
        public bool? IsIntraday { get; set; }
        public string StockKey { get; set; }
        public string? SegmentKey { get; set; }
        public string? ExpiryKey { get; set; }
        public string CallByKey { get; set; }
        public string? ImageUrl { get; set; }
        public Guid PublicKey { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
