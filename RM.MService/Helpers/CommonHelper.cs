namespace RM.MService.Helpers
{
    internal class CommonHelper
    {
        public static string ValidateTradingSymbol(string name)
        {
            if (name.Contains("-"))
            {
                name = name.Replace("-", "_");
            }
            return name;
        }
    }
}
