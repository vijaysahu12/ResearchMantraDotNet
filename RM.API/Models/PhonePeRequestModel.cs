using System;

namespace RM.API.Models
{
    public class PhonePeRequestModel
    {
        public string? FullName { get; set; }
        public string Mobile { get; set; }
        public Guid? PublicKey { get; set; }
        public string ProductName { get; set; }
        public decimal? RequestAmount { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? Status { get; set; }
        public decimal? PaidAmount { get; set; }
        public string Duration { get; set; }
        public string CouponCode { get; set; }
        public string? PaymentInstrumentType { get; set; }
        public string? MerchantTransactionId { get; set; }
        public string? TransactionId { get; set; }
    }
}
