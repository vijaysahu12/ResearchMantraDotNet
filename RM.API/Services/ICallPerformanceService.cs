
using RM.API.Dtos;
using RM.API.Models.Reports;
using RM.CommonService.Helpers;
using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.KingResearchContext;
using RM.Model;
using RM.Model.Common;
using RM.Model.ResponseModel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RM.API.Services
{
    public interface ICallPerformanceService
    {
        Task<CallPerformance> GetCallById(Guid publicKey);
        Task<object> GetCallPerformance(string Search, string CallBy, string StrategyKey, DateTime StartDate, DateTime EndDate, int PageNumber, int PageSize, string SortOrder, string SortBy, TokenVariables token);
        Task<object> GetCallPerformanceExcel(string Search, string CallBy, string StrategyKey, DateTime StartDate, DateTime EndDate, int PageNumber, int PageSize, string SortOrder, string SortBy, TokenVariables token);

        Task<ApiCommonResponseModel> GetCallsSummaryReports(CallPerformanceReportRequest request);
        Task<ApiCommonResponseModel> GetCallsTopPerformersReports(CallPerformanceReportRequest request);
        Task<ApiCommonResponseModel> GetCallsTopBuzzerReports(CallPerformanceReportRequest request);
        Task<ApiCommonResponseModel> GetCallPerformanceHeatMapData(QueryValues queryValues);
        Task<ApiCommonResponseModel> GetCallDetails(QueryValues queryValues);
    }

    public class CallPerformanceService : ICallPerformanceService
    {
        private readonly KingResearchContext _context;
        private readonly ApiCommonResponseModel apiCommonResponseModel = new();
        public CallPerformanceService(KingResearchContext context)
        {
            _context = context;
        }

        public async Task<CallPerformance> GetCallById(Guid publicKey)
        {
            CallPerformance result = await _context.CallPerformances.Where(item => item.PublicKey == publicKey).FirstOrDefaultAsync();
            return result;
        }

        public async Task<object> GetCallPerformance(string Search, string CallBy, string StrategyKey, DateTime StartDate, DateTime EndDate, int PageNumber, int PageSize, string SortOrder, string SortBy, TokenVariables token)
        {
            _ = token.PublicKey;
            _ = token.RoleKey;


            QueryValues query = new()
            {
                RequestedBy = CallBy,
                SearchText = Search,
                FromDate = StartDate,
                ToDate = EndDate,
                PageNumber = PageNumber,
                PageSize = PageSize,
                PrimaryKey = StrategyKey,
                SortOrder = SortOrder,
                SortExpression = SortBy
            };

            List<SqlParameter> sqlParameters2 = ProcedureCommonSqlParameters.GetCommonSqlParameters(query);

            SqlParameter parameterOutValue = new()
            {
                ParameterName = "TotalCount",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output,
            };
            SqlParameter parameterOutValue1 = new()
            {
                ParameterName = "StrategyKey",
                Value = string.IsNullOrEmpty(StrategyKey) ? DBNull.Value : StrategyKey,
                SqlDbType = System.Data.SqlDbType.VarChar,
                Size = 120,
            };
            sqlParameters2.Add(parameterOutValue1);
            sqlParameters2.Add(parameterOutValue);

            dynamic resultObject = new System.Dynamic.ExpandoObject();

            resultObject.Data = await _context.SqlQueryToListAsync<GetCallPerformanceResponseModel>(ProcedureCommonSqlParametersText.GetCallPerformance, sqlParameters2.ToArray());

            resultObject.TotalCount = parameterOutValue.Value;

            apiCommonResponseModel.StatusCode = HttpStatusCode.OK;
            apiCommonResponseModel.Data = resultObject;
            apiCommonResponseModel.Message = "Successful";
            return apiCommonResponseModel;

        }
        public async Task<object> GetCallPerformanceExcel(string Search, string CallBy, string StrategyKey, DateTime StartDate, DateTime EndDate, int PageNumber, int PageSize, string SortOrder, string SortBy, TokenVariables token)
        {
            try
            {
                string LoginUser = token.PublicKey;
                string LoginUserRole = token.RoleKey;


                QueryValues query = new()
                {
                    RequestedBy = CallBy,
                    SearchText = Search,
                    FromDate = StartDate,
                    ToDate = EndDate,
                    PageNumber = PageNumber,
                    PageSize = PageSize,
                    PrimaryKey = StrategyKey,
                    SortOrder = SortOrder,
                    SortExpression = SortBy
                };

                List<SqlParameter> sqlParameters2 = ProcedureCommonSqlParameters.GetCommonSqlParameters(query);

                SqlParameter parameterOutValue = new()
                {
                    ParameterName = "TotalCount",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output,
                };
                SqlParameter parameterOutValue1 = new()
                {
                    ParameterName = "StrategyKey",
                    Value = string.IsNullOrEmpty(StrategyKey) ? DBNull.Value : StrategyKey,
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Size = 50,
                };
                sqlParameters2.Add(parameterOutValue1);
                sqlParameters2.Add(parameterOutValue);

                dynamic resultObject = new System.Dynamic.ExpandoObject();

                resultObject.Data = await _context.SqlQueryToListAsync<GetCallPerformanceResponseModel>(ProcedureCommonSqlParametersText.GetCallPerformanceExcel, sqlParameters2.ToArray());

                resultObject.TotalCount = parameterOutValue.Value;

                apiCommonResponseModel.StatusCode = HttpStatusCode.OK;
                apiCommonResponseModel.Data = resultObject;
                apiCommonResponseModel.Message = "Successful";
                return apiCommonResponseModel;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public async Task<ApiCommonResponseModel> GetCallsSummaryReports(CallPerformanceReportRequest request)
        {

            SqlParameter[] sqlParameters = new[]
            {
                new SqlParameter
                {
                    ParameterName = "StartDate",
                    Value = request.StartDate ==null ? Convert.DBNull : UtcToIstDateTime.UtcStringToIst(Convert.ToDateTime(request.StartDate)),
                    SqlDbType = System.Data.SqlDbType.Date,
                } ,new SqlParameter
                {
                    ParameterName = "EndDate",
                    Value = request.EndDate == null ? Convert.DBNull : UtcToIstDateTime.UtcStringToIst(Convert.ToDateTime(request.EndDate)),
                    SqlDbType = System.Data.SqlDbType.Date,
                },new SqlParameter
                {
                    ParameterName = "CallBy",
                    Value = request.CallBy is null or "" ? Convert.DBNull : request.CallBy,
                    SqlDbType = System.Data.SqlDbType.VarChar,
                }
            };

            System.Collections.Generic.List<GetProcedureJsonResponse> getSummary = await _context.SqlQueryToListAsync<GetProcedureJsonResponse>("EXEC GetCallsSummaryReports {0} , {1} , {2} ", sqlParameters);
            apiCommonResponseModel.StatusCode = HttpStatusCode.OK;
            apiCommonResponseModel.Data = getSummary.FirstOrDefault()?.JsonData;
            return apiCommonResponseModel;
        }

        public async Task<ApiCommonResponseModel> GetCallsTopBuzzerReports(CallPerformanceReportRequest request)
        {
            SqlParameter[] sqlParameters = new[]
                        {
                new SqlParameter
                {
                    ParameterName = "StartDate",
                    Value = request.StartDate != null  ? UtcToIstDateTime.UtcStringToIst(Convert.ToDateTime(request.StartDate))  : Convert.DBNull ,
                    SqlDbType = System.Data.SqlDbType.Date,
                } ,new SqlParameter
                {
                    ParameterName = "EndDate",
                    Value = request.EndDate != null  ? UtcToIstDateTime.UtcStringToIst(Convert.ToDateTime(request.EndDate)) : Convert.DBNull ,
                    SqlDbType = System.Data.SqlDbType.Date,
                },new SqlParameter
                {
                    ParameterName = "CallBy",
                    Value = request.CallBy is null or "" ? Convert.DBNull : request.CallBy,
                    SqlDbType = System.Data.SqlDbType.VarChar,
                }
            };

            List<CallsTopBuzzerReportsResponseModel> result = await _context.SqlQueryToListAsync<CallsTopBuzzerReportsResponseModel>("EXEC GetCallsTopBuzzerReports {0} , {1} , {2} ", sqlParameters);

            apiCommonResponseModel.StatusCode = HttpStatusCode.OK;
            apiCommonResponseModel.Data = result;
            apiCommonResponseModel.Message = "";
            return apiCommonResponseModel;
        }

        public async Task<ApiCommonResponseModel> GetCallsTopPerformersReports(CallPerformanceReportRequest request)
        {
            SqlParameter[] sqlParameters = new[]
                        {
                new SqlParameter
                {
                    ParameterName = "StartDate",
                    Value = request.StartDate == null ? Convert.DBNull :  UtcToIstDateTime.UtcStringToIst(Convert.ToDateTime(request.StartDate)),
                    SqlDbType = System.Data.SqlDbType.Date,
                },new SqlParameter
                {
                    ParameterName = "EndDate",
                    //Value = request.EndDate ?? Convert.DBNull,
                    Value = request.EndDate == null ? Convert.DBNull : UtcToIstDateTime.UtcStringToIst(Convert.ToDateTime(request.EndDate)),
                    SqlDbType = System.Data.SqlDbType.Date,
                },new SqlParameter
                {
                    ParameterName = "CallBy",
                    Value = request.CallBy is null or "" ? Convert.DBNull : request.CallBy,
                    SqlDbType = System.Data.SqlDbType.VarChar,
                }
            };

            System.Collections.Generic.List<CallsTopPerformersReportsResponseModel> result = await _context.SqlQueryToListAsync<CallsTopPerformersReportsResponseModel>("EXEC GetCallsTopPerformersReports {0} , {1} , {2} ", sqlParameters);

            apiCommonResponseModel.StatusCode = HttpStatusCode.OK;
            apiCommonResponseModel.Data = result;
            apiCommonResponseModel.Message = "";
            return apiCommonResponseModel;
        }

        public async Task<ApiCommonResponseModel> GetCallPerformanceHeatMapData(QueryValues queryValues)
        {
            try
            {

                SqlParameter[] sqlParameters = new[]
                          {
                new SqlParameter
                {
                    ParameterName = "FromDate",
                    Value = string.IsNullOrEmpty(queryValues.FromDate.ToString()) ? DBNull.Value : UtcToIstDateTime.UtcStringToIst(Convert.ToDateTime(queryValues.FromDate)) ,
                    SqlDbType = System.Data.SqlDbType.Date,
                },new SqlParameter
                {
                    ParameterName = "ToDate",
                    Value = string.IsNullOrEmpty(queryValues.ToDate.ToString()) ? DBNull.Value : UtcToIstDateTime.UtcStringToIst(Convert.ToDateTime(queryValues.ToDate)),
                    SqlDbType = System.Data.SqlDbType.Date,
                },new SqlParameter
                {
                    ParameterName = "CallByKey",
                    Value = queryValues.RequestedBy is null or "" ? DBNull.Value: queryValues.RequestedBy,
                    SqlDbType = System.Data.SqlDbType.VarChar,
                }
            };

                apiCommonResponseModel.Data = await _context.SqlQueryToListAsync<JsonResponseModel>(ProcedureCommonSqlParametersText.CallPerformancePNLReport, sqlParameters.ToArray());
                if (apiCommonResponseModel.Data == null)
                {
                    apiCommonResponseModel.Message = "No Data For Current Params";
                    apiCommonResponseModel.StatusCode = HttpStatusCode.NotFound;
                }

                apiCommonResponseModel.Message = "Data Fetch Successfull";
                apiCommonResponseModel.StatusCode = HttpStatusCode.OK;

            }
            catch (Exception ex)
            {
                var dd = ex;
            }
            return apiCommonResponseModel;

        }

        public async Task<ApiCommonResponseModel> GetCallDetails(QueryValues queryValues)
        {
            //callByKey in primaryKey
            //callDate in toDate

            try
            {
                SqlParameter[] sqlParameters = new[]
                         {
                new SqlParameter
                {
                    ParameterName = "CallByKey",
                    Value = string.IsNullOrEmpty(queryValues.PrimaryKey.ToString()) ? DBNull.Value :Guid.Parse( queryValues.PrimaryKey),
                    SqlDbType = System.Data.SqlDbType.UniqueIdentifier,
                },new SqlParameter
                {
                    ParameterName = "CallDate",
                    Value = string.IsNullOrEmpty(queryValues.ToDate.ToString()) ? DBNull.Value : UtcToIstDateTime.UtcStringToIst(Convert.ToDateTime(queryValues.ToDate)) ,
                    SqlDbType = System.Data.SqlDbType.Date,
                }

            };

                apiCommonResponseModel.Data = await _context.SqlQueryToListAsync<GetProcedureJsonResponse>(ProcedureCommonSqlParametersText.GetCallDetails, sqlParameters.ToArray());
                if (apiCommonResponseModel.Data == null)
                {
                    apiCommonResponseModel.Message = "No Data For Current Params";
                    apiCommonResponseModel.StatusCode = HttpStatusCode.NotFound;
                }

                apiCommonResponseModel.Message = "Data Fetch Successfull";
                apiCommonResponseModel.StatusCode = HttpStatusCode.OK;

            }
            catch (Exception ex)
            {
                var dd = ex;
            }
            return apiCommonResponseModel;

        }


    }
}
