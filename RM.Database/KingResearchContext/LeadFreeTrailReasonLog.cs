using System;
using System.ComponentModel.DataAnnotations;

namespace RM.Database.ResearchMantraContext
{
    public class LeadFreeTrailReasonLog
    {
        [Key]
        public int LogId { get; set; }
        public int LeadFreeTrialId { get; set; }
        public Guid LeadKey { get; set; }
        public Guid ServiceKey { get; set; }
        public string Reason { get; set; }
        public string ServiceName { get; set; }
        public DateTime FreeTrailStartDate { get; set; }
        public DateTime FreeTrailEndDate { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
