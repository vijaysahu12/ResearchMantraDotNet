using System;

namespace RM.Model
{
    public class ProductsMResponseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? DescriptionTitle { get; set; }
        public string? Category { get; set; }
        public string? GroupName { get; set; }
        public decimal Price { get; set; }
        public string? ListImage { get; set; }
        public int HeartsCount { get; set; }
        public string? BuyButtonText { get; set; }
        public int ThumbsUpCount { get; set; }
        public int ContentCount { get; set; }
        public int VideoCount { get; set; }
        public bool UserHasHeart { get; set; }
        public bool UserHasThumbsUp { get; set; }
        public decimal OverAllRating { get; set; }
        public double UserRating { get; set; }
        public bool Liked { get; set; }
        public bool IsInMyBucket { get; set; }
        public bool IsInValidity { get; set; }
        public string? IsIosCouponEnabled { get; set; }
    }

    public class ProductsByIdMResponseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Code { get; set; }
       public int? CommunityId { get; set; }
        public string? CommunityName { get; set; }
        public string Description { get; set; }
        public string? DescriptionTitle { get; set; }
        public int CategoryId { get; set; }
        public string Category { get; set; }
        public bool AccessToScanner {  get; set; }  
        public decimal Price { get; set; }
        public decimal PaidAmount { get; set; }
        public string? LandscapeImage { get; set; }
        public string? BuyButtonText { get; set; }
        public string Discount { get; set; }
        public string UserRating { get; set; }
        public string Liked { get; set; }
        public string EnableSubscription { get; set; }
        public bool IsHeart { get; set; }
        public bool IsThumbsUp { get; set; }

        public string SubscriptionData { get; set; }
        public string ExtraBenefits { get; set; }
        public bool IsInMyBucket { get; set; }
        public bool IsInValidity { get; set; }
        public bool IsQueryFormEnabled { get; set; }

        public int ContentCount { get; set; }
        public int VideoCount { get; set; }
        public int DaysToGo { get; set; }
        public string? BonusProducts { get; set; }
        public string? ScannerBonusProductId { get; set; }
    }

    public class ContentModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailImage { get; set; }
        public string ListImage { get; set; }
        public string AttachmentType { get; set; }
        public string Attachment { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}