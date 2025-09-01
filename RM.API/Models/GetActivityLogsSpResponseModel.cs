using System;

namespace RM.API.Models
{
    public class GetActivityLogsSpResponseModel
    {
        public long SlNo { get; set; }
        public string LeadName { get; set; }
        public string ActivityType { get; set; }
        public string Message { get; set; }
        public string MobileNumber { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
    }
}
