using FirebaseAdmin.Messaging;
using RM.API.Dtos;
using RM.API.Helpers;
using RM.API.Models;
using RM.API.Models.Constants;
using RM.API.Services;
using RM.CommonServices.Helpers;
using RM.Database.Extension;
using RM.Database.KingResearchContext;
using RM.Model;
using RM.Model.Common;
using RM.Model.DB.Tables;
using RM.Model.RequestModel;
using RM.Model.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

// Any sales person can create leads, but make sure to avoide the duplicate records based on mobilenumber 
// Any sales person can take leads on his name if that is not assignedTo any other sales person
// Any sales person can take leads on his name if it is not touched from last 45 days 
// Any Sales Person can request only to admin to take the leads which are assigned to some other sales person.
// Admin can forcefully change the assigned leads to Any other sales person 
// As a sales person While Fetching Leads I should should be able to see only my leads wher assignedTo = my name 
// As an Admin I can see other leads also 
// At any point lead source should not change to other source
// As a sales person I can modify only my leads which are assiged to me only 
// As an Admin, I can import the leads, or I can assign the bulk leads to any other sales person 
// As an admin, I can see the PR of any leads of any sales person 
// As an sales person, I can get leads in bulk on my name only condition is leads AssignedTo should be empty.  

namespace RM.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LeadsController : ControllerBase
    {
        private readonly KingResearchContext _context;
        private readonly ILeadService _leadService;
        private readonly IActivityService _activityService;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IConfiguration _config;
        private ApiCommonResponseModel apiCommonResponse = new();

        public LeadsController(KingResearchContext context, IConfiguration config, ILeadService leadService, IActivityService activityService, IPurchaseOrderService purchaseOrderService)
        {
            _context = context;
            _config = config;
            _leadService = leadService;
            _activityService = activityService;
            _purchaseOrderService = purchaseOrderService;
        }

        // GET: api/Leads
        [HttpGet]
        public async Task<ActionResult> GetsLeads()
        {
            TokenAnalyser tokenAnalyser = new();
            TokenVariables tokenVariables = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                tokenVariables = tokenAnalyser.FetchTokenValues(identity);
            }
            string UserKey = tokenVariables.PublicKey.ToString();
            string RoleKey = tokenVariables.RoleKey.ToString();
            string AdminRole = _config.GetValue<string>("AppSettings:AdminRole");

            if (RoleKey == AdminRole)
            {
                var leads = await (from l in _context.Leads
                                   from ls in _context.LeadSources
                                   from lt in _context.LeadTypes
                                   from s in _context.Services
                                   from u in _context.Users
                                   orderby l.Id descending
                                   where (l.LeadSourceKey == ls.PublicKey.ToString()) && (l.LeadTypeKey == lt.PublicKey.ToString()) && (l.ServiceKey == s.PublicKey.ToString()) && (l.IsDelete == 0) && (l.CreatedBy == UserKey) && (l.CreatedBy == u.PublicKey.ToString())
                                   select new { l.Id, LeadName = l.FullName, Phone = l.MobileNumber, Email = l.EmailId, ServiceOpted = s.Name, l.CreatedOn, LeadSource = ls.Name, LeadType = lt.Name, CreatedBy = u.FirstName, l.PublicKey, l.IsSpam }).ToListAsync();

                return Ok(leads.ToList());
            }
            else
            {
                var leads = await (from l in _context.Leads
                                   from ls in _context.LeadSources
                                   from lt in _context.LeadTypes
                                   from s in _context.Services
                                   from u in _context.Users
                                   orderby l.Id descending
                                   where (l.LeadSourceKey == ls.PublicKey.ToString()) && (l.LeadTypeKey == lt.PublicKey.ToString()) && (l.ServiceKey == s.PublicKey.ToString()) && (l.IsDelete == 0) && (l.CreatedBy == u.PublicKey.ToString())
                                   select new { l.Id, LeadName = l.FullName, Phone = l.MobileNumber, Email = l.EmailId, ServiceOpted = s.Name, l.CreatedOn, LeadSource = ls.Name, LeadType = lt.Name, CreatedBy = u.FirstName, l.PublicKey, l.IsSpam }).ToListAsync();

                return Ok(leads.ToList());
            }
        }

        [HttpPost("GetFilteredLeads")]
        public async Task<ActionResult<Lead>> GetFilteredLeads(QueryValues queryValues)
        {
            TokenAnalyser tokenAnalyser = new();

            TokenVariables tokenVariables = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                tokenVariables = tokenAnalyser.FetchTokenValues(identity);
            }

            string loginUser = tokenVariables.PublicKey;
            string loginUserRole = tokenVariables.RoleKey;
            queryValues.RoleKey = loginUserRole;
            var userTemp = await _context.Users.Where(item => item.PublicKey == Guid.Parse(loginUser)).FirstOrDefaultAsync();
            var roles = await _context.Roles.Where(item => item.PublicKey == Guid.Parse(userTemp.RoleKey)).FirstOrDefaultAsync();

            // Set AssignTo = Login User PublicKey only if role is not Admin
            if (roles?.Name?.ToLower() != "admin" && roles?.Name.ToLower() != "dm-ashok" &&roles?.Name?.ToLower() != "globleadmin" && roles?.Name?.ToLower() != "dm_manager")
            {
                if (roles?.Name?.ToLower() == "sales lead")
                {
                    if (string.IsNullOrEmpty(queryValues.AssignedTo))
                    {
                        queryValues.AssignedTo = null;
                    }
                }
                else
                {
                    queryValues.AssignedTo = loginUser;
                }
            }


            if (!roles.Name.Equals("admin", StringComparison.CurrentCultureIgnoreCase) && userTemp.PublicKey != Guid.Parse(loginUser))
            {
                apiCommonResponse.Message = "Invalid login session";
                apiCommonResponse.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest();
            }

            SqlParameter parameterOutValue = new()
            {
                ParameterName = "TotalCount",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output,
            };

            SqlParameter parameterLtcCount = new()
            {
                ParameterName = "LTCCount",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output,
            };

            SqlParameter[] sqlParameters = new[]
            {
                 new SqlParameter { ParameterName = "IsPaging",      Value = queryValues.IsPaging,SqlDbType = SqlDbType.Int},
                    new SqlParameter { ParameterName = "PageSize",      Value = queryValues.PageSize,SqlDbType = SqlDbType.Int,},
                    new SqlParameter { ParameterName = "PageNumber", Value = queryValues.PageNumber, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "SortExpression",Value = queryValues.SortExpression == "" ?  DBNull.Value: Convert.ToString(queryValues.SortExpression),SqlDbType = SqlDbType.VarChar, Size = 50},
                    new SqlParameter { ParameterName = "SortOrder",     Value = queryValues.SortOrder == "" ?  "DESC" : queryValues.SortOrder,SqlDbType = SqlDbType.VarChar, Size = 50},
                    new SqlParameter { ParameterName = "FromDate",      Value = queryValues.FromDate ?? Convert.DBNull ,SqlDbType = SqlDbType.Date},
                    new SqlParameter { ParameterName = "ToDate",        Value = queryValues.ToDate  ??  Convert.DBNull ,SqlDbType = SqlDbType.Date},
                    //new SqlParameter { ParameterName = "Id",            Value = queryValues.Id == 0 ? Convert.DBNull : queryValues.Id  ,SqlDbType = SqlDbType.Int, Size = 50},
                    new SqlParameter { ParameterName = "PrimaryKey",    Value = queryValues.PrimaryKey == "" ?  Convert.DBNull : queryValues.PrimaryKey,SqlDbType = SqlDbType.VarChar, Size = 50},
                    new SqlParameter { ParameterName = "SecondaryKey",  Value = queryValues.SecondaryKey  == "" ?  Convert.DBNull : queryValues.SecondaryKey,SqlDbType = SqlDbType.VarChar,},
                    new SqlParameter { ParameterName = "ThirdKey",      Value = queryValues.ThirdKey == "" ?  DBNull.Value : queryValues.ThirdKey  ,SqlDbType = SqlDbType.VarChar, Size = 50},
                    new SqlParameter { ParameterName = "FourthKey",     Value = queryValues.FourthKey == "" ?  Convert.DBNull : queryValues.FourthKey,SqlDbType = SqlDbType.VarChar, Size = 50},
                    new SqlParameter { ParameterName = "FifthKey",      Value = queryValues.FifthKey == "" ?  DBNull.Value : queryValues.FifthKey,SqlDbType = SqlDbType.VarChar, Size = 50},
                    new SqlParameter { ParameterName = "CreatedBy",     Value = queryValues.RequestedBy == null ?  DBNull.Value : queryValues.RequestedBy,SqlDbType = SqlDbType.VarChar, Size = 50},
                    new SqlParameter { ParameterName = "AssignedTo",    Value = string.IsNullOrEmpty(queryValues.AssignedTo) ? DBNull.Value : queryValues.AssignedTo,SqlDbType = SqlDbType.VarChar,Size = 50},
                    new SqlParameter { ParameterName = "LoggedInUser",  Value = loginUser   ,SqlDbType = SqlDbType.VarChar, Size = 50},
                    new SqlParameter { ParameterName = "RoleKey",       Value = queryValues.RoleKey ,SqlDbType = SqlDbType.VarChar, Size = 50},
                    new SqlParameter { ParameterName = "SearchText",    Value = queryValues.SearchText == "" ?  DBNull.Value : queryValues.SearchText , SqlDbType = SqlDbType.VarChar, Size = 50},
                    parameterOutValue,
                parameterLtcCount
            };

            _context.Database.SetCommandTimeout(180); // Set the timeout to 3 minutes for the query
            // Fetch both paged and approved leads
            var leaddtos = await _context.SqlQueryToListAsync<GetFilteredLeadsProcedureResponse>(
                "EXEC Sp_Get_Leads @IsPaging = {0}, @PageSize = {1}, @PageNumber = {2}, @SortExpression = {3}, @SortOrder = {4}, @FromDate = {5}, @ToDate = {6}, @PrimaryKey = {7}, @SecondaryKey = {8}, @ThirdKey = {9}, @FourthKey = {10}, @FifthKey = {11}, @CreatedBy = {12}, @AssignedTo = {13}, @LoggedInUser = {14}, @RoleKey = {15}, @SearchText = {16}, @TotalCount = {17} OUTPUT, @LTCCount = {18} OUTPUT",
                sqlParameters
            );

            bool hasApprovedCreatedLead = Convert.ToInt32(parameterLtcCount.Value) > 0;

            if (roles.Name.Equals("admin", StringComparison.OrdinalIgnoreCase) || roles.Name.Equals("dm_manager", StringComparison.OrdinalIgnoreCase)
                || roles.Name.Equals("dm-ashok", StringComparison.OrdinalIgnoreCase)
                || roles.Name.Equals("globleadmin", StringComparison.OrdinalIgnoreCase)
                || roles.Name.Equals("sales lead",StringComparison.OrdinalIgnoreCase))
            {
                // Admin-specific logic to check for approved leads
                var adminCreatedApprovedLeads = leaddtos
                    .Where(x => x.PurchaseOrderStatus?.Equals("Approved", StringComparison.OrdinalIgnoreCase) == true && x.AssignedTo == loginUser)
                    .ToList();

                if (adminCreatedApprovedLeads.Any())
                {
                    apiCommonResponse.StatusCode = HttpStatusCode.OK;
                    apiCommonResponse.Message = "First complete your LTC process";  // Corrected message for admin

                    JsonResponseModel jsonTemp = new()
                    {
                        JsonData = JsonConvert.SerializeObject(leaddtos),
                        TotalCount = parameterOutValue.Value != DBNull.Value? Convert.ToInt32(parameterOutValue.Value): 0
                    };

                    apiCommonResponse.Data = jsonTemp;
                    return Ok(apiCommonResponse);
                }
                else
                {
                    // Admin has not created approved leads → show all leads (approved + non-approved)
                    apiCommonResponse.StatusCode = HttpStatusCode.OK;
                    apiCommonResponse.Message = "Showing all leads";  // Optional message
                    JsonResponseModel jsonTemp = new()
                    {
                        JsonData = JsonConvert.SerializeObject(leaddtos),
                        TotalCount = parameterOutValue.Value != DBNull.Value ? Convert.ToInt32(parameterOutValue.Value) : 0
                    };


                    apiCommonResponse.Data = jsonTemp;
                    return Ok(apiCommonResponse);
                }
            }
            else
            {
                // For non-admin: Check approved leads (LTC process)
                var userApprovedLeads = leaddtos
                    .Where(x => x.PurchaseOrderStatus?.Equals("Approved", StringComparison.OrdinalIgnoreCase) == true)
                    .ToList();

                if (userApprovedLeads.Any())
                {
                    apiCommonResponse.StatusCode = HttpStatusCode.OK;
                    apiCommonResponse.Message = "First complete your LTC process";

                    JsonResponseModel jsonTemp = new()
                    {
                        JsonData = JsonConvert.SerializeObject(userApprovedLeads),
                        TotalCount = userApprovedLeads.Count
                    };

                    apiCommonResponse.Data = jsonTemp;
                    return Ok(apiCommonResponse);
                }
                else
                {
                    // If no approved leads, show all assigned leads
                    apiCommonResponse.StatusCode = HttpStatusCode.OK;
                    apiCommonResponse.Message = "Showing all leads";

                    JsonResponseModel jsonTemp = new()
                    {
                        JsonData = JsonConvert.SerializeObject(leaddtos),
                        TotalCount = Convert.ToInt32(parameterOutValue.Value) // Total count for all leads
                    };

                    apiCommonResponse.Data = jsonTemp;
                    return Ok(apiCommonResponse);
                }
            }
        }

        [HttpGet("GetSpamLeads")]
        public async Task<ActionResult> GetSpamLeads()
        {
            var leads = await (from l in _context.Leads
                               from ls in _context.LeadSources
                               from lt in _context.LeadTypes
                               from s in _context.Services
                               from u in _context.Users
                               orderby l.Id descending
                               where (l.LeadSourceKey == ls.PublicKey.ToString()) && (l.LeadTypeKey == lt.PublicKey.ToString()) && (l.ServiceKey == s.PublicKey.ToString()) && (l.IsDelete == 0) && (l.IsSpam == 1) && (l.CreatedBy == u.PublicKey.ToString())
                               select new { l.Id, LeadName = l.FullName, Phone = l.MobileNumber, Email = l.EmailId, CreatedBy = u.FirstName, ServiceOpted = s.Name, l.CreatedOn, LeadSource = ls.Name, LeadType = lt.Name, l.PublicKey, l.IsSpam }).ToListAsync();

            return Ok(leads.ToList());
        }

        // GET: api/Leads/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLead(string id)
        {
            try
            {
                var lead = await (from l in _context.Leads
                                  join ls in _context.LeadSources on l.LeadSourceKey equals ls.PublicKey.ToString() into lsJoin
                                  from ls in lsJoin.DefaultIfEmpty()
                                  join lt in _context.LeadTypes on l.LeadTypeKey equals lt.PublicKey.ToString() into ltJoin
                                  from lt in ltJoin.DefaultIfEmpty()
                                  join s in _context.Services on l.ServiceKey equals s.PublicKey.ToString() into sJoin
                                  from s in sJoin.DefaultIfEmpty()
                                  where l.PublicKey.ToString() == id
                                  select new
                                  {
                                      l.FullName,
                                      l.CountryCode,
                                      l.MobileNumber,
                                      l.AlternateMobileNumber,
                                      l.EmailId,
                                      l.ServiceKey,
                                      l.LeadSourceKey,
                                      l.LeadTypeKey,
                                      l.Remarks,
                                      ServiceName = s != null ? s.Name : null,
                                      LeadSourceName = ls != null ? ls.Name : null,
                                      LeadTypeName = lt != null ? lt.Name : null
                                  }).FirstOrDefaultAsync();

                return lead == null ? NotFound() : Ok(lead);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // PUT: api/Leads/5

        [HttpPut("{id}")]
        public async Task<IActionResult> PutLead(string id, Lead lead)
        {
            TokenAnalyser tokenAnalyser = new();
            TokenVariables tokenVariables = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                tokenVariables = tokenAnalyser.FetchTokenValues(identity);
            }
            string LoginUser = tokenVariables.PublicKey;
            string LoginUserRole = tokenVariables.RoleKey;

            Lead updatedLead = await _context.Leads.Where(c => c.PublicKey == lead.PublicKey).FirstOrDefaultAsync();

            if (updatedLead == null) { return NotFound(); }

            var role = await _context.Roles.Where(item => item.PublicKey == Guid.Parse(LoginUserRole)).FirstOrDefaultAsync();
            // Authorization check: Only Admin OR Assigned user can modify the lead
            //ToDO Apply the logic for "Admin" role also 
            if ((role.Name != "Admin"  && role.Name != "GlobleAdmin") && !string.Equals(updatedLead.AssignedTo?.Trim(), LoginUser?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return Forbid(); // 403 Forbidden
            }


            updatedLead.ModifiedBy = LoginUser;
            updatedLead.MobileNumber = lead.MobileNumber;
            updatedLead.CountryCode = lead.CountryCode;
            updatedLead.FullName = lead.FullName;
            updatedLead.AlternateMobileNumber = lead.AlternateMobileNumber;
            updatedLead.ServiceKey = lead.ServiceKey;
            updatedLead.LeadTypeKey = lead.LeadTypeKey;
            updatedLead.EmailId = lead.EmailId;
            updatedLead.Remarks = lead.Remarks;
            updatedLead.ModifiedOn = DateTime.Now;

            // adding into userActivity for updating lead
            await _activityService.UserLog(LoginUser, lead.PublicKey, ActivityTypeEnum.LeadModified, "10.PutLead");

            // adding into leadActivity for updating lead
            await _activityService.LeadLog(updatedLead.PublicKey.ToString(), LoginUser, ActivityTypeEnum.LeadModified, description: "11.PutLead");

            _context.Entry(updatedLead).State = EntityState.Modified;
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Lead> entry = _context.Entry(updatedLead);
            entry.Property(e => e.PublicKey).IsModified = false;
            entry.Property(e => e.CreatedOn).IsModified = false;
            entry.Property(e => e.CreatedBy).IsModified = false;


            _ = await _context.SaveChangesAsync();

            return Ok();
        }

        // POST: api/Leads
        //When creating Lead
        [HttpPost]
        public async Task<IActionResult> PostLead(Lead lead)
        {
            if (!ModelState.IsValid)
            {
                apiCommonResponse.Message = "Please Provide Valid Input";
                apiCommonResponse.StatusCode = HttpStatusCode.BadRequest;
                return Ok(apiCommonResponse);
            }

            lead.MobileNumber = lead.MobileNumber?.Trim();

            if (string.IsNullOrEmpty(lead.MobileNumber))
            {
                return BadRequest(new { message = "Mobile number cannot be empty or null." }); // 400 Bad Request
            }

            TokenAnalyser tokenAnalyser = new();
            TokenVariables tokenVariables = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                tokenVariables = tokenAnalyser.FetchTokenValues(identity);
            }
            string UserKey = tokenVariables.PublicKey.ToString();
            lead.CreatedBy = UserKey;
            lead.AssignedTo = UserKey;
            lead.ModifiedBy = UserKey;
            lead.CreatedOn = DateTime.Now;
            lead.ModifiedOn = DateTime.Now;
            lead.StatusId = (int)PurchaseOrdersStatusEnum.Fresh;

            Lead isLeadAlreadyExists = await _context.Leads.Where(c => c.MobileNumber == lead.MobileNumber).FirstOrDefaultAsync();

            if (isLeadAlreadyExists == null)
            {
                _ = await _context.Leads.AddAsync(lead);
                _ = await _context.SaveChangesAsync();

                Lead LeadRecent = await _context.Leads.FirstOrDefaultAsync(c => c.Id == lead.Id);
                // Fetch the names for the keys from the database
                string leadTypeName = await _context.LeadTypes
                    .Where(l => l.PublicKey == Guid.Parse(lead.LeadTypeKey))
                    .Select(l => l.Name)
                    .FirstOrDefaultAsync() ?? "Unknown";

                string leadSourceName = await _context.LeadSources
                    .Where(l => l.PublicKey == Guid.Parse(lead.LeadSourceKey))
                    .Select(l => l.Name)
                    .FirstOrDefaultAsync() ?? "Unknown";

                string assignedToName = await _context.Users
                    .Where(u => u.PublicKey == Guid.Parse(lead.AssignedTo))
                    .Select(u => u.FirstName + "" + u.LastName)
                    .FirstOrDefaultAsync() ?? "Unassigned";

                var service = await _context.Services
                        .Where(s => s.PublicKey == Guid.Parse(lead.ServiceKey))
                        .Select(s => new { s.Id, s.Name })
                        .FirstOrDefaultAsync();

                int serviceId = service?.Id ?? 0; // Default to 0 if not found
                string serviceName = service?.Name ?? "Unknown";


                string statusName = await _context.Status
                    .Where(s => s.Id == lead.StatusId)
                    .Select(s => s.Name)
                    .FirstOrDefaultAsync() ?? "Unknown";

                // add user activity for creating lead
                await _activityService.UserLog(UserKey, LeadRecent.PublicKey, ActivityTypeEnum.CreatedLead, "7.PostLeads");
                // add leads activity for creating lead
                await _activityService.LeadLog(LeadRecent.PublicKey.ToString(), UserKey, ActivityTypeEnum.CreatedLead, description: "8.PostLeads");

                if (!string.IsNullOrEmpty(lead.Remarks) && lead.Remarks.Length > 3)
                {
                    Enquiry enquiry = new()
                    {
                        Details = lead.Remarks,
                        IsLead = 1,
                        IsAdmin = 0,
                        ReferenceKey = LeadRecent.PublicKey.ToString(),
                        CreatedBy = UserKey
                    };

                    _ = await _context.Enquiries.AddAsync(enquiry);
                }
                var response = new
                {
                    lead.Id,
                    lead.PublicKey,
                    lead.FullName,
                    lead.EmailId,
                    lead.MobileNumber,
                    lead.Remarks,
                    LeadTypeKey = lead.LeadTypeKey,
                    LeadTypeName = leadTypeName,
                    LeadSourceKey = lead.LeadSourceKey,
                    LeadSourceName = leadSourceName,
                    AssignedTo = lead.AssignedTo,
                    AssignedToName = assignedToName,
                    ServiceKey = lead.ServiceKey,
                    ServiceName = serviceName,
                    ServiceId = serviceId,
                    StatusId = lead.StatusId,
                    StatusName = statusName,
                    PriorityStatus = lead.PriorityStatus,
                    PurchaseOrderKey = lead.PurchaseOrderKey,
                    CreatedBy = lead.CreatedBy,
                    CreatedOn = lead.CreatedOn,
                    ModifiedBy = lead.ModifiedBy,
                    ModifiedOn = lead.ModifiedOn,

                };


                apiCommonResponse.Data = response;
                apiCommonResponse.Message = "Successfully Added";
                apiCommonResponse.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                User user = await _context.Users.Where(user => user.PublicKey.ToString() == isLeadAlreadyExists.CreatedBy.ToString()).FirstOrDefaultAsync();
                string createdBy = "";
                string createdDate = "";

                if (user != null)
                {
                    createdBy = "' had been already added by '" + (user != null ? user.FirstName : "");
                    if (isLeadAlreadyExists != null)
                    {
                        createdDate = "' on " + isLeadAlreadyExists.CreatedOn.ToString();
                    }
                }
                apiCommonResponse.Message = "Already Exists: '" + lead.FullName + " " + createdBy + " " + createdDate;
                apiCommonResponse.StatusCode = HttpStatusCode.Ambiguous;
            }

            _ = await _context.SaveChangesAsync();

            return Ok(apiCommonResponse);

        }

        // POST: api/Leads

        [AllowAnonymous]
        [HttpPost("WebsiteLeads")]
        public async Task<ActionResult<Lead>> PostWebsiteLead(Lead lead)
        {

            lead.MobileNumber = lead.MobileNumber?.Trim();

            Lead leadduplicate = await _context.Leads.FirstOrDefaultAsync(c => c.MobileNumber == lead.MobileNumber);

            string adminKey = _config.GetValue<string>("AppSettings:DefaultAdmin");
            lead.CreatedBy = adminKey;
            Enquiry enquiry = new();

            if (leadduplicate == null)
            {
                _ = _context.Leads.Add(lead);
                _ = await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(lead.Remarks) && lead.Remarks.Length > 3)
                {
                    Lead recentLeadAdded = await _context.Leads.FirstOrDefaultAsync(c => c.Id == lead.Id);
                    enquiry.Details = lead.Remarks;
                    enquiry.IsLead = 1;
                    enquiry.IsAdmin = 0;
                    enquiry.ReferenceKey = recentLeadAdded.PublicKey.ToString();
                    enquiry.CreatedBy = adminKey;

                    _ = _context.Enquiries.Add(enquiry);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(lead.Remarks) && lead.Remarks.Length > 3)
                {
                    enquiry.Details = lead.Remarks;
                    enquiry.IsLead = 1;
                    enquiry.ReferenceKey = leadduplicate.PublicKey.ToString();
                    enquiry.CreatedBy = adminKey;
                    _ = _context.Enquiries.Add(enquiry);
                }
            }

            _ = await _context.SaveChangesAsync();

            return CreatedAtAction("GetLead", new { id = lead.Id }, lead);
        }

        [HttpPost("CustomerKYC"), DisableRequestSizeLimit]
        [AllowAnonymous]
        public async Task<IActionResult> CustomerKYC()
        {
            try
            {
                using StreamReader reader = new(HttpContext.Request.Body);
                string body = await reader.ReadToEndAsync();

                CustomerKYCModal kycData = JsonConvert.DeserializeObject<CustomerKYCModal>(body);

                //ToDo: Check if KYC Is already completed
                Lead leadResult = await _context.Leads.Where(lead => lead.MobileNumber == kycData.mobile.Trim()).FirstOrDefaultAsync();
                CustomerKyc kycDataResult = await _context.CustomerKycs.Where(kyc => kyc.LeadKey == leadResult.PublicKey.ToString()).FirstOrDefaultAsync();

                // Profile Image Save
                kycData.user_img.filename = "Profile_" + kycData.mobile + "_" + kycData.Name + "." + kycData.user_img.filename.Split(".")[1];
                string profileImageDbPath = SaveImage(kycData.user_img);

                if (leadResult == null)
                {
                    _ = _context.Leads.Add(new Lead
                    {
                        FullName = kycData.Name,
                        MobileNumber = kycData.mobile,
                        EmailId = kycData.email_id,
                        CreatedOn = DateTime.Now,
                        ModifiedOn = DateTime.Now,
                        Remarks = "ByeClient",
                        ProfileImage = profileImageDbPath,
                        CreatedBy = "kyc",
                        ModifiedBy = "kyc"
                    });

                    _ = await _context.SaveChangesAsync();
                    leadResult = await _context.Leads.Where(lead => lead.MobileNumber == kycData.mobile.Trim()).FirstOrDefaultAsync();
                }

                // Pan Image Save
                kycData.pan_img.filename = "PAN_" + kycData.mobile + "_" + kycData.Name + "." + kycData.pan_img.filename.Split(".")[1];
                string dbPathForPan = SaveImage(kycData.pan_img);

                CustomerKYCModal kycTempData = kycData;

                if (kycDataResult != null)
                {
                    if ((bool)kycDataResult.Verified)
                    {
                        return Ok(new { Message = "KYC Verification Already Completed." });
                    }

                    if (kycDataResult.Status.ToUpper() == "NEW")
                    {
                        return Ok(new { Message = "KYC Already submitted. Please wait for the approval from the Kingresearch Team." });
                    }
                    else if (kycDataResult.Status.ToUpper() == "REJECTED")
                    {
                        leadResult.FullName = kycData.Name;
                        leadResult.MobileNumber = kycData.mobile;
                        leadResult.EmailId = kycData.email_id;
                        leadResult.CreatedOn = DateTime.Now;
                        leadResult.ModifiedOn = DateTime.Now;
                        leadResult.Remarks = "ByeClient After Rejection";
                        leadResult.ProfileImage = profileImageDbPath;
                        leadResult.CreatedBy = "kyc";
                        leadResult.ModifiedBy = "kyc";
                        //_context.Entry(leadResult).State = EntityState.Modified;

                        kycTempData.pan_img = null;
                        kycTempData.user_img = null;

                        kycDataResult.Verified = false;
                        kycDataResult.ModifiedOn = DateTime.Now;
                        kycDataResult.ModifiedBy = "kyc";
                        kycDataResult.Pan = kycData.pan;
                        kycDataResult.Panurl = dbPathForPan;
                        //_context.Entry(kycDataResult).State = EntityState.Modified;
                    }
                }
                else
                {
                    Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<CustomerKyc> res = _context.CustomerKycs.Add(new CustomerKyc
                    {
                        LeadKey = leadResult.PublicKey.ToString(),
                        CreatedOn = DateTime.Now,
                        ModifiedOn = DateTime.Now,
                        Pan = kycData.pan,
                        Panurl = dbPathForPan,
                        Remarks = "Created while KYC Form filling by client",
                        Status = PartnerAccountStatus.Pending.ToString(),
                        Verified = false,
                        ModifiedBy = "customer",
                        JsonData = JsonConvert.SerializeObject(kycTempData)
                    });
                }

                _ = await _context.SaveChangesAsync();
                return Ok(new { Message = "KYC Successfully Completed." });
            }
            catch (Exception ex)
            {
                FileHelper.WriteToFile("IntradingViews", "Exception while calling InTradingViews: " + ex.ToString());
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [NonAction]
        public string SaveImage(KYCFile fileData)
        {
            try
            {
                //string ImgStr, string ImgName
                string folderName = Path.Combine("KYC", "Documents");
                string pathToSave = Path.Combine(@"C:\KingResearch\", folderName);

                string fileName = fileData.filename.Trim('"');
                string fullPath = Path.Combine(pathToSave, fileName);
                string dbPath = Path.Combine(folderName, fileName);

                byte[] bytes = Convert.FromBase64String(fileData.value);

                using (FileStream stream = new(fullPath, FileMode.Create))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                }

                return dbPath;
            }
            catch (Exception ex)
            {
                FileHelper.WriteToFile("IntradingViews", "Exception while calling InTradingViews: " + ex.ToString());
                throw;
            }
        }

        [HttpGet("CheckLeadDuplicateByNumber")]
        public async Task<IActionResult> CheckLeadDuplicateByNumber(string mobileNumber, string leadKey)
        {
            apiCommonResponse = new ApiCommonResponseModel();

            if (string.IsNullOrEmpty(mobileNumber))
            {
                apiCommonResponse.StatusCode = HttpStatusCode.OK;
                apiCommonResponse.Message = "Please provide mobile number to check duplicate";
            }
            else
            {
                _ = Guid.TryParse(leadKey, out Guid guidOutput);
                SqlParameter[] sqlParameters = new[]
                {
               new SqlParameter { ParameterName = "mobileNumber",Value = mobileNumber ,SqlDbType = SqlDbType.VarChar},
               new SqlParameter { ParameterName = "leadKey",Value = guidOutput  ,SqlDbType = SqlDbType.UniqueIdentifier},
            };
                apiCommonResponse.StatusCode = HttpStatusCode.OK;
                apiCommonResponse.Data = await _context.SqlQueryToListAsync<DuplicateLeadExists>("CheckLeadDuplicateByNumber @mobileNumber = {0}, @leadKey = {1}", sqlParameters);
            }
            return Ok(apiCommonResponse);
        }

        [HttpGet("CheckLeadStatus")]
        public async Task<IActionResult> CheckLeadStatus(string mobileNumber, string leadKey)
        {
            apiCommonResponse = new ApiCommonResponseModel();

            if (string.IsNullOrEmpty(mobileNumber))
            {
                apiCommonResponse.StatusCode = HttpStatusCode.OK;
                apiCommonResponse.Message = "Please provide mobile number to check Lead Status";
            }
            else
            {
                _ = Guid.TryParse(leadKey, out Guid guidOutput);
                SqlParameter[] sqlParameters = new[]
                {
               new SqlParameter { ParameterName = "mobileNumber",Value = mobileNumber ,SqlDbType = SqlDbType.VarChar},
               new SqlParameter { ParameterName = "leadKey",Value = guidOutput  ,SqlDbType = SqlDbType.UniqueIdentifier},
            };
                apiCommonResponse.StatusCode = HttpStatusCode.OK;
                apiCommonResponse.Data = await _context.SqlQueryToListAsync<DuplicateLeadExists>("GetLeadStatus @mobileNumber = {0}, @leadKey = {1}", sqlParameters);
            }
            return Ok(apiCommonResponse);
        }



        [HttpPost("MarkAsFavourite")]
        public async Task<IActionResult> MarkAsFavourite(MarkAsFavouriteRequestModel request)
        {
            TokenAnalyser tokenAnalyser = new();
            TokenVariables tokenVariables = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                tokenVariables = tokenAnalyser.FetchTokenValues(identity);
            }

            Lead result = await _context.Leads.Where(lead => lead.PublicKey == request.LeadKey).FirstOrDefaultAsync();
            result.Favourite = request.Favourite;
            result.ModifiedOn = DateTime.Now;
            result.ModifiedBy = tokenVariables.PublicKey;
            _ = await _context.SaveChangesAsync();

            return Ok(apiCommonResponse);
        }

        #region Junk Leads

        [HttpPost("GetJunkLeads")]
        public async Task<ActionResult<Lead>> GetJunkLeads(QueryValues queryValues)
       {
            _ = Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out Guid loggedInUser);
            var role = (UserClaimsHelper.GetClaimValue(User, "roleName"));

            if (role.ToLower() == "admin" || role.ToLower()== "sales lead" || role.ToLower() == "globleadmin")
            {
                return Ok(await _leadService.GetJunkLeads(queryValues,loggedInUser));
            }
            
            else
            {
                return BadRequest();
            }
        }

        [HttpGet("GetFollowupReminders")]
        public async Task<ActionResult> GetFollowupReminders(Guid leadKey)
        {
            apiCommonResponse.Message = "Success";
            apiCommonResponse.StatusCode = HttpStatusCode.OK;
            apiCommonResponse.Data = await _context.FollowUpReminders.Where(lead => lead.LeadKey == leadKey).OrderByDescending(item => item.ModifiedOn).Take(10).ToListAsync();
            return Ok(apiCommonResponse);
        }
        [HttpGet("GetFollowupRemindersAll")]
        public async Task<ActionResult> GetFollowupRemindersAll(string type)
        {
            TokenAnalyser tokenAnalyser = new();
            TokenVariables tokenVariables = null;
            int systemUserId = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));


            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                tokenVariables = tokenAnalyser.FetchTokenValues(identity);
            }

            string userKey = tokenVariables.PublicKey.ToString(); // Guid in string form
            //Guid userId = tokenVariables.UserId; // Numeric ID (used for SupervisorId)
            string roleKey = tokenVariables.RoleKey.ToString();

            Role role = await _context.Roles.FirstOrDefaultAsync(r => r.PublicKey == Guid.Parse(roleKey));
            string roleName = role?.Name?.ToLower();

            List<Guid> subordinateKeys = new();

            if (roleName == "sales lead")
            {
                subordinateKeys = await _context.Users
                    .Where(u =>( u.SupervisorId == systemUserId||u.Id== systemUserId) && (u.IsDisabled == null || u.IsDisabled == 0))
                    .Select(u => u.PublicKey.Value)
                    .ToListAsync();
            }

            var remindersQuery = from reminder in _context.FollowUpReminders
                                 join lead in _context.Leads on reminder.LeadKey equals lead.PublicKey
                                 where
                                    (roleName == "admin") || (roleName == "globleadmin") ||
                                    (reminder.CreatedBy == Guid.Parse(userKey)) ||
                                    (roleName == "sales lead" && subordinateKeys.Contains(reminder.CreatedBy))
                                 orderby reminder.NotifyDate descending
                                 select new
                                 {
                                     FullName = lead.FullName,
                                     MobileNumber = lead.MobileNumber,
                                     reminder.LeadKey,
                                     reminder.Id,
                                     reminder.IsActive,
                                     reminder.NotifyDate,
                                     reminder.Comments,
                                     reminder.CreatedBy,
                                 };

            var reminders = await remindersQuery.ToListAsync();

            apiCommonResponse.Message = "Success";
            apiCommonResponse.StatusCode = HttpStatusCode.OK;
            apiCommonResponse.Data = reminders;

            return Ok(apiCommonResponse);
        }


        [HttpPost("ManageReminders")]
        public async Task<ActionResult> ManageReminders(SetRemindersRequestModel request)
        {
            apiCommonResponse.Message = "Reminder Added Successfully";
            apiCommonResponse.StatusCode = HttpStatusCode.OK;
            if (string.IsNullOrEmpty(request.Comments) || string.IsNullOrWhiteSpace(request.Comments))
            {
                return BadRequest();
            }
            TokenAnalyser tokenAnalyser = new();
            TokenVariables tokenVariables = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                tokenVariables = tokenAnalyser.FetchTokenValues(identity);
            }
            string UserKey = tokenVariables.PublicKey.ToString();
            string RoleKey = tokenVariables.RoleKey.ToString();

            FollowUpReminder exists = new();
            if (request.Action.ToUpper() == "DELETE")
            {
                exists = _context.FollowUpReminders.Where(item => item.Id == request.Id).FirstOrDefault();
                if (exists != null)
                {
                    _ = _context.FollowUpReminders.Remove(exists);
                    await _activityService.UserLog(UserKey, request.LeadKey, ActivityTypeEnum.FollowUpReminderDeleted, "9.ManageReminders");
                    _ = await _context.SaveChangesAsync();
                    apiCommonResponse.Message = "Reminder deleted successfully";
                }
                else
                {
                    apiCommonResponse.Message = "Not Found";
                    apiCommonResponse.StatusCode = HttpStatusCode.NotFound;
                }

                return Ok(apiCommonResponse);
            }

            else if (request.Action.ToUpper() == "EDIT")
            {
                exists = request.Id != 0 && request.Action.ToUpper() == "EDIT"
                ? await _context.FollowUpReminders.Where(item => item.Id == request.Id).FirstOrDefaultAsync()
                : await _context.FollowUpReminders.Where(item => item.LeadKey == request.LeadKey && item.NotifyDate == request.NotifyDate).FirstOrDefaultAsync();
                if (exists != null)
                {
                    Lead updateLead = await _context.Leads.Where(item => item.PublicKey == request.LeadKey).FirstOrDefaultAsync();
                    updateLead.ModifiedOn = DateTime.Now;
                    _ = await _context.SaveChangesAsync();
                    // adding into useractivity when creating follow up reminder
                    await _activityService.UserLog(UserKey, exists.LeadKey, ActivityTypeEnum.FollowUpReminderEdited, "6.ManageReminders");

                    _ = await _context.SaveChangesAsync();
                    exists.ModifiedOn = DateTime.Now;
                    exists.CreatedBy = Guid.Parse(UserKey);
                    exists.LeadKey = request.LeadKey;
                    exists.NotifyDate = request.NotifyDate;
                    exists.Comments = request.Comments;
                    exists.IsActive = request.IsActive;
                    apiCommonResponse.Message = "Reminder Updated Successfully";
                }
            }
            else
            {
                exists = await _context.FollowUpReminders.Where(item => item.LeadKey == request.LeadKey && item.NotifyDate == request.NotifyDate).FirstOrDefaultAsync();

                if (exists != null)
                {
                    apiCommonResponse.StatusCode = HttpStatusCode.Conflict;
                    apiCommonResponse.Message = "Follow-up Reminders Already Exist";
                    return Ok(apiCommonResponse);
                }
                Lead updateLead = await _context.Leads.Where(item => item.PublicKey == request.LeadKey).FirstOrDefaultAsync();
                updateLead.ModifiedOn = DateTime.Now;
                _ = await _context.SaveChangesAsync();
                exists = new FollowUpReminder
                {
                    Comments = request.Comments,
                    CreatedBy = Guid.Parse(UserKey),
                    ModifiedOn = DateTime.Now,
                    LeadKey = request.LeadKey,
                    NotifyDate = request.NotifyDate,
                    Id = request.Id,
                    IsActive = request.IsActive
                };
                _ = _context.FollowUpReminders.Add(exists);

                // added into useactivity
                await _activityService.UserLog(UserKey, updateLead.PublicKey, ActivityTypeEnum.FollowUpReminderAdded, "ManageReminders");
            }
            _ = await _context.SaveChangesAsync();
            return Ok(apiCommonResponse);
        }

        #endregion Junk Leads

        [HttpPost("LeadAllotments")]
        public async Task<IActionResult> LeadAllotments(LeadAllotmentsRequestModel leadList)
        {
            apiCommonResponse.StatusCode = HttpStatusCode.BadRequest;
            if (ModelState.IsValid && leadList != null && leadList.Leads.Count > 0)
            {
                var role = (UserClaimsHelper.GetClaimValue(User, "roleName"));

                string UserKey = UserClaimsHelper.GetClaimValue(User, "userPublicKey");
                //string roleKey = tokenVariables.RoleKey.ToString();
                apiCommonResponse = await _leadService.LeadAllotments(leadList, UserKey, leadList.OverrideAllotment);

            }
            return Ok(apiCommonResponse);
        }

        [AllowAnonymous]
        [HttpPost("ExcelUpload")]
        public async Task<IActionResult> ImportExcel([FromForm] ExcelImportModal tempModal)
        {
            // Validate all required keys
            if (string.IsNullOrWhiteSpace(tempModal.leadType) ||
                string.IsNullOrWhiteSpace(tempModal.leadSource) ||
                string.IsNullOrWhiteSpace(tempModal.leadService))
            {
                ApiCommonResponseModel responseModel = new ApiCommonResponseModel();
                responseModel.StatusCode = HttpStatusCode.BadRequest;
                responseModel.Message = "All fields are required and cannot be empty";
                return Ok(responseModel);
            }

            QueryValues jsonData = new()
            {
                PrimaryKey = tempModal.leadType,
                SecondaryKey = tempModal.leadSource,
                ThirdKey = tempModal.leadService,
                FourthKey = tempModal.publicKey,
                SixthKey = tempModal.comments
            };

            ApiCommonResponseModel result = await _leadService.ImportExcelForLeads(tempModal.excelFile, jsonData);
            return Ok(result);
        }

        [HttpPost("ActivityLogs")]
        public async Task<IActionResult> ActivityLogs(QueryValues queryValues)
        {
            _ = Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out Guid loggedInUser);
            var role = (UserClaimsHelper.GetClaimValue(User, "roleName"));

            if (role.ToLower() == "admin" || role.ToLower() == "globleadmin" || role.ToLower() == "sales lead" || role.ToLower() == "bde")
            {
                return Ok(await _leadService.GetActivityLogs(queryValues ,loggedInUser));
            }

            else
            {
                return BadRequest();
            }

            // ApiCommonResponseModel result = await _leadService.GetActivityLogs(queryValues);
            // return Ok(result);
        }

        [HttpPost("SelfAssignJunkLeads")]
        public async Task<IActionResult> SelfAssignJunkLeads(QueryValues queryValues)
        {
            TokenAnalyser tokenAnalyser = new();
            TokenVariables tokenVariables = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                tokenVariables = tokenAnalyser.FetchTokenValues(identity);
            }

            ApiCommonResponseModel responseModel = new ApiCommonResponseModel();
            responseModel.Data = await _leadService.SelfAssignJunkLeads(Guid.Parse(tokenVariables.PublicKey), queryValues);
            return Ok(responseModel);
        }

        [AllowAnonymous]
        [HttpGet("GetSelfAssignJunkLeads")]
        public async Task<IActionResult> GetSelfAssignJunkLeads()
        {
            ApiCommonResponseModel responseModel = new ApiCommonResponseModel();
            responseModel.Data = await _leadService.GetSelfAssignJunkLeads();
            return Ok(responseModel);
        }

        [AllowAnonymous]
        [HttpPost("SaveCodeLineContactInfo")]
        public async Task<IActionResult> SaveCodeLineContactInfo(CodeLineContactInfoRequestModel request)
        {
            var details = new CodelineContactInfo()
            {
                ContactNumber = request.ContactNumber,
                CreatedOn = DateTime.Now,
                Email = request.Email,
                FullName = request.FullName,
                Reason = request.Reason
            };

            _context.CodelineContactInfo.Add(details);
            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpGet("GetFreeTrailReasons/{freeTrailId:int}")]
        public async Task<IActionResult> GetLeadFreeTrailReasons(int freeTrailId)
        {
            return Ok(await _leadService.GetFreeTrailReasons(freeTrailId));
        }

        [AllowAnonymous]
        [HttpPost("GetAllLeadFreeTrials")]
        public async Task<IActionResult> GetAllLeadFreeTrials([FromBody] QueryValues query)            

        {
            _ = Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out Guid loggedInUser);
            var response = await _leadService.GetAllLeadFreeTrialsAsync(query,loggedInUser);
            return Ok(response);
        }


        [HttpGet("changeStatusCustomerToComplete")]
        public async Task<IActionResult> ChangeStatusCustomerToComplete()
        {
            var response = await _leadService.ChangeStatusCustomerToComplete();

            return Ok(response);
        }


        [HttpPost("GetPurchaseOrderById")]
        public async Task<IActionResult> GetPurchaseOrderById(QueryValues query, Guid loggedInUser)
        {
            var respo = await _purchaseOrderService.GetFilteredPurchaseOrders(query,loggedInUser);
            return Ok(respo);
        }
    }
}