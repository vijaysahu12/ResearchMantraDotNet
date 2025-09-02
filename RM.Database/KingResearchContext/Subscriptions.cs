using System;

namespace RM.Database.ResearchMantraContext
{
    public class Subscriptions
    {

        public class SubscriptionDurationM
        {
            public int Id { get; set; } // Primary Key
            public string Name { get; set; } // Name of the subscription duration
            public int Months { get; set; } // Number of months for the duration
            public bool? IsActive { get; set; } // Nullable Boolean for Active Status
            public DateTime CreatedOn { get; set; } // Timestamp when created
            public DateTime ModifiedOn { get; set; } // Timestamp when last modified
        }

        public class SubscriptionPlanM
        {
            public int Id { get; set; } // Primary Key
            public string Name { get; set; } // Name of the subscription plan
            public string Description { get; set; } // Optional description for the subscription plan
            public bool? IsActive { get; set; } // Nullable Boolean for Active Status
            public DateTime CreatedOn { get; set; } // Timestamp when created
            public DateTime ModifiedOn { get; set; } // Timestamp when last modified
            public Guid? ModifiedBy { get; set; } // User ID of the last user to modify the record

        }

        public class SubscriptionMappingM
        {
            public int Id { get; set; } // Primary Key
            public int SubscriptionDurationId { get; set; } // Foreign Key to SubscriptionDuration
            public decimal DiscountPercentage { get; set; }
            public int ProductId { get; set; } // Product ID associated with the subscription
            public int SubscriptionPlanId { get; set; } // Foreign Key to SubscriptionPlan
            public bool? IsActive { get; set; } // Nullable Boolean for Active Status
            public DateTime CreatedOn { get; set; } // Timestamp when created
            public DateTime ModifiedOn { get; set; } // Timestamp when last modified
            public Guid? ModifiedBy { get; set; } // User ID of the last user to modify the record

        }
    }
}