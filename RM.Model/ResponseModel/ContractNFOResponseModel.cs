using System.Collections.Generic;

namespace RM.Model.ResponseModel
{
    public class NFOModel
    {
        public List<ContractNFOResponseModel> NFO { get; set; }
    }

    public class NSEModel
    {
        public List<NSE> NSE { get; set; }
    }

    public class NSE
    {
        public string exch { get; set; }
        public string exchange_segment { get; set; }
        public object expiry_date { get; set; }
        public string formatted_ins_name { get; set; }
        public string instrument_type { get; set; }
        public string lot_size { get; set; }
        public string option_type { get; set; }
        public string pdc { get; set; }
        public string strike_price { get; set; }
        public string symbol { get; set; }
        public string tick_size { get; set; }
        public string token { get; set; }
        public string trading_symbol { get; set; }
    }
    public class ContractNFOResponseModel
    {
        public string exch { get; set; }
        public string exchange_segment { get; set; }
        public object expiry_date { get; set; }
        public string formatted_ins_name { get; set; }
        public string instrument_type { get; set; }
        public string lot_size { get; set; }
        public string option_type { get; set; }
        public string pdc { get; set; }
        public string strike_price { get; set; }
        public string symbol { get; set; }
        public string tick_size { get; set; }
        public string token { get; set; }
        public string trading_symbol { get; set; }
    }
}
