using System;
using System.ComponentModel.DataAnnotations;

namespace RM.API.Dtos
{
    public class CustomerKYCDto
    {
        public long Id { get; set; }
        public string LeadKey { get; set; }
        [StringLength(50)]
        public string PANURL { get; set; }
        [StringLength(10)]
        public string PAN { get; set; }
        public bool Verified { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsDelete { get; set; }
        [StringLength(50)]
        public string Status { get; set; }
        [StringLength(200)]
        public string Remarks { get; set; }
        public string JsonData { get; set; }
    }


    public class GetCustomerKYCDto
    {
        public long Id { get; set; }
        public string LeadKey { get; set; }
        public Guid PublicKey { get; set; }
        public string FullName { get; set; }
        public string MobileNumber { get; set; }
        public string EmailId { get; set; }

        public string PAN { get; set; }
        public string PANURL { get; set; }

        public string Remarks { get; set; }
        public string ProfileImage { get; set; }
        public string Status { get; set; }
        public bool Verified { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}
