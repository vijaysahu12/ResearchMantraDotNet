using System;

namespace RM.Database.KingResearchContext
{
    public class CouponsM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid PublicKey { get; set; }
        public string? Description { get; set; }
        public int? DiscountInPercentage { get; set; }
        public decimal? DiscountInPrice { get; set; }
        public int TotalRedeems { get; set; }
        public int? RedeemLimit { get; set; }
        public int? ProductValidityInDays { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public bool Def { get; set; }
        public bool IsVisible { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? ModifiedBy { get; set; }
    }
}
