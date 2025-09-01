using System;

namespace RM.Database.KingResearchContext
{
    public class Groups
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public bool? IsCommentEnabled { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        // Other properties as needed
    }


    public class MyBucketM
    {
        public long Id { get; set; }
        public Guid MobileUserKey { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsExpired { get; set; }
        public bool? Notification { get; set; }
    }

    public class ProductCommunityMapping
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int CommunityId { get; set; } //CommunityId is also a productId with category type of community
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public long CreatedBy { get; set; }
        public long ModifiedBy { get; set; }
        public int? DurationInDays { get; set; }

    }

}
