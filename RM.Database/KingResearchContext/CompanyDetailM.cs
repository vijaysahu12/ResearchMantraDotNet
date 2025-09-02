using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RM.Database.ResearchMantraContext
{
    public class CompanyDetailM
    {
        public int Id { get; set; }
        public int BasketId { get; set; }
        public string? Name { get; set; }
        public string? Symbol { get; set; }
        public string? Description { get; set; }
        public string? ShortSummary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MarketCap { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PE { get; set; }

        public string? ChartImageUrl { get; set; }
        public string? OtherImage { get; set; }
        public string? WebsiteUrl { get; set; }
        public DateTime? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public DateTime? PublishDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public bool IsPublished { get; set; }
        public string? TrialDescription { get; set; }
        public bool IsFree { get; set; }
        public decimal? CurrentPrice { get; set; }
        public decimal? SharesInCrores { get; set; }
        public decimal? TTMNetProfitInCrores { get; set; }
        public decimal? FaceValue { get; set; }
        public decimal? NetWorth { get; set; }
        public decimal? PromotersHolding { get; set; }
        public decimal? ProfitGrowth { get; set; }
        public decimal? YesterdayPrice { get; set; }
    }

    public class CompanyDetailMessageM
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CompanyDetailId { get; set; }

        [Required, MaxLength(200)]
        public string Message { get; set; }

        public bool IsActive { get; set; }

        [Required]
        public DateTime ModifiedOn { get; set; } = DateTime.Now;

        [Required]
        public Guid ModifiedBy { get; set; }
    }
}