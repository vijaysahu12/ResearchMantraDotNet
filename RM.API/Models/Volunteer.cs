using System;

namespace RM.API.Models
{
    public partial class Volunteer
    {
        public long Id { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Pincode { get; set; }
        public string RegionKey { get; set; }
        public string Image { get; set; }
        public string CustomerKey { get; set; }
        public string MobileNumber { get; set; }
        public string WhatsappNumber { get; set; }
        public string LandlineNumber { get; set; }
        public string EmergencyContact { get; set; }
        public string EmailId { get; set; }
        public DateTime? Dob { get; set; }
        public DateTime? Doa { get; set; }
        public DateTime? Doj { get; set; }
        public string BloodGroup { get; set; }
        public string Gender { get; set; }
        public string Qualifications { get; set; }
        public string PresentEmploymentDetails { get; set; }
        public string PreviousExperience { get; set; }
        public string WhySsbk { get; set; }
        public string TechnicalSkills { get; set; }
        public string Hobbies { get; set; }
        public string ExtraCurricularActivites { get; set; }
        public string PresentRoleSsbk { get; set; }
        public string AboutYourself { get; set; }
        public string AdminRemarks { get; set; }
        public byte? IsDisabled { get; set; }
        public byte? IsDelete { get; set; }
        public Guid? PublicKey { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
    }
}
