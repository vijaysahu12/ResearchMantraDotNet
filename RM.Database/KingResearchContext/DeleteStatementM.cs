using System;

namespace RM.Database.ResearchMantraContext
{
    public class DeleteStatementM
    {
        public int Id { get; set; }
        public string Statement { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsDelete { get; set; }
    }
}
