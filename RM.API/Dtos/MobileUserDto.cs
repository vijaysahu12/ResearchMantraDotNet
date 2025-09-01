using System;
using System.ComponentModel.DataAnnotations;

namespace RM.API.Dtos
{
    public class MobileUserDto
    {
        [Key]
        public long MobileUserId { get; set; }
        [StringLength(36)]
        public Guid? LeadKey { get; set; }

        [StringLength(6)]
        public string OneTimePassword { get; set; }
        public bool IsOtpVerified { get; set; }

        [StringLength(200)]
        public string MobileToken { get; set; }


        [StringLength(10)]
        public string DeviceType { get; set; }

        [StringLength(100)]
        public string IMEI { get; set; }

        [StringLength(50)]
        public string StockNature { get; set; }

        public bool AgreeToTerms { get; set; }
        public bool SameForWhatsApp { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }

        [StringLength(1000)]
        public string ProfileImage { get; set; }
        [StringLength(200)]
        public string About { get; internal set; }
    }
}
