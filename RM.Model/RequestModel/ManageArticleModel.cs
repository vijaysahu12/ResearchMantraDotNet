using System;
using System.Collections.Generic;

namespace RM.Model.RequestModel
{
    public class ManageArticleModel
    {
        public string Name { get; set; }
        public int CompanyId { get; set; }
        public bool IsPublished { get; set; }
        public bool IsFree { get; set; }
        public int BasketId { get; set; }
        public string Description { get; set; }
        public string ShortSummary { get; set; }
        public decimal MarketCap { get; set; }
        public decimal PE { get; set; }
        public string Symbol { get; set; }
        public string ChartUrl { get; set; }
        public string OtherUrl { get; set; }
        public string WebsiteUrl { get; set; }
        public bool LT_OP_Uptrend { get; set; }
        public bool Future_Visibility { get; set; }
        public bool Futuristic_Sector { get; set; }
        public bool HNI_Institutional_PromotersBuy { get; set; }
        public bool Special_Situations { get; set; }
        public bool ST_OP_UpTrend { get; set; }
        public List<LastYearMonthlyPrice> LastYearMonthlyPrices { get; set; }
        public List<LastTenYearSales> LastTenYearSales { get; set; }
        public List<LastTenYearSales> PromotersHoldingInPercent { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal Shares { get; set; }
        public decimal TtmNetProfit { get; set; }
        public decimal ProfitGrowth { get; set; }
        public decimal FaceValue { get; set; }
        public string RSI { get; set; }
        public string MACD { get; set; }
        public decimal NetWorth { get; set; }
        public decimal PromotersHolding { get; set; }
        public Guid LoggedInUserKey { get; set; }
        public bool? IsActive { get; set; }
        public decimal? YesterdayPrice { get; set; }
    }

    public class LastYearMonthlyPrice
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public string Month { get; set; }
        public string Symbol { get; set; }
    }

    public class YearValue
    {
        public DateTime Year { get; set; }
        public string Value { get; set; }
    }

    public class LastTenYearSales
    {
        public string Metric { get; set; }
        public List<YearValue> Values { get; set; }
    }
}