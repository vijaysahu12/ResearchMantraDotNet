using System;

namespace RM.API.Models
{
    public class MobileUserResponse
    {
        public long Id { get; set; }
        public Guid? PublicKey { get; set; }
        public string MobileNumber { get; set; }
        public string FullName { get; set; }
        public char? Gender { get; set; }
        public string Pincode { get; set; }
        public string ProfileImage { get; set; }
        public string IMEI { get; set; }
        public string EmailId { get; set; }
        public string RoleKey { get; set; }
    }
}
