using RM.CommonServices.Services;
using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.KingResearchContext;
using RM.Model;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static RM.Model.Models.PerformanceModel;

namespace RM.MService.Services
{
    public interface IPerformanceService
    {

        Task<ApiCommonResponseModel> GetPerformance(string code);
        ApiCommonResponseModel GetPerformanceHeader();
    }

    public class PerformanceService : IPerformanceService
    {
        private readonly ApiCommonResponseModel _responseModel = new ApiCommonResponseModel();
        private readonly KingResearchContext _dbContext;
        private readonly MongoDbService _mongoDbService;

        public PerformanceService(KingResearchContext context, MongoDbService mongoDbService)
        {
            _dbContext = context;
            _mongoDbService = mongoDbService;
        }


        /// <summary>
        /// this method is used to get the scanner performance of all scanner for history
        /// </summary>
        public async Task<ApiCommonResponseModel> GetPerformance(string code)
        {
            if (!string.Equals(code, "si", StringComparison.OrdinalIgnoreCase) && !string.Equals(code, "mb", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(code, "KKBA", StringComparison.OrdinalIgnoreCase)) code = "KALKIBAATAAJ";
                else if (string.Equals(code, "BREAKFAST", StringComparison.OrdinalIgnoreCase)) code = "BREAKFAST";
                else return _responseModel;

                //var notificationsData = await _mongoDbService.GetNotificationFromTopic(code);
                var breakfastNotificationsFromSql = await _dbContext.ScannerPerformanceM.Where(x => x.Topic == code).ToListAsync();
                var formattedNotifications = breakfastNotificationsFromSql
                .Where(n => !string.IsNullOrEmpty(n.TradingSymbol)
                            )
                .GroupBy(n => new
                {
                    Date = n.SentAt.Date,
                    Symbol = n.TradingSymbol
                })
                .OrderByDescending(group => group.Key.Date)
                .Select(group =>
                {
                    var orderedNotifications = group.OrderBy(n => n.CreatedOn).ToList();
                    var firstNotification = orderedNotifications.First();
                    var lastNotification = orderedNotifications.Count > 1 ? orderedNotifications.Last() : null;
                    return new Trade
                    {
                        Symbol = group.Key.Symbol,
                        Cmp = null,
                        Duration = "N/A",
                        InvestmentMessage = firstNotification.Ltp > 0 && lastNotification != null
                            ? $"Captured {Math.Round(lastNotification.Ltp - firstNotification.Ltp ?? 0)} {(Math.Round(lastNotification.Ltp ?? 0 - firstNotification.Ltp ?? 0) > 0 ? "levels" : "level")}."
                            : "Not applicable for this.",
                        Roi = firstNotification.Ltp != 0 && lastNotification != null
                            ? Math.Round(((lastNotification.Ltp - firstNotification.Ltp) / firstNotification.Ltp ?? 0) * 100, 2).ToString()
                            : "N/A",
                        Status = "Open",
                        EntryPrice = CurrencyHelper.ConvertToINRFormat(firstNotification.Ltp ?? 0),
                        ExitPrice = lastNotification != null ? CurrencyHelper.ConvertToINRFormat(lastNotification.Ltp ?? 0) : null,
                    };
                }).ToList();

                var performance = new PerformanceData
                {
                    Balance = "N/A",
                    Statistics = new Statistics
                    {
                        TotalTrades = formattedNotifications.Count(),
                        TotalProfitable = 0,
                        TotalLoss = 0,
                        TradeClosed = 0,
                        TradeOpen = 0
                    },
                    Trades = formattedNotifications
                };

                _responseModel.StatusCode = System.Net.HttpStatusCode.OK;
                _responseModel.Data = performance;
                return _responseModel;
            }


            SqlParameter totalTrades = new()
            {
                ParameterName = "TotalTrades",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output,
            };
            SqlParameter totalProfitable = new()
            {
                ParameterName = "TotalProfitable",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output,
            };
            SqlParameter totalLoss = new()
            {
                ParameterName = "TotalLoss",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output,
            };
            SqlParameter tradeClosed = new()
            {
                ParameterName = "TradeClosed",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output,
            };
            SqlParameter tradeOpen = new()
            {
                ParameterName = "TradeOpen",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output,
            };
            SqlParameter balance = new()
            {
                ParameterName = "Balance",
                SqlDbType = System.Data.SqlDbType.NVarChar,
                Size = 100,
                Direction = System.Data.ParameterDirection.Output,
            };

            List<SqlParameter> sqlParameters = new()
            {
                 new SqlParameter
                {
                    ParameterName = "Code",
                     Value = string.IsNullOrWhiteSpace(code) ? DBNull.Value : code,
                     SqlDbType = System.Data.SqlDbType.VarChar,
                     Size = 50
                },
                totalTrades,totalProfitable,totalLoss,tradeClosed,balance,tradeOpen
            };

            var spResult = await _dbContext.SqlQueryToListAsync<Trade>(ProcedureCommonSqlParametersText.GetCallPerformanceM, sqlParameters.ToArray());

            int totalTradesValue = totalTrades.Value == DBNull.Value ? 0 : (int)totalTrades.Value;
            int totalProfitableValue = totalProfitable.Value == DBNull.Value ? 0 : (int)totalProfitable.Value;
            int totalLossValue = totalLoss.Value == DBNull.Value ? 0 : (int)totalLoss.Value;
            int tradeClosedValue = tradeClosed.Value == DBNull.Value ? 0 : (int)tradeClosed.Value;
            int tradeOpenValue = tradeOpen.Value == DBNull.Value ? 0 : (int)tradeOpen.Value;
            string balanceValue = balance.Value == DBNull.Value ? string.Empty : (string)balance.Value;


            var performanceData = new PerformanceData
            {
                Balance = balanceValue,
                Statistics = new Statistics
                {
                    TotalTrades = totalTradesValue,
                    TotalProfitable = totalProfitableValue,
                    TotalLoss = totalLossValue,
                    TradeClosed = tradeClosedValue,
                    TradeOpen = tradeOpenValue
                },
                Trades = spResult
            };


            _responseModel.Data = performanceData;
            _responseModel.StatusCode = System.Net.HttpStatusCode.OK;

            return _responseModel;

        }

        public ApiCommonResponseModel GetPerformanceHeader()
        {
            var valu = new List<PerformanceCategory>{
            new PerformanceCategory
            {
                Id = 1,
                Code = "SI",
                Name = "Short Investment"
            },
            new PerformanceCategory
            {
                Id =2,
                Code = "MB",
                Name = "Multibagger"
            },
            new PerformanceCategory
            {
                Id =3,
                Code = "KKBA",
                Name = "Kal Ki Baat Aaj"
            }, new PerformanceCategory
            {
                Id =4,
                Code = "BREAKFAST",
                Name = "Breakfast"
            }};

            _responseModel.Data = valu;
            _responseModel.StatusCode = System.Net.HttpStatusCode.OK;
            return _responseModel;
        }
    }

    public class PerformanceCategory
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
}