using Azure.Storage.Blobs.Models;
using FirebaseAdmin.Messaging;
using RM.CommonServices;
using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.KingResearchContext;
using RM.Model;
using RM.Model.Common;
using RM.Model.RequestModel.Notification;
using RM.Model.ResponseModel;
using RM.MService.Helpers;
using RM.NotificationService;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Data;
using System.Net;
using static RM.Model.ResponseModel.ScreenerModel;
using Index = RM.Model.Index;

namespace RM.MService.Services
{
    public class ScreenerService
    {
        private readonly IMobileNotificationService _notificationService;

        private readonly FirebaseRealTimeDb _realDb;
        private readonly StockMarketContractsService _stockData;
        private readonly KingResearchContext _context;
        readonly ApiCommonResponseModel apiCommonResponse = new();
        readonly IConfiguration _configuration;
        private static DateTime? lastTriggerDate = null; // Static field to store the last trigger date

        public ScreenerService(KingResearchContext repository, StockMarketContractsService stockData, IMobileNotificationService notificationService, IConfiguration configuration, FirebaseRealTimeDb realDb)
        {
            _notificationService = notificationService;
            _realDb = realDb;
            _stockData = stockData;
            _context = repository;
            _configuration = configuration;
        }

        /// <summary>
        /// This Method Is Used To Bind The Screener Category and all the screener under each ScreenerCategory.
        /// </summary>
        public async Task<ApiCommonResponseModel> GetScreenerCategoryData(QueryValues query)
        {
            // Get screener data
            var sqlParameters = ProcedureCommonSqlParameters.GetCommonSqlParameters(query);
            var procedureResponse = await _context.SqlQueryToListAsync<GetScreenerDetailssP>(
                ProcedureCommonSqlParametersText.GetScreenerDetails,
                sqlParameters.ToArray()
            );

            // Parse MobileUserKey from query
            var hasValidUser = Guid.TryParse(query.LoggedInUser, out Guid mobileUserKey);

            // Fetch MyBucket product IDs only if user is valid
            var myBucketProductIds = hasValidUser
                ? await _context.MyBucketM
                   .Where(m => m.MobileUserKey == mobileUserKey && (m.EndDate == null || m.EndDate > DateTime.Today))
                    .Select(m => m.ProductId)
                    .ToListAsync()
                : new List<int>();

            // Fetch MISCANNER product ID
            var miscannerProductId = await _context.ProductsM
                .Where(p => p.Code == "MISCANNER")
                .Select(p => (int?)p.Id)
                .FirstOrDefaultAsync();

            // Filter and simplify data
            var result = SimplifyData(
                procedureResponse,
                code => code.Equals("MISCANNER", StringComparison.OrdinalIgnoreCase) ? miscannerProductId : null,
                productId => myBucketProductIds.Contains(productId)
            );

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Data = result
            };
        }


        private static List<ScreenerCategoryModel> SimplifyData(
    List<GetScreenerDetailssP> procedureResponse,
    Func<string, int?> getProductIdByCodeFunc,
    Func<int, bool> isProductInMyBucketFunc)
        {
            return procedureResponse
                .GroupBy(r => new { r.CategoryId, r.CategoryName, r.CategoryDescription, r.CategoryImage })
                .Select(group => new ScreenerCategoryModel
                {
                    CategoryId = group.Key.CategoryId,
                    CategoryName = group.Key.CategoryName,
                    CategoryDescription = group.Key.CategoryDescription,
                    Image = group.Key.CategoryImage,
                    Screeners = group
                        .Where(screener =>
                        {
                            // Apply filtering only for MISCANNER
                            if (!string.Equals(screener.Code, "MISCANNER", StringComparison.OrdinalIgnoreCase))
                                return true;

                            var productId = getProductIdByCodeFunc(screener.Code);
                            return productId.HasValue && isProductInMyBucketFunc(productId.Value);
                        })
                        .Select(s => new ScreenerModel.Screener
                        {
                            Id = s.ScreenerId,
                            Name = s.ScreenerName,
                            Code = s.Code,
                            Icon = s.ScreenerIcon,
                            BackgroundColor = s.BackgroundColor,
                            ScreenerDescription = s.ScreenerDescription
                        })
                        .ToList()
                })
                .ToList();
        }

