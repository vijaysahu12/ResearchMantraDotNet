using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace RM.Model.RequestModel
{
    public class ManageUserDetailsRequestModel
    {
        [Required]
        public string PublicKey { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string EmailId { get; set; }

        [Required]
        public string Mobile { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string Dob { get; set; }
        public IFormFile ProfileImage { get; set; }

    }

    public class MobileOtpVerificationRequestModel
    {

        [Required]
        public string MobileUserKey { get; set; }
        [Required]
        public string FirebaseFcmToken { get; set; }

        [Required]
        public string Otp { get; set; }
        [Required]
        public string DeviceType { get; set; }
        public string Version { get; set; }
    }
}
