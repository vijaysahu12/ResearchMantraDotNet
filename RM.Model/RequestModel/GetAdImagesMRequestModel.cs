using System;
using System.Collections.Generic;

namespace RM.Model.RequestModel
{
    public class GetAdImagesMRequestModel
    {
        public string SearchText { get; set; }
        public string Type { get; set; }
    }
    public class GetPmImagesMRequestModel
    {
        public string SearchText { get; set; }

    }
    public class GetPromotionResponseModel
    {
        public int Id { get; set; }
        public List<PromoMediaModel> mediaItems { get; set; } = new(); // ✅ image + buttons
       // public List<PromoButtonModel> ListOfButtons { get; set; } = new();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? MediaType { get; set; }
        public bool? ShouldDisplay { get; set; }
        public int? MaxDisplayCount { get; set; }
        public int? DisplayFrequency { get; set; }
        public string? LastShownAt { get; set; } 
        public bool? GlobalButtonAction { get; set; }
        public string? Target { get; set; } 
        public string? ProductName { get; set; }
        public int? ProductId { get; set; }
    }
    public class GetPromotionResponse
    {
        public int Id { get; set; }
        public List<PromoMediaModel> mediaItems { get; set; } = new();
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? MediaType { get; set; }
        public bool? ShouldDisplay { get; set; }
        public int? MaxDisplayCount { get; set; }
        public int? DisplayFrequency { get; set; }
        public string? LastShownAt { get; set; } = "";
        public bool? GlobalButtonAction { get; set; }
        public string? Target { get; set; }
        public string? ProductName { get; set; }
        public int? ProductId { get; set; }
        public bool? IsNotification { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? CreatedBy { get; set; } = "";
        public string? ModifiedBy { get; set; } = "";
        public bool? IsActive { get; set; }
    }


    public class PromoButtonModel
    {
       public string? Target { get; set; }
        public string ButtonName { get; set; }
        public string ActionUrl { get; set; }
        public string? ProductName { get; set; }
        public int? ProductId { get; set; }
    }
    

    public class RequestPopUpModel
    {
       public  string? fcmToken { get; set; }
        public string? deviceType { get; set; }
        public string? version { get; set; }
        public List<int>? PromoIds { get; set; }

    }
}