        public async Task<ApiCommonResponseModel> GetScreenerData(string code, int screenerId)
        {
            SqlParameter[] sqlParameters = new[]
            {
        new SqlParameter
        {
            ParameterName = "screenerId",
            Value = screenerId,
            SqlDbType = SqlDbType.Int,
        },
    };

            var procedureResponse = await _context.SqlQueryToListAsync<GetScreenerDataSpResponseModel>(
                ProcedureCommonSqlParametersText.GetScreenerData,
                sqlParameters.ToArray()
            );

            #region Calculate the netChange from previous day price 
            var finalResponse = new List<object>();

            if (procedureResponse != null)
            {
                foreach (var response in procedureResponse)
                {
                    var symbolData = await _stockData.FilterData(response.Symbol, null, exchange: "NSE");
                    double previousDayPrice = Convert.ToDouble(symbolData.FirstOrDefault()?.Price ?? 0);
                    double netChange = 0.0;
                    double perChange = 0.0;

                    if (previousDayPrice > 0)
                    {
                        perChange = CalculatePercentageChange(Convert.ToDouble(response.TriggerPrice), previousDayPrice);
                        netChange = Math.Round(Convert.ToDouble(response.TriggerPrice) - previousDayPrice, 2);
                    }

                    finalResponse.Add(new
                    {
                        response.Symbol,
                        response.TriggerPrice,
                        NetChange = netChange,
                        PercentageChange = perChange,
                        response.ChartUrl,
                        response.Exchange,
                        response.Name,
                        response.Logo,
                        response.Id,
                        response.ModifiedOn
                    });
                }
            }

            #endregion

            return new ApiCommonResponseModel
            {
                Data = finalResponse,
                StatusCode = System.Net.HttpStatusCode.OK
            };
        }

        public void MapChartInkDataIntoDatabase()
        {

        }
        public async Task<ApiCommonResponseModel> ScreenerDataBinding(string body)
        {
            var apiResponse = new ApiCommonResponseModel();
            if (string.IsNullOrEmpty(body))
            {
                apiResponse.Message = "Body is empty";
                apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
            }
            else
            {
                ChartInkPost jsonObject = JsonConvert.DeserializeObject<ChartInkPost>(body.Replace("'", "\"")); // Replace single quotes with double quotes

                if (jsonObject != null)
                {
                    CommonServices.Helpers.FileHelper.WriteToFile(jsonObject.alert_name + "_" + DateTime.Now.ToString("ddMMM"), body);
                    apiResponse.Message = await this.InitializeChartinkData(jsonObject);
                    apiResponse.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    apiResponse.Message = "ChartInk Data Not Found";
                    apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                }
            }
            return apiResponse;
        }
        /// <summary>
        /// Chart ink data will come in model and then it will bind to respective table like Breakfast , 52week high.log
        /// </summary>
        private async Task<string> InitializeChartinkData(ChartInkPost jsonObject)
        {
            var message = "";
            if (jsonObject == null || jsonObject.stocks == null || jsonObject.trigger_prices == null)
            {
                message = "Object is null";
                return message;
            }
            List<string> stockList = jsonObject.stocks.Split(",").ToList();
            List<string> stockPriceList = jsonObject.trigger_prices.Split(",").ToList();

            // Ensure both lists have the same number of elements
            if (stockList.Count != stockPriceList.Count)
            {
                message = "The Price and Stock name list are not of the same length.";
                return message;
            }


            if (jsonObject.alert_name == "NiftyBankniftyUpdates")
            {
                apiCommonResponse.Message = "NiftyBankniftyUpdates Initated successfully";
                await BankniftyNiftyUpdate(jsonObject);
            }
            else if ((jsonObject.alert_name.ToLower() == "R43MinBreakOutStocks".ToLower() || jsonObject.alert_name.ToLower() == "R45minBreakfast".ToLower()))
            {
                apiCommonResponse.Message = "R43MinBreakOutStocks Initated successfully";
                await R4BreakOutStocks(jsonObject);

                DateTime today = DateTime.UtcNow.Date; // Use DateTime.UtcNow or DateTime.Now depending on your time zone needs
                if (lastTriggerDate == null || lastTriggerDate.Value.Date != today)
                {
                    await FirstTimeTriggered(); // Call the FirstTimeTriggered method
                    lastTriggerDate = today;   // Update the last trigger date
                }
            }
            else if ("TopGainers".ToLower() == jsonObject.alert_name.ToLower())
            {
                apiCommonResponse.Message = "TopGainersInitated successfully";
                await GetTopGainers(stockList, stockPriceList);
            }
            else if ("TopLosers".ToLower() == jsonObject.alert_name.ToLower())
            {
                apiCommonResponse.Message = "TopLossers Initated successfully";
                await GetTopLosers(stockList, stockPriceList);
            }
            else if (jsonObject.alert_name.ToLower() == "scalpingstrategy")
            {
                await ScalpingStrategyStocks(jsonObject);
                apiCommonResponse.Message = "ScalpingStrategy Initated successfully";
            }
            else
            {
                apiCommonResponse.Message = "MISCANNER Initated successfully";
                await OtherDataProcess(stockList, stockPriceList, jsonObject.alert_name, message);
            }
            return message;
        }

