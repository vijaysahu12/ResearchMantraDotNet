using System;

namespace RM.Model.ResponseModel
{
    public class PartnerDematAccountResponse
    {
        public int? SlNo { get; set; }
        public int? Id { get; set; }
        public string? PartnerId { get; set; }
        public string? PartnerName { get; set; }
        public Guid? PublicKey { get; set; }
        public long? UserId { get; set; }
        public string? API { get; set; }
        public string? SecretKey { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? FullName { get; set; }
        public string? Mobile { get; set; }
        public string? EmailId { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public string? StatusDescription { get; set; }
    }
}