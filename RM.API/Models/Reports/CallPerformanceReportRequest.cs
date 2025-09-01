using System;

namespace RM.API.Models.Reports
{
    public class CallPerformanceReportRequest
    {
        public string? ReportType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? StrategyKey { get; set; }

        public string? CallBy { get; set; }
    }
}
