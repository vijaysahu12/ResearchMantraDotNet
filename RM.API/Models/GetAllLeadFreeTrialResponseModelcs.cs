using System;

namespace RM.API.Models
{
    public class GetAllLeadFreeTrialResponseModel
    {
        public int Id { get; set; }
        public string LeadName { get; set; }
        public Guid LeadKey { get; set; }
        public string LeadNumber { get; set; }
        public string? LeadEmail { get; set; }
        public Guid ServiceKey { get; set; }
        public string ServiceName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public int Validity { get; set; }
        public int ReasonLogCount { get; set; }
        public string StatusText { get; set; }
    }

}
