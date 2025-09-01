//using Azure.Storage.Blobs.Models;
using RM.CommonServices;
using RM.CommonServices.Helpers;
using RM.CommonServices.Services;
using RM.Model;
using RM.Model.Common;
using RM.Model.RequestModel;
using RM.Model.RequestModel.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using FileHelper = RM.MService.Helpers.FileHelper;

namespace RM.MobileAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class PushNotificationController : ControllerBase
    {
        private readonly IMobileNotificationService _notificationService;
        private readonly SchedulerServiceMobile _schedulerService;
        private readonly MongoDbService _mongoDbServiceService;
        public PushNotificationController(IMobileNotificationService notificationService, SchedulerServiceMobile schedulerService, MongoDbService mongoDbService)
        {
            _notificationService = notificationService;
            _schedulerService = schedulerService;
            _mongoDbServiceService = mongoDbService;
        }

        [AllowAnonymous]
        [HttpPost("GetNotifications")]
        public async Task<IActionResult> GetNotifications([FromBody] GetNotificationRequestModel queryValues)
        {
            return Ok(await _notificationService.GetNotifications(queryValues));
        }

        [HttpGet("MarkAllNotificationAsRead")]
        public async Task<IActionResult> MarkAllNotificationAsRead()
        {
            Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out Guid mobileUserKey);
            return Ok(await _notificationService.MarkAllNotificationAsRead(mobileUserKey));
        }
        [HttpPost("FirebaseSendToDeviceToken")]
        [AllowAnonymous]
        public async Task<IActionResult> FirebaseSendToDeviceToken(FirebaseNotificationRequestWithToken request)
        {
            return Ok(await _notificationService.FirebaseNotificationToToken(request));
        }

        [HttpPost("FirebaseSendToTopic")]
        [AllowAnonymous]
        public async Task<IActionResult> FirebaseSendToTopic(NotificationRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(await _notificationService.SendNotificationToActiveToken(request));
        }

        [HttpPost("MarkNotificationAsRead")]

        public async Task<IActionResult> MarkNotificationAsRead([FromBody] IdModel notificationObjectId)
        {
            return Ok(await _notificationService.MarkNotificationAsRead(notificationObjectId));
        }

        [AllowAnonymous]
        [HttpGet("GetUnreadNotificationCount")]
        public async Task<IActionResult> GetUnreadNotificationCount(Guid userKey)
        {
            return Ok(await _notificationService.GetUnreadNotificationCount(userKey));
        }

        [HttpGet("ExpiryNotification")]
        [AllowAnonymous]
        public async Task<IActionResult> ExpiryNotification()
        {
            return Ok(await _notificationService.SendSubscriptionExpiryCheckNotification());
        }

        //This method we are calling from jarvis algo for (Breakfast(optional for now), MOrning Shorts(yes) etc)
        [HttpPost("SavePushNotification")]
        [AllowAnonymous]
        public async Task<IActionResult> SavePushNotification(SaveNotificationRequestModel request)
        {
            FileHelper.WriteToFile(request.Topic + "_SavePushNotification" + "_" + DateTime.Now.ToShortDateString(), JsonConvert.SerializeObject(request));
            return Ok(await _notificationService.SendNotificationToActiveTokenV1(request));
        }

        [HttpPost]
        [Route("InTradingViewsMessage")]
        [AllowAnonymous]
        public async Task<IActionResult> InTradingViewsMessage([FromBody] NotificationRequestModel request)
        {
            var response = await _notificationService.SendNotificationToActiveToken(request);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("GetScannerNotification")]
        public async Task<IActionResult> GetScannerNotification(QueryValues queryValues)
        {
            if (queryValues.LoggedInUser is null)
            {
                Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out Guid mobileUserKey);
                queryValues.LoggedInUser = mobileUserKey.ToString();
            }
            return Ok(await _notificationService.GetScannerNotifications(queryValues));
        }


        [HttpGet("GetTodayScannerNotification")]
        public async Task<IActionResult> GetTodayScannerNotification(string? Ids)
        {
            int[] Id = Array.Empty<int>();

            if (!string.IsNullOrWhiteSpace(Ids))
            {
                Id = Ids.Split(',')
                             .Select(id =>
                                 int.TryParse(id.Trim(), out var parsedId) ? parsedId : (int?)null)
                             .Where(id => id.HasValue)
                             .Select(id => id.Value)
                             .ToArray();
            }
            Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out Guid mobileUserKey);

            //#if DEBUG
            //            Guid.TryParse("3B21B407-6D64-EF11-8175-00155D23D79C", out Guid mobileUserKeyTemp);
            //            mobileUserKey = mobileUserKeyTemp;
            //#endif
            Guid loggedInUser = mobileUserKey;

            var mongoData = await _mongoDbServiceService.GetTodayScannerNotification(loggedInUser, Id);

            return Ok(new ApiCommonResponseModel
            {
                Data = mongoData,
                StatusCode = HttpStatusCode.OK,
                Message = "Successful"
            });
        }


        [HttpPost]
        [Route("ManageProductNotification")]

        public async Task<IActionResult> ManageProductNotification([FromBody] ManageProductNotificationModel request)
        {
            var result = await _notificationService.ManageProductNotification(request.AllowNotify, request.mobileUserKey, request.productId);
            return Ok(result);
        }

        [HttpPost("SendAdvertismentNotification")]
        [AllowAnonymous]
        public async Task<IActionResult> SendAdvertismentNotification([FromForm] AdModel request)
        {

            var result = await _notificationService.SendAdvertismentNotification(request);
            return Ok(result);
        }

        [HttpPost("SendAdvertismentNotification2")]
        [AllowAnonymous]
        public async Task<IActionResult> SendAdvertismentNotification2([FromBody] AdvertismentModel2 request)
        {
            var result = await _notificationService.SendAdvertismentNotification2(request);
            return Ok(result);
        }

        /// <summary>
        ///  To send notification to only one mobile number for testing purpose and this will not save the notification.
        /// </summary>
        [HttpPost]
        [Route("SendNotificationToMobile")]
        [AllowAnonymous]
        public async Task<IActionResult> SendNotificationToMobile(NotificationToMobileRequestModel param)
        {
            var receiver = await _notificationService.SendNotificationToMobile(param);
            //FileHelper.WriteToFile("SendNotificationToMobile_" + DateTime.Now.ToShortDateString(), "----------------------Completed----------------------");
            return Ok(receiver);
        }

        //Calling From Angular CRM to shoot the free notification to all valid Mobile Users.
        [HttpPost("SendFreeNotification")]
        [AllowAnonymous]
        public async Task<IActionResult> SendFreeNotification(SendFreeNotificationRequestModel param)
        {
            var receiver = await _notificationService.SendFreeNotification(param);
            return Ok(receiver);
        }

        //Calling From Angular CRM to shoot the free notification to all valid Mobile Users.
        [HttpPost("SendNotificationViaCrm")]
        [AllowAnonymous]
        public async Task<IActionResult> SendNotificationViaCrm([FromForm] SendFreeNotificationRequestModel param)
        {
            var receiver = await _notificationService.SendNotificationViaCrm(param);
            return Ok(receiver);
        }

        [HttpPost("SendFreeWhatsAppNotification")]
        [AllowAnonymous]
        public async Task<IActionResult> SendFreeWhatsAppNotification(SendFreeWhatsAppNotificationRequestModel param)
        {
            var receiver = await _notificationService.SendFreeWhatsappNotification(param);
            return Ok(receiver);
        }

        [HttpPost]
        [Route("ValidateAllFCMTokens")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateAllFCMTokens()
        {
            var receiver = await _notificationService.ValidateAllFCMTokens(new NotificationToMobileRequestModel());
            return Ok(receiver);
        }

        [HttpPost]
        [Route("DeleteSevenDaysOldNotification")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteSevenDaysOldNotification()
        {
            return Ok(await _notificationService.DeleteSevenDaysOldNotification());
        }

        [HttpPost("ScheduleNotification")]
        [AllowAnonymous]
        public async Task<IActionResult> ManageScheduleNotification([FromForm] ScheduledNotificationRequestModel notification)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _notificationService.ManageScheduleNotification(notification));
            }
            else
            {
                return Ok(new { StatusCode = HttpStatusCode.BadRequest, Message = "Send required data to continue." });
            }
        }

        [AllowAnonymous]
        [HttpGet("SaveNotificationDataAndRemove")]
        public async Task<ActionResult> SaveNotificationDataAndRemove()
        {
            _ = await _schedulerService.RemoveFirebaseDataAndSaveIntoCallPerformanceTable();
            return Ok("working_" + DateTime.Now);
        }

        [HttpDelete("DeleteNotification")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteNotification([FromQuery] string notificationId)
        {
            if (string.IsNullOrEmpty(notificationId))
            {
                return Ok(new { StatusCode = HttpStatusCode.BadRequest, Message = "NotificationId and MobileUserKey are required." });
            }

            var result = await _notificationService.DeleteNotification(notificationId);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("ScheduleTheTask")]
        public async Task<IActionResult> ScheduleTheTask(string? schedulerName, string? generateToken)
        {

            if (schedulerName == null || schedulerName == "updateStockPrice")
            {
                await _schedulerService.ScheduleTheTaskOn5MinIntervalUsingWindowTaskScheduler();
                await _schedulerService.SendScheduledNotification();
                await _schedulerService.ScheduleThePromotionForMobileAppPopUp();


            }

            else if (schedulerName == "updateMyBucket")
            {
                await _schedulerService.ChangeStatusOfMyBucketForExpiredService();
            }
            else if (schedulerName == "updateGenerateToken")
            {
                await _schedulerService.UpdatePartnerAccountGenerateToken(generateToken);
            }
            return Ok("Tasks processed successfully.");
        }

        [HttpDelete("Delete-push-notification/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> RestorePushNotification(string id)
        {
            var isUpdated = await _mongoDbServiceService.DeletePushNotification(id);

            if (isUpdated)
            {
                return Ok(new { Message = "Push Notification restored successfully!" });
            }

            return BadRequest(new { Message = "Failed to restore Push Notification." });
        }
    }
}