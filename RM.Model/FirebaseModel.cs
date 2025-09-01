using System;
using System.Collections.Generic;

namespace RM.Model
{

    public class ChartInkPost
    {
        public string stocks { get; set; }//":"WIPRO,TATAPOWER",
        public string trigger_prices { get; set; }//":"553.5,117.5",
        public string triggered_at { get; set; }//":"9:30 am",
        public string scan_name { get; set; }//":"Copy - Above R3 R4 Volume - Buying - final - 01\/06\/2021 9:00 PM",
        public string scan_url { get; set; }//":"copy-above-r3-r4-volume-buying-final-01-06-2021-9-00-pm-204",
        public string alert_name { get; set; }//":"Top Stocks of the day.",
        public string webhook_url { get; set; }//":"http:\/\/jarvisalgo.in\/JarvisAPI\/ChartInk"

    }

    public class RealTimeTopGainerLooserMarketValues
    {
        public string CurrentDate { get; set; }
        public List<TopGainersLosersData> TopGainers { get; set; }
        public List<TopGainersLosersData> TopLosers { get; set; }
        //public Index Index { get; set; }
    }

    public static class CurrentDateClass
    {
        public static string CurrentDate { get; set; }
    }
    public class TopGainersLosersData
    {
        public double ltp { get; set; }
        public double netChange { get; set; }
        public double percentChange { get; set; }
        public int symbolToken { get; set; }
        public string tradingSymbol { get; set; }
    }
    public class SymbolList
    {
        public string TradingSymbol { get; set; }
        public string Tsym { get; set; }
        public double Price { get; set; }
        public DateTime ContractDate { get; set; }
        public string Token { get; set; }
        public string OptionType { get; set; }
        public string LotSize { get; set; }
        public string Exchange { get; set; }
        public string StrikePrice { get; set; }
        public string Month { get; internal set; }

    }

    public class Index
    {
        public List<string> BNF { get; set; }
        public List<string> Nifty { get; set; }
        public List<string> Sensex { get; set; }
    }

    public class CamrillaR4Model
    {
        public string Ltp { get; set; }
        public string Close { get; set; }
        public string DayHigh { get; set; }
        public double NetChange { get; set; }
        public string PercentChange { get; set; }
        public string SymbolToken { get; set; }
        public string TradingSymbol { get; set; }
        public string ViewChart { get; set; }
    }

    public class CamrillaScanner
    {
        public string CurrentDate { get; set; }
        public List<CamrillaR4Model> Breakfast { get; set; }

    }

    public class ScalpingStrategyStocks
    {
        public string Ltp { get; set; }
        public string Close { get; set; }
        public string DayHigh { get; set; }
        public double NetChange { get; set; }
        public string PercentChange { get; set; }
        public string SymbolToken { get; set; }
        public string TradingSymbol { get; set; }
        public string ViewChart { get; set; }
    }

    public class ScalpingStrategyScanner
    {
        public string CurrentDate { get; set; }
        public List<ScalpingStrategyStocks> Stocks { get; set; }

    }
}
