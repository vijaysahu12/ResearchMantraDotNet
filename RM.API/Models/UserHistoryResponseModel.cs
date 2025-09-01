using System;

namespace RM.API.Models
{
    public class UserHistoryResponseModel
    {
        public string? FullName { get; set; }
        public string? EmailId { get; set; }
        public string? Mobile { get; set; }
        public Guid PublicKey { get; set; }
        public string? FirebaseFcmToken { get; set; }
        public string? DeviceType { get; set; }
        public bool? IsActive { get; set; }
        public string? Gender { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool? CanCommunityPost { get; set; }
        public int? BucketCount { get; set; }
        public int? PurchaseCount { get; set; }
        public string? BucketData { get; set; }
        public string? PurchaseData { get; set; }
        public string? FreeTrailData { get; set; }
        public string? FreeTrailStatus { get; set; }
        public string? TicketOpenStatus { get; set; }
        public string? DeviceVersion { get; set; }
    }

}
