using System;

namespace RM.Database.ResearchMantraContext
{
    public class UserRefreshTokenM
    {
        public int Id { get; set; }
        public Guid MobileUserKey { get; set; }
        public string RefreshToken { get; set; }
        public string DeviceType { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool? IsRevoked { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
