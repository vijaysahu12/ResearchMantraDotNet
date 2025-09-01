using ClosedXML.Excel;
using RM.API.Dtos;
using RM.API.Helpers;
using RM.API.Models;
using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.KingResearchContext;
using RM.Database.MongoDbContext;
using RM.Model;
using RM.Model.Common;
using RM.Model.DB.Tables;
using RM.Model.MongoDbCollection;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RM.API.Services
{
    public interface ILeadService
    {
        Task<ApiCommonResponseModel> GetActivityLogs(QueryValues publicKey , Guid loggedInUser);

        Task<ApiCommonResponseModel> GetSelfAssignJunkLeads();

        Task<ApiCommonResponseModel> ImportExcelForLeads(IFormFile file, QueryValues jsonRequest);

        Task<ApiCommonResponseModel> LeadAllotments(LeadAllotmentsRequestModel leadList, string loggedInUserKey, bool overrideAllotment);

        Task<ApiCommonResponseModel> SelfAssignJunkLeads(Guid userKey, QueryValues queryValues);

        Task<ApiCommonResponseModel> GetFreeTrailReasons(int freeTrailId);

        Task<ApiCommonResponseModel> GetAllLeadFreeTrialsAsync(QueryValues request, Guid loggedInUser);

        Task<ApiCommonResponseModel> ChangeStatusCustomerToComplete();

        Task<ApiCommonResponseModel> GetJunkLeads(QueryValues queryValues, Guid loggedInUser);
    }

    public class LeadService : ILeadService
    {
        private readonly KingResearchContext _context;
        private readonly ApiCommonResponseModel apiCommonResponse = new();
        private readonly IActivityService _activityService;
        private readonly IMongoRepository<Log> _log;

        public LeadService(KingResearchContext context, IActivityService activityService, IMongoRepository<Log> log)
        {
            _log = log;
            _context = context;
            _activityService = activityService;
        }

        /// <summary>
        /// Allot the junk leads or un assigned leads to the sales person.
        /// Only admin or assginedTo =null value can be self assigned
        /// So that they can followup the leads and get the business
        /// </summary>
        /// <param name="leadList"></param>
        /// <param name="loggedInUserKey"></param>
        /// <returns></returns>
        public async Task<ApiCommonResponseModel> LeadAllotments(LeadAllotmentsRequestModel leadList, string loggedInUserKey, bool overrideAllotment = false)
        {
            apiCommonResponse.StatusCode = HttpStatusCode.BadRequest;

            if (!overrideAllotment)
            {
                List<Guid> publicKeys = leadList.Leads.Select(lead => Guid.Parse(lead.PublicKey)).ToList();

                var query = await (from po in _context.PurchaseOrders
                                   join l in _context.Leads on po.LeadId equals l.Id
                                   join s in _context.Status on po.Status equals s.Id
                                   where publicKeys.Contains(l.PublicKey)
                                         && po.IsExpired == false
                                         && po.Status != 4 && po.Status != 24        // 4 = Completed, 24 = Customer
                                   select new
                                   {
                                       l.FullName,
                                       l.MobileNumber,
                                       Status = s.Name,
                                       po.PaidAmount,
                                       po.CreatedOn,
                                       po.StartDate,
                                       po.EndDate,
                                   }).ToListAsync();

                if (query.Any())
                {
                    apiCommonResponse.StatusCode = HttpStatusCode.Forbidden;
                    apiCommonResponse.Message = "Active Pr Exists";
                    apiCommonResponse.Data = query;
                    return apiCommonResponse;
                }
            }
            var checkUser = await _context.Users.Where(item => item.PublicKey == Guid.Parse(loggedInUserKey)).FirstOrDefaultAsync();
            var rol = await _context.Roles.Where(item => item.PublicKey == Guid.Parse(checkUser.RoleKey)).FirstOrDefaultAsync();

            foreach (LeadKeyList lead in leadList.Leads)
            {
                Lead tempLead = await _context.Leads.Where(item => item.PublicKey == Guid.Parse(lead.PublicKey)).FirstOrDefaultAsync();
                if (tempLead is not null && (rol.Name == "Admin" || rol.Name == "GlobleAdmin" || string.IsNullOrEmpty(tempLead.AssignedTo) || (rol.Name == "Sales Lead" || string.IsNullOrEmpty(tempLead.AssignedTo) || 
                    (DateTime.Now - tempLead.ModifiedOn).TotalDays >= 45)))
                {
                    await _activityService.UserLog(loggedInUserKey, tempLead.PublicKey, ActivityTypeEnum.LeadAllocated, "13.LeadAllotments");
                    await _activityService.LeadLog(tempLead.PublicKey.ToString(), loggedInUserKey, ActivityTypeEnum.LeadAllocated, tempLead.AssignedTo, leadList.AlloteePublicKey, description: "13.LeadAllotments");

                    //chagning the leads assigned to
                    tempLead.AssignedTo = leadList.AlloteePublicKey;
                    tempLead.ModifiedOn = DateTime.Now;
                    tempLead.ModifiedBy = loggedInUserKey;
                    //if (!string.IsNullOrEmpty(leadList.LeadTypeKey))
                    //{
                    //    tempLead.LeadTypeKey = leadList.LeadTypeKey;
                    //}
                    var allocatedLeadType = _context.LeadTypes.FirstOrDefault(x => x.Name == "Allocated");
                    tempLead.LeadTypeKey = allocatedLeadType?.PublicKey?.ToString() ?? string.Empty;
                    if (string.IsNullOrEmpty(tempLead.LeadSourceKey))
                    {
                        tempLead.LeadSourceKey = leadList.LeadSourceKey;
                    }
                }
                _ = await _context.SaveChangesAsync();
            }
            apiCommonResponse.StatusCode = HttpStatusCode.OK;
            return apiCommonResponse;
        }

        public async Task<ApiCommonResponseModel> ImportExcelForLeads(IFormFile file, QueryValues queryValues)
        {
            List<string> expectedHeaders = new()
                {
                    "FullName", // Adjust these headers based on your Excel file
                    "CountryCode", // Adjust these headers based on your Excel file
                    "MobileNumber",
                    "EmailId",
                    "City",
                    "Budget",
                    "Remarks",
                };

            apiCommonResponse.Data = null;
            List<ImportBulkLeadsSpResponseModel> importExcelReportList = new();

            if (file == null || file.Length <= 0)
            {
                apiCommonResponse.StatusCode = HttpStatusCode.BadRequest;
            }

            #region Validate the input file

            string mobilePattern = @"^\d{10}$";
            //ToDo: Add the validation code  ✅
            //ToDo : Validate Mobile Number and emailId (regex )✅
            //ToDo : Add Button to download the excel sample✅
            //ToDo : Read the excel file and upload it to db.( check if excel file rows are same like sample) ✅

            #endregion Validate the input file

            // Create a Random object
            Random random = new();

            // Generate a random 6-digit number

            queryValues.FifthKey = random.Next(100000, 999999).ToString();
            using XLWorkbook package = new(file.OpenReadStream());
            IXLWorksheet worksheet = package.Worksheet(1);

            List<LeadsImport> LeadsImportTableData = new();
            int nullRow = 0;

            #region Read the headers from the excel file

            // Read the headers from the Excel file
            List<string> excelHeaders = new();
            for (int col = 1; col <= 7; col++)
            {
                excelHeaders.Add(worksheet.Cell(1, col).Value.ToString());
            }

            if (!expectedHeaders.SequenceEqual(excelHeaders, StringComparer.OrdinalIgnoreCase))
            {
                // Find non-matching elements
                var mismatches = expectedHeaders.Zip(excelHeaders, (a, b) => new { Source = a, Destination = b, IsMatching = a == b })
                                      .Where(x => !x.IsMatching)
                                      .ToList();

                // Headers do not match the expected headers; handle the error as needed
                apiCommonResponse.StatusCode = HttpStatusCode.OK;
                apiCommonResponse.Message = "Please follow the sample excel provided.";
                apiCommonResponse.Exceptions = mismatches;
                return apiCommonResponse;
            }

            #endregion Read the headers from the excel file

            int rowCount = worksheet.RowsUsed().Count();

            string FullName = string.Empty;
            string CountryCode = string.Empty;
            string MobileNumber = string.Empty;
            string EmailId = string.Empty;
            string City = string.Empty;
            string Budget = string.Empty;
            string Remarks = string.Empty;
            string Marking = string.Empty;
            string Gender = string.Empty;

            for (int row = 2; row <= rowCount; row++)
            {
                // insert all the numbers in the excel to the variable to retrun which numbers has not been inseted at the end

                FullName = worksheet.Cell(row, 1).Value.ToString()?.Trim() ?? string.Empty;
                CountryCode = worksheet.Cell(row, 2).Value.ToString()?.Trim() ?? string.Empty;
                MobileNumber = worksheet.Cell(row, 3).Value.ToString()?.Trim() ?? string.Empty;
                EmailId = worksheet.Cell(row, 4).Value.ToString()?.Trim().Replace(" ", "") ?? string.Empty;
                City = worksheet.Cell(row, 5).Value.ToString()?.Trim() ?? string.Empty;
                Budget = worksheet.Cell(row, 6).Value.ToString()?.Trim() ?? string.Empty;

                Remarks = (worksheet.Cell(row, 7).Value.ToString()?.Trim() ?? string.Empty);
                Remarks = string.IsNullOrEmpty(Budget) ? Remarks : " Budget: " + Budget + " M: " + Remarks + " _ " + queryValues.FifthKey ?? string.Empty;
                Marking = queryValues.FifthKey ?? string.Empty;
                MobileNumber = NormalizeMobileNumber(CountryCode, MobileNumber);

                //bool isValid = !string.IsNullOrEmpty(MobileNumber) && Regex.IsMatch(MobileNumber, mobilePattern);

                bool isValid = ValidateData(MobileNumber, CountryCode, FullName, City, Remarks, EmailId);
                MobileNumber = MobileNumber.Trim();
                if (isValid)
                {
                    LeadsImportTableData.Add(new LeadsImport
                    {
                        FullName = FullName,
                        Gender = Gender,
                        CountryCode = CountryCode,
                        MobileNumber = MobileNumber,
                        EmailId = EmailId,
                        Remarks = Remarks,
                        City = City,
                        Marking = Marking
                    });
                }

                var action = isValid ? "Valid" : "Invalid";

                importExcelReportList.Add(new ImportBulkLeadsSpResponseModel
                {
                    FullName = FullName,
                    Gender = Gender,
                    CountryCode = CountryCode,
                    MobileNumber = MobileNumber,
                    EmailId = EmailId,
                    City = City,
                    Action = action
                });
            }

            if (LeadsImportTableData.Count > 0)
            {
                // Save data to the database using Entity Framework Core
                _context.LeadsImport.AddRange(LeadsImportTableData);
                _ = await _context.SaveChangesAsync();

                #region Execute SP For Merge

                SqlParameter parameterOutValue = new()
                {
                    ParameterName = "TotalCount",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output,
                };

                SqlParameter[] sqlParameters = new[]
                {
                    new SqlParameter { ParameterName = "PrimaryKey",    Value = queryValues.PrimaryKey, SqlDbType = SqlDbType.VarChar, Size = 50 },
                    new SqlParameter { ParameterName = "SecondaryKey",  Value = queryValues.SecondaryKey, SqlDbType = SqlDbType.VarChar, Size = 50 },
                    new SqlParameter { ParameterName = "ThirdKey",      Value = queryValues.ThirdKey, SqlDbType = SqlDbType.VarChar, Size = 50 },
                    new SqlParameter { ParameterName = "FourthKey",     Value = string.IsNullOrEmpty(queryValues.FourthKey) ? DBNull.Value : queryValues.FourthKey, SqlDbType = SqlDbType.VarChar, Size = 50 },
                    new SqlParameter { ParameterName = "FifthKey",      Value = string.IsNullOrEmpty(queryValues.FifthKey) ? DBNull.Value : queryValues.FifthKey, SqlDbType = SqlDbType.VarChar, Size = 50 },
                    new SqlParameter { ParameterName = "AssignedTo",    Value = string.IsNullOrEmpty(queryValues.AssignedTo) ? DBNull.Value : queryValues.AssignedTo, SqlDbType = SqlDbType.VarChar, Size = 50 },
                    parameterOutValue
                };
                var result = await _context.SqlQueryToListAsync<ImportBulkLeadsSpResponseModel>("exec ImportBlulkLeads @PrimaryKey = {0},@SecondaryKey = {1},@ThirdKey = {2},@FourthKey = {3},@FifthKey = {4}, @AssignedTo = {5} ,   @TotalCount={6} OUTPUT  ", sqlParameters);

                //ToDo : Fixed Merged Code
                var mergedList = importExcelReportList.Select(reportItem =>
                {
                    var matchingItem = result.Find(r => r.MobileNumber == reportItem.MobileNumber);
                    if (matchingItem != null)
                    {
                        reportItem.Action = matchingItem.Action; // Update status
                    }
                    return reportItem;
                }).ToList();

                //TestModal res = new()
                //{
                //    AffectedRecords = Convert.ToInt32(parameterOutValue.Value)
                //};

                #endregion Execute SP For Merge

                apiCommonResponse.Data = mergedList;
            }
            //string wrongNumbers = string.Join(", ", incorrectRecords.Select(record => record.MobileNumber)).ToString();
            apiCommonResponse.StatusCode = HttpStatusCode.OK;
            return apiCommonResponse;
        }

        private bool ValidateData(string mobile, string countryCode, string fullName, string city, string remarks, string emailId)
        {
            // MobileNumber: Required and must not exceed 100 characters
            if (string.IsNullOrEmpty(mobile) || mobile.Length > 12 || !Regex.IsMatch(mobile, @"^\+?[0-9]{7,15}$"))
            {
                return false;
            }
            // CountryCode: Optional, but must not exceed 10 characters
            if (!string.IsNullOrEmpty(emailId) && emailId.Length > 200)
            {
                return false;
            }

            // CountryCode: Optional, but must not exceed 10 characters
            if (!string.IsNullOrEmpty(countryCode) && countryCode.Length > 4)
            {
                return false;
            }

            // FullName: Optional, but must not exceed 200 characters
            if (!string.IsNullOrEmpty(fullName) && fullName.Length > 200)
            {
                return false;
            }

            // City: Optional, but must not exceed 50 characters
            if (!string.IsNullOrEmpty(city) && city.Length > 50)
            {
                return false;
            }

            // Remarks: Optional, but must not exceed 2000 characters
            if (!string.IsNullOrEmpty(remarks) && remarks.Length > 2000)
            {
                return false;
            }

            // If all checks pass, the data is valid
            return true;
        }

        /// <summary>
        /// This method gets all the lead activity along with filtering where the params are passed in queryvalues
        /// </summary>
        /// Everyhting is passed in QueryValues
        /// <param name="PageNumber">pass search text if dont want to search anyone then pass ""</param>
        /// <param name="PageSize">self explanatory</param>
        /// <param name="SearchText">self explanatory</param>
        /// <param name="FromDate">self explanatory</param>
        /// <param name="ToDate">self explanatory</param>
        /// <returns> A list of JsonResponseModel </returns>
        public async Task<ApiCommonResponseModel> GetActivityLogs(QueryValues request , Guid loggedInUser)
        {

            try
            {
                SqlParameter sqlOutputParameter = new()
                {
                    ParameterName = "TotalCount",
                    Value = DBNull.Value,
                    Direction = ParameterDirection.Output,
                    SqlDbType = SqlDbType.BigInt
                };

                List<SqlParameter> sqlParameters = new()
                {
                    new SqlParameter { ParameterName = "PageSize", Value = request.PageSize ,SqlDbType = SqlDbType.Int},
                    new SqlParameter { ParameterName = "PageNumber", Value = request.PageNumber  ,SqlDbType = SqlDbType.Int},
                    new SqlParameter { ParameterName = "PrimaryKey",    Value = request.PrimaryKey =="" ? DBNull.Value : request.PrimaryKey ,SqlDbType = SqlDbType.VarChar, Size = 50},
                    new SqlParameter { ParameterName = "FromDate", Value = request.FromDate == null ? DBNull.Value  : request.FromDate ,SqlDbType = SqlDbType.DateTime},
                    new SqlParameter { ParameterName = "ToDate", Value = request.ToDate == null ? DBNull.Value : request.ToDate ,SqlDbType = SqlDbType.DateTime},
                    new SqlParameter { ParameterName = "LoggedInUser",Value = loggedInUser == Guid.Empty ? DBNull.Value : loggedInUser.ToString(), SqlDbType = SqlDbType.VarChar, Size = 50},
                    new SqlParameter { ParameterName = "SearchText", Value = request.SearchText == null ? DBNull.Value : request.SearchText ,SqlDbType = SqlDbType.VarChar, Size = 50},
                    sqlOutputParameter
                };
                apiCommonResponse.Data = new { data = await _context.SqlQueryToListAsync<GetActivityLogsSpResponseModel>(ProcedureCommonSqlParametersText.GetActivityLogs, sqlParameters.ToArray()), TotalCount = sqlOutputParameter.Value };

                apiCommonResponse.StatusCode = HttpStatusCode.OK;
                apiCommonResponse.Message = "Data Fetched Successfully.";
            }
            catch (Exception ex)
            {
                var dd = ex;
            }

            return apiCommonResponse;
        }

        // for assigning 20 leads to loggedIn user
        public async Task<ApiCommonResponseModel> SelfAssignJunkLeads(Guid userKey, QueryValues queryValues)
        {
            int leadPulledCount = 0;
            int userLeadPullsCount = _context.LeadPullLimit.Where(item => item.CreatedOn.Date == DateTime.Today && item.UserKey == userKey).ToList().Sum(item => item.PullCount);
            int pullLimit = int.Parse(_context.Settings.Where(item => item.Code == "PullLeadMaxCount").First().Value);
            if (userLeadPullsCount + leadPulledCount > pullLimit)
            {
                apiCommonResponse.Message = "You have exceeded the lead pull limit for today.";
                apiCommonResponse.StatusCode = HttpStatusCode.Forbidden;
                return apiCommonResponse;
            }

            List<SqlParameter> sqlParameters = new()
            {
                new SqlParameter { ParameterName = "userKey", Value = userKey.ToString() == "" ? DBNull.Value : userKey ,SqlDbType = SqlDbType.UniqueIdentifier},
            };
            //xfhfgh
            var response = await _context.SqlQueryToListAsync<PullLeadProcedureRespoonse>(ProcedureCommonSqlParametersText.GetSelfAssignJunkLeads, sqlParameters.ToArray());

            if (response[0].Message.Contains("No"))
            {
                apiCommonResponse.Message = response[0].Message;
                apiCommonResponse.StatusCode = HttpStatusCode.Forbidden;
                return apiCommonResponse;
            }

            List<string> leadKeyList = queryValues.PrimaryKey.Split(',').ToList();

            var recordsToUpdate = _context.Leads.Where(item => leadKeyList.Contains(item.PublicKey.ToString())).ToList();

            foreach (Lead record in recordsToUpdate)
            {
                if (record.AssignedTo == null)
                {
                    record.AssignedTo = userKey.ToString();
                    record.ModifiedBy = userKey.ToString();
                    record.ModifiedOn = DateTime.Now;
                    record.StatusId = (int)ActivityTypeEnum.LeadPulled;
                    //if (!string.IsNullOrEmpty(queryValues.SecondaryKey))
                    //{
                    //    record.LeadTypeKey = queryValues.SecondaryKey.ToString();
                    //}
                    var allocatedLeadType = _context.LeadTypes.FirstOrDefault(x => x.Name == "Allocated");
                    record.LeadTypeKey = allocatedLeadType?.PublicKey?.ToString() ?? string.Empty;
                    if (!string.IsNullOrEmpty(queryValues.ThirdKey))
                    {
                        record.LeadSourceKey = queryValues.ThirdKey.ToString();
                    }
                    // adding lead activity
                    LeadActivity leadActivityForAssigning = new()
                    {
                        LeadKey = record.PublicKey,
                        ActivityType = (int)ActivityTypeEnum.LeadPulled,
                        Message = "Lead Pulled",
                        Source = null,
                        Destination = userKey,
                        CreatedOn = DateTime.Now,
                        CreatedBy = userKey
                    };

                    leadPulledCount++;

                    _ = _context.LeadActivity.Add(leadActivityForAssigning);
                }
            }
            // adding useractivity
            await _activityService.UserLog(userKey.ToString(), Guid.Empty, ActivityTypeEnum.LeadPulled, "14.SelfAssignJunkLeads");

            // adding into LeadPullLimit Table [ current requirement is to not allow pull 40 leads i.e., 2 times max per day ]
            LeadsPullLimit leadsPullLimit = new()
            {
                UserKey = userKey,
                CreatedOn = DateTime.Now,
                PullCount = leadPulledCount
            };

            _ = _context.LeadPullLimit.Add(leadsPullLimit);

            _ = await _context.SaveChangesAsync();

            apiCommonResponse.Message = $"Successfully assigned {leadPulledCount} leads.";
            apiCommonResponse.StatusCode = HttpStatusCode.OK;
            return apiCommonResponse;
        }

        public async Task<ApiCommonResponseModel> GetSelfAssignJunkLeads()
        {
            List<SqlParameter> sqlParameters = new()
            {
                new SqlParameter { ParameterName = "userKey", Value =  DBNull.Value ,SqlDbType = SqlDbType.VarChar},
            };
            apiCommonResponse.Data = await _context.SqlQueryToListAsync<PullLeadProcedureRespoonse>(ProcedureCommonSqlParametersText.GetSelfAssignJunkLeads, sqlParameters.ToArray());
            return apiCommonResponse;
        }

        public async Task<ApiCommonResponseModel> GetFreeTrailReasons(int freeTrailId)
        {
            var responseModel = new ApiCommonResponseModel();
            var result = await (from rcl in _context.LeadFreeTrailReasonLog
                                join mu in _context.Users on rcl.CreatedBy equals mu.PublicKey
                                where rcl.LeadFreeTrialId == freeTrailId
                                select new
                                {
                                    rcl.LogId,
                                    rcl.LeadFreeTrialId,
                                    rcl.Reason,
                                    rcl.CreatedBy,
                                    rcl.CreatedDate,
                                    rcl.ServiceKey,
                                    rcl.ServiceName,
                                    rcl.FreeTrailStartDate,
                                    rcl.FreeTrailEndDate,
                                    UserName = mu.FirstName + " " + mu.LastName,
                                }).ToListAsync();

            if (result.Count == 0)
            {
                Console.WriteLine("No records found");
            }

            responseModel.Data = result;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetAllLeadFreeTrialsAsync(QueryValues request , Guid loggedInUser)
        {
            SqlParameter sqlOutputParameter = new()
            {
                ParameterName = "@TotalCount",
                Direction = ParameterDirection.Output,
                SqlDbType = SqlDbType.Int
            };

            List<SqlParameter> sqlParameters = new()
            {
                new SqlParameter { ParameterName = "@PageSize", Value = request.PageSize ,SqlDbType = SqlDbType.Int},
                new SqlParameter { ParameterName = "@PageNumber", Value = request.PageNumber  ,SqlDbType = SqlDbType.Int},
                new SqlParameter { ParameterName = "@PrimaryKey", Value = request.PrimaryKey == "" ? DBNull.Value : request.PrimaryKey ,SqlDbType = SqlDbType.VarChar, Size = 50},
                new SqlParameter { ParameterName = "@FromDate", Value = request.FromDate == null ? DBNull.Value  : request.FromDate ,SqlDbType = SqlDbType.DateTime},
                new SqlParameter { ParameterName = "@ToDate", Value = request.ToDate == null ? DBNull.Value : request.ToDate ,SqlDbType = SqlDbType.DateTime},
                new SqlParameter { ParameterName = "@LoggedInUser",Value = loggedInUser == Guid.Empty ? DBNull.Value : loggedInUser.ToString(), SqlDbType = SqlDbType.VarChar, Size = 50},
                new SqlParameter { ParameterName = "@SearchText", Value = request.SearchText == null ? DBNull.Value : request.SearchText ,SqlDbType = SqlDbType.VarChar, Size = 100},
                    sqlOutputParameter
            };

            apiCommonResponse.Data = await _context.SqlQueryToListAsync<GetAllLeadFreeTrialResponseModel>(ProcedureCommonSqlParametersText.GetLeadFreeTrials, sqlParameters.ToArray());
            apiCommonResponse.Total = sqlOutputParameter.Value != DBNull.Value ? Convert.ToInt32(sqlOutputParameter.Value) : 0; ;
            apiCommonResponse.StatusCode = HttpStatusCode.OK;
            apiCommonResponse.Message = "Data Fetched Successfully.";

            return apiCommonResponse;
        }

        public async Task<ApiCommonResponseModel> GetJunkLeads(QueryValues queryValues ,Guid loggedInUser)
        {
            SqlParameter parameterOutValue = new()
            {
                ParameterName = "TotalCount",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output,
            };

            var apiCommonResponse = new ApiCommonResponseModel();
            SqlParameter[] sqlParameters = new[]
            {
                new SqlParameter { ParameterName = "@IsPaging",      Value = queryValues.IsPaging,SqlDbType = SqlDbType.Int},
                new SqlParameter { ParameterName = "@PageSize",      Value = queryValues.PageSize,SqlDbType = SqlDbType.Int},
                new SqlParameter { ParameterName = "@PageNumber",   Value = queryValues.PageNumber ,SqlDbType = SqlDbType.Int},
                new SqlParameter { ParameterName = "@SortOrder",     Value = queryValues.SortOrder == "" ?  "DESC" : queryValues.SortOrder,SqlDbType = SqlDbType.VarChar, Size = 50},
                new SqlParameter { ParameterName = "@SortExpression",Value = queryValues.SortExpression == "" ?  DBNull.Value: Convert.ToString(queryValues.SortExpression),SqlDbType = SqlDbType.VarChar, Size = 50},
                new SqlParameter { ParameterName = "@FromDate",      Value = queryValues.FromDate ?? Convert.DBNull ,SqlDbType = SqlDbType.DateTime},
                new SqlParameter { ParameterName = "@ToDate",        Value = queryValues.ToDate  ??  Convert.DBNull ,SqlDbType = SqlDbType.DateTime},
                new SqlParameter { ParameterName = "@PrimaryKey",    Value = queryValues.PrimaryKey == "" ?  Convert.DBNull : queryValues.PrimaryKey,SqlDbType = SqlDbType.VarChar, Size = 50},
                new SqlParameter { ParameterName = "@SecondaryKey",  Value = queryValues.SecondaryKey  == "" ?  Convert.DBNull : queryValues.SecondaryKey,SqlDbType = SqlDbType.VarChar,Size = 50},
                new SqlParameter { ParameterName = "@ThirdKey",      Value = queryValues.ThirdKey == "" ?  Convert.DBNull : queryValues.ThirdKey  ,SqlDbType = SqlDbType.VarChar, Size = 50},
                new SqlParameter { ParameterName = "@FourthKey",     Value = queryValues.FourthKey == "" ?  Convert.DBNull : queryValues.FourthKey,SqlDbType = SqlDbType.VarChar, Size = 50},
                new SqlParameter { ParameterName = "@FifthKey",      Value = queryValues.FifthKey == "" ?  DBNull.Value : queryValues.FifthKey,SqlDbType = SqlDbType.VarChar, Size = 50},
                new SqlParameter { ParameterName = "@CreatedBy",      Value = string.IsNullOrWhiteSpace(queryValues.RequestedBy) ? DBNull.Value : queryValues.RequestedBy,SqlDbType = SqlDbType.VarChar,Size = 50 },
                new SqlParameter { ParameterName = "@LoggedInUser",Value = loggedInUser == Guid.Empty ? DBNull.Value : loggedInUser.ToString(), SqlDbType = SqlDbType.VarChar, Size = 50},
                new SqlParameter { ParameterName = "@AssignedTo",    Value = queryValues.AssignedTo == "" ?  DBNull.Value : queryValues.AssignedTo,SqlDbType = SqlDbType.VarChar, Size = 50},
                new SqlParameter { ParameterName = "@SearchText",    Value = queryValues.SearchText == "" ?  DBNull.Value : queryValues.SearchText , SqlDbType = SqlDbType.VarChar, Size = 100},
                parameterOutValue
                };

            _context.Database.SetCommandTimeout(180);

            List<GetProcedureJsonResponse> junkleads = await _context.SqlQueryToListAsync<GetProcedureJsonResponse>(ProcedureCommonSqlParametersText.GetJunkLeads, sqlParameters);

            apiCommonResponse.StatusCode = HttpStatusCode.OK;
            JsonResponseModel jsonTemp = new()
            {
                JsonData = junkleads.FirstOrDefault()?.JsonData,
                TotalCount = Convert.ToInt32(parameterOutValue.Value)
            };
            apiCommonResponse.Data = jsonTemp;
            return (apiCommonResponse);
        }

        public async Task<ApiCommonResponseModel> ChangeStatusCustomerToComplete()
        {
            var msg = "Status from Customer to Complete Completed.";
            var purchaseOrders = await _context.PurchaseOrders
                .Where(item => item.IsActive == true && item.EndDate < DateTime.Now && item.Status == 24)
                .Take(1)
                .ToListAsync();

            var tempMobile = new StringBuilder();

            if (purchaseOrders != null && purchaseOrders.Any())
            {
                foreach (var po in purchaseOrders)
                {
                    if (po.LeadId > 0) // Check if LeadId is not the default empty Guid
                    {
                        var lead = await _context.Leads.FindAsync(po.LeadId); // Use FindAsync for efficient lookup by primary key

                        if (lead != null)
                        {
                            if (lead.PublicKey != Guid.Empty)
                            {
                                await _activityService.LeadLog(lead.PublicKey.ToString(), "3CA214D0-8CB8-EB11-AAF2-00155D53687A", ActivityTypeEnum.MarkAsComplete, description: "Customer to Complete");
                            }
                            po.Status = 4; // Mark as complete
                            po.ModifiedOn = DateTime.Now;
                            po.ModifiedBy = Guid.Parse("3CA214D0-8CB8-EB11-AAF2-00155D53687A");
                            po.IsExpired = true;

                            if (!string.IsNullOrEmpty(lead.MobileNumber))
                            {
                                if (tempMobile.Length > 0)
                                {
                                    tempMobile.Append(", ");
                                }
                                tempMobile.Append(lead.MobileNumber);
                            }
                            else
                            {
                                await _log.AddAsync(new Log
                                {
                                    Category = "ChangeStatusCustomerToComplete",
                                    CreatedOn = DateTime.Now,
                                    Message = $"Lead with Id: {lead.Id} has no mobile number",
                                    Source = "CustomerToComplete"
                                });
                            }
                        }
                        else
                        {
                            await _log.AddAsync(new Log
                            {
                                Category = "ChangeStatusCustomerToComplete",
                                CreatedOn = DateTime.Now,
                                Message = $"LeadId = {po.LeadId} not found for PurchaseOrder.Id: {po.Id}",
                                Source = "CustomerToComplete"
                            });
                        }
                    }
                    else
                    {
                        await _log.AddAsync(new Log
                        {
                            Category = "ChangeStatusCustomerToComplete",
                            CreatedOn = DateTime.Now,
                            Message = $"LeadId not set in PurchaseOrder table with Po.Id: {po.Id}",
                            Source = "CustomerToComplete"
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                return new ApiCommonResponseModel
                {
                    Data = "", // Trim leading comma and spaces
                    StatusCode = HttpStatusCode.OK,
                    Message = "No eligible purchase orders were found."
                };
                // Log or handle the case where no eligible purchase orders were found
            }

            return new ApiCommonResponseModel
            {
                Data = tempMobile.ToString().TrimStart(',', ' ').Trim(), // Trim leading comma and spaces
                StatusCode = HttpStatusCode.OK,
                Message = msg
            };
        }

        public static string NormalizeMobileNumber(string countryCode, string mobileNumber)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber) || string.IsNullOrWhiteSpace(countryCode))
                return mobileNumber;

            try
            {
                var phoneUtil = PhoneNumberUtil.GetInstance();
                int code = int.TryParse(countryCode, out var c) ? c : 0;

                var region = phoneUtil.GetRegionCodeForCountryCode(code);
                var phoneNumber = mobileNumber.StartsWith("+")
                    ? phoneUtil.Parse(mobileNumber, null)
                    : phoneUtil.Parse(mobileNumber, region);

                return phoneNumber.NationalNumber.ToString();
            }
            catch (NumberParseException)
            {
                return mobileNumber;
            }
        }
    }
}