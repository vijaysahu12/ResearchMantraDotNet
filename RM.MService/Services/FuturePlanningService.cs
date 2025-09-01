namespace RM.MService.Services
{
    public class FuturePlanningService
    {
        //FV
        public double FutureValue(double rate, int nper, double pmt, double pv = 0, int type = 0)
        {
            // Validate inputs (optional but good practice)
            if (nper <= 0)
            {
                throw new ArgumentOutOfRangeException("nper must be greater than zero.");
            }

            if (type != 0 && type != 1)
            {
                throw new ArgumentOutOfRangeException("type must be 0 or 1.");
            }


            double fv;

            if (rate == 0) // Handle the case where the interest rate is 0 to avoid division by zero.
            {
                fv = -pv - pmt * nper;
            }
            else
            {
                fv = pv * Math.Pow(1 + rate, nper) + pmt * ((Math.Pow(1 + rate, nper) - 1) / rate) * (1 + rate * type);
            }

            return fv;

            //nper = yearsToRetirement * 12 → This converts years to months because SIPs are usually monthly.
        }
        //PV
        public double PresentValue(double rate, int nper, double pmt, double fv = 0, int type = 0)
        {
            // Validate inputs (optional but good practice)
            if (nper <= 0)
            {
                throw new ArgumentOutOfRangeException("nper must be greater than zero.");
            }

            if (type != 0 && type != 1)
            {
                throw new ArgumentOutOfRangeException("type must be 0 or 1.");
            }

            double pv;

            if (rate == 0) // Handle the case where the interest rate is 0 to avoid division by zero.
            {
                pv = -fv - pmt * nper;
            }
            else
            {
                pv = (-fv + pmt * ((Math.Pow(1 + rate, nper) - 1) / rate) * (1 + rate * type)) / Math.Pow(1 + rate, nper);
            }

            return pv;
        }
        //PMT
        public double Payment(double rate, int nper, double pv, double fv = 0, int type = 0)
        {
            // Validate inputs
            if (nper <= 0)
            {
                throw new ArgumentOutOfRangeException("nper must be greater than zero.");
            }

            if (type != 0 && type != 1)
            {
                throw new ArgumentOutOfRangeException("type must be 0 or 1.");
            }

            double pmt;

            if (rate == 0) // Handle the case where the interest rate is 0 to avoid division by zero.
            {
                pmt = (-fv - pv) / nper;
            }
            else
            {
                pmt = (-fv + pv * Math.Pow(1 + rate, nper)) / (((Math.Pow(1 + rate, nper) - 1) / rate) * (1 + rate * type));
            }


            return pmt;
        }
    }

    public class ListOfInvestmentPlans
    {
        public required string PlanName { get; set; }
        public double InterestRate { get; set; }
        public string MonthlySipAmount { get; set; }
        public List<ProjectedGrowth> ProjectedGrowths { get; set; }
    }

    public class ProjectedGrowth
    {
        public int Year { get; set; }
        public double Amount { get; set; }
    }
    public class FuturePlanningRequest
    {
        public int CurrentAge { get; set; }
        public int RetirementAge { get; set; }
        public double CurrentMonthlyExpense { get; set; }
        public double InterestRate { get; set; }
        public int AnyCurrentInvestment { get; set; }
    }
    public class TradeSettings
    {
        //public string TradingSymbol { get; set; }
        public double RiskFactor { get; set; }
        public double EntryPrice { get; set; }
        //public double ExitPrice { get; set; }
        public double TargetPrice { get; set; }
        public double CapitalAmount { get; set; }
        public double StopLoss { get; set; }
        public bool IsBuy { get; set; }

    }

    public class RRC
    {
        public double RiskAmount { get; set; }
        public int RecommendedQuantity { get; set; }
        public double TargetPrice { get; set; }
        public double ProfitAndLoss { get; set; }
        public string RiskRewardRatio { get; set; }
    }
}



//// Example usage:
//public class Example
//{
//    public static void Main(string[] args)
//    {
//        // Example 1: Future Value of a Lump Sum Investment
//        double fv1 = Financial.FutureValue(0.04 / 4, 10 * 4, 0, -1000);
//        Console.WriteLine($"Future Value (Lump Sum): {fv1:C}"); // Output: e.g., $1,489.85

//        // Example 2: Future Value of an Annuity
//        double fv2 = Financial.FutureValue(0.06 / 12, 20 * 12, -200, 0);
//        Console.WriteLine($"Future Value (Annuity): {fv2:C}"); // Output: e.g., $81,939.67

//        // Example 3: Future value when rate is 0
//        double fv3 = Financial.FutureValue(0, 10 * 4, -100, -1000);
//        Console.WriteLine($"Future Value (Rate 0): {fv3:C}"); // Output: e.g., $5,000.00
//    }
//}