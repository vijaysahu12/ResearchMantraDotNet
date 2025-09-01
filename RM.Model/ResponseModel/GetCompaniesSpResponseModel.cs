using System;

namespace RM.Model.ResponseModel
{
    public class GetCompaniesSpResponseModel
    {
        public int BasketId { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? GroupName { get; set; }
        public decimal? Price { get; set; }
        public string? BuyButtonText { get; set; }
        public string? ProductDescription { get; set; }
        public decimal? OverallRating { get; set; }
        public int HeartsCount { get; set; }
        public double? UserRating { get; set; }
        public bool? UserHasHeart { get; set; }
        public bool? IsInMyBucket { get; set; }
        public bool? IsInValidity { get; set; }
        public string? CompanyStatus { get; set; }
        public string ListImage { get; set; }
        public bool? IsFree { get; set; }
        public string Name { get; set; }
        public string ChartImageUrl { get; set; }
        public string OtherImage { get; set; }
        public string WebsiteUrl { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? PublishDate { get; set; }
        public decimal? MarketCap { get; set; }
        public string ShortSummary { get; set; }
        public decimal? PE { get; set; }



    }
}
