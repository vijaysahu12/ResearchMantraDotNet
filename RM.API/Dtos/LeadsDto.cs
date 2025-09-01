using System;
using System.ComponentModel.DataAnnotations;

namespace RM.API.Dtos
{
    public class LeadsDto
    {
        public long Id { get; set; }

        [StringLength(150)]
        public string FullName { get; set; }

        [StringLength(100)]
        public string MobileNumber { get; set; }
        public string AlternateMobileNumber { get; set; }

        [StringLength(100)]
        public string EmailId { get; set; }

        [StringLength(100)]
        public string ServiceKey { get; set; }

        [StringLength(100)]
        public string? LeadTypeKey { get; set; }

        [StringLength(100)]
        public string LeadSourceKey { get; set; }

        [StringLength(300)]
        public string Remarks { get; set; }

        public Guid? PublicKey { get; set; }

        public DateTime? CreatedOn { get; set; }

        [StringLength(100)]
        public string CreatedBy { get; set; }

        [StringLength(100)]
        public string AssignedTo { get; set; }

        public bool? Favourite { get; set; }
        //public int TotalRecordCount { get; set; }
        public int? StatusId { get; set; }
        public string? StatusName { get; set; }
        public string? PurchaseOrderStatus { get; set; }
        public Guid? PurchaseOrderKey { get; set; }
    }

    public class JunkLeadsDto
    {
        public long Id { get; set; }

        [StringLength(150)]
        public string FullName { get; set; }

        [StringLength(100)]
        public string MobileNumber { get; set; }

        [StringLength(100)]
        public string EmailId { get; set; }

        [StringLength(100)]
        public string ServiceKey { get; set; }

        [StringLength(100)]
        public string? LeadTypeKey { get; set; }

        [StringLength(100)]
        public string LeadSourceKey { get; set; }

        [StringLength(300)]
        public string Remarks { get; set; }


        public Guid? PublicKey { get; set; }

        public DateTime? CreatedOn { get; set; }

        [StringLength(100)]
        public string CreatedBy { get; set; }

        [StringLength(100)]
        public string AssignedTo { get; set; }

        public int TotalRecordCount { get; set; }
        public int? StatusId { get; set; }
        public string? StatusName { get; set; }
        public Guid? PurchaseOrderKey { get; set; }
    }
}