        private async Task OtherDataProcess(List<string> stockList, List<string> stockPriceList, string alert_name, string message)
        {
            List<SymbolList> symbolData;

            DataTable tvpTable = new()
            {
                TableName = "TVP_ScreenerStock"
            };
            tvpTable.Columns.Add("Symbol", typeof(string)).MaxLength = 50;
            tvpTable.Columns.Add("Exchange", typeof(string)).MaxLength = 10;
            tvpTable.Columns.Add("LastPrice", typeof(decimal));
            for (int i = 0; i < stockList.Count; i++)
            {
                if (stockList[i] != "BANKNIFTY" && stockList[i] != "NIFTY" && stockList[i] != "FINNIFTY" && stockList[i] != "BANKEX")
                {
                    symbolData = await _stockData.FilterData(stockList[i], null, "NSE");
                    double price = Convert.ToDouble(stockPriceList[i]);//Convert.ToDouble(symbolData.FirstOrDefault()?.Price);
                    tvpTable.Rows.Add(stockList[i], symbolData.FirstOrDefault()?.Exchange, price);
                }
            }

            List<SqlParameter> sqlParameters2 = new()
            {
                new SqlParameter { ParameterName = "@ScreenerStocks", Value = tvpTable, SqlDbType = SqlDbType.Structured, TypeName = "TVP_ScreenerStock" },
                new SqlParameter { ParameterName = "@ScreenerCategoryName", Value = alert_name, SqlDbType = SqlDbType.VarChar, Size = 100 }
            };
            var resykt = await _context.SqlQueryToListAsync<CommonResponse>(ProcedureCommonSqlParametersText.ManageScreenerStocksData, sqlParameters2.ToArray());

            if (resykt != null)
            {
                message = "Successfully imported total= " + resykt?.FirstOrDefault().Total + " Screener Data";
            }
        }

        private async Task FirstTimeTriggered()
        {
            // ToDo : Logic to get the users list and then send push notification to them
            _ = _notificationService.SendFreeNotification(new SendFreeNotificationRequestModel
            {
                Body = "Check MyScanner; the stock has been triggered for the day.",
                NotificationScreenName = "scannersScreen",
                TargetAudience = "BREAKFAST",
                Title = "My scanner stock has been triggered.",
                Topic = "ANNOUNCEMENT",
                TurnOnNotification = true
            });
        }
        private double CalculatePercentageChange(double newPrice, double oldPrice)
        {
            double change = newPrice - oldPrice;
            double percentageChange = (change / oldPrice) * 100;
            return Math.Round(percentageChange, 2);
        }
        public Task R4BreakoutStocksDataProcess(string requestBody)
        {
            throw new NotImplementedException();
        }

