using DocumentFormat.OpenXml.Office2010.Excel;
using RM.API.Models;
using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.ResearchMantraContext;
using RM.Model;
using RM.Model.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using EntityState = Microsoft.EntityFrameworkCore.EntityState;

namespace RM.API.Services
{
    public interface ICustomerService
    {
        ///<summary>
        /// Get Customer List
        /// </summary>
        /// <param name="queryValues"></param>
        /// <return></return>
        Task<ApiCommonResponseModel> GetFilteredCustomers(QueryValues queryValues);
        ApiCommonResponseModel GetCustomerById(Guid Guid);
        Task<ApiCommonResponseModel> ManageCustomers(CustomerRequest customPostData, string loginUser);
        Task<ApiCommonResponseModel> GetFilteredCustomerKyc(QueryValues searchText);
        Task<ApiCommonResponseModel> GetBde(int systemUserId, string roleName);
        ApiCommonResponseModel GetCustomerByRowId(Guid id);
        Task<ApiCommonResponseModel> UpdateAdvisoryForm(AdvisoryUpdateRequestModel request, string loggedInUser);
    }
    public class CustomerService : ICustomerService
    {
        private readonly ResearchMantraContext _context;
        private readonly ApiCommonResponseModel responseModel = new();
        public CustomerService(ResearchMantraContext context)
        {
            _context = context;
        }

        public ApiCommonResponseModel GetCustomerById(Guid leadPublicKey)
        {
            responseModel.StatusCode = HttpStatusCode.OK;

            Lead lead = _context.Leads.Where(c => c.PublicKey.ToString() == leadPublicKey.ToString()).FirstOrDefault();
            responseModel.Data = lead;
            return responseModel;
        }

        public ApiCommonResponseModel GetCustomerByRowId(Guid id)
        {
            RpfForm form = _context.RpfForms.Where(c => c.Id.ToString().ToLower() == id.ToString().ToLower()).FirstOrDefault();
            responseModel.Data = form;
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> UpdateAdvisoryForm(AdvisoryUpdateRequestModel request, string loggedInUser)
        {
            RpfForm form = _context.RpfForms.Where(c => c.Id.ToString().ToLower() == request.PublicKey.ToString().ToLower()).FirstOrDefault();

            if(loggedInUser == null)
            {
                ApiCommonResponseModel response = new ApiCommonResponseModel()
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Message = "You are not authorized to make the chages."
                };
                return response;
            }
            else if (form != null && loggedInUser != null)
            {
                form.Status = request.Status;
                form.AltMobile = request.AltMobile;
                form.ServiceName = request.ServiceName;
                form.IsRpfApproved = (form.Status.ToLower() == "accepted") ? true : false;
                form.RpfApprovedBy = loggedInUser;
                form.RpfApprovedOn = DateTime.Now;

                await _context.SaveChangesAsync();

                ApiCommonResponseModel response = new ApiCommonResponseModel()
                {
                    Data = form,
                    StatusCode = HttpStatusCode.OK,
                    Message = "Risk Profile Form Updated Successfully."
                };
                return response;

            }
            else
            {
                ApiCommonResponseModel response = new ApiCommonResponseModel()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "No such users"
                };
                return response;
            }
        }

