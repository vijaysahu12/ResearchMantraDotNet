using System;

namespace RM.Database.KingResearchContext
{
    public class WatiSentMessageLog
    {
        public long Id { get; set; }
        public long WatiId { get; set; }
        public long LeadId { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
