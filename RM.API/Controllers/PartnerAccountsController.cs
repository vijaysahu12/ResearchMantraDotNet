using RM.API.Dtos;
using RM.API.Helpers;
using RM.API.Services;
using RM.CommonServices.Helpers;
using RM.Database.ResearchMantraContext;
using RM.Model.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RM.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PartnerAccountsController : ControllerBase
    {
        private readonly ResearchMantraContext _context;
        private readonly IPartnerService _ipartnerService;
        private TokenAnalyser tokenObject = new();
        private TokenVariables loginUser = null;

        public PartnerAccountsController(ResearchMantraContext context, IPartnerService iPartnerService)
        {
            _context = context;
            _ipartnerService = iPartnerService;
            //loginUser = tokenObject.FetchTokenValues(HttpContext.User.Identity as ClaimsIdentity);
        }

        [AllowAnonymous]
        // GET: api/PartnerAccounts
        [HttpPost("GetPartnerAccounts")]
        public async Task<IActionResult> GetPartnerAccounts(QueryValues queryValues)
        {
            var result = await _ipartnerService.GetAll(queryValues);
            return Ok(result);
        }
        [AllowAnonymous]
        [HttpPost("GetPartnerAccountCommnets")]
        public async Task<IActionResult> GetPartnerAccountsComments(QueryValues queryValues)
        {
            var result = await _ipartnerService.GetPartnerComments(queryValues);
            return Ok(result);
        }
        [AllowAnonymous]
        [HttpGet("GetByMobile/{mobileNumber}")]
        public async Task<IActionResult> GetByMobile(string mobileNumber)
        {
            var result = _ipartnerService.GetByMobile(mobileNumber);

            return Ok(result);
        }

        [HttpGet("GetAllMobiles")]
        public IActionResult GetAllMobiles()
        {
            var mobileNumbers = _context.PartnerAccounts
                .Where(p => !string.IsNullOrEmpty(p.MobileNumber) && (p.IsDelete == 0 || p.IsDelete == null))
                .Select(p => p.MobileNumber)
                .Distinct()
                .ToList();
            return Ok(mobileNumbers);
        }

        // GET: api/PartnerAccounts/5
        [HttpGet("{id}")]
        public ActionResult<PartnerAccount> GetPartnerAccounts(long id)
        {
            var dd = _ipartnerService.GetById(id);
            return Ok(dd);
        }

        [HttpGet("GetEmployees")]
        public async Task<IActionResult> GetEmployees(long id)
        {
            try
            {
                var users = await _context.Users.Where(item => (item.IsDisabled ?? 0) == 0 && (item.IsDelete ?? 0) == 0).Select(item => new
                {
                    item.FirstName,
                    item.LastName,
                    item.Id
                }).ToListAsync();
                //var partnerComments = await _context.PartnerComments.Where(item => item.PartnerId == Convert.ToInt32(id)).OrderByDescending(item => item.CreatedOn).ToListAsync();
                return Ok(users);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet("GetPartnerComments")]
        public async Task<IActionResult> GetPartnerAccountsAndComments(long id)
        {
            var partnerComments = _ipartnerService.GetPartnerAccountsAndComments(id);
            return partnerComments == null ? NotFound() : Ok(partnerComments);
        }

        // PUT: api/PartnerAccounts/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public IActionResult PutPartnerAccounts(long id, AllParnterAccountCode queryValues)
        {
            queryValues.PublicKey = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));

            if (id != queryValues.Id && !ModelState.IsValid)
            {
                return BadRequest();
            }
            queryValues.Id = id;

            loginUser = tokenObject.FetchTokenValues(HttpContext.User.Identity as ClaimsIdentity);
            if (loginUser is not null)
            {
                queryValues.ModifiedBy = loginUser.PublicKey;
                queryValues.CreatedBy = loginUser.PublicKey;
            }
            else
            {
                return Unauthorized();
            }
            var response = _ipartnerService.Manage(loginUser, queryValues);
            return Ok(response);
        }

        /// <summary>
        /// https://www.kingresearch.co.in/demat-account/
        /// Usign this API to add partner accounts from kingresearch.co.in
        /// </summary>
        [AllowAnonymous]
        [HttpPost("AddPartnerAccounts")]
        public ActionResult<Lead> AddPartnerAccountsPartner(PartnerAccountRequestModel queryValues)
        {
            var rr = _ipartnerService.Add(queryValues);
            return Ok(rr);
        }

        [HttpDelete("DeletePartnerAccounts")]
        public async Task<ActionResult<Lead>> DeletePartnerAccounts(long id)
        {
            loginUser = tokenObject.FetchTokenValues(HttpContext.User.Identity as ClaimsIdentity);

            _ipartnerService.Delete(id, loginUser);
            return Ok();
        }

        [HttpPost]
        [Route("GetPartnerAccountsSummaryReport")]
        public async Task<IActionResult> GetPartnerAccountsSummaryReport(QueryValues request)
        {
            Model.ApiCommonResponseModel result = await _ipartnerService.GetPartnerAccountsSummaryReport(request);
            return Ok(result);
        }

        [HttpPost]
        [Route("GetPartnerCount")]
        public async Task<IActionResult> GetPartnerCount(QueryValues request)
        {
            Model.ApiCommonResponseModel result = await _ipartnerService.GetPartnerCountAsync(request);
            return Ok(result);
        }

        [HttpPost]
        [Route("GetPartnerStatusCount")]
        public async Task<IActionResult> GetPartnerStatusCount(QueryValues request)
        {
            Model.ApiCommonResponseModel result = await _ipartnerService.GetPartnerStatusCountAsync(request);
            return Ok(result);
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("GetPartnerReferralLinks")]
        public async Task<IActionResult> GetPartnerReferralLinks()
        {
            return Ok(await _ipartnerService.GetPartnerReferralLinks());
        }
    }
}