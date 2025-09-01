using System;

namespace RM.Database.KingResearchContext
{
    public class PartnerAccountsM
    {
        public int Id { get; set; }
        public string PartnerName { get; set; }
        public string PartnerId { get; set; }
        public int? BrokerId { get; set; }
        public int? PartnerIdSecond { get; set; }
        public int? MobileUserId { get; set; }
        public string API { get; set; }
        public string? GenerateToken { get; set; }
        public string SecretKey { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? ModifiedBy { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }


    }
}
