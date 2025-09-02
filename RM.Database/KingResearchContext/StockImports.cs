using System;

namespace RM.Database.ResearchMantraContext
{
    public class StockImports
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public string TradingSymbol { get; set; }
        public string StrikePrice { get; set; }
        public string LotSize { get; set; }
        public string Token { get; set; }
        public string Exch { get; set; }
        public string OptionType { get; set; }
        public DateTime ContractDate { get; set; }
        public string Month { get; set; }
    }
}
