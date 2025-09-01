using RM.CommonServices;
using RM.CommonServices.Helpers;
using RM.Model;
using RM.Model.RequestModel.Notification;
using RM.MService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace RM.MobileAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ScreenerController(ScreenerService service, IMobileNotificationService notificationService) : ControllerBase
    {
        private readonly ScreenerService _service = service;

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get()
        {
            var query = new Model.Common.QueryValues();

            // Optional: only try to assign user if authenticated
            var userPublicKey = UserClaimsHelper.GetClaimValue(User, "userPublicKey");
            if (!string.IsNullOrEmpty(userPublicKey) && Guid.TryParse(userPublicKey, out Guid parsedUserKey))
            {
                query.LoggedInUser = parsedUserKey.ToString();
            }

            var apiCommonResponse = await _service.GetScreenerCategoryData(query);
            return Ok(apiCommonResponse);
        }


        [AllowAnonymous]
        [HttpGet("GetScreenerData")]
        public async Task<IActionResult> GetScreenerData(string code, int screenerId)
        {
            var apiCommonResponse = new ApiCommonResponseModel
            {
                Data = await _service.GetScreenerData(code, screenerId),
                StatusCode = System.Net.HttpStatusCode.OK
            };
            return Ok(apiCommonResponse);

        }

        [HttpPost]
        [Route("/ChartInk")]
        [AllowAnonymous]
        public async Task<IActionResult> ChartInk()
        {
            using var reader = new StreamReader(HttpContext.Request.Body);

            var body = await reader.ReadToEndAsync();
            reader.Close();


//#if DEBUG
//            List<string> bodies = new()
//            {
//                "{'stocks':'BDL,AUBANK,OBEROIRLTY,JUBLFOOD,CHAMBLFERT,BEL','trigger_prices':'1983.7,825.5,1931.2,713.4,569.25,427','triggered_at':'9:17 am','scan_name':'R43MinBreakOutStocks','scan_url':'r43minbreakoutstocks','alert_name':'R43MinBreakOutStocks','webhook_url':'https:mobileapi.kingresearch.co.inChartInk'}"
//               // "{'stocks':'IREDA,IDFCFIRSTB,JUBLFOOD,AMBUJACEM,SHREECEM','trigger_prices':'173.53,75.28,714.45,582.85,31490','triggered_at':'9:20 am','scan_name':'scalpingStrategy','scan_url':'scalpingstrategy-2','alert_name':'scalpingStrategy','webhook_url':'https:mobileapi.kingresearch.co.in.ChartInk'}"
//            };
//            foreach (var bod in bodies)
//            {
//                //ChartInkPost data = JsonConvert.DeserializeObject<ChartInkPost>(bod.Replace("'", "\"")); // Replace single quotes with double quotes
//                var response = await _service.ScreenerDataBinding(bod);
//                return Ok(response);
//            }

//#endif

            var rr = await _service.ScreenerDataBinding(body.Replace("'", "\""));
            return Ok(rr);

        }


        [AllowAnonymous]
        [HttpPost]
        [Route("UpdateContracts")]
        public async Task<IActionResult> UpdateContracts(NotificationToMobileRequestModel obj)
        {
            await _service.UpdateContracts(obj);
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("/ValidateContracts")]
        public IActionResult ValidateContracts()
        {
            var result = _service.GetNfoContracts();
            return Ok(result);
        }

        [HttpPost]
        [Route("/MangeFirebaseNotification")]
        [AllowAnonymous]
        public async Task<IActionResult> MangeFirebaseNotification()
        {
            await _service.MangeFirebaseNotification();
            return Ok();
        }
        //[AllowAnonymous]
        //[HttpPost("ScreenerData")]
        //public async Task<IActionResult> GetScreenerData(ScannerRequestModel request)
        //{
        //    var apiCommonResponse = new ApiCommonResponseModel
        //    {
        //        Data = await _service.GetPaginatedScannerData(request),
        //        StatusCode = System.Net.HttpStatusCode.OK
        //    };
        //    return Ok(apiCommonResponse);

        //}
    }
}