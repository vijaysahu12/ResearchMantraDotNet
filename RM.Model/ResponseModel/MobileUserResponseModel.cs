using System;

namespace RM.Model.ResponseModel
{
    public class MobileUserResponseModel
    {
        public long Id { get; set; }
        public string FullName { get; set; }
        public string EmailId { get; set; }
        public string Mobile { get; set; }
        public string FirebaseFcmToken { get; set; }
        public Guid PublicKey { get; set; }
        public string DeviceType { get; set; }
        public string DeviceVersion { get; set; }
        public bool? IsActive { get; set; }
        public string Gender { get; set; }
        public bool? CanCommunityPost { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? BucketCount { get; set; }
        public string? BucketData { get; set; }
    }
}