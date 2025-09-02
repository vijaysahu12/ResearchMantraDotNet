using System;
using System.ComponentModel.DataAnnotations;

namespace RM.Database.ResearchMantraContext
{
    public class PaymentRequestStatusM
    {
        [Key]
        public long Id { get; set; }
        public int ProductId { get; set; }
        public string MerchantTransactionId { get; set; } // This is phonePe MerchantTransactionId which Flutter team generating before the request
        public string Status { get; set; }
        public decimal? Amount { get; set; }
        public int SubcriptionModelId { get; set; }
        public int SubscriptionMappingId { get; set; }
        public string CouponCode { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
