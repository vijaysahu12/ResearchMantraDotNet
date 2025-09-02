using RM.API.Dtos;
using RM.API.Helpers;
using RM.API.Models;
using RM.API.Services;
using RM.BlobStorage;
using RM.Database.Extension;
using RM.Database.ResearchMantraContext;
using RM.Model;
using RM.Model.Common;
using RM.Model.DB.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RM.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EnquiriesController
        : ControllerBase
    {
        private readonly ResearchMantraContext _context;
        private readonly IActivityService _activityService;
        private readonly IAzureBlobStorageService _azureBlobStorageService;


        public EnquiriesController(ResearchMantraContext context, IActivityService activityService, IAzureBlobStorageService azureBlobStorageService)
        {
            _context = context;
            _activityService = activityService;
            _azureBlobStorageService = azureBlobStorageService;

        }

        // GET: api/Enquiries
        //[HttpGet]
        //public async Task<ActionResult> GetEnquiries()
        //{
        //    TokenAnalyser tokenAnalyser = new();
        //    TokenVariables tokenVariables = null;
        //    if (HttpContext.User.Identity is ClaimsIdentity identity)
        //    {
        //        tokenVariables = tokenAnalyser.FetchTokenValues(identity);
        //    }
        //    string LoginUser = tokenVariables.PublicKey;


        //    SqlParameter[] sqlParameters = new[]
        //    {
        //            new SqlParameter { ParameterName = "RequestedBy", Value = LoginUser  ,SqlDbType = System.Data.SqlDbType.VarChar},
        //    };

        //    List<EnquiryDto> enquiries = await _context.SqlQueryToListAsync<EnquiryDto>($"Sp_GetEnquiries @RequestedBy ", sqlParameters);

        //    return Ok(enquiries);

        //}


        [HttpPost("GetFilteredEnquiries")]
        public async Task<ActionResult> GetEnquiries(QueryValues queryValues)
        {
            ApiCommonResponseModel apiCommonResponse = new();
            TokenAnalyser tokenAnalyser = new();

            TokenVariables tokenVariables = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                tokenVariables = tokenAnalyser.FetchTokenValues(identity);
            }


            queryValues.RoleKey = tokenVariables.RoleKey;
            queryValues.LoggedInUser = tokenVariables.PublicKey;

            SqlParameter parameterOutValue = new()
            {
                ParameterName = "TotalCount",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output,
            };

            SqlParameter[] sqlParameters = new[]
            {

                 new SqlParameter { ParameterName = "IsPaging",      Value = queryValues.IsPaging,SqlDbType = System.Data.SqlDbType.Int},
                 new SqlParameter { ParameterName = "PageSize",      Value = queryValues.PageSize,SqlDbType = System.Data.SqlDbType.Int,},
                 new SqlParameter { ParameterName = "PageNumber ",   Value = queryValues.PageNumber ,SqlDbType = System.Data.SqlDbType.Int,},
                 new SqlParameter { ParameterName = "SortExpression",Value = string.IsNullOrEmpty(queryValues.SortExpression) ?  DBNull.Value: Convert.ToString(queryValues.SortExpression),SqlDbType = System.Data.SqlDbType.VarChar, Size = 50},
                 new SqlParameter { ParameterName = "SortOrder",     Value = string.IsNullOrEmpty(queryValues.SortOrder)  ?  "DESC" : queryValues.SortOrder,SqlDbType = System.Data.SqlDbType.VarChar, Size = 50},
                 new SqlParameter { ParameterName = "FromDate",      Value = queryValues.FromDate ?? Convert.DBNull ,SqlDbType = System.Data.SqlDbType.DateTime},
                 new SqlParameter { ParameterName = "ToDate",        Value = queryValues.ToDate  ??  Convert.DBNull ,SqlDbType = System.Data.SqlDbType.DateTime},
                 new SqlParameter { ParameterName = "LoggedInUser",  Value = queryValues.LoggedInUser   ,SqlDbType = System.Data.SqlDbType.VarChar, Size = 50},
                 new SqlParameter { ParameterName = "SearchText",    Value = queryValues.SearchText == "" ?  DBNull.Value : queryValues.SearchText , SqlDbType = System.Data.SqlDbType.VarChar, Size = 50},
                 parameterOutValue

            };

            var enquiries = await _context.SqlQueryToListAsync<EnquiryDto>("Sp_GetEnquiries @IsPaging = {0},@PageSize = {1},@PageNumber = {2},@SortExpression={3}, @SortOrder={4},@FromDate = {5},@ToDate = {6}, @LoggedInUser={7}, @SearchText={8}, @TotalCount={9} OUTPUT ", sqlParameters);
            string jsonString = JsonConvert.SerializeObject(enquiries);

            apiCommonResponse.StatusCode = HttpStatusCode.OK;
            JsonResponseModel jsonTemp = new()
            {
                JsonData = jsonString,
                TotalCount = Convert.ToInt32(parameterOutValue.Value)
            };
            apiCommonResponse.Data = jsonTemp;
            return Ok(apiCommonResponse);


        }


        // PUT: api/Enquiries/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEnquiries(string id, Enquiry enquiries)
        {
            if (id != enquiries.PublicKey.ToString())
            {
                return BadRequest();
            }

            _context.Entry(enquiries).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Enquiry> entry = _context.Entry(enquiries);
            entry.Property(e => e.PublicKey).IsModified = false;
            entry.Property(e => e.CreatedOn).IsModified = false;
            entry.Property(e => e.CreatedBy).IsModified = false;

            try
            {
                _ = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EnquiriesExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        [HttpPost("PullLeadsEnquiries")]
        public IActionResult PullLeadsEnquiries(Enquiry enquiry)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                List<Enquiry> enquiriesTemp = _context.Enquiries.Where(item => item.ReferenceKey == enquiry.ReferenceKey).ToList();
                var enquiries = (from e in enquiriesTemp
                                 join u in _context.Users
                        on e.CreatedBy equals u.PublicKey.ToString()
                                 orderby e.CreatedOn descending
                                 select new
                                 {
                                     e.Details,
                                     CreatedBy = u.FirstName + " " + u.LastName,
                                     e.CreatedOn,
                                 }).ToList();


                //var enquiries = await _context.Enquiries.Where(item => item.ReferenceKey == enquiry.ReferenceKey.ToString()).ToListAsync();

                //List<EnquiryDto> enquiries = await _context.EnquiryDtos.FromSqlInterpolated($"Sp_PullLeadEnquiries {1}, {enquiry.ReferenceKey.ToString()}").ToListAsync();

                return enquiries == null ? NotFound() : Ok(enquiries);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }

        }




        // POST: api/Enquiries


        [HttpPost]
        public async Task<ActionResult<Enquiry>> PostEnquiries(Enquiry enquiries)
        {
            TokenAnalyser tokenAnalyser = new();
            TokenVariables tokenVariables = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                tokenVariables = tokenAnalyser.FetchTokenValues(identity);
            }
            string UserKey = tokenVariables.PublicKey.ToString();
            enquiries.CreatedBy = UserKey;
            enquiries.ModifiedBy = UserKey;
            enquiries.ModifiedOn = DateTime.Now;
            enquiries.CreatedOn = DateTime.Now;
            _ = _context.Enquiries.Add(enquiries);
            
            Guid publicKey = Guid.Parse(enquiries.ReferenceKey);

            // adding into useractivity
            await _activityService.UserLog(UserKey, publicKey, ActivityTypeEnum.EnquiryAdded, "12.PostEnquiries");
            await _activityService.LeadLog(enquiries.ReferenceKey , UserKey, ActivityTypeEnum.EnquiryAdded,description: "12.PostEnquiries");

            try
            {
                Lead updateLead = await _context.Leads.Where(item => item.PublicKey == publicKey).FirstOrDefaultAsync();
                updateLead.ModifiedOn = DateTime.Now;

                if (updateLead != null)
                {
                    updateLead.Remarks = enquiries.Details;
                }
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Exception dd = ex;
            }

            return CreatedAtAction("GetEnquiries", new { id = enquiries.Id }, enquiries);
        }

        // GET: api/ServiceTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Enquiry>> GetEnquiry(string id)
        {
            Enquiry enquiry = await _context.Enquiries.FirstOrDefaultAsync(c => c.PublicKey.ToString() == id);

            return enquiry == null ? (ActionResult<Enquiry>)NotFound() : (ActionResult<Enquiry>)enquiry;
        }

        //[HttpPost("DeleteEnquiry")]
        //public async Task<ActionResult> DeleteEnquiry(string id)
        //{
        //    var enquiry = await _context.Enquiries.FirstOrDefaultAsync(e => e.PublicKey.ToString() == id);
        //    if (enquiry == null)
        //    {
        //        return NotFound();
        //    }
        //    enquiry.IsDelete = 1;

        //    _context.Enquiries.Add(enquiry);
        //    await _context.SaveChangesAsync();

        //    return Ok(enquiry);
        //}



        private bool EnquiriesExists(string id)
        {
            return _context.Enquiries.Any(e => e.PublicKey.ToString() == id);
            //var enquiries = _context.Enquiries
            //    .Where(c => c.IsLead == enquiry.IsLead)
            //    .Where(c => c.ReferenceKey == enquiry.ReferenceKey)
            //    .ToList();
            //return Ok(enquiries);

            //var enquiries = from e in _context.Enquiries
            //               from ut in _context.UserTypes
            //               from l in _context.Leads
            //              // from c in _context.Customers
            //               where ((e.IsLead == ut.Id) && ((e.ReferenceKey == l.PublicKey.ToString())) && (e.IsDelete == 0))
            //               orderby e.Id descending
            //               select new { Id = e.Id, Enquiry = e.Details, CustomerType=ut.Name, LeadName = l.FullName,  CreatedOn = e.CreatedOn, ReferenceKey = e.ReferenceKey, PublicKey = e.PublicKey, SentBy = e.IsAdmin};

        }

        [AllowAnonymous]
        [HttpPost("PostComplaint")]
        public async Task<ActionResult> PostComplaint([FromForm] TicketRequestModel request)
        {
            List<string> images = new();
            if (request.Images != null)
            {
                foreach (var image in request.Images)
                {
                    images.Add(await _azureBlobStorageService.UploadImage(image));
                }
            }
            var complaint = new Complaints()
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Mobile = request.Mobile,
                Message = request.Message,
                CreatedOn = DateTime.Now,
                Images = request.Images is not null ? string.Join(",", images) : null
            };

            _ = _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();
            return Ok();
        }


        [AllowAnonymous]
        [HttpPost("GetAllComplaints")]
        public async Task<ActionResult> GetAllComplaints(QueryValues queryValues)
        {
            ApiCommonResponseModel apiCommonResponse = new();

            // Output parameter for TotalCount
            SqlParameter parameterOutValue = new()
            {
                ParameterName = "TotalCount",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output,
            };

            // SQL parameters for stored procedure
            SqlParameter[] sqlParameters = new[]
            {
                new SqlParameter { ParameterName = "IsPaging",      Value = queryValues.IsPaging,SqlDbType = System.Data.SqlDbType.Int},
                 new SqlParameter { ParameterName = "PageSize",      Value = queryValues.PageSize,SqlDbType = System.Data.SqlDbType.Int,},
                 new SqlParameter { ParameterName = "PageNumber ",   Value = queryValues.PageNumber ,SqlDbType = System.Data.SqlDbType.Int,},
                new SqlParameter { ParameterName = "FromDate", Value = queryValues.FromDate ?? Convert.DBNull, SqlDbType = SqlDbType.DateTime },
                new SqlParameter { ParameterName = "ToDate", Value = queryValues.ToDate ?? Convert.DBNull, SqlDbType = SqlDbType.DateTime },
                new SqlParameter { ParameterName = "SearchText", Value = string.IsNullOrEmpty(queryValues.SearchText) ? DBNull.Value : queryValues.SearchText, SqlDbType = SqlDbType.VarChar, Size = 50 },
                parameterOutValue // Output parameter for TotalCount
            };

            try
            {
                // Execute stored procedure asynchronously and get the result
                var complaints = await _context.SqlQueryToListAsync<Complaints>("GetAllComplaints  @IsPaging = {0},@PageSize = {1},@PageNumber = {2}, @FromDate = {3}, @ToDate = {4}, @SearchText = {5}, @TotalCount = {6} OUTPUT", sqlParameters);

                // Construct response data
                apiCommonResponse.StatusCode = HttpStatusCode.OK;

                // Create response model with complaints data and total count\

                int TotalCount = Convert.ToInt32(parameterOutValue.Value); // Assign the total count from the output parameter
           

                apiCommonResponse.Data = complaints;
                apiCommonResponse.Total = TotalCount;

                return Ok(apiCommonResponse);
            }
            catch (Exception ex)
            {
                // Handle any exceptions and log them (optional)
                apiCommonResponse.StatusCode = HttpStatusCode.InternalServerError;
                apiCommonResponse.Message = "An error occurred while fetching complaints: " + ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, apiCommonResponse);
            }
        }

    }
}
