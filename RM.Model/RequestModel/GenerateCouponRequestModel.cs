using System;

namespace RM.Model.RequestModel
{
    public class GenerateCouponRequestModel
    {

        public Guid? MobileUserKey { get; set; }

        public int? ProductId { get; set; }

        public string CouponType { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal? DiscountAmount { get; set; }
        public int ValidityInDays { get; set; }
        public long RedeemLimit { get; set; }

    }


}