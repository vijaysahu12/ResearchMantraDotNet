using System;
using System.ComponentModel.DataAnnotations;

namespace RM.Database.KingResearchContext
{
    public class ReasonChangeLog
    {
        [Key]
        public int LogId { get; set; }
        public long MyBucketId { get; set; }
        public int ProductId { get; set; }
        public string Reason { get; set; }
        public string ProductName { get; set; }
        public DateTime? BucketStartDate { get; set; }
        public DateTime? BucketEndDate { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
    public class ReasonChangePurchase
    {
        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public string Reason { get; set; }
        public Guid CreatedBy { get; set; }
        //public string CreatedByName { get; set; } // optional if you want to join Users table
        public DateTime CreatedDate { get; set; }
        public int? ProductId { get; set; }
        public string Product { get; set; }
        public decimal? NetAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

}
