using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.KingResearchContext;
using RM.Model;
using RM.Model.Common;
using RM.Model.DB.Tables;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Net;

namespace RM.API.Services
{
    public interface ILeadService
    {
        Task<ApiCommonResponseModel> GetActivityLogs(QueryValues publicKey);
        Task<ApiCommonResponseModel> GetSelfAssignJunkLeads();
        Task<ApiCommonResponseModel> SelfAssignJunkLeads(Guid userKey, QueryValues queryValues);
    }

    public class LeadService : ILeadService
    {
        private readonly KingResearchContext _context;
        private readonly ApiCommonResponseModel apiCommonResponse = new();



        public LeadService(IConfiguration config)
        {
        }



        public async Task<ApiCommonResponseModel> GetActivityLogs(QueryValues request)
        {
            try
            {
                List<SqlParameter> sqlParameters = new()
                {
                new SqlParameter { ParameterName = "PageSize", Value = request.PageSize ,SqlDbType = SqlDbType.Int},
                new SqlParameter { ParameterName = "PageNumber", Value = request.PageNumber  ,SqlDbType = SqlDbType.Int},
                new SqlParameter { ParameterName = "PrimaryKey",    Value = request.PrimaryKey =="" ? DBNull.Value : request.PrimaryKey ,SqlDbType = SqlDbType.VarChar, Size = 50},
                new SqlParameter { ParameterName = "FromDate", Value = request.FromDate == null ? DBNull.Value  : request.FromDate ,SqlDbType = SqlDbType.DateTime},
                new SqlParameter { ParameterName = "ToDate", Value = request.ToDate == null ? DBNull.Value : request.ToDate ,SqlDbType = SqlDbType.DateTime},
                new SqlParameter { ParameterName = "SearchText", Value = request.SearchText == null ? DBNull.Value : request.SearchText ,SqlDbType = SqlDbType.VarChar, Size = 50},
            };
                apiCommonResponse.Data = await _context.SqlQueryToListAsync<JsonResponseModel>(ProcedureCommonSqlParametersText.GetActivityLogs, sqlParameters.ToArray());

                if (apiCommonResponse.Data is null)
                {
                    apiCommonResponse.Data = null;
                    apiCommonResponse.StatusCode = HttpStatusCode.OK;
                    apiCommonResponse.Message = "No Activity For This Search Found.";
                }
                apiCommonResponse.StatusCode = HttpStatusCode.OK;
                apiCommonResponse.Message = "Data Fetched Successfully.";
            }
            catch (Exception ex)
            {
                var dd = ex;
            }

            return apiCommonResponse;
        }

        public async Task<ApiCommonResponseModel> SelfAssignJunkLeads(Guid userKey, QueryValues queryValues)
        {

            string leadKeyListString = queryValues.PrimaryKey;
            List<string> leadKeyList = leadKeyListString.Split(',').ToList();


            var recordsToUpdate = _context.Leads.Where(item => leadKeyList.Contains(item.PublicKey.ToString())).ToList();

            foreach (Lead record in recordsToUpdate)
            {
                if (record.AssignedTo == null)
                {
                    record.AssignedTo = userKey.ToString();
                    record.ModifiedBy = userKey.ToString();
                    record.ModifiedOn = DateTime.Now;
                    record.StatusId = (int)ActivityTypeEnum.LeadPulled;
                    if (!string.IsNullOrEmpty(queryValues.SecondaryKey))
                    {
                        record.LeadTypeKey = queryValues.SecondaryKey.ToString();
                    }
                    if (!string.IsNullOrEmpty(queryValues.ThirdKey))
                    {
                        record.LeadSourceKey = queryValues.ThirdKey.ToString();
                    }                    // adding lead activity
                    LeadActivity leadActivityForAssigning = new()
                    {
                        LeadKey = record.PublicKey,
                        ActivityType = 21,
                        Message = "Lead Pulled",
                        Source = null,
                        Destination = userKey,
                        CreatedOn = DateTime.Now,
                        CreatedBy = userKey
                    };
                    _ = _context.LeadActivity.Add(leadActivityForAssigning);
                }
            }
            // adding useractivity
            //await _activityService.UserLog(userKey.ToString(), Guid.Empty, ActivityTypeEnum.LeadPulled);



            _ = await _context.SaveChangesAsync();

            apiCommonResponse.Message = "Successfully assigned to self";
            apiCommonResponse.StatusCode = HttpStatusCode.OK;
            return apiCommonResponse;
        }

        public async Task<ApiCommonResponseModel> GetSelfAssignJunkLeads()
        {
            try
            {
                apiCommonResponse.Data = await _context.SqlQueryToListAsync<JsonResponseModel>(ProcedureCommonSqlParametersText.GetSelfAssignJunkLeads);
                return apiCommonResponse;
            }
            catch (Exception ex)
            {
                var dd = ex;
                return apiCommonResponse;

            }


        }
    }


}