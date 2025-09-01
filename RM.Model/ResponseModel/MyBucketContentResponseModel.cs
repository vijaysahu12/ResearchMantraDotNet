using System;

namespace RM.Model.ResponseModel
{
    public class MyBucketContentResponseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Startdate { get; set; }
        public DateTime? Enddate { get; set; }
        public string? ListImage { get; set; }
        public int? DaysToGo { get; set; }
        public bool ShowReminder { get; set; }
        public string CategoryName { get; set; }
        public string? BuyButtonText { get; set; }
        public decimal Price { get; set; }
        public bool IsHeart { get; set; }
        public bool? NotificationEnabled { get; set; }

    }

    public class MyBucketExpiredServiceModel
    {
        public string MyBucketIds { get; set; }
        public Guid MobileUserKey { get; set; }
        public string ProductNames { get; set; }
        public string FirebaseFcmToken { get; set; }
        public string Status { get; set; }
    }



}
