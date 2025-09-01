namespace JarvisAlgo.Partner
{
    public static class StockNameValidation
    {
        public static string StockNameUnderscoreCheck(string stockName)
        {
            if (stockName != null && stockName.Contains("-"))
            {
                stockName = stockName.Replace('-', '_');
            }
            return stockName;
        }

        public static string StockNamePostFixCheck(string stockName)
        {
            stockName = stockName.ToLower().ToUpper();
            if (stockName != null)
            {
                stockName = stockName.Replace("-I", "");
                //stockName = stockName.Replace("_i", ""); 
                stockName = stockName.Replace(".NSE", "");
                //stockName = stockName.Replace(".nse", "");
                stockName = stockName.Replace("_NSE", "");
                //stockName = stockName.Replace("_nse", "");
                stockName = stockName.Replace(".NFO", "");
                stockName = stockName.Replace("_NFO", "");
            }
            return stockName;
        }
    }
}