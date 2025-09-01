using System.Collections.Generic;

namespace RM.Model.ResponseModel
{
    public class ScreenerModel
    {
        public class GetScreenerDetailssP
        {
            public int ScreenerId { get; set; }
            public string ScreenerName { get; set; }
            public string Code { get; set; }
            public string ScreenerDescription { get; set; }
            public bool ScreenerIsActive { get; set; }
            public string ScreenerIcon { get; set; }
            public int CategoryId { get; set; }
            public string CategoryName { get; set; }
            public string CategoryDescription { get; set; }
            public string? CategoryImage { get; set; }
            public string? BackgroundColor { get; set; }
            public bool CategoryIsActive { get; set; }
            public bool SubscriptionStatus { get; set; }
        }
        public class ScreenerCategoryModel
        {
            public int CategoryId { get; set; } // Unique ID of the category
            public string CategoryName { get; set; } // Name of the category
            public string CategoryDescription { get; set; } // Description of the category
            public bool SubscriptionStatus { get; set; }
            public string Image { get; set; }
            public string BackgroundColor { get; set; }
            public List<ScreenerModel.Screener> Screeners { get; set; } = new List<ScreenerModel.Screener>(); // List of screeners under this category
        }

        public class Screener
        {
            public int Id { get; set; } // Unique ID of the screener
            public string Name { get; set; } // Name of the screener
            public string Code { get; set; } // Code of the screener
            public string ScreenerDescription { get; set; } // Description of the screener
            public string Icon { get; set; }
            public string BackgroundColor { get; set; }
        }


        public class ScreenerStockDataRequest
        {
            public string ScreenerCategoryId { get; set; }
            public string Symbol { get; set; } // Ticker symbol of the stock (e.g., AAPL, MSFT)
            public decimal LastPrice { get; set; } // Current real-time price of the stock
        }

        public class GetScreenerDataSpResponseModel
        {
            public long Id { get; set; }
            public string Symbol { get; set; }
            public string Name { get; set; }
            public string Logo { get; set; }
            public string Exchange { get; set; }
            public decimal TriggerPrice { get; set; }
            public string ChartUrl { get; set; }
            public string ModifiedOn { get; set; }
        }

        //public class ScreenerStockResponse
        //{
        //    public int Id { get; set; }
        //    public string ScreenerId { get; set; }
        //    public string Logo { get; set; } // URL or path to the stock logo image
        //    public string Symbol { get; set; } // Ticker symbol of the stock (e.g., AAPL, MSFT)
        //    public string Name { get; set; } // Full name of the stock (e.g., Apple Inc.)
        //    public decimal LastPrice { get; set; } // Current real-time price of the stock
        //    public decimal ProfitAndLoss { get; set; } // Profit or loss percentage or value
        //    public string ChartUrl { get; set; } // URL to the stock chart
        //    public DateTime ModifiedOn { get; set; }
        //}
    }

}