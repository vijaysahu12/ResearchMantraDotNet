namespace RM.Database.KingResearchContext
{
    public class CompanyTypeM
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Symbol { get; set; }
        public bool? LTOPUptrend { get; set; }
        public bool? STOPOpUpTrend { get; set; }
        public bool? FuturisticSector { get; set; }
        public bool? HNIInstitutionalPromotersBuy { get; set; }
        public bool? SpecialSituations { get; set; }
        public bool? FutureVisibility { get; set; }
    }
}
