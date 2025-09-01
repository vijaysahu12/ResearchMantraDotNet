using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM.Model.RequestModel
{
    public class CheckMobileNumberRequestModel
    {
        public string MobileNumber { get; set; }
        public Guid MobileUserKey { get; set; }
    }
}
