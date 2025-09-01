using System;

namespace RM.API.Models.PushNotification
{
    public class PushNotificationVM
    {
        public string AppId { get; set; }
        public string ApiKey { get; set; }
        public string ApiUrl { get; set; }
    }
    public class PushNotificationModel : PushNotificationVM
    {
        public string Headings { get; set; }
        public string Contents { get; set; }
        public string Included_Segments { get; set; }
        public string Url { get; set; }
    }


    public class PushNotificationSpResponseModel
    {
        public long Id { get; set; }
        public Guid UserId { get; set; }
        public string Message { get; set; }
        public DateTime? ReadDate { get; set; }
        public bool? IsRead { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsImportant { get; set; }
    }
}
