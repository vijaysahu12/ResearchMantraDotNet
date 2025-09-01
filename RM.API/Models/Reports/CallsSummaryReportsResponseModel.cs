namespace RM.API.Models.Reports
{
    public class CallsSummaryReportsResponseModel
    {
        public int TotalCalls { get; set; }
        public int SuccessCalls { get; set; }
        public int ClosedCalls { get; set; }
        public int StopLossCalls { get; set; }
        public int Gar { get; set; }
        public int TargetRange { get; set; }
        public int Active { get; set; }
        public int NotActive { get; set; }
    }
}