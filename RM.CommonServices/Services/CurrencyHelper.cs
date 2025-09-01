using System.Globalization;

namespace RM.CommonServices.Services
{
    public class CurrencyHelper
    {
        public static string ConvertToINRFormat(decimal amount)
        {
            CultureInfo indianCulture = new CultureInfo("en-IN");
            string formattedAmount = amount.ToString("N0", indianCulture);
            formattedAmount = "₹" + formattedAmount;
            return formattedAmount;
        }
    }
}
