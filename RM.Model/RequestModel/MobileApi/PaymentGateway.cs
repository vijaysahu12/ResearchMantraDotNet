using System;
using System.ComponentModel.DataAnnotations;

namespace RM.Model.RequestModel.MobileApi
{
    class PaymentGateway
    {
    }


    public class PurchaseOrderData
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int ProductValidity { get; set; }
        public string? BonusProduct { get; set; }
        public int BonusProductValidity { get; set; }
        public string? Community { get; set; }
        public string? ProductCategory { get; set; }
    }

    public class PhoneNumberModel
    {
        [Required(ErrorMessage = "Phone number is required.")]
        public string? PhoneNumber { get; set; } // The phone number to validate

        [Required(ErrorMessage = "Country code is required.")]
        public string? CountryCode { get; set; }  // The country code, such as "US" for the United States or "IN" for India
    }
}
