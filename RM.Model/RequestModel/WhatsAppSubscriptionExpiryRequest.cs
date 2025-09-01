using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM.Model.RequestModel
{
    public class WhatsAppSubscriptionExpiryRequest
    {
        public string CountryCode { get; set; }
        public string MobileNumber { get; set; }
        public string CustomerName { get; set; }
        public string DaysLeft { get; set; }
        public string ServiceName { get; set; }
    }
}
