using System;
namespace RM.Model.ResponseModel
{
    public class GetMobileProductsSpResponseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string? CategoryCode { get; set; }
        public string? Description { get; set; }
        public string? DescriptionTitle { get; set; }
        public string? ListImage { get; set; }
        public string? LandscapeImage { get; set; }
        public decimal Price { get; set; }
        public DateTime? LastModified { get; set; }
        public decimal? DiscountAmount { get; set; }
        public int? DiscountPercent { get; set; }
        public int CategoryId { get; set; }
        public string Category { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ContentCount { get; set; }
        public int? SubscriptionId { get; set; }
        public string? ImageUrl { get; set; }
        public string? DistinctAttachmentTypes { get; set; }
        public bool CanPost { get; set; }
    }
}
