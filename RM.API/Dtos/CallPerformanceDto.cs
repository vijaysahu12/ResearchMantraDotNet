using Microsoft.AspNetCore.Http;
using System;

namespace RM.API.Dtos
{
    public class CallPerformanceDto
    {
        public int Id { get; set; }
        public string TradeType { get; set; }
        public DateTime CallDate { get; set; }
        public string? CallByKey { get; set; }
        public string StrategyKey { get; set; }
        public string StockKey { get; set; }
        public string ImageUrl { get; set; }
        public IFormFile Image { get; set; }
        public int? LotSize { get; set; }
        public string? SegmentKey { get; set; }
        public string? ExpiryKey { get; set; }
        public string? OptionValue { get; set; }
        public decimal? EntryPrice { get; set; }
        public decimal? Target1Price { get; set; }
        public decimal? Target2Price { get; set; }
        public decimal? Target3Price { get; set; }
        public decimal? StopLossPrice { get; set; }
        public string EntryTime { get; set; }
        public string TriggerTime { get; set; }
        public decimal? ExitPrice { get; set; }

        //public decimal? ExpectedReturn { get; set; }
        public decimal? ResultHigh { get; set; }
        public decimal? PlottedCapital { get; set; }
        public decimal? Pnl { get; set; }
        public decimal? Roi { get; set; }
        public string? Remarks { get; set; }

        public string? CallStatus { get; set; }
        public bool? IsPublic { get; set; }
        //public decimal? ActualReturn { get; set; }
        //public decimal? NetResult { get; set; }
        public bool? IsIntraday { get; set; }
        public string? ResultTypeKey { get; set; }

        public bool? IsDisabled { get; set; }
        public bool? IsDelete { get; set; }
        public bool IsCallActivate { get; set; }
        public Guid? PublicKey { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }

    }
}
