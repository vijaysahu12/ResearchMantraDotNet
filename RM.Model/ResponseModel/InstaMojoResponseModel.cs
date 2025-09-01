namespace RM.Model.ResponseModel
{
    public class InstaMojoResponseModel
    {
        public string quantity { get; set; }
        public string status { get; set; }
        public string buyer { get; set; }
        public string currency { get; set; }
        public double amount { get; set; }
        public string fees { get; set; }
        public string? longurl { get; set; }
        public string? shorturl { get; set; }

        public string? purpose { get; set; }
        public object variants { get; set; }
        public string mac { get; set; }
        public string payment_id { get; set; }
        public string offer_slug { get; set; }
        public string offer_title { get; set; }
        public string buyer_name { get; set; }
        public string buyer_phone { get; set; }
        public string unit_price { get; set; }
        public object custom_fields { get; set; }

    }
}
