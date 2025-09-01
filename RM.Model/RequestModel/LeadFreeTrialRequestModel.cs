using System;

namespace RM.Model.RequestModel
{
    public class LeadFreeTrialRequestModel
    {

        public int Id { get; set; }
        public Guid LeadKey { get; set; }
        public Guid ServiceKey { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public Guid? ModifiedBy { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }

        public string Reason { get; set; }

    }
}
