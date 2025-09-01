using System.Collections.Generic;

namespace RM.API.Models.Reports
{
    public class PartnerSummaryReportRootResponse
    {
        public List<PartnerSummaryReportResponse> responseData { get; set; }
        public int BenchMark { get; set; }
        public string MinValue { get; set; }
        public string MaxValue { get; set; }
        public string Labels { get; set; }
    }
    public class PartnerSummaryReportResponse
    {
        public string Date { get; set; }
        public int TotalRegistration { get; set; }
        public int TotalConversion { get; set; }
        public int TotalModification { get; set; }
    }
}