using System;

namespace RM.Model.ResponseModel
{
    public class KycResponseModel
    {
        public int Id { get; set; }
        //public int LeadId { get; set; }
        public string ClientName { get; set; }
        public string Mobile { get; set; }
        public string CountryCode { get; set; }
        public string Email { get; set; }
        public DateTime PaymentDate { get; set; }
        public int ModeOfPayment { get; set; }
        public string City { get; set; }
        public int ServiceId { get; set; }
        public bool KycStatus { get; set; }
        public decimal PaidAmount { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string ServiceName { get; set; }
        public Guid PublicKey { get; set; }
        public string CreatorName { get; set; }
    }
}
