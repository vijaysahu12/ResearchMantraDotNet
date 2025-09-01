using System;

namespace RM.Database.KingResearchContext
{
    public class FollowUpReminder
    {
        public int Id { get; set; }
        public Guid LeadKey { get; set; }
        public string Comments { get; set; }
        public DateTime NotifyDate { get; set; }
        public DateTime ModifiedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public bool IsActive { get; set; }

    }
}
