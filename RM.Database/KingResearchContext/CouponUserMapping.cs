using System;

namespace RM.Database.KingResearchContext
{
    public class CouponUserMappingM
    {
        public int Id { get; set; }
        public int CouponId { get; set; }
        public Guid? MobileUserKey { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
    }
}
