using System;
using System.ComponentModel.DataAnnotations;

namespace RM.Model.RequestModel
{
    public class ManageCouponModel
    {
        public string CouponName { get; set; }
        public string Description { get; set; }
        public int? DiscountInPercentage { get; set; }
        public Guid? CouponKey { get; set; }
        public decimal? DiscountInPrice { get; set; }
        public int? RedeemLimit { get; set; }
        public int? ProductValidityInDays { get; set; }
        public Guid CreatedBy { get; set; }
        public string ProductIds { get; set; }
        public string MobileNumbers { get; set; }
        [Required]
        public string Type { get; set; }

        public bool? Override { get; set; }

    }
}
