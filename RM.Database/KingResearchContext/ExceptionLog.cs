using System;

namespace RM.Database.KingResearchContext.Tables
{
    public class ExceptionLog
    {
        public int Id { get; set; }
        public string ExceptionType { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
