using RM.API.Dtos;
using RM.API.Helpers;
using RM.API.Models;
using RM.API.Services;
using RM.CommonServices.Helpers;
using RM.Database.ResearchMantraContext;
using RM.Model;
using RM.Model.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RM.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customers;
        //
        public CustomersController(ICustomerService customers)
        {
            _customers = customers;
        }

        // GET: api/Customers
        [HttpPost("GetFilteredCustomers")]
        public async Task<ActionResult> GetFilteredCustomers(QueryValues queryValues)
        {



           var loginuser =  Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));

            queryValues.LoggedInUser = loginuser.ToString();

            ApiCommonResponseModel customers = await _customers.GetFilteredCustomers(queryValues);
            return Ok(customers);
        }


        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, ex);
        //        throw ex;
        //    }
        //}

        // GET: api/Customers/5
        [HttpGet("{id}")]
        public ActionResult<ApiCommonResponseModel> GetCustomer(string id)
        {
            Guid CustomerId = Guid.Parse(id);
            ApiCommonResponseModel responseModel = _customers.GetCustomerById(CustomerId);
            return Ok(responseModel);
        }

        // GET: api/Customers/5
        [HttpPost("GetFilteredCustomerKyc")]
        public async Task<ActionResult<Customer>> GetFilteredCustomerKyc(QueryValues filter)
        {
            return Ok(await _customers.GetFilteredCustomerKyc(filter));
        }

        [HttpGet("GetCustomerFromAdvisory/{id}")]
        public ActionResult<ApiCommonResponseModel> GetCustomerByRowId(string id)
        {
            Guid customerId = Guid.Parse(id);
            ApiCommonResponseModel responseModel = _customers.GetCustomerByRowId(customerId);
            return Ok(responseModel);
        }

        [HttpPut("KYCUpdate")]
        public async Task<ActionResult<ApiCommonResponseModel>> UpdateAdvisoryForm(AdvisoryUpdateRequestModel request)
        {
            string loggedInUser = UserClaimsHelper.GetClaimValue(User, "userPublicKey");
            ApiCommonResponseModel response = await _customers.UpdateAdvisoryForm(request, loggedInUser);
            return Ok(response);
        }

        //[HttpGet("GetCustomerKYCList")]
        //[AllowAnonymous]
        //public async Task<IActionResult> GetCustomerKYCList(string? searchText)
        //{
        //    try
        //    {
        //        //TokenAnalyser tokenAnalyser = new TokenAnalyser();
        //        //var identity = HttpContext.User.Identity as ClaimsIdentity;
        //        //TokenVariables tokenVariables = null;
        //        //if (identity != null)
        //        //{
        //        //    tokenVariables = tokenAnalyser.FetchTokenValues(identity);
        //        //}

        //        //string LoginUser = tokenVariables.PublicKey;
        //        //string LoginUserRole = tokenVariables.RoleKey;
        //        var sqlParameters = new[]
        //        {
        //            new SqlParameter
        //            {
        //                ParameterName = "Symbol",
        //                Value = searchText,
        //                SqlDbType = SqlDbType.VarChar,
        //            }
        //        };
        //        List<GetCustomerKYCDto> kycList = await _context.SqlQueryAsync<GetCustomerKYCDto>("exec spGetCustomerKYC @SearchText ", sqlParameters);
        //        return Ok(kycList);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, ex);
        //    }
        //}

        // PUT: api/Customers/5

        //[HttpPost("KYCUpdate")]
        //public async Task<IActionResult> KYCUpdate(CustomerKycRequest param)
        //{


        //    Guid.TryParse(param.PublicKey, out Guid leadKey);

        //    if (leadKey == null)
        //    {
        //        return BadRequest();
        //    }

        //    var customerKYCData = _context.CustomerKycs.Where(it => it.LeadKey == leadKey.ToString()).FirstOrDefault();
        //    var ss = Enum.Parse(typeof(PartnerAccountStatus), param.Status);
        //    if (ss == null)
        //    {
        //        return BadRequest();
        //    }

        //    customerKYCData.Status = ss.ToString();
        //    customerKYCData.Verified = ss.ToString().ToLower() == "accepted" ? true : false;
        //    _context.CustomerKycs.Update(customerKYCData);
        //    await _context.SaveChangesAsync();
        //    return Ok();
        //}

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(string id, CustomerRequest customer)
        {
            if (Guid.Parse(id) != customer.PublicKey)
            {
                return BadRequest();
            }

            TokenAnalyser tokenAnalyser = new();
            TokenVariables tokenVariables = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                tokenVariables = tokenAnalyser.FetchTokenValues(identity);
            }
            string LoginUser = tokenVariables.PublicKey;
            _ = tokenVariables.RoleKey;

            ApiCommonResponseModel result = await _customers.ManageCustomers(customer, LoginUser);

            return Ok(result);
        }

        // POST: api/Customers


        //[HttpPost]
        //public async Task<ActionResult<Customer>> PostCustomer(Customer customer)
        //{
        //    _context.Customers.Add(customer);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetCustomer", new { id = customer.Id }, customer);
        //}

        //private bool CustomerExists(string id)
        //{
        //    return _context.Customers.Any(e => e.PublicKey.ToString() == id);
        //}


        [AllowAnonymous]
        [HttpGet("GetBde")]
        public async Task<IActionResult> GetBde()
        {
            int systemUserId = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));
            string roleName = UserClaimsHelper.GetClaimValue(User, "roleName");



            return Ok(await _customers.GetBde(systemUserId, roleName));
        }

        [AllowAnonymous]
        [HttpGet("DownloadKyc")]
        public IActionResult DownloadByPath([FromQuery] string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                return NotFound();
            var fileName = Path.GetFileName(filePath);

            var fileBytes = System.IO.File.ReadAllBytes(filePath);

            return File(fileBytes, "application/pdf", fileName);
        }
    }
}
