using System;
using System.ComponentModel.DataAnnotations;

namespace RM.API.Models.Mobile
{
    public class MobileRequest
    {
        [Required]
        public string Mobile { get; set; }
        [Required]
        public string RequestType { get; set; }
        public long MobileUserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string OneTimePassword { get; set; }
        public bool IsOtpVerified { get; set; }

        public string MobileToken { get; set; }
        public string DeviceType { get; set; }

        public string IMEI { get; set; }

        public string StockNature { get; set; }

        public bool AgreeToTerms { get; set; }
        public bool SameForWhatsApp { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }

        public string ProfileImage { get; set; }

        public string About { get; internal set; }

    }
}
