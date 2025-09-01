//namespace RM.API.Models
//{
//    using System;
//    using System.Collections.Generic;
//    using System.ComponentModel.DataAnnotations;
//    using System.ComponentModel.DataAnnotations.Schema;
//    using System.Data.Entity.Spatial;

//    public partial class Lead
//    {
//        public long Id { get; set; }

//        [StringLength(150)]
//        public string? FullName { get; set; }

//        public char? Gender { get; set; }
//        [StringLength(100)]
//        public string MobileNumber { get; set; }

//        [StringLength(100)]
//        public string? EmailId { get; set; }

//        [StringLength(100)]
//        public string? ServiceKey { get; set; }

//        [StringLength(100)]
//        public string? LeadTypeKey { get; set; }

//        [StringLength(100)]
//        public string? LeadSourceKey { get; set; }

//        [StringLength(300)]
//        public string? Remarks { get; set; }

//        public byte? IsSpam { get; set; }

//        public byte? IsWon { get; set; }

//        public byte? IsDisabled { get; set; }

//        public byte? IsDelete { get; set; }

//        public Guid PublicKey { get; set; }

//        public DateTime CreatedOn { get; set; }

//        [StringLength(100)]
//        public string? CreatedBy { get; set; }

//        public DateTime? ModifiedOn { get; set; }

//        [StringLength(100)]
//        public string? ModifiedBy { get; set; }

//        [StringLength(100)]
//        public string? AssignedTo { get; set; }

//        public string? PriorityStatus { get; set; }


//        public string? ProfileImage { get; internal set; }
//        public string? City { get; internal set; }
//        public string? Pincode { get; internal set; }
//    }
//}


using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class LeadAllotmentsRequestModel
{
    [Required]
    public List<LeadKeyList> Leads { get; set; }
    [Required]
    public string AlloteePublicKey { get; set; }
    public bool SelfAssign { get; set; }
    public string? LeadTypeKey { get; set; }
    public string? LeadSourceKey { get; set; }
    public bool OverrideAllotment { get; set; }
}


public class LeadKeyList
{
    public string PublicKey { get; set; }
}