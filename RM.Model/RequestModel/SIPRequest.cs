using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM.Model.RequestModel
{
    public class SIPRequest
    {
        public double MonthlyInvestment { get; set; } // SIP amount
        public double AnnualReturns { get; set; } // Annual interest rate in %
        public int InvestmentPeriod { get; set; } // Duration in Years
        public int IncrementalRate { get; set; } // If true, apply 6% inflation
    }
}
