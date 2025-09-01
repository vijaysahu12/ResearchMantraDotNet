using Microsoft.AspNetCore.Http;
using System;

namespace RM.Model.RequestModel
{
    public class ScheduledNotificationRequestModel
    {
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public long PublicKey { get; set; }
        public string NotificationScreenName { get; set; }
        public string TargetAudience { get; set; }
        public string Body { get; set; }
        public string Topic { get; set; }
        public string Title { get; set; }
        public IFormFile Image { get; set; }
        public string? Mobile { get; set; }
        public bool AllowRepeat { get; set; }
        public DateTime ScheduledTime { get; set; }

    }
}
