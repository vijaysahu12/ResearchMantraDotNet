using System;
using System.ComponentModel.DataAnnotations;

namespace RM.API.Models
{
    //    using System;
    //    using System.Collections.Generic;
    //    using System.ComponentModel.DataAnnotations;
    //    using System.ComponentModel.DataAnnotations.Schema;
    //    using System.Data.Entity.Spatial;

    //    public partial class Customer
    //    {
    //        public long Id { get; set; }

    //        [StringLength(100)]
    //        public string CustomerTypeKey { get; set; }

    //        [StringLength(100)]
    //        public string SegmentKey { get; set; }

    //        public decimal? TotalPurchases { get; set; }


    //        [StringLength(300)]
    //        public string Remarks { get; set; }


    //        [StringLength(100)]
    //        public string LeadKey { get; set; }

    //        public byte? IsDelete { get; set; }

    //        public Guid? PublicKey { get; set; }

    //        public DateTime? CreatedOn { get; set; }

    //        [StringLength(100)]
    //        public string CreatedBy { get; set; }

    //        public DateTime? ModifiedOn { get; set; }

    //        [StringLength(100)]
    //        public string ModifiedBy { get; set; }
    //    }


    public partial class CustomerRequest
    {
        public long Id { get; set; }

        //[StringLength(100)]
        //public string CustomerTypeKey { get; set; }

        //[StringLength(100)]
        //public string SegmentKey { get; set; }

        public decimal? TotalPurchases { get; set; }


        [StringLength(300)]
        public string Remarks { get; set; }


        [StringLength(100)]
        public string LeadKey { get; set; }

        public byte? IsDelete { get; set; }

        public Guid? PublicKey { get; set; }

        public string FullName { get; set; }
        public string EmailId { get; set; }
        public string City { get; set; }
        public string MobileNumber { get; set; }
        public string PinCode { get; set; }
        public Guid? PurchaseOrderKey { get; set; }
    }

    public class CustomerDto
    {
        public long Id { get; set; }
        public string Fullname { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Remarks { get; set; }
        //public string CustomerTypeKey { get; set; }
        //public string SegmentKey { get; set; }
        public DateTime CreatedOn { get; set; }
        public string PublicKey { get; set; }
        public bool KYCVerified { get; set; }
        public string PANURL { get; set; }
        public string ProfileImage { get; set; }
        public Guid? PurchaseOrderKey { get; set; }
    }

    public class CustomerKycRequest
    {
        public string PublicKey { get; set; }
        public string Status { get; set; }
    }
}
