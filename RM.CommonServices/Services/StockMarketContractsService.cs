using RM.Model;

namespace RM.CommonServices
{

    public class StockMarketContractsService
    {
        private List<SymbolList> _stockContratsList; // Shared data list
        private readonly object _lock = new(); // Lock for thread-safe operations
        private bool _isDataLoaded; // Flag to track whether data is already loaded
        public StockMarketContractsService()
        {
            _stockContratsList = new List<SymbolList>();
            _isDataLoaded = false;

        }
        public bool IsStockListNull()
        {
            return _stockContratsList == null || _stockContratsList.Count == 0;
        }
        public async Task LoadStockDetailsFromFile(bool forceRefresh = false)
        {
            try
            {
                if (!forceRefresh && _isDataLoaded)
                {
                    Console.WriteLine("Data is already loaded. Use forceRefresh to reload.");
                    return;
                }

                // Ensure file path is correct
                string filePath = @"D:\SymbolData\symbolData.txt";
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"File not found: {filePath}");
                    return;
                }

                // Open the file stream for reading
                using var symbolDataJson = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                // Deserialize the JSON data
                var stockListt = System.Text.Json.JsonSerializer.DeserializeAsyncEnumerable<SymbolList>(symbolDataJson);

                if (_stockContratsList != null)
                {
                    lock (_stockContratsList) // Ensure thread safety for shared data
                    {
                        _stockContratsList.Clear();
                    }
                }

                if (stockListt is not null)
                {
                    // Add stocks to _dataList
                    await foreach (var stock in stockListt)
                    {
                        if (stock != null)
                        {
                            lock (_stockContratsList) // Ensure thread safety while modifying the list
                            {
                                _stockContratsList.Add(stock);
                            }
                        }
                    }
                    _isDataLoaded = true; // Mark data as loaded

                    Console.WriteLine("Stock data loaded successfully.");
                }
                else
                {
                    Console.WriteLine("No data found in the file.");
                }
            }
            catch (Exception ex)
            {
                // Handle and log any exceptions
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the current data list. This is thread-safe and available for all requests.
        /// </summary>
        public List<SymbolList> GetData()
        {
            lock (_lock)
            {
                return _stockContratsList; // Return the shared data
            }
        }
        /// <summary>
        /// Filters the data based on the provided criteria.
        /// </summary>
        public async Task<List<SymbolList>> FilterData(string? tradingSymbol = null, string? optionType = null, string? exchange = null)
        {
            if (_stockContratsList is null || _stockContratsList.Count == 0)
            {
                await LoadStockDetailsFromFile();
            }

            if (_stockContratsList is null) return new List<SymbolList>(); // Ensure it’s initialized

            lock (_lock)
            {
                string currentMonthAbbr = DateTime.UtcNow.ToString("MMM").ToUpper();

                var filteredList = _stockContratsList
                    .Where(item => item != null)
                    .Where(item =>
                        (string.IsNullOrEmpty(tradingSymbol) || item.TradingSymbol?.IndexOf(tradingSymbol, StringComparison.OrdinalIgnoreCase) >= 0) &&
                        (string.IsNullOrEmpty(optionType) || item.OptionType?.IndexOf(optionType, StringComparison.OrdinalIgnoreCase) >= 0) &&
                        (string.IsNullOrEmpty(exchange) || item.Exchange?.IndexOf(exchange, StringComparison.OrdinalIgnoreCase) >= 0))
                    .ToList();

                return filteredList
                    .OrderByDescending(item => item.Tsym != null && item.Tsym.Contains(currentMonthAbbr, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }



    }
}