using System.Collections.Generic;

namespace RM.Model.ResponseModel
{
    public class GetCompanyDetailsResponseModel
    {
        public class CompanyModel
        {
            public int CompanyId { get; set; }
            public int BasketId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string ChartUrl { get; set; }
            public string WebsiteUrl { get; set; }
            public string OtherUrl { get; set; }
            public bool LtPopUptrend { get; set; }
            public bool FutureVisibility { get; set; }
            public bool FuturisticSector { get; set; }
            public bool HniInstitutionalPromotersBuy { get; set; }
            public bool SpecialSituations { get; set; }
            public bool StOpUpTrend { get; set; }
            public List<LastTenMonthType> LastYearMonthlyPrices { get; set; }
            public List<LastTenYearSalesType> LastTenYearSales { get; set; }

            public CompanyModel()
            {
                LastYearMonthlyPrices = new List<LastTenMonthType>();
                LastTenYearSales = new List<LastTenYearSalesType>();
            }
        }

        public class LastTenMonthType
        {
            public decimal? Price { get; set; }
            public string Month { get; set; }
            public string Symbol { get; set; }
        }

        public class YearValue
        {
            public int Year { get; set; }
            public decimal? Value { get; set; }
        }

        public class LastTenYearSalesType
        {
            public string Metric { get; set; }
            public List<YearValue> Values { get; set; }

            public LastTenYearSalesType()
            {
                Values = new List<YearValue>();
            }
        }
    }
}
