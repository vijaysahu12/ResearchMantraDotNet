using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RM.Model.RequestModel.Notification
{
    public class CommonNotificationRequestModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Body { get; set; }

        [Required]
        public string? Topic { get; set; }
    }
    public class NotificationRequestModel : CommonNotificationRequestModel
    {
        public string TradingSymbol { get; set; }
        public string TransactionType { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Exchange { get; set; }
        public string Scanner { get; set; }
        public bool EnableTradingButton { get; set; }
        public string CallType { get; set; }
        public string OrderType { get; set; }
        public string Validity { get; set; }
        public int Product { get; set; }
        public string ProductName { get; set; }
        public string Complexity { get; set; }
        public string Token { get; set; }
        public string AppCode { get; set; }
        public Guid CreatedBy { get; set; }
        public bool TestNotification { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }



    public class SaveNotificationRequestModel : CommonNotificationRequestModel
    {
        public string TradingSymbol { get; set; }
        public string TransactionType { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Exchange { get; set; }
        public string Scanner { get; set; }
        public bool EnableTradingButton { get; set; }
        public string CallType { get; set; }
        public string OrderType { get; set; }
        public string Validity { get; set; }
        public string Product { get; set; }
        public string ProductName { get; set; }
        public string Complexity { get; set; }
        public string Token { get; set; }
        public string AppCode { get; set; }
        public Guid CreatedBy { get; set; }
        public bool TestNotification { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    public class FirebaseNotificationRequestWithToken
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Body { get; set; }
        [Required]
        public string DeviceToken { get; set; }
        [Required]
        public int ProductId { get; set; }
        [Required]
        public string Type { get; set; }
    }
    //public class NotificationReceiverResponseModel
    //{

    //    public string FirebaseFcmToken { get; set; }
    //    public Guid MobileUserKey { get; set; }

    //}
    public class GetNotificationRequestModel
    {
        [Required]
        public int CategoryId { get; set; }
        public bool ShowUnread { get; set; }
        [Required]
        public Guid MobileUserKey { get; set; }
        [Required]
        public int PageSize { get; set; }
        [Required]
        public int PageNumber { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    public class ManageProductNotificationModel
    {
        public bool AllowNotify { get; set; }
        public Guid mobileUserKey { get; set; }
        public int productId { get; set; }
    }

    public class AdvertismentModel : CommonNotificationRequestModel
    {
        public IFormFile CampaignImage { get; set; }
        public string TargetAudience { get; set; }
        public string? RedirectUrl { get; set; }
        public Dictionary<string, string> Data { get; set; }


    }

    public class AdModel
    {
        public IFormFile CampaignImage { get; set; }
        public string TargetAudience { get; set; }
        public string Data { get; set; }
    }





    public class NotificationToMobileRequestModel : CommonNotificationRequestModel
    {
        public string Mobile { get; set; }
        public string ScreenName { get; set; }
        public string NotificationImage { get; set; }
        public string Scanner { get; set; }
    }

    public class NotificationToMobileBlogRequestModel : CommonNotificationRequestModel
    {

        public string Mobile { get; set; }
        public string ScreenName { get; set; }
        public string? NotificationImage { get; set; }

    }

    public class SendWhatsappMessageRequestModel
    {
        public List<string> MobileNumbers { get; set; }
        public string TemplateId { get; set; }
        public string? ContainerMeta { get; set; }
        public List<string> Params { get; set; }
        public string TargetType { get; set; }
        public string TemplateName { get; set; }
        public string? LeadTypeKey { get; set; }
        public string? LeadSourceKey { get; set; }
        public string? filter { get; set; }
        public string? PartnerWith { get; set; }
        public string? Source { get; set; }
        public string? Category { get; set; }
        public string? PoStatus { get; set; }
        public int? StatusType { get; set; }
        public List<string> ServiceKeys { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
    }

    public class SendFreeNotificationRequestModel : CommonNotificationRequestModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Mobile { get; set; }
        public string TargetAudience { get; set; } //TargetAudianceCategory Code
        public bool TurnOnNotification { get; set; }
        public string NotificationScreenName { get; set; }
        public string? ProductName { get; set; }
        public int? ProductId { get; set; }
        public IFormFile? Image { get; set; }
    }

    public class SendFreeWhatsAppNotificationRequestModel : CommonNotificationRequestModel
    {
        public string TargetAudience { get; set; } //TargetAudianceCategory Code
        public string TemplateName { get; set; }
    }

    //public class ScheduledNotification : CommonNotificationRequestModel
    //{
    //    public int Id { get; set; }
    //    [Required]
    //    public string LandingScreen { get; set; }
    //    [Required]
    //    public string TargetAudience { get; set; }
    //    [Required]
    //    public DateTime ScheduledTime { get; set; }
    //    public bool AllowRepeat { get; set; } = false; // Default value of 0
    //    public bool IsSent { get; set; } = false; // Default value of 0
    //    public long CreatedBy { get; set; }
    //    public long ModifiedBy { get; set; }
    //    public DateTime CreatedOn { get; set; }
    //    public DateTime ModifiedOn { get; set; }
    //}
}