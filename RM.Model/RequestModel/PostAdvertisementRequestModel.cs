using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace RM.Model.RequestModel
{
    public class PostAdvertisementRequestModel
    {
        public int? ImageId { get; set; }
        public int? ProductId { get; set; }
        public IFormFileCollection ImageList { get; set; }

        public string Type { get; set; }
        public DateTime? ExpireOn { get; set; }
        public string Url { get; set; }
        public string? ProductName { get; set; }
    }
    public class PromotionRequestModel
    {
       
        public int? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? Description { get; set; }
        public List<IFormFile>? MediaFiles { get; set; }
        public List<IFormFile> PdfFiles { get; set; } // All uploaded PDFs (flat list)

        public string? MediaType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? MaxDisplayCount { get; set; }
        public int? DisplayFrequency { get; set; }
        public bool IsActive { get; set; }
        public bool? ShouldDisplay { get; set; }
        public bool? GlobalButtonAction { get; set; }
        public bool? IsNotification { get; set; }
        public DateTime ScheduleDate { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Target { get; set; }
        public string? ListOfButtons { get; set; }
        public Guid UserId { get; set; }
        public string? ActionUrl { get; set; }
        public string? ImageUrlsRaw { get; set; } // <-- New for deserialization
        public string? VideoUrlsRaw { get; set; }
        public List<string>? ImageUrls { get; set; }
        public string? MediaButtonMappings { get; set; }


    }
    public class PromtionButtonModel
    {
        public string ButtonName { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;

        public string? Target { get; set; }         // Optional target screen per button
        public int? ProductId { get; set; }         // Optional product mapping per button
        public string? ProductName { get; set; }

     //   public bool? GlobalButtonAction { get; set; }
      //  public bool? ShouldDisplay { get; set; }

    }
    public class PromoMediaModel
    {
        public string mediaUrl { get; set; } = string.Empty;
        public List<PromoButtonModel> Buttons { get; set; } = new(); // ✅ FIXED

    }
   

    public class MediaButtonMappingModel
    {
        public int MediaIndex { get; set; } // 0-based index
        public List<PromoButtonModel> Buttons { get; set; } = new();
    }
    public class MediaButtonMapping
    {
        public int MediaIndex { get; set; }
        public List<PromoButtonModel> Buttons { get; set; } = new();
    }

}