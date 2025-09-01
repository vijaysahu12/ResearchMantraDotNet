using System;
using System.ComponentModel.DataAnnotations;

namespace RM.Model.RequestModel
{
    public class ValidateCouponRequestModel
    {

        public Guid? MobileUserKey { get; set; }
        [Required]
        public int ProductId { get; set; }
        [Required]
        public string CouponCode { get; set; }
        public int SubscriptionDurationId { get; set; }

    }

    public class ByPassPaymentGatewayRequestModel : ValidateCouponRequestModel
    {
        [Required] public string ActionType { get; set; } // apply or get
    }

    public class GetPaymentGatewayDetailsModel
    {
        public string Api { get; set; }
        public string CompanyCode { get; set; }
        public string HashKey { get; set; }
        public string PackageName { get; set; }
    }
}
