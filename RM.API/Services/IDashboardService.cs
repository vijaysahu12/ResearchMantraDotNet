using RM.API.Dtos;
using RM.API.Models;
using RM.ChatGPT;
using RM.CommonServices;
using RM.CommonServices.Services;
using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.KingResearchContext;
using RM.Database.MongoDbContext;
using RM.Model;
using RM.Model.Common;
using RM.Model.Models;
using RM.Model.MongoDbCollection;
using RM.Model.ResponseModel;
using RM.MService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RM.API.Services
{

    public interface IDashboardService
    {
        Task<ApiCommonResponseModel> GetDetails();
        Task<ApiCommonResponseModel> GetSalesDashboardDetails(QueryValues request);
        Task<ApiCommonResponseModel> GetMobileDashboard(MobileDashboardQueryValues request);
        Task<ApiCommonResponseModel> GetLogsAndExceptionsForMobileDashboard();
    }
    public class DashboardService : IDashboardService
    {
        private readonly ApiCommonResponseModel responseModel = new();
        private readonly KingResearchContext _context;
        private readonly PreAndPostMarketService _preAndPostMarketService;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<PreMarketReport.PreMarketCollection> _collection;
        private readonly IMongoCollection<PostMarketReport.PostMarketCollection> _postCollection;
        private readonly IMongoCollection<Log> _logs;
        private readonly IMongoCollection<ExceptionLog> _exceptionLogs;
        private readonly StockInsightService _insightService;
        private readonly IMobileNotificationService _notificationService;
        private readonly IMemoryCache _cache;
        private readonly IMobileService _mobileService;

        public DashboardService(KingResearchContext context,
                    IOptions<MongoDBSettings> mongoDBSettings,
                    StockInsightService insightService,
                    IMobileNotificationService notificationService,
                    IMobileService mobileService, 
                    IMongoRepository<ExceptionLog> exceptionLog, 
                    IMongoRepository<Log> mongoRepo)
        {
            _context = context;
            _insightService = insightService;
            _notificationService = notificationService;
            _mobileService = mobileService;

            _preAndPostMarketService = new PreAndPostMarketService(mongoDBSettings, insightService, notificationService);

            // Optional: initialize _database and collections if needed separately
            var client = new MongoClient(mongoDBSettings.Value.ConnectionURI);
            _database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _collection = _database.GetCollection<PreMarketReport.PreMarketCollection>("PreMarketData");
            _postCollection = _database.GetCollection<PostMarketReport.PostMarketCollection>("PostMarketData");
        }
        public async Task<ApiCommonResponseModel> GetDetails()
        {
            responseModel.Data = await _context.SqlQueryFirstOrDefaultAsync<DashboardDto>("exec GetDashboard ");
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetSalesDashboardDetails(QueryValues request)
        {
            //Form Date and To Date comes with incorrect time and it is adjusted in the SP to the 00:00:01 of from date and 23:59:59 of the Todate.
            SqlParameter[] sqlParameters = new[]
            {
                    new SqlParameter { ParameterName = "LoggedInUser",  Value = request.LoggedInUser ,SqlDbType = System.Data.SqlDbType.VarChar, Size = 50},
                    new SqlParameter { ParameterName = "FromDate",      Value = request.FromDate != null ? request.FromDate :  DBNull.Value ,SqlDbType = System.Data.SqlDbType.DateTime},
                    new SqlParameter { ParameterName = "ToDate",        Value = request.ToDate != null ? request.ToDate:  DBNull.Value ,SqlDbType = System.Data.SqlDbType.DateTime},
                    new SqlParameter { ParameterName = "ThreeMonthPerformaceChartPeriodType", Value = request.ThreeMonthPerformaceChartPeriodType != null ? request.ThreeMonthPerformaceChartPeriodType :  DBNull.Value , SqlDbType = System.Data.SqlDbType.VarChar, Size = 10 },
                    new SqlParameter { ParameterName = "SalesPersonPublicKey", Value = request.PrimaryKey != null ? request.PrimaryKey :  DBNull.Value , SqlDbType = System.Data.SqlDbType.VarChar, Size = 50 },
                    new SqlParameter { ParameterName = "ActiveUserPeriodType", Value = request.SecondaryKey != null ? request.SecondaryKey :  DBNull.Value , SqlDbType = System.Data.SqlDbType.VarChar, Size = 10 },
                    new SqlParameter { ParameterName = "LeadsPerPersonPublicKey", Value = request.ThirdKey != null ? request.ThirdKey :  DBNull.Value , SqlDbType = System.Data.SqlDbType.VarChar, Size = 50}
            };

            System.Collections.Generic.List<SalesDashboardDetails> DashboardDetails = await _context.SqlQueryToListAsync<SalesDashboardDetails>(ProcedureCommonSqlParametersText.GetSalesDashboardReport, sqlParameters);
            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Successfully Fetched Dashboard Data";
            responseModel.Data = DashboardDetails;
            return responseModel;

        }

        
        public async Task<ApiCommonResponseModel> GetMobileDashboard(MobileDashboardQueryValues request)
        {
            SqlParameter[] sqlParameters = new[]
            {
                    new SqlParameter { ParameterName = "LoggedInUser",  Value = request.LoggedInUser ,SqlDbType = System.Data.SqlDbType.VarChar, Size = 50},
                    new SqlParameter { ParameterName = "FromDate",      Value = request.FromDate != null ? request.FromDate :  DBNull.Value ,SqlDbType = System.Data.SqlDbType.DateTime},
                    new SqlParameter { ParameterName = "ToDate",        Value = request.ToDate != null ? request.ToDate:  DBNull.Value ,SqlDbType = System.Data.SqlDbType.DateTime},
                    new SqlParameter { ParameterName = "ProductPurchasePeriodType", Value = request.PrimaryKey != null ? request.PrimaryKey :  DBNull.Value , SqlDbType = System.Data.SqlDbType.VarChar, Size = 10 },
                    new SqlParameter { ParameterName = "ClientRevenuePeriodType", Value = request.SecondaryKey != null ? request.SecondaryKey :  DBNull.Value , SqlDbType = System.Data.SqlDbType.VarChar, Size = 10 },
                    new SqlParameter { ParameterName = "ProductRevenuePeriodType", Value = request.ThirdKey != null ? request.ThirdKey :  DBNull.Value , SqlDbType = System.Data.SqlDbType.VarChar, Size = 10 },
                    new SqlParameter { ParameterName = "FreeTrialPeriodType", Value = request.FourthKey != null ? request.FourthKey :  DBNull.Value , SqlDbType = System.Data.SqlDbType.VarChar, Size = 10 },
                    new SqlParameter { ParameterName = "PaymentGatewayPeriodType", Value = request.FifthKey != null ? request.FifthKey :  DBNull.Value , SqlDbType = System.Data.SqlDbType.VarChar, Size = 10 },
                    new SqlParameter { ParameterName = "ProductPurchaseRevenuePeriodType ", Value = request.SixthKey != null ? request.SixthKey :  DBNull.Value , SqlDbType = System.Data.SqlDbType.VarChar, Size = 10 },
                    new SqlParameter { ParameterName = "CouponUsagePeriodType ", Value = request.SeventhKey != null ? request.SeventhKey :  DBNull.Value , SqlDbType = System.Data.SqlDbType.VarChar, Size = 10 },
                    new SqlParameter { ParameterName = "ProductRevenueGraphPeriodType", Value = request.EigthKey != null ? request.EigthKey :  DBNull.Value , SqlDbType = System.Data.SqlDbType.VarChar, Size = 10 },
                    new SqlParameter { ParameterName = "ProductPurchaseRevenueCategory", Value = request.Ninthkey != null || request.Ninthkey !="" ? request.Ninthkey :  DBNull.Value , SqlDbType = System.Data.SqlDbType.VarChar, Size = 10 },
                    new SqlParameter { ParameterName = "ProductLikePeriodType", Value = request.TenthKey != null ? request.TenthKey :  DBNull.Value , SqlDbType = System.Data.SqlDbType.VarChar, Size = 10 },
                    new SqlParameter { ParameterName = "UsersReoportPeriodType", Value = request.EleventhKey != null ? request.EleventhKey :  DBNull.Value , SqlDbType = System.Data.SqlDbType.VarChar, Size = 10 },
                    new SqlParameter { ParameterName = "RevenueByCityPeriodType", Value = request.TwelfthKey != null ? request.TwelfthKey :  DBNull.Value , SqlDbType = System.Data.SqlDbType.VarChar, Size = 10 }
            };

            var preMarketReport = await _preAndPostMarketService.GetPaginatedMarketDataAsync(1,1);
            var json = JsonConvert.SerializeObject(preMarketReport.Data);
            var jArray = JArray.Parse(json);
            var preMarketList = jArray
                .Select(j => j.ToObject<PreMarketReportModel>())
                .ToList();

            var today = DateTime.Today;
            var preMarketReportJson = preMarketList?.FirstOrDefault(x => x.CreatedOn.Date == today);



            var postMarketReport = await _preAndPostMarketService.GetPaginatedPostMarketDataAsync(1, 1);
            var postJson = JsonConvert.SerializeObject(postMarketReport.Data);
            var postJArray = JArray.Parse(postJson);
            var postMarketList = postJArray
                .Select(j => j.ToObject<PostMarketResponseModel>())
                .ToList();
            var postMarketReportJson = postMarketList?
                .FirstOrDefault(x => x.CreatedOn.Date == today);

            System.Collections.Generic.List<MobileDashBoardDetails> dashboardDetails = await _context.SqlQueryToListAsync<MobileDashBoardDetails>(ProcedureCommonSqlParametersText.GetMobileDashboard, sqlParameters);

            

            var combinedData = new
            {
                DashboardDetails = dashboardDetails,
                PreMarketReportJson = preMarketReportJson,
                PostMarketReportJson = postMarketReportJson
            };

            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Successfully Fetched Dashboard Data";
            responseModel.Data = combinedData;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetLogsAndExceptionsForMobileDashboard()
        {
            var logsCount = _mobileService.TotalLogCount();
            var exceptionCount = _mobileService.TotalExceptionCount();

            var totalLogsCount = logsCount != null ? logsCount.Result.Total : 0;
            var totalExceptionCount = exceptionCount != null ? exceptionCount.Result.Total : 0;
            var combinedData = new
            {
                TotalLogsCount = totalLogsCount,
                TotalExceptionCount = totalExceptionCount,
            };
            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Logs and Exception Count extracted successfully";
            responseModel.Data = combinedData;
            return responseModel;
        }
    }
}
