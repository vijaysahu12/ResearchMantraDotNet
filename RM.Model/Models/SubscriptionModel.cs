using System;
using System.Collections.Generic;

namespace RM.Model.Models
{
    public class SubscriptionModel
    {
        public class SubscriptionRequestModel
        {
            public int ProductId { get; set; }
            public int SubscriptionPlanId { get; set; }
            public Guid MobileUserKey { get; set; }
            public string DeviceType { get; set; }
        }
        public class SubscriptionPlanWithProductSP
        {
            public int SubscriptionMappingId { get; set; } // ID from SubscriptionMapping table
            public int ProductId { get; set; } // ID from ProductsM table
            public string ProductName { get; set; } // Product name from ProductsM table
            public decimal ActualPrice { get; set; } // Actual price of the product
            public decimal DiscountPrice { get; set; } // Price after applying the discount percentage
            public decimal NetPayment { get; set; } // Price after applying the discount percentage
            public string CouponCode { get; set; } // Price after applying the discount percentage
            public string SubscriptionDurationName { get; set; } // Name of the subscription duration
            public int Months { get; set; } // Number of months for the subscription duration
            public int SubscriptionPlanId { get; set; } // ID from SubscriptionPlan table
            public string PlanName { get; set; } // Name of the subscription plan
            public string PlanDescription { get; set; } // Description of the subscription plan
            public string PerMonth { get; set; }
            public DateTime ExpireOn { get; set; }
            public bool IsRecommended { get; set; }
            public int SubscriptionDurationId { get; set; }
        }


        public class SubscriptionPlanGroupModel
        {
            public List<SubscriptionPlanResponseModel> SubscriptionPlans { get; set; }
        }
        public class SubscriptionPlanResponseModel
        {
            public int ProductId { get; set; }
            public int SubscriptionPlanId { get; set; }
            public string Name { get; set; }
            public string PaymentGatewayName { get; set; }
            public List<SubscriptionDurationDetail> SubscriptionDurations { get; set; }
        }

        public class SubscriptionDetailsResponseModel
        {
            public long SlNo { get; set; }
            public int MappingId { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public int DurationId { get; set; }
            public decimal? ProductPrice { get; set; }
            public decimal? ProductPriceWithDuration { get; set; }
            public decimal DiscountPrice { get; set; }

            public string DurationName { get; set; }
            public int DurationMonths { get; set; }
            public decimal? DiscountPercentage { get; set; }
            public bool DurationStatus { get; set; }
            public bool PlanStatus { get; set; }
            public string PlanName { get; set; }
            public int PlanId { get; set; }
            public bool MappingStatus { get; set; }
            public DateTime CreatedOn { get; set; }
            public string? ModifiedBy { get; set; }

        }


        public class SubscriptionMappingRequestModel
        {
            public int SubscriptionDurationId { get; set; }
            public decimal DiscountPercentage { get; set; }
            public int ProductId { get; set; }
            public int SubscriptionPlanId { get; set; }
            public bool? IsActive { get; set; }
            public Guid? ModifiedBy { get; set; }

        }

        public class SubscriptionMappingUpdateRequest
        {
            public int MappingId { get; set; }

            public int ProductId { get; set; }

            public int SubscriptionPlanId { get; set; }

            public int SubscriptionDurationId { get; set; }
            public decimal DiscountPercentage { get; set; }
            public bool IsActive { get; set; }

            public Guid? ModifiedBy { get; set; }
        }

        public class SubscriptionPlanRequest
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public bool IsActive { get; set; }

            public Guid? ModifiedBy { get; set; }

        }

        public class SubscriptionDurationDetail
        {
            public int SubscriptionDurationId { get; set; }
            public string SubscriptionDurationName { get; set; }
            public int Months { get; set; }
            public decimal DiscountPrice { get; set; }
            public bool SubscriptionDurationActive { get; set; }
            public string CouponCode { get; set; }
            public decimal NetPayment { get; set; }
            public decimal ActualPrice { get; set; }
            public DateTime ExpireOn { get; set; }
            public string PerMonth { get; set; }
            public bool IsRecommended { get; set; }
            public int SubscriptionMappingId { get; set; }
            public decimal? DefaultCouponDiscount { get; set; }

        }

    }
}
