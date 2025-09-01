namespace RM.API.Models
{
    public class SalesDashboardDetails
    {
        public int? CurrentTargets { get; set; }
        public decimal? TotalSales { get; set; }
        public decimal? TotalApprovedAmount { get; set; }
        public decimal? TotalPendingAmount { get; set; }
        public decimal? TotalCustomerRevenue { get; set; }
        public int? TotalSalesInCount { get; set; }
        public string? TopFiveDealJson { get; set; }
        public string? TotalSalesPerDayJson { get; set; }
        public string? EveryDayPerformanceJson { get; set; }
        public string? TotalSalesPerPersonJson { get; set; }
        public string? MonthlySalesReportJson { get; set; }
        public string? TotalSalesPerService { get; set; }
        public string? LeadTypes { get; set; }
        public string? LeadStatus { get; set; }
        public int? TotalLeads { get; set; }
        public int? AllocatedLeads { get; set; }
        public int? UnallocatedLeads { get; set; }
        public int? TotalCustomerCount { get; set; }
        public string? MostSalesInCityJson { get; set; }
        public string? MostPaymentMethodJson { get; set; }
        public string? PrCounts { get; set; }
        public string? SalesPerPersonWithPr { get; set; }
        public string? WeeklyPerformanceJson { get; set; }
        public string? ThreeMonthPerfOfLgdInUser { get; set; }
        public int? TotalPerfOfLgdInUser { get; set; }
        public string? RecentFiveEnquires { get; set; }
        public string? MonthlyCustomerDetails { get; set; }
        public int? EarningFromNewLeads { get; set; }
        public string? LeadSoruceEarnings { get; set; }
        public string? ProductRevenueForSalesPerson { get; set; }
        public string? ShowSalesPersonDropDown { get; set; }
        public string? SubscriptionReport { get; set; }
        public string? ActiveUsersReportJson { get; set; }
        public string? LeadsUnderSalesPersonJson { get; set; }
    }
}