namespace RM.Model.RequestModel
{
    public class WhatsAppOrderConfirmationRequest
    {
        public string MobileNumber { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty; 
        public string CountryCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Products { get; set; } = string.Empty;
        public string ValidityInDays { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string ProductValue { get; set; } = string.Empty;
        public string BonusProduct { get; set; } = string.Empty;
        public string BonusProductValidity { get; set; } = string.Empty;
        public string Community { get; set; } = string.Empty;
        public string ProductCategory { get; set; } = string.Empty;
    }
}
