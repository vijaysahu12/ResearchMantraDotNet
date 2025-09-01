using RM.Model.Models;
using RM.Model.MongoDbCollection;
using RM.Model.RequestModel.MobileApi;
using MongoDB.Bson.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace RM.Model.RequestModel
{
    public class CashFreeWebhookRequestModel
    {
        public Data data { get; set; }
        public DateTime event_time { get; set; }
        public string type { get; set; }
    }

    public class Data
    {
        public Order order { get; set; }
        public Payment payment { get; set; }
        public CustomerDetails customer_details { get; set; }
        public ChargesDetails charges_details { get; set; }
        public PaymentGatewayDetails payment_gateway_details { get; set; }
    }


    public class PaymentGatewayDetails
    {
        public string gateway_name { get; set; }
        public string gateway_order_id { get; set; }
        public string gateway_payment_id { get; set; }
        public string gateway_status_code { get; set; }
        public string gateway_order_reference_id { get; set; }
        public string gateway_settlement { get; set; }
        public string gateway_reference_name { get; set; }
    }

    public class Order
    {
        public string order_id { get; set; }
        public double order_amount { get; set; }
        public string order_currency { get; set; }
        public Dictionary<string, string> order_tags { get; set; }
    }

    public class Payment
    {
        public long cf_payment_id { get; set; }
        public string payment_status { get; set; }
        public double payment_amount { get; set; }
        public string payment_currency { get; set; }
        public string payment_message { get; set; }
        public DateTime payment_time { get; set; }
        public string bank_reference { get; set; }
        public string auth_id { get; set; }
        public PaymentMethod payment_method { get; set; }
        public string payment_group { get; set; }
    }

    public class PaymentMethod
    {
        public Upi upi { get; set; }
    }

    public class Upi
    {
        public string channel { get; set; }
        public string upi_id { get; set; }
    }

    public class CustomerDetails
    {
        public string customer_name { get; set; }
        public string customer_id { get; set; }
        public string customer_email { get; set; }
        public string customer_phone { get; set; }
    }

    public class ChargesDetails
    {
        public double service_charge { get; set; }
        public double service_tax { get; set; }
        public double settlement_amount { get; set; }
        public string settlement_currency { get; set; }
        public string service_charge_discount { get; set; }

    }

}
