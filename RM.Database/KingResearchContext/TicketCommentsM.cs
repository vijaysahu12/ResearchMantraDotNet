using System;

namespace RM.Database.KingResearchContext
{
    public class TicketCommentsM
    {
        public long Id { get; set; }
        public long TicketId { get; set; }
        public string Comment { get; set; }
        public long? CommentMobileUserId { get; set; }
        public long? CommentByCrmId { get; set; }
        public bool IsDelete { get; set; }
        public string Images { get; set; }
       // public string CommentByCrmUserName { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}