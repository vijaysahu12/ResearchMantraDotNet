using RM.Model.RequestModel.Notification;
using System;

namespace RM.Database.KingResearchContext
{
    public class PushNotificationsM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Type { get; set; }
        public Guid ReceivedBy { get; set; }
        public int CategoryId { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadDate { get; set; }
        public bool IsDelete { get; set; }
        public Guid Createdby { get; set; }
        public DateTime CreatedOn { get; set; }

    }


    public class ScheduledNotification : CommonNotificationRequestModel
    {
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public string LandingScreen { get; set; }
        public string TargetAudience { get; set; }
        public string? MobileNumber { get; set; }
        public string Image { get; set; }
        public bool? IsActive { get; set; }
        public DateTime ScheduledTime { get; set; }
        public DateTime? ScheduledEndTime { get; set; }
        public bool AllowRepeat { get; set; } = false; // Default value of 0
        public bool IsSent { get; set; } = false; // Default value of 0
        public long CreatedBy { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}
