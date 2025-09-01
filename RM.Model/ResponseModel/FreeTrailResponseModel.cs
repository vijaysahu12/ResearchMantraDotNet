using System;

namespace RM.Model.ResponseModel
{
    public class FreeTrailResponseModel
    {
        public string FullName { get; set; }
        public string Mobile { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? ProductNames { get; set; }
        public int? Validity { get; set; }
        public string? Status { get; set; }
        public string? PublicKey { get; set; }
    }
}
