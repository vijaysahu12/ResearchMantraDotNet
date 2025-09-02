using System;

namespace RM.Database.ResearchMantraContext
{
    public class WebSocketConnection
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ConnectionId { get; set; }
        public DateTime CreatedOn { get; set; }

    }
}
