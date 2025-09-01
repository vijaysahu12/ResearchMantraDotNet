using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM.Model.ResponseModel
{
    public class SIPResponse
    {
        public double ExpectedAmount { get; set; } // Future Value (inflation-adjusted if true)
        public double AmountInvested { get; set; } // Total amount invested
        public double WealthGain { get; set; } // Profit earned
        public bool ApplyInflation { get; set; } // Inflation applied or not
        public double IncrementalRateInFuture { get; set; } // Inflation 
        public List<SIPProjection> ProjectedSipReturnsTable { get; set; } // Yearly SIP Projections
    }
}
