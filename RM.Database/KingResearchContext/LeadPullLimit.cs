using System;

namespace RM.Database.KingResearchContext
{
    public class LeadsPullLimit
    {
        public int Id { get; set; }
        public Guid UserKey { get; set; }
        public DateTime CreatedOn { get; set; }
        public int PullCount { get; set; }

    }
}
