namespace RM.CommonServices.Services
{
    public class PaymentResponseCodeMessage
    {
        public static readonly Dictionary<string, string> StatusMessages = new Dictionary<string, string>
        {
            { "SUCCESS", "The transaction was successful." },
            { "PENDING", "The transaction was initiated but not successfull yet." },
            { "FAILED", "The transaction has been failed from payment gateway." },
            { "NOT_FOUND", "Record not found, The transaction was never iniated from our app." },
            { "AUTHENTICATION_FAILED", "Authentication failed. Please try again." },
            { "CARD_NOT_ALLOWED", "The card is not allowed for this transaction." },
            { "PG_ERROR", "There was an error with the payment gateway." },
            { "PG_BACKBONE_ERROR", "The payment gateway backbone encountered an error." },
            { "TXN_AUTO_FAILED", "The transaction was automatically marked as failed." },
            { "XB", "Status code XB." },
            { "Z9", "Status code Z9." }
        };

        public static string GetPaymentResponseMessage(string code)
        {
            return StatusMessages.TryGetValue(code, out var message) ? message : "Unknown status code.";
        }
    }
}