        private async Task GetTopGainers(List<string> stockList, List<string> stockPriceList)
        {
            var topGainers = new List<TopGainersLosersData>();

            for (int i = 0; i < stockList.Count; i++)
            {
                var symbol = stockList[i].ToUpper();
                if (symbol is "BANKNIFTY" or "NIFTY" or "SENSEX") continue;

                var symbolData = await _stockData.FilterData(stockList[i], null, "NSE");

                if (symbolData.Count > 1)
                {
                    symbolData = symbolData.Where(item => item.TradingSymbol == stockList[i]).ToList();
                }

                var price = Convert.ToDouble(symbolData.FirstOrDefault()?.Price);
                var ltp = Convert.ToDouble(stockPriceList[i]);

                if (symbolData != null && price > 0)
                {
                    var netChange = Math.Round(ltp - price, 2);
                    var perChange = CalculatePercentageChange(ltp, price);

                    if (netChange > 0)
                    {
                        topGainers.Add(new TopGainersLosersData
                        {
                            netChange = netChange,
                            percentChange = perChange,
                            ltp = ltp,
                            tradingSymbol = stockList[i],
                        });
                    }
                }
            }
            var updateDate = false;

            if (topGainers != null && topGainers.Count > 0)
            {
                updateDate = true;
                topGainers = topGainers.OrderByDescending(item => item.percentChange).Distinct().Take(10).ToList();
                await _realDb.UpdateRealTimeMarket("TopGainers", JsonConvert.SerializeObject(topGainers));
            }
            await _realDb.UpdateCreatedDateTimeForRealTimeMarketValues(updateDate);
        }

        private async Task GetTopLosers(List<string> stockList, List<string> stockPriceList)
        {
            var topLosers = new List<TopGainersLosersData>();

            for (int i = 0; i < stockList.Count; i++)
            {
                var symbol = stockList[i].ToUpper();
                if (symbol is "BANKNIFTY" or "NIFTY" or "SENSEX") continue;

                var symbolData = await _stockData.FilterData(stockList[i], null, "NSE");

                if (symbolData.Count > 1)
                {
                    symbolData = symbolData.Where(item => item.TradingSymbol == stockList[i]).ToList();
                }

                var price = Convert.ToDouble(symbolData.FirstOrDefault()?.Price);
                var ltp = Convert.ToDouble(stockPriceList[i]);

                if (symbolData != null && price > 0)
                {
                    var netChange = Math.Round(ltp - price, 2);
                    var perChange = CalculatePercentageChange(ltp, price);

                    if (netChange < 0)
                    {
                        topLosers.Add(new TopGainersLosersData
                        {
                            netChange = netChange,
                            percentChange = perChange,
                            ltp = ltp,
                            tradingSymbol = stockList[i],
                        });
                    }
                }
            }
            var updateDate = false;

            if (topLosers != null && topLosers.Count > 0)
            {
                updateDate = true;
                topLosers = topLosers.OrderByDescending(item => item.percentChange).Distinct().Take(10).ToList();
                await _realDb.UpdateRealTimeMarket("TopLosers", JsonConvert.SerializeObject(topLosers));
            }
            await _realDb.UpdateCreatedDateTimeForRealTimeMarketValues(updateDate);

        }


