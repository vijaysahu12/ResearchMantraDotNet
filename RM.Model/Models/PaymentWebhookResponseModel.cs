using System;
using System.Collections.Generic;

namespace RM.Model.Models
{
    public class PaymentWebhookResponseModel
    {
        public string? Response { get; set; }
    }

    public class PhoneReportResponseModel

    {
        public List<PhonePeReportPaymentDetails> PaymentDetails { get; set; }

    }


    public class PhonePeReportPaymentDetails
    {
        public string ProductName { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public int TotalUserCount { get; set; }
        public int ThreeMonthsCount { get; set; }
        public int SixMonthsCount { get; set; }
        public int TwelveMonthsCount { get; set; }
        public int OneMonthsCount { get; set; }
        public int FreeMonthsCount { get; set; }

    }
    public class PhonePeReportPaymentChartDetails
    {
        //  public int? ProductId { get; set; }
        public string? Duration { get; set; }
        public string ProductName { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public int TotalUserCount { get; set; }

    }

    public class InstaMojoWebhookData
    {
        public string payment_id { get; set; }
        public string status { get; set; }
        public string shorturl { get; set; }
        public string longurl { get; set; }
        public string purpose { get; set; }
        public string amount { get; set; }
        public string fees { get; set; }
        public string currency { get; set; }
        public string buyer { get; set; }
        public string buyer_name { get; set; }
        public string buyer_phone { get; set; }
        public string payment_request_id { get; set; }
        public string mac { get; set; }
    }

    //public class InstaMojoPaymentVerificationRequestModel
    //{
    //    public string payment_id { get; set; }
    //    public Guid MobileUserKey { get; set; }
    //    public string PaidAmount { get; set; }
    //    public int SubcriptionModelId { get; set; }
    //    public int SubscriptionMappingId { get; set; }
    //    public int ProductId { get; set; }
    //    public string MerchantTransactionId { get; set; }
    //    public bool Status { get; set; }
    //    public string CouponCode { get; set; }
    //}
}