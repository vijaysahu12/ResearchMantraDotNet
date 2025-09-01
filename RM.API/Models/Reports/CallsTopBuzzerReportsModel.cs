namespace RM.API.Models.Reports
{
    public class CallsTopBuzzerReportsResponseModel
    {
        public string StrategyKey { get; set; }
        public string StrategyName { get; set; }
        public decimal Pnl { get; set; }
        public decimal Percentage { get; set; }
    }
}
