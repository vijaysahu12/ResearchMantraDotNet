using System;

namespace RM.API.Models
{
    public class CombinedLogEntry
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public string Category { get; set; }
        public DateTime CreatedOn { get; set; }
        public string EntryType { get; set; }
    }
}