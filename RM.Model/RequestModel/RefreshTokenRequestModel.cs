using System;
using System.ComponentModel.DataAnnotations;

namespace RM.Model.RequestModel
{
    public class RefreshTokenRequestModel
    {
        [Required]
        public string RefreshToken { get; set; }
        [Required]
        public string DeviceType { get; set; }
        public string Version { get; set; }
        [Required]
        public Guid MobileUserKey { get; set; }
    }
}
