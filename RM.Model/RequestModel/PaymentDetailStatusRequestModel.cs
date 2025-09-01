using System;
using System.ComponentModel.DataAnnotations;

namespace RM.Model.RequestModel
{
    public class PaymentDetailStatusRequestModel
    {
        [Required]
        public int ProductId { get; set; }
        [Required]
        public string MerchantTransactionID { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public string CouponCode { get; set; }
        [Required]
        public int SubcriptionModelId { get; set; }
        public int SubscriptionMappingId { get; set; }
        [Required]
        public Guid MobileUserKey { get; set; }
    }

    public class ProductModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public int ProductValidity { get; set; }
        public string PaymentStatus { get; set; }
    }

}
