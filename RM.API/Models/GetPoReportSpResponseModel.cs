using System;
namespace RM.API.Models
{
    public class GetPoReportSpResponseModel
    {
        public string? ClientName { get; set; }
        public string? Mobile { get; set; }
        public string? StatusName { get; set; }
        public string? Remark { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal? PaidAmount { get; set; }
        public string? ModeOfPayment { get; set; }
        public string? TransasctionReference { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? LeadSource { get; set; }
    }
}
