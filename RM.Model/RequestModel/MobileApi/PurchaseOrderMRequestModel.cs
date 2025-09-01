using System.ComponentModel.DataAnnotations;

namespace RM.Model.RequestModel.MobileApi
{
    public class PurchaseOrderMRequestModel
    {
        [Required]
        public string MobileUserKey { get; set; }
        [Required]
        public int ProductId { get; set; }

        public int? SubscriptionMappingId { get; set; }

        [Required]
        public string TransactionId { get; set; }
        public string MerchantTransactionId { get; set; }
        [Required]
        public double PaidAmount { get; set; }

        public string CouponCode { get; set; }
    }
}