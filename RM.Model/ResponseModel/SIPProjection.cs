using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RM.Model.ResponseModel
{
    public class SIPProjection
    {
        public int Duration { get; set; } // Year
        public double SIPAmount { get; set; } // Monthly SIP Investment
        public double InvestedAmount { get; set; } // Total amount invested 
        public double TotalInvestedAmount { get; set; } // Total amount invested 
        public double IncrementalRateInFuture { get; set; } // Future Value in ₹
        public double FutureValue { get; set; }
    }
}
