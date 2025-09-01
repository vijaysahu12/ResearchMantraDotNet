using System;
using System.ComponentModel.DataAnnotations;

namespace RM.Model.RequestModel
{
    public class UpdateFcmTokenRequestModel
    {
        [Required]
        public Guid MobileUserKey { get; set; }
        [Required]
        public string FcmToken { get; set; }
    }
}
