using RM.API.Dtos;
using RM.API.Helpers;
using RM.Database.ResearchMantraContext;
using RM.Model;
using RM.Model.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using RM.API.Services;
using RM.CommonServices.Helpers;
namespace RM.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly ResearchMantraContext _context;
        private readonly IConfiguration _config;
        private readonly IDashboardService _dashboardService;

        public DashboardController(ResearchMantraContext context, IConfiguration config, IDashboardService dashboardService)
        {
            _context = context;
            _config = config;
            _dashboardService = dashboardService;

        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            //TokenVariables tokenVariables = TokenAnalyserStatic.FetchTokenPart2(HttpContext);

            //return Ok(await _dashboardService.GetDetails(tokenVariables.PublicKey));


            //_ = Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out Guid loggedInUser);
            var role = (UserClaimsHelper.GetClaimValue(User, "roleName"));

            if (role.ToLower() == "admin" || role.ToLower() == "globleadmin" || role.ToLower() == "sales lead")
            {
                return Ok(await _dashboardService.GetDetails());
            }

            else
            {
                return BadRequest();
            }


        }
        [HttpPost("GetSalesDashboard")]
        public async Task<IActionResult> GetSalesDashboardDetails(QueryValues request)
        {
            TokenVariables tokenVariables = TokenAnalyserStatic.FetchTokenPart2(HttpContext);
            request.LoggedInUser = tokenVariables.PublicKey.ToString();

            ApiCommonResponseModel res = await _dashboardService.GetSalesDashboardDetails(request);
            return Ok(res);
        }

        [HttpPost("GetMobileDashboard")]
        public async Task<IActionResult> GetMobileDashboard(MobileDashboardQueryValues request)
        {
            var response = await _dashboardService.GetMobileDashboard(request);
            return Ok(response);
        }

        [HttpPost("GetLogsAndExceptionsForMobileDashboard")]
        public async Task<IActionResult> GetLogsAndExceptionsForMobileDashboard(MobileDashboardQueryValues request)
        {
            var response = await _dashboardService.GetLogsAndExceptionsForMobileDashboard();
            return Ok(response);
        }

    }
}
