using System;
namespace RM.API.Models
{
    public class GetEmployeeWorkStatusSpResponse
    {
        public int Id { get; set; }
        public string EmpName { get; set; }
        public int LeadCount { get; set; }
        public int FollowUpLeads { get; set; }
        public int TotalPurchaseOrders { get; set; }
        public decimal TotalPRPayment { get; set; }
        public decimal ApprovedPr { get; set; }
        public decimal Ltc { get; set; }
        public int RejectedPrCount { get; set; }
        public decimal RejectedPr { get; set; }
        public int PendingPrCount { get; set; }
        public decimal PendingPr { get; set; }
        public int UntouchedLeads { get; set; }
        public DateTime? LastLeadActivityDate { get; set; }
    }
}
