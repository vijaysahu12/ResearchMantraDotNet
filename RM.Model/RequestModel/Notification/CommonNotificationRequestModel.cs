using System;

namespace RM.Model.RequestModel.Notification
{
    public class MobileUserDto
    {
        public string FirebaseFcmToken { get; set; }
        public Guid MobileUserKey { get; set; }
    }

    public class MyBucketWithFcmTokenModel
    {
        public string FirebaseFcmToken { get; set; }
        public Guid MobileUserKey { get; set; }
        public string Topic { get; set; }
        public string ProductName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}