using System;

namespace RM.Model.Models
{
    public class ProductBonusMappingM
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int BonusProductId { get; set; }
        public int? DurationInDays { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public int? CreatedBy { get; set; }
    }
}
