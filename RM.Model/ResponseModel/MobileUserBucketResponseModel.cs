using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM.Model.ResponseModel
{
    public class MobileUserBucketResponseModel
    {
        public string FullName { get; set; }
        public string MobileNumber { get; set; }
        public string EmailID { get; set; }
        public string ProductName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DaysToGo { get; set; }
        public long SNo { get; set; }
        public string? FirebaseFcmToken { get; set; }
        public Guid? PublicKey { get; set; }
        public int? OldDevice { get; set; }
        public int? Notification { get; set; }
        public int? ProductID { get; set; }
    }

}
