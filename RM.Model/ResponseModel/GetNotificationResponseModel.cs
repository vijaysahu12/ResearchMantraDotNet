using RM.Model.MongoDbCollection;
using System.Collections.Generic;

namespace RM.Model.ResponseModel
{
    public class GetNotificationResponseModel
    {
        public long UnreadCount { get; set; }
        public List<PushNotification> Notifications { get; set; }
    }
}
