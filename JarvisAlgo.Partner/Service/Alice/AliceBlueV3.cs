using RestSharp;
using System.Threading.Tasks;

namespace JarvisAlgo.Partner.Service.Alice
{
    public class AliceBlueV3
    {
        public string _userId = "AB081077";
        public string _apiKey = "Kq90QqAsKY95iO0YgbYsPbk6RNUcJ283ua0G9mlcHJiYPq9QSgXREIe5CxCxs4pxqINCgidOnQ42ORxrR6lXm9zD3xAucMOsaJIpXJAh7xBGIetEFCjCZSLqSHY6C8q3";
        public string _baseUrl = "https://ant.aliceblueonline.com/rest/AliceBlueAPIService/api/";
        public string _sessionId = "";
        public string ws = "ws/createWsSession";
        public string WS_ENDPOINT = "wss://ws1.aliceblueonline.com/NorenWS";

        public AliceBlueV3()
        {
        }

        public string GetHeaders()
        {
            return "Bearer " + _userId + " " + _sessionId;
        }
        public async Task<LtpRequestModelResponse> GetScripQuoteDetails(string exchange, string symbolToken)
        {
            string requestURl = "https://ant.aliceblueonline.com/rest/AliceBlueAPIService/api/ScripDetails/getScripQuoteDetails?exch=" + exchange + "&symbol=" + symbolToken;
            //ContractNFOResponseModel dd = new();
            var client = new RestClient();
            var request = new RestRequest()
            {
                Method = Method.Post,
                Resource = requestURl
            };
            request.AddHeader("Authorization", GetHeaders());

            var objRequest = new LtpRequestModelRequest()
            {
                symbol = symbolToken,
                exch = exchange
            };
            var body = Newtonsoft.Json.JsonConvert.SerializeObject(objRequest);

            request.AddParameter("application/json", body, ParameterType.RequestBody);

            RestResponse response = await client.ExecuteAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<LtpRequestModelResponse>(response.Content);
            }
            return new LtpRequestModelResponse();
        }
    }
    public class LtpRequestModelResponse
    {
        public string optiontype { get; set; }
        public int SQty { get; set; }
        public string vwapAveragePrice { get; set; }
        public string LTQ { get; set; }
        public int DecimalPrecision { get; set; }
        public string openPrice { get; set; }
        public string LTP { get; set; }
        public string Ltp { get; set; }
        public string BRate { get; set; }
        public string defmktproval { get; set; }
        public string symbolname { get; set; }
        public string noMktPro { get; set; }
        public int BQty { get; set; }
        public string mktpro { get; set; }
        public string LTT { get; set; }
        public string TickSize { get; set; }
        public int Multiplier { get; set; }
        public string strikeprice { get; set; }
        public string TotalSell { get; set; }
        public string High { get; set; }
        public string stat { get; set; }
        public string yearlyLowPrice { get; set; }
        public string yearlyHighPrice { get; set; }
        public string exchFeedTime { get; set; }
        public int BodLotQty { get; set; }
        public string PrvClose { get; set; }
        public string Change { get; set; }
        public string SRate { get; set; }
        public string Series { get; set; }
        public string TotalBuy { get; set; }
        public string Low { get; set; }
        public string UniqueKey { get; set; }
        public string PerChange { get; set; }
        public object companyname { get; set; }
        public string TradeVolume { get; set; }
        public string TSymbl { get; set; }
        public string Exp { get; set; }
        public string LTD { get; set; }
    }
    public class OrderRepetitionList
    {
        public string orderId { get; set; }
        public int count { get; set; }
        public string newOrderId { get; set; }
    }
    public class LtpRequestModelRequest
    {
        public string exch { get; set; }
        public string symbol { get; set; }
    }
}
