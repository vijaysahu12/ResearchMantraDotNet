using System;

namespace RM.API.Dtos
{
    public partial class EnquiryDto
    {
        public int Id { get; set; }
        public string Details { get; set; }
        public string ReferenceKey { get; set; }
        public string LeadName { get; set; }
        public string MobileNumber { get; set; }

        public bool Favourite { get; set; }

        public Guid? PublicKey { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }

    }
}
