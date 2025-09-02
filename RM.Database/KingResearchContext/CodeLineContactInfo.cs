using System;

namespace RM.Database.ResearchMantraContext
{
    public class CodelineContactInfo
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }
        public string? Reason { get; set; }
        public DateTime? CreatedOn { get; set; }
    }

}
