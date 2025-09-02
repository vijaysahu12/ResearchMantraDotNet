using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RM.Database.ResearchMantraContext
{
    public class CompanyReportCommonModel
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
    }
    public class LastOneYearMonthlyPriceM : CompanyReportCommonModel
    {
        public string Symbol { get; set; }
        public string Month { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
    }
    public class LastTenYearSalesM : CompanyReportCommonModel
    {
        public DateTime Year { get; set; }
        public string Sales { get; set; }
        public string OpProfit { get; set; }
        public string NetProfit { get; set; }
        public string OTM { get; set; }
        public string NPM { get; set; }
        public string PromotersPercent { get; set; }
    }

    public class PromotersHoldingInPercentM : CompanyReportCommonModel
    {
        public DateTime Year { get; set; }
        public string Promoters { get; set; }
        public string FIIs { get; set; }
        public string DIIs { get; set; }
        public string Public { get; set; }
        public string ShareHolders { get; set; }

    }
}