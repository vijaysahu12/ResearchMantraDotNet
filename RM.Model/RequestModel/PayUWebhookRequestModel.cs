using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM.Model.RequestModel
{
    public class PayUWebhookRequestModel
    {
        public string mihpayid { get; set; }
        public string mode { get; set; }
        public string status { get; set; }
        public string key { get; set; }
        public string txnid { get; set; }
        public string amount { get; set; }
        public string addedon { get; set; }
        public string productinfo { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string zipcode { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string udf1 { get; set; }
        public string udf2 { get; set; }
        public string udf3 { get; set; }
        public string udf4 { get; set; }
        public string udf5 { get; set; }
        public string udf6 { get; set; }
        public string udf7 { get; set; }
        public string udf8 { get; set; }
        public string udf9 { get; set; }
        public string udf10 { get; set; }
        public string card_token { get; set; }
        public string card_no { get; set; }
        public string field0 { get; set; }
        public string field1 { get; set; }
        public string field2 { get; set; }
        public string field3 { get; set; }
        public string field4 { get; set; }
        public string field5 { get; set; }
        public string field6 { get; set; }
        public string field7 { get; set; }
        public string field8 { get; set; }
        public string field9 { get; set; }
        public string payment_source { get; set; }
        public string PG_TYPE { get; set; }
        public string error { get; set; }
        public string error_Message { get; set; }
        public string net_amount_debit { get; set; }
        public string name_on_card { get; set; }
        public string cardnum { get; set; }
        public string cardhash { get; set; }
        public string discount { get; set; }
        public string offer_key { get; set; }
        public string offer_availed { get; set; }
        public string unmappedstatus { get; set; }
        public string hash { get; set; }
        public string bank_ref_no { get; set; }
        public string bank_ref_num { get; set; }
        public string bankcode { get; set; }
        public string surl { get; set; }
        public string curl { get; set; }
        public string furl { get; set; }
        public string pa_name { get; set; }
        public MeCode meCode { get; set; }
    }

    public class MeCode
    {
        public string pgMerchantId { get; set; }
        public string merchantVpa { get; set; }
    }

    public class PayUWebhookWrapper
    {
        public string message { get; set; }
        public string date { get; set; }
        public string transactionId { get; set; }
        public string status { get; set; }
        public string payuId { get; set; }
        public PayUWebhookRequestModel fullFormData { get; set; }
    }

}