        public async Task<ApiCommonResponseModel> GetFilteredCustomerKyc(QueryValues queryValues)
        {
            ApiCommonResponseModel responseModel = new();

            SqlParameter[] sqlParameters = new[]
            {
                new SqlParameter("@IsPaging", queryValues.IsPaging),
                new SqlParameter("@PageSize", queryValues.PageSize),
                new SqlParameter("@PageNumber", queryValues.PageNumber),
                new SqlParameter("@SortExpression", string.IsNullOrWhiteSpace(queryValues.SortExpression) ? DBNull.Value : queryValues.SortExpression),
                new SqlParameter("@SortOrder", string.IsNullOrWhiteSpace(queryValues.SortOrder) ? "DESC" : queryValues.SortOrder),
                new SqlParameter("@RequestedBy", string.IsNullOrWhiteSpace(queryValues.RequestedBy) ? DBNull.Value : queryValues.RequestedBy),
                new SqlParameter("@SearchText", string.IsNullOrWhiteSpace(queryValues.SearchText) ? DBNull.Value : queryValues.SearchText),
                new SqlParameter("@FromDate", queryValues.FromDate.HasValue ? queryValues.FromDate.Value : DBNull.Value),
                new SqlParameter("@ToDate", queryValues.ToDate.HasValue ? queryValues.ToDate.Value : DBNull.Value),
                new SqlParameter("@StrategyKey", string.IsNullOrWhiteSpace(queryValues.ThirdKey) ? DBNull.Value : queryValues.ThirdKey)
            };

            try
            {
                // Step 1: Call the stored procedure and get the JSON string
                var jsonResult = await _context.SqlQueryToListAsync<GetProcedureJsonResponse>(
                    @"EXEC GetFilteredCustomerKyc 
                        @IsPaging, 
                        @PageSize, 
                        @PageNumber, 
                        @SortExpression, 
                        @SortOrder, 
                        @RequestedBy, 
                        @SearchText, 
                        @FromDate, 
                        @ToDate,
                        @StrategyKey", sqlParameters);

                var jsonData = jsonResult.FirstOrDefault()?.JsonData;
                if (string.IsNullOrWhiteSpace(jsonData))
                {
                    responseModel.Data = null;
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                    responseModel.Message = "No data found";
                    return responseModel;
                }

                // Step 2: Parse JSON into JsonNode
                var rootNode = JsonNode.Parse(jsonData);
                if (rootNode == null)
                {
                    responseModel.StatusCode = HttpStatusCode.InternalServerError;
                    responseModel.Message = "Failed to parse JSON";
                    return responseModel;
                }

                // Step 3: Extract values
                var fullDataNode = rootNode["FullData"]?.ToJsonString();
                var serviceNamesNode = rootNode["ServiceNames"]?.ToJsonString();
                var totalCount = rootNode["TotalCount"]?.GetValue<int>() ?? 0;
                string rawEscapedJson = fullDataNode;

                // Step 1: Unescape the JSON string (convert from escaped form)
                string unescapedJson = JsonSerializer.Deserialize<string>(rawEscapedJson);

                // Step 2: Deserialize the actual list
                List<KycItem> fullData = string.IsNullOrWhiteSpace(unescapedJson)
                    ? new List<KycItem>()
                    : JsonSerializer.Deserialize<List<KycItem>>(unescapedJson);


                List<string> serviceNames = new();

                try
                {
                    var unescapedServiceNamesNode = JsonSerializer.Deserialize<string>(serviceNamesNode);
                    var arrayDoc = JsonDocument.Parse(unescapedServiceNamesNode);
                    if (arrayDoc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in arrayDoc.RootElement.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.Object && item.TryGetProperty("ServiceName", out var serviceNameProp))
                            {
                                if (serviceNameProp.ValueKind == JsonValueKind.String)
                                {
                                    var value = serviceNameProp.GetString();
                                    if (!string.IsNullOrWhiteSpace(value))
                                        serviceNames.Add(value);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to parse serviceNamesNode: " + ex.Message);
                }



                var FinalResult = new KycJsonResponseModel
                {
                    TotalCount = totalCount,
                    FullData = fullData,
                    ServiceNames = serviceNames
                };

                responseModel.Data = FinalResult;
                responseModel.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                responseModel.Message = "Error while fetching KYC data";
                // optionally log ex.Message
            }
            return responseModel;
        }



        public async Task<ApiCommonResponseModel> GetFilteredCustomers(QueryValues queryValues)
        {

            try
            {


                responseModel.Message = "Successfully";
                responseModel.StatusCode = HttpStatusCode.OK;
                SqlParameter parameterOutValue = new()
                {
                    ParameterName = "TotalCount",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output,
                };
                SqlParameter parameterOutValue2 = new()
                {
                    ParameterName = "TotalAmount",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output,
                };




                //else if (checkIfAdmin.Name == "Admin" && queryValues.RequestedBy is not null)
                //{
                //    queryValues.RequestedBy = "";
                //}


                SqlParameter[] sqlParameters = new[]
                {
                        new SqlParameter { ParameterName = "@IsPaging",      Value = queryValues.IsPaging,SqlDbType = System.Data.SqlDbType.Int},
                        new SqlParameter { ParameterName = "@PageSize",      Value = queryValues.PageSize,SqlDbType = System.Data.SqlDbType.Int,},
                        new SqlParameter { ParameterName = "@PageNumber", Value = queryValues.PageNumber, SqlDbType = System.Data.SqlDbType.Int },
                        new SqlParameter { ParameterName = "@SortExpression",Value = queryValues.SortExpression == "" ?  DBNull.Value: Convert.ToString(queryValues.SortExpression),SqlDbType = System.Data.SqlDbType.VarChar, Size = 50},
                        new SqlParameter { ParameterName = "@SortOrder",     Value = queryValues.SortOrder == "" ?  "DESC" : queryValues.SortOrder,SqlDbType = System.Data.SqlDbType.VarChar, Size = 50},
                        new SqlParameter { ParameterName = "@RequestedBy",     Value = queryValues.RequestedBy is ""  or null ? Convert.DBNull : queryValues.RequestedBy ,SqlDbType = System.Data.SqlDbType.VarChar, Size = 50},
                        new SqlParameter { ParameterName = "@SearchText",    Value = queryValues.SearchText is "" or null  ?  DBNull.Value : queryValues.SearchText , SqlDbType = SqlDbType.VarChar, Size = 100},
                        new SqlParameter { ParameterName = "@FromDate",      Value = queryValues.FromDate ??  Convert.DBNull ,SqlDbType = SqlDbType.DateTime},
                        new SqlParameter { ParameterName = "@ToDate", Value = queryValues.ToDate ?? Convert.DBNull , SqlDbType = SqlDbType.DateTime },
                        new SqlParameter { ParameterName = "@StrategyKey", Value = queryValues.PrimaryKey == "" ? Convert.DBNull : queryValues.PrimaryKey, SqlDbType = System.Data.SqlDbType.VarChar, Size = 50 },
                        new SqlParameter { ParameterName = "@SecondaryKey", Value = queryValues.SecondaryKey == "" ? Convert.DBNull : queryValues.SecondaryKey, SqlDbType = System.Data.SqlDbType.VarChar, Size = 50 },
                        new SqlParameter { ParameterName = "@LoggedInUser",  Value = queryValues.LoggedInUser   ,SqlDbType = SqlDbType.VarChar, Size = 50},

                        parameterOutValue,parameterOutValue2
                };

                System.Collections.Generic.List<GetProcedureJsonResponse> customers = await _context.SqlQueryFirstOrDefaultAsync<GetProcedureJsonResponse>(
                @"exec GetCustomers @IsPaging={0}, @PageSize={1}, @PageNumber= {2}, @SortExpression={3}, 
                @SortOrder={4} , @RequestedBy={5}, @SearchText={6}, @FromDate={7}, @ToDate={8}, @StrategyKey={9},@SecondaryKey={10}, @LoggedInUser ={11}, @TotalCount={12} OUTPUT, @TotalAmount={13} OUTPUT", sqlParameters);


                dynamic runTimeObject = new ExpandoObject();
                runTimeObject.jsonData = customers.FirstOrDefault().JsonData;
                runTimeObject.totalCount = parameterOutValue?.Value;
                runTimeObject.totalAmount = parameterOutValue2?.Value;

                responseModel.Data = runTimeObject;


            }
            catch (Exception)
            {
            }
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> ManageCustomers(CustomerRequest customer, string LoginUser)
        {
            responseModel.Message = string.Empty;
            responseModel.StatusCode = HttpStatusCode.OK;

            if (!string.IsNullOrEmpty(customer.LeadKey))
            {
                Guid leadKey = Guid.Parse(customer.LeadKey);

                // Use sync query (EF6 safe)
                Lead lead = _context.Leads.FirstOrDefault(item => item.PublicKey == leadKey);

                if (lead != null)
                {
                    lead.City = customer.City;
                    lead.PinCode = customer.PinCode;
                    lead.EmailId = customer.EmailId;
                    lead.FullName = customer.FullName;
                    lead.Remarks = customer.Remarks;
                    lead.MobileNumber = customer.MobileNumber;

                    _context.Entry(lead).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetBde(int systemUserId, string roleName)
        {
            var responseModel = new ApiCommonResponseModel();

            string category = "0"; // default fallback

            if (roleName.Equals("admin", StringComparison.OrdinalIgnoreCase) || roleName.Equals("dm_manager", StringComparison.OrdinalIgnoreCase) || roleName.Equals("globleadmin", StringComparison.OrdinalIgnoreCase))
            {
                category = "bde";
            }
            else if (roleName.Equals("sales lead", StringComparison.OrdinalIgnoreCase))
            {
                category = systemUserId.ToString(); // numeric triggers supervisor logic
            }

            List<SqlParameter> sqlParameters = new()
    {
        new SqlParameter
        {
            ParameterName = "Category",
            Value = category,
            SqlDbType = SqlDbType.VarChar,
            Size = 50
        },
    };

            responseModel.Data = await _context.SqlQueryAsync<GetProcedureJsonResponse>(
                ProcedureCommonSqlParametersText.GetAnalysts, sqlParameters.ToArray());

            responseModel.Message = responseModel.Data is null ?
                "No Records Available For Current Parameters" :
                "Fetched Successfully";

            return responseModel;
        }

    }
}
