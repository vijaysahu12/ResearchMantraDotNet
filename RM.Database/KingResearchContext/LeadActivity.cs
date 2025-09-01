using System;

namespace RM.Database.KingResearchContext
{
    public partial class LeadActivity
    {
        public long Id { get; set; }
        public Guid LeadKey { get; set; }
        public int ActivityType { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
        public Guid? Source { get; set; }
        public Guid? Destination { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