        public async Task TopGainerLooserDataProcess(ChartInkPost jsonObject)
        {
            try
            {
                List<string> stockList = jsonObject.stocks.Split(",").ToList();
                List<string> stockPriceList = jsonObject.trigger_prices.Split(",").ToList();
                // Ensure both lists have the same number of elements
                if (stockList.Count != stockPriceList.Count)
                {
                    Console.WriteLine("The lists are not of the same length.");
                    return;
                }
                var netChange = 0.0;
                double price = 0.0;
                List<SymbolList> symbolData = null;

                List<TopGainersLosersData> topGainersTemp = new();
                List<TopGainersLosersData> topLoosersTemp = new();

                for (int i = 0; i < stockList.Count; i++)
                {
                    if (stockList[i].ToUpper() != "BANKNIFTY" || stockList[i].ToUpper() != "NIFTY" || stockList[i].ToUpper() != "SENSEX")
                    {
                        symbolData = await _stockData.FilterData(stockList[i], null, "NSE");

                        if (symbolData.Count() > 1)
                        {
                            symbolData = symbolData.Where(item => item.TradingSymbol == stockList[i]).ToList();
                        }

                        price = Convert.ToDouble((symbolData.FirstOrDefault()?.Price));
                        if (symbolData != null && price > 0)
                        {
                            var perChange = CalculatePercentageChange(Convert.ToDouble(stockPriceList[i]), price);

                            netChange = Math.Round(Convert.ToDouble(stockPriceList[i]) - price, 2);
                            if (netChange > 0)
                            {
                                topGainersTemp.Add(new TopGainersLosersData
                                {
                                    netChange = netChange,
                                    percentChange = perChange,
                                    ltp = Convert.ToDouble(stockPriceList[i]),
                                    tradingSymbol = stockList[i],
                                });
                            }
                            else
                            {
                                topLoosersTemp.Add(new TopGainersLosersData
                                {
                                    netChange = netChange,
                                    percentChange = perChange,
                                    ltp = Convert.ToDouble(stockPriceList[i]),
                                    tradingSymbol = stockList[i],
                                });
                            }
                        }
                    }
                }

                var updateDate = false;

                if (topGainersTemp != null && topGainersTemp.Count > 0)
                {
                    updateDate = true;
                    topGainersTemp = topGainersTemp.OrderByDescending(item => item.percentChange).Distinct().Take(10).ToList();
                    await _realDb.UpdateRealTimeMarket("TopGainers", JsonConvert.SerializeObject(topGainersTemp));
                }

                if (topLoosersTemp != null && topLoosersTemp.Count > 0)
                {
                    updateDate = true;
                    topLoosersTemp = topLoosersTemp.OrderByDescending(item => item.percentChange).Distinct().Take(10).ToList();
                    await _realDb.UpdateRealTimeMarket("TopLosers", JsonConvert.SerializeObject(topLoosersTemp));
                }
                await _realDb.UpdateCreatedDateTimeForRealTimeMarketValues(updateDate);
            }
            catch (Exception ex)
            {
                CommonServices.Helpers.FileHelper.WriteToFile("Exception" + DateTime.Now.ToString("ddMMM"), ex.ToString());
            }
        }
        public async Task R4BreakOutStocks(ChartInkPost jsonObject)
        {

            List<string> stockList = jsonObject.stocks.Split(",").ToList();
            List<string> stockPriceList = jsonObject.trigger_prices.Split(",").ToList();

            var netChange = 0.0;
            double price = 0.0;
            List<SymbolList> symbolData = null;

            if (stockList.Count != stockPriceList.Count)
            {
                Console.WriteLine("The lists are not of the same length.");
                return;
            }

            if (_stockData.IsStockListNull())
            {
                _ = _stockData.LoadStockDetailsFromFile();
            }

            CamrillaScanner camrillaScanner = new()
            {
                Breakfast = new()
            };

            var tradingUrl = _configuration["AppSettings:TradingViewChartUrl"];


            for (int i = 0; i < stockList.Count; i++)
            {
                symbolData = await _stockData.FilterData(stockList[i], null, "NSE");
                price = Convert.ToDouble((symbolData.FirstOrDefault()?.Price));
                netChange = Math.Round(Convert.ToDouble(stockPriceList[i]) - price, 2);
                if (symbolData != null && price > 0)
                {
                    var perChange = CalculatePercentageChange(Convert.ToDouble(stockPriceList[i]), price);
                    camrillaScanner.Breakfast.Add(new CamrillaR4Model
                    {
                        //PercentChange = perChange.ToString(),             // Code Commented DUE TO VIjay sir Request
                        TradingSymbol = stockList[i],
                        Ltp = stockPriceList[i],
                        Close = stockPriceList[i],
                        NetChange = netChange,
                        ViewChart = tradingUrl + "NSE:" + CommonHelper.ValidateTradingSymbol(stockList[i])
                    });
                }
            }
            var fileName = "BreakfastScanner_" + DateTime.Now.ToString("ddMMM");
            var filePath = @"D:\\" + "SymbolData\\" + fileName;

            if (camrillaScanner.Breakfast != null && camrillaScanner.Breakfast.Count > 0)
            {
                string symbolDataJson = "";

                if (File.Exists(filePath))
                {
                    symbolDataJson = File.ReadAllText(filePath);
                }

                List<CamrillaR4Model> fileData = JsonConvert.DeserializeObject<List<CamrillaR4Model>>(symbolDataJson);

                if (fileData is not null)
                {
                    camrillaScanner.Breakfast = fileData.Concat(camrillaScanner.Breakfast)
                    .GroupBy(item => item.TradingSymbol) // Assuming 'Name' is a unique identifier for BreakfastItem
                    .Select(group => group.First())
                    .ToList();
                }

                camrillaScanner.CurrentDate = DateTime.Now.ToString("yy-MM-dd HH:mm:ss");
                camrillaScanner.Breakfast = camrillaScanner.Breakfast.OrderByDescending(item => item.PercentChange).Take(50).ToList();
            }

            if (camrillaScanner.Breakfast is not null && camrillaScanner.Breakfast.Count > 0)
            {
                // Serialize just the list — not the object that contains "Breakfast"
                var breakfastListJson = JsonConvert.SerializeObject(camrillaScanner.Breakfast);

                await _realDb.UpdateBreakfastChildNode("Breakfast", breakfastListJson);
                await _realDb.UpdateCreatedDateTimeForBreakfast(true);
                CommonServices.Helpers.FileHelper.ReplaceFileContent(filePath, JsonConvert.SerializeObject(camrillaScanner.Breakfast));
            }

        }
        public async Task BankniftyNiftyUpdate(ChartInkPost jsonObject)
        {

            List<string> stockList = jsonObject.stocks.Split(",").ToList();
            List<string> stockPriceList = jsonObject.trigger_prices.Split(",").ToList();
            // Ensure both lists have the same number of elements

            var netChange = 0.0;
            double price = 0.0;
            List<SymbolList>? symbolData = null;

            if (stockList.Count != stockPriceList.Count)
            {
                Console.WriteLine("The lists are not of the same length.");
                return;
            }

            if (_stockData.IsStockListNull())
            {
                await _stockData.LoadStockDetailsFromFile();
            }

            Index indexPrice = new();

            for (int i = 0; i < stockList.Count; i++)
            {
                symbolData = await _stockData.FilterData(stockList[i], "XX", "NFO");
                price = Convert.ToDouble((symbolData.FirstOrDefault()?.Price));

                if (symbolData != null && price > 0)
                {
                    var perChange = CalculatePercentageChange(Convert.ToDouble(stockPriceList[i]), price);

                    if (stockList[i].ToUpper() == "BANKNIFTY")
                    {
                        indexPrice.BNF = new()
                                    {
                                        stockPriceList[i],
                                        perChange.ToString()
                                    };
                    }
                    else if (stockList[i].ToUpper() == "NIFTY")
                    {
                        indexPrice.Nifty = new()
                                    {
                                        stockPriceList[i],
                                        perChange.ToString()
                                    };
                    }
                    else if (stockList[i].ToUpper() == "SENSEX")
                    {
                        indexPrice.Sensex = new()
                                    {
                                        stockPriceList[i],
                                        perChange.ToString()
                                    };
                    }
                }
            }

            if ((indexPrice != null && (indexPrice.BNF is not null || indexPrice.Nifty is not null)))
            {
                await _realDb.UpdateRealTimeMarket("Index", JsonConvert.SerializeObject(indexPrice));
                await _realDb.UpdateCreatedDateTimeForRealTimeMarketValues(true);
            }
        }

