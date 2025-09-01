using System;

namespace RM.Model.ResponseModel
{
    public class PushNotificationResponseModel
    {
        public string ObjectId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public Guid ReceivedBy { get; set; }
        public bool EnableTradingButton { get; set; }
        public string Token { get; set; }
        public string AppCode { get; set; }
        public string Exchange { get; set; }
        public string TradingSymbol { get; set; }
        public string TransactionType { get; set; }
        public int Quantity { get; set; }
        public string OrderType { get; set; }
        public decimal Price { get; set; }
        public string Validity { get; set; }
        public string Product { get; set; }
        public string Complexity { get; set; }
        public int CategoryId { get; set; }
        public bool IsRead { get; set; }
        public bool IsDelete { get; set; }
        public DateTime? ReadDate { get; set; }
        public string Topic { get; set; }
        public DateTime? CreatedOn { get; set; }
    }

    public class UserListForPushNotificationModel
    {
        public string FirebaseFcmToken { get; set; }
        public Guid PublicKey { get; set; }
        public string FullName { get; set; }
        public bool OldDevice { get; set; }
        public bool? Notification { get; set; }
    }

    public class SendReminderToUserPushNotificationModel
    {
        public string FirebaseFcmToken { get; set; }
        public Guid PublicKey { get; set; }
        public string FullName { get; set; }
        public bool OldDevice { get; set; }
        public bool? Notification { get; set; }
        public long? DaysToGo { get; set; }
        public string? ProductName { get; set; }
        public long? ProductId { get; set; }


    }

}
