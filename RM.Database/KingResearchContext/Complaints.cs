using System;
using RM.Database.KingResearchContext;

namespace RM.Database.KingResearchContext
{
    public class Complaints
    {

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string? Images { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public DateTime CreatedOn { get; set; }

    }
}
