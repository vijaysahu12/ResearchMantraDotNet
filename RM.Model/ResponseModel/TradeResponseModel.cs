namespace RM.Model.ResponseModel
{
    public class TradeResponseModel
    {
        public long? Id { get; set; }
        public string Symbol { get; set; }
        public string? Roi { get; set; }
        public string? EntryPrice { get; set; }
        public string Status { get; set; }
        public string? Duration { get; set; }
        public int? Cmp { get; set; }
        public string? ExitPrice { get; set; }
        public string? InvestmentMessage { get; set; }
    }
}
