using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RM.Database.KingResearchContext
{
    public class KingResearchCouponsM
    {

        public int Id { get; set; }
        public Guid PublicKey { get; set; }
        public Guid UserKey { get; set; }
        public string Code { get; set; }
        public int ProductId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public bool IsRedeemed { get; set; }
        public DateTime CreatedOn { get; set; }

    }
}