        public async Task ScalpingStrategyStocks(ChartInkPost jsonObject)
        {
            List<string> stockList = jsonObject.stocks.Split(",").ToList();
            List<string> stockPriceList = jsonObject.trigger_prices.Split(",").ToList();

            var netChange = 0.0;
            double price = 0.0;
            List<SymbolList> symbolData = null;

            if (stockList.Count != stockPriceList.Count)
            {
                Console.WriteLine("The lists are not of the same length.");
                return;
            }

            if (_stockData.IsStockListNull())
            {
                _ = _stockData.LoadStockDetailsFromFile();
            }

            ScalpingStrategyScanner scalpingScanner = new()
            {
                Stocks = new()
            };

            var tradingUrl = _configuration["AppSettings:TradingViewChartUrl"];

            for (int i = 0; i < stockList.Count; i++)
            {
                symbolData = await _stockData.FilterData(stockList[i], null, "NSE");
                price = Convert.ToDouble((symbolData.FirstOrDefault()?.Price));
                netChange = Math.Round(Convert.ToDouble(stockPriceList[i]) - price, 2);

                if (symbolData != null && price > 0)
                {
                    var perChange = CalculatePercentageChange(Convert.ToDouble(stockPriceList[i]), price);
                    scalpingScanner.Stocks.Add(new ScalpingStrategyStocks
                    {
                        PercentChange = perChange.ToString(),
                        TradingSymbol = stockList[i],
                        Ltp = stockPriceList[i],
                        Close = stockPriceList[i],
                        NetChange = netChange,
                        ViewChart = tradingUrl + "NSE:" + CommonHelper.ValidateTradingSymbol(stockList[i])
                    });
                }
            }

            var fileName = "ScalpingStrategy_" + DateTime.Now.ToString("ddMMM");
            var filePath = @"D:\SymbolData\" + fileName;

            if (scalpingScanner.Stocks != null && scalpingScanner.Stocks.Count > 0)
            {
                string symbolDataJson = "";

                if (File.Exists(filePath))
                {
                    symbolDataJson = File.ReadAllText(filePath);
                }

                List<ScalpingStrategyStocks> fileData = JsonConvert.DeserializeObject<List<ScalpingStrategyStocks>>(symbolDataJson);


                if (fileData is not null)
                {
                    scalpingScanner.Stocks = fileData.Concat(scalpingScanner.Stocks)
                    .GroupBy(item => item.TradingSymbol) // Assuming 'Name' is a unique identifier for BreakfastItem
                    .Select(group => group.First())
                    .ToList();
                }

                scalpingScanner.CurrentDate = DateTime.Now.ToString("yy-MM-dd HH:mm:ss");
                scalpingScanner.Stocks = scalpingScanner.Stocks.OrderByDescending(item => item.PercentChange).Take(50).ToList();
            }

            if (scalpingScanner.Stocks is not null && scalpingScanner.Stocks.Count > 0)
            {
                // TODO: Currently saving BreakfastScanner data under "BreakfastScanner/Scalping". This is a temporary structure for grouping.
                // In the future, split "Scalping" and "BreakfastScanner" into separate root-level nodes if needed.
                await _realDb.UpdateBreakfastChildNode("Scalping", JsonConvert.SerializeObject(scalpingScanner.Stocks));
                CommonServices.Helpers.FileHelper.ReplaceFileContent(filePath, JsonConvert.SerializeObject(scalpingScanner.Stocks));
            }
        }

        public async Task UpdateContracts(NotificationToMobileRequestModel obj)
        {
            if (_stockData.IsStockListNull())
            {
                await _stockData.LoadStockDetailsFromFile(true);
            }
            await _notificationService.SendNotificationToMobile(obj);
        }
        public List<SymbolList> GetNfoContracts()
        {
            return _stockData.GetData();
        }
        public void UpdateStockPrice()
        {
            //Update Stock Price for firebase stocks 
        }

        public async Task MangeFirebaseNotification()
        {
            var data = new
            {
                Message = "Call the api to fetch the content to display on mobile screen",
                CurrentDate = DateTime.Now.ToString("dd-MMM-yy HH:MM:ss")
            };
            await _realDb.UpdateNotification(data);
        }
    }
}