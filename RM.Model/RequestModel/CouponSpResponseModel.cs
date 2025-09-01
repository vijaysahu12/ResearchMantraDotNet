using System;
using System.Collections.Generic;

namespace RM.Model.RequestModel
{
    public class CouponSpResponseModel
    {
        public class Coupon
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsActive { get; set; }
            public bool IsDelete { get; set; }
            public ICollection<CouponProductMapping> CouponProductMappings { get; set; }
            public ICollection<CouponUserMapping> CouponUserMappings { get; set; }
        }

        public class CouponProductMapping
        {
            public int Id { get; set; }
            public int CouponId { get; set; }
            public int ProductID { get; set; }
            public Coupon Coupon { get; set; }
        }

        public class CouponUserMapping
        {
            public int Id { get; set; }
            public int CouponId { get; set; }
            public Guid MobileUserKey { get; set; }
            public Coupon Coupon { get; set; }
        }

        public class CouponDto
        {
            public int CouponId { get; set; }
            public string Name { get; set; }
            public string ProductIds { get; set; }
            public string MobileUserKeys { get; set; }
        }
    }
}
