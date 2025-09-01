using System;

namespace RM.Database.KingResearchContext
{
    public class LeadFreeTrial
    {
        public int Id { get; set; }
        public Guid LeadKey { get; set; }
        public Guid ServiceKey { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? ModifiedBy { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

}
