using MongoDB.Bson.Serialization.IdGenerators;
using System;

namespace RM.Model.Common
{
    public class QueryValues
    {
        public int Id { get; set; }
        public string PrimaryKey { get; set; }
        public string SecondaryKey { get; set; }
        public string ThirdKey { get; set; }
        public string FourthKey { get; set; }
        public string FifthKey { get; set; }
        public string SixthKey { get; set; }
        public int POStatus { get; set; }
        public string SearchText { get; set; }
        public int IsPaging { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public string SortOrder { get; set; }
        public string SortExpression { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int IsAdmin { get; set; }
        public string RequestedBy { get; set; }
        public string AssignedTo { get; set; }
        public string RoleKey { get; set; }
        public string LoggedInUser { get; set; }
        public string DurationName{ get; set; }
        public string PlanName   { get; set; }
        public string DeviceVersion { get; set; }
        public int? SubscriptionPlanId { get; set; }
        public int? SubscriptionDurationId { get; set; }
        public int? PostTypeId { get; set; }
        public int? DaysToGo { get; set; }
        public int? ProductId { get; set; }
        public string? ThreeMonthPerformaceChartPeriodType { get; set; } // "months", "quarters", "years"

    }
    public class PhonePePaymentReportChartResponceModel
    {
        public int? ProductId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string DurationType { get; set; }
    }

    public class AdvisoryUpdateRequestModel
    {
        public string PublicKey { get; set; }
        public string Status { get; set; }
        public string AltMobile { get; set; }
        public string ServiceName { get; set; }
    }

    public class MobileDashboardQueryValues
    {
        public string LoggedInUser { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string PrimaryKey { get; set; }
        public string SecondaryKey { get; set; }
        public string ThirdKey { get; set; }
        public string FourthKey { get; set; }
        public string FifthKey { get; set; }
        public string SixthKey { get; set; }
        public string SeventhKey { get; set; }
        public string EigthKey { get; set; }
        public string Ninthkey { get; set; }
        public string TenthKey { get; set; }
        public string EleventhKey { get; set; }
        public string TwelfthKey { get; set; }
    }
}
