using Microsoft.AspNetCore.Http;
using System;

namespace RM.API.Models
{
    public class MobileProductRequestModel
    {

        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string DescriptionTitle { get; set; }
        public decimal Price { get; set; }
        public string? DiscountAmount { get; set; }
        public string? DiscountPercent { get; set; }
        public int Category { get; set; }
        public int? SubscriptionId { get; set; }
        public int Id { get; set; }
        public Guid CreatedBy { get; internal set; }
        public IFormFile ListImage { get; set; }
        public IFormFile LandscapeImage { get; set; }
        public string AttachmentType { get; set; }
        public string VideoUrl { get; set; }
        public bool CanPost { get; set; }

    }
}
