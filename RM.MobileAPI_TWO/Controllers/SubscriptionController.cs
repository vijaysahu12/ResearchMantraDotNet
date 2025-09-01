using RM.CommonServices.Helpers;
using RM.MService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static RM.Model.Models.SubscriptionModel;

namespace RM.MobileAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class SubscriptionController(SubscriptionPlanService subscriptionService) : ControllerBase
    {
        private readonly SubscriptionPlanService _subscriptionService = subscriptionService;

        [AllowAnonymous]
        [HttpPost("GetSubscriptionById")]
        public async Task<IActionResult> Get(SubscriptionRequestModel request)
        {
            Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out Guid result);
            request.MobileUserKey = result;
            var response = await _subscriptionService.GetSubscriptionById(request);
            return Ok(response);
        }
    }
}