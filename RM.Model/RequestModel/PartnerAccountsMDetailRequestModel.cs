using System;

namespace RM.Model.RequestModel
{
    public class PartnerAccountsMDetailRequestModel
    {
        //[RegularExpression("^(AliceBlue|Zerodha)$", ErrorMessage = "Invalid PartnerName. Allowed values are 'AliceBlue' or 'Zerodha'.")]
        public string PartnerName { get; set; }
        public string PartnerId { get; set; }
        //public int MobileUserId { get; set; }
        public string API { get; set; }
        public string SecretKey { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
