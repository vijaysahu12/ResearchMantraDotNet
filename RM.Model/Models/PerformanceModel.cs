using RM.Model.ResponseModel;
using System.Collections.Generic;

namespace RM.Model.Models
{
    public class PerformanceModel
    {
        public class TradeMetrics
        {

        }

        public class Investment
        {
            public int PurchasePrice { get; set; }
            public int CurrentMarketValue { get; set; }
        }

        public class Trade
        {
            //public long? Id { get; set; }
            public string Symbol { get; set; }
            public string? Roi { get; set; }
            public string? EntryPrice { get; set; }
            public string Status { get; set; }
            public string? Duration { get; set; }
            public int? Cmp { get; set; }
            public string? ExitPrice { get; set; }
            public string? InvestmentMessage { get; set; }
        }

        public class Statistics
        {
            public int TotalTrades { get; set; }
            public int TotalProfitable { get; set; }
            public int TotalLoss { get; set; }
            public int TradeClosed { get; set; }
            public int TradeOpen { get; set; }
        }

        public class PerformanceData
        {
            public string? Balance { get; set; }
            public Statistics Statistics { get; set; }
            public List<Trade> Trades { get; set; }
            public List<TradeResponseModel> TradesResponse { get; set; }
        }
    }
}