//using RM.API.Dtos;
//using System;
//using System.Collections.Generic;

//namespace RM.API.Models
//{
//    public partial class CallPerformance
//    {
//        public int Id { get; set; }
//        public string TradeType { get; set; }
//        public DateTime? CallDate { get; set; }
//        public string StrategyKey { get; set; }
//        public string CallServiceKey { get; set; }
//        public string IsIntraday { get; set; }
//        public string StockKey { get; set; }
//        public int? LotSize { get; set; }
//        public string SegmentKey { get; set; }
//        public string ExpiryKey { get; set; }
//        public string OptionValue { get; set; }
//        public decimal? EntryPrice { get; set; }
//        public decimal? StopLossPrice { get; set; }
//        public decimal? Target1Price { get; set; }
//        public decimal? Target2Price { get; set; }
//        public decimal? Target3Price { get; set; }
//        public decimal? ExpectedReturn { get; set; }
//        public string CallStatus { get; set; }
//        public decimal? ExitPrice { get; set; }
//        public decimal? ResultHigh { get; set; }
//        public string ResultTypeKey { get; set; }
//        public decimal? ActualReturn { get; set; }
//        public decimal? NetResult { get; set; }
//        public string Remarks { get; set; }
//        public string CallByKey { get; set; }
//        public byte? IsPublic { get; set; }
//        public byte? IsDisabled { get; set; }
//        public byte? IsDelete { get; set; }
//        public Guid? PublicKey { get; set; }
//        public DateTime? CreatedOn { get; set; }
//        public string CreatedBy { get; set; }
//        public DateTime? ModifiedOn { get; set; }
//        public string ModifiedBy { get; set; }
//    }

using RM.API.Dtos;
using System.Collections.Generic;
public class CallPerformanceReportResponse
{
    public List<CallPerformanceDto> PerformanceReport { get; set; }
    public decimal TotalM2MWithHighPrice { get; set; }
    public decimal TotalM2MWithExitPrice { get; set; }
}
