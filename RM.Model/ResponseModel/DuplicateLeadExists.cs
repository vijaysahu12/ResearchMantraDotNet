using System;

namespace RM.Model.ResponseModel
{
    public class DuplicateLeadExists
    {
        public string Result { get; set; }
        public string LeadName { get; set; }

        public string? AssignedTo { get; set; }
        public Guid? PublicKey { get; set; }
        public string MobileNumber { get; set; }
        public string? Remarks { get; set; }
        public string EmailId { get; set; }
        public string ModifiedOn { get; set; }
    }
}
