using System;

namespace RM.Model.ResponseModel
{
    public class ScheduledNotificationModel
    {
        public int NotificationId { get; set; }
        public int? ProductId { get; set; }
        public string Title { get; set; }
        public string MobileNumber { get; set; }
        public string Body { get; set; }
        public string Topic { get; set; }
        public string TargetAudience { get; set; }
        public string LandingScreen { get; set; }
        public string Image { get; set; }
        public DateTime ScheduleDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
    }
}
