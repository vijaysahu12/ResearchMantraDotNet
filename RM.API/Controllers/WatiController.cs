using RM.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace RM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WatiController : ControllerBase
    {
        private readonly IWatiApiService _watiApiService;

        public WatiController(IWatiApiService watiApiService)
        {
            _watiApiService = watiApiService;
        }

        [HttpPost("SendTemplateMessageMarketManthan")]
        public async Task<IActionResult> SendTemplateMessageMarketManthan([Required] int numberOfLeads)
        {
            //TokenAnalyser tokenAnalyser = new();
            //TokenVariables tokenVariables = null;

            //if (HttpContext.User.Identity is ClaimsIdentity identity)
            //{
            //    tokenVariables = tokenAnalyser.FetchTokenValues(identity);
            //}
            //string loggedInUser = tokenVariables.PublicKey;

            return Ok(await _watiApiService.SendTemplateMessageMarketManthan(numberOfLeads, "3CA214D0-8CB8-EB11-AAF2-00155D53687A"));
        }
        [HttpPost("SendTextToNumber")]
        public async Task<IActionResult> SendTextToNumber(string mobileNumber, string message)
        {
            return Ok(await _watiApiService.SendTextToNumber(mobileNumber, message));
        }

        [HttpGet("GetTemplateName")]
        public async Task<IActionResult> GetTemplateName()
        {
            return Ok(await _watiApiService.GetTemplateName());
        }

        [HttpGet("GetRemainingMessagesToSendCount")]
        public async Task<IActionResult> GetRemainingMessagesToSendCount()
        {
            return Ok(await _watiApiService.GetRemainingMessagesToSendCount());

        }
        //[HttpPost("SendBulkMessages")]
        //public async Task<IActionResult> SendBulkMessages()
        //{
        //    return Ok(await _watiApiService.SendBulkMessages());

        //}

    }
}
