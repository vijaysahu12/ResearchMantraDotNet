using System;
using System.ComponentModel.DataAnnotations;

namespace RM.Model.RequestModel
{
    public class LogoutRequestModel
    {
        [Required]
        public Guid MobileUserKey { get; set; }
        [Required]
        public string FcmToken { get; set; }
    }

    public class AccountDeleteRequestModel
    {
        [Required]
        public Guid MobileUserKey { get; set; }
        [Required]
        public string Reason { get; set; }
    }
}
