using System;

namespace RM.Database.KingResearchContext
{
    public class PaymentResponseStatusM
    {
        public int Id { get; set; }
        public bool? Success { get; set; }
        public string? Code { get; set; }
        public string? Message { get; set; }
        public string? MerchantId { get; set; }
        public string? MerchantTransactionId { get; set; }
        public string? TransactionId { get; set; }
        public decimal? Amount { get; set; }
        public string? State { get; set; }
        public string? ResponseCode { get; set; }
        public string? PaymentInstrumentType { get; set; }
        public string? PaymentInstrumentUtr { get; set; }
        public string? PaymentInstrumentUpiTransactionId { get; set; }
        public string? PaymentInstrumentCardNetwork { get; set; }
        public string? PaymentInstrumentAccountType { get; set; }
        public decimal? FeesContextAmount { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }

    public enum PaymentResponseCode
    {
        SUCCESS,
        AUTHENTICATION_FAILED,
        CARD_NOT_ALLOWED,
        PG_ERROR,
        PG_BACKBONE_ERROR,
        TXN_AUTO_FAILED,
        XB,
        Z9
    }


}
