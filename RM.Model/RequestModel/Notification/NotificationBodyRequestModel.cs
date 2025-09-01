namespace RM.Model.RequestModel.Notification
{
    public class NotificationBodyRequestModel
    {
        public string Message { get; set; }
        public bool EnableTradingButton { get; set; }
        public string Token { get; set; }
        public string AppCode { get; set; }
        public string Exchange { get; set; }
        public string TradingSymbol { get; set; }
        public string TransactionType { get; set; }
        public int Quantity { get; set; }
        public string OrderType { get; set; }
        public decimal Price { get; set; }
        public string Validity { get; set; }
        public string Product { get; set; }
        public string Complexity { get; set; }
    }
}
