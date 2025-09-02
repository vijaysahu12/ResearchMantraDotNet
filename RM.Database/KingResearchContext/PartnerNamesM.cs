using System;

namespace RM.Database.ResearchMantraContext
{
    public class PartnerNamesM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ReferralLink { get; set; }
        public string ReferralLinkKr { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}