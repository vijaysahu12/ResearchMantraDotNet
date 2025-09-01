namespace RM.Model.Models
{

    public class PaymentInstrument
    {
        public string Type { get; set; }
        public string Utr { get; set; }
        public string UpiTransactionId { get; set; }
        public string CardNetwork { get; set; }
        public string AccountType { get; set; }
    }

    public class FeesContext
    {
        public decimal Amount { get; set; }
    }

    public class PhonePeData
    {
        public string MerchantId { get; set; }
        public string MerchantTransactionId { get; set; } // This is phonePe MerchantTransactionId which Flutter team generating before the request
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string State { get; set; }
        public string ResponseCode { get; set; }
        public PaymentInstrument PaymentInstrument { get; set; }
        public FeesContext FeesContext { get; set; }
    }
    public class PhonePePaymentConvertedResponseStatusModel
    {
        public bool Success { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public PhonePeData Data { get; set; }
    }

}
