using System;

namespace RM.Model.ResponseModel
{
    public class ExpiredServiceResponseModel
    {
        public string FullName { get; set; }
        public Guid UserKey { get; set; }
        public string Receiver { get; set; }
        public string CC { get; set; }
        public string SubscriptionName { get; set; }
        public string MobileNumber { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DaysToGo { get; set; }
        public string Name { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
    }
}