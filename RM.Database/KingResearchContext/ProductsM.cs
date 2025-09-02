using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RM.Database.ResearchMantraContext
{
    public class ProductsM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string? Description { get; set; }
        public string? DescriptionTitle { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountAmount { get; set; }
        public int? DiscountPercent { get; set; }
        public string? ImageUrl { get; set; }
        public string? ListImage { get; set; }
        public string? LandscapeImage { get; set; }
        public int CategoryID { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid? ModifiedBy { get; set; }
        public int? SubscriptionId { get; set; }
        public bool IsImportant { get; set; }
        public bool IsQueryFormEnabled { get; set; }
        public bool CanPost { get; set; }

    }
}