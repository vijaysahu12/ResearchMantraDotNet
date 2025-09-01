using System.ComponentModel.DataAnnotations;

namespace RM.Model.RequestModel
{
    public class OtpLoginRequestModel
    {
        [Required]
        [RegularExpression("^[0-9]{10}$", ErrorMessage = "Invalid mobile number")]
        public string MobileNumber { get; set; }

        [Required]
        [RegularExpression("^[0-9]{2}$", ErrorMessage = "Invalid mobile number")]
        public string CountryCode { get; set; }
    }
}
