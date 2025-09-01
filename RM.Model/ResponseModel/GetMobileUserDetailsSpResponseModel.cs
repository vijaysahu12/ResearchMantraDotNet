using System;

namespace RM.Model.ResponseModel
{
    public class GetMobileUserDetailsSpResponseModel
    {
        public string? Fullname { get; set; }
        public string? ProfileImage { get; set; }
        public string? Gender { get; set; }
        public Guid PublicKey { get; set; }
        public bool HasActiveProduct { get; set; }
    }
}
