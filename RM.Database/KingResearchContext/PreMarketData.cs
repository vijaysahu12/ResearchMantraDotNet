using System;

namespace RM.Database.ResearchMantraContext
{
    public class PreMarketData
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsOpen { get; set; }
        public decimal? Ltp { get; set; }
        public decimal? Change { get; set; }
        public decimal? ChangePercent { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
