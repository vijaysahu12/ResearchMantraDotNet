using System;

namespace RM.Model.ResponseModel
{
    public class GetUserResponseModel
    {
        public string FullName { get; set; }
        public string EmailId { get; set; }
        public string Mobile { get; set; }
        public string Gender { get; set; }
        public string City { get; set; }
        public DateTime? Dob { get; set; }
        public Guid PublicKey { get; set; }
        public string ProfileImage { get; set; }
    }
}
