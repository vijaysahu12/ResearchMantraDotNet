using System;

namespace RM.Model.ResponseModel
{
    public class CouponResponseModel
    {
        public int CouponId { get; set; }
        public Guid PublicKey { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int? DiscountInPercentage { get; set; }
        public decimal? DiscountInPrice { get; set; }
        public int TotalRedeems { get; set; }
        public int? RedeemLimit { get; set; }
        public int? ProductValidityInDays { get; set; }
        public bool IsActive { get; set; }
        public bool IsVisible { get; set; }
        public bool IsDelete { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ProductIds { get; set; }
        public string MobileNumbers { get; set; }
    }
}