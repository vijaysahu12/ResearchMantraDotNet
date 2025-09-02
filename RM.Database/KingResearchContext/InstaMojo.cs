using System;
using System.Collections.Generic;

namespace RM.Database.ResearchMantraContext
{

    public class InstaMojo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Currency { get; set; }
        public double Amount { get; set; }
        public string Quantity { get; set; }
        public string? Status { get; set; }
        public double Fees { get; set; }
        public string? Purpose { get; set; }
        public string PaymentId { get; set; }
        public string Description { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public int MailSent { get; set; }
        public int hasEntered { get; set; }
    }
    public class InstaMojosMarketManthan
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Currency { get; set; }
        public double Amount { get; set; }
        public string Quantity { get; set; }
        public string? Status { get; set; }
        public double Fees { get; set; }
        public string? Purpose { get; set; }
        public string PaymentId { get; set; }
        public string Description { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public int MailSent { get; set; }
        public int hasEntered { get; set; }
    }


    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class BillingAddress
    {
        public object address { get; set; }
        public object city { get; set; }
        public object postal_code { get; set; }
        public object state { get; set; }
        public object country { get; set; }
    }



    public class Buyer
    {
        public string name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
    }

    public class CustomFields
    {
        public ShippingAddress shipping_address { get; set; }
        public object additional_custom_fields { get; set; }
        public object gstin { get; set; }
        public object company_name { get; set; }
        public BillingAddress billing_address { get; set; }
    }

    public class Detail
    {
        public string amount { get; set; }
        public string purpose { get; set; }
        public int quantity { get; set; }
    }

    public class Payment
    {
        public string payment_id { get; set; }
        public string status { get; set; }
        public double amount { get; set; }
        public string currency { get; set; }
        public string completed_at { get; set; }
        public string resource_uri { get; set; }
    }

    public class InstaMojoRequestModel
    {
        public Payment payment { get; set; }
        public Buyer buyer { get; set; }
        public Seller seller { get; set; }
        public List<Detail> details { get; set; }
        public string page_id { get; set; }
        public object discount_code { get; set; }
        public CustomFields custom_fields { get; set; }
        public string message_authentication_code { get; set; }
    }

    public class Seller
    {
        public string account_id { get; set; }
    }

    public class ShippingAddress
    {
        public object address { get; set; }
        public object city { get; set; }
        public object postal_code { get; set; }
        public object state { get; set; }
        public object country { get; set; }
    }


    public class OctEventRegistration
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Currency { get; set; }
        public double Amount { get; set; }
        public string Quantity { get; set; }
        public string? Status { get; set; }
        public double Fees { get; set; }
        public string? Purpose { get; set; }
        public string PaymentId { get; set; }
        public string Description { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public int MailSent { get; set; }
        public int hasEntered { get; set; }
    }

}
