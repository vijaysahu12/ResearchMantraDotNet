using RM.CommonServices.Helpers;
using RM.Model.RequestModel;
using RM.MService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;


namespace RM.MobileAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        public readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        /// <summary>
        /// User receives the otp in the provided whatsapp number.
        /// User cannot send otp more than 3 time withing 30mins.
        /// If a deleted user logs in then its record is deleted from mongoDB.
        /// If the user logs in for the first time in the application then the user will receive a welcome message in whatsapp.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [Route("OtpLogin")]
        public async Task<IActionResult> OtpLogin([Required] string mobileNumber, [Required] string countryCode)
        {
            return Ok(await _accountService.OtpLogin(mobileNumber, countryCode));
        }

        /// <summary>
        /// This method handles the OTP verification process for mobile users through whatsapp.
        /// It verifies the OTP sent to the user's mobile number in whatsapp and returns a response based on the result of the verification attempt.
        /// If the users fails to verify the otp 3 times then user has to wait for 30 mins and then retry.
        /// If the user logging in to a new device then it will send a notification to the old device to logout.
        /// Here access token is generated and refresh is updated only if it doesnt't exist.
        /// SP used: OtpLoginVerificationAndGetSubscription
        /// Table used : MobileUsers, Leads, MyBucketM
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [Route("OtpLoginVerificationAndGetSubscription")]
        public async Task<IActionResult> OtpLoginVerificationAndGetSubscription([FromBody] MobileOtpVerificationRequestModel paramRequest)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _accountService.OtpLoginVerificationAndGetSubscription(paramRequest));
            }
            else
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// To update the user details like profile image, name, email, city, etc.
        /// From here the new user login will receive a welcome message in whatsapp along with mail.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("ManageUserDetails")]
        public async Task<IActionResult> ManageUserDetails([FromForm] ManageUserDetailsRequestModel queryValues)
        {
            return Ok(await _accountService.ManageUserDetails(queryValues));
        }

        /// <summary>
        /// This is to delete the profile image of the user.
        /// </summary>
        [HttpDelete("RemoveProfileImage")]
        public async Task<IActionResult> RemoveProfileImage(Guid publickey)
        {
            return Ok(await _accountService.RemoveProfileImage(publickey));
        }

        /// <summary>
        /// To fetch the user details to show in the profile screen.
        /// </summary>
        [HttpGet]
        [Route("GetUserDetails")]
        public async Task<IActionResult> GetUserDetails(string mobileUserKey)
        {
            if (mobileUserKey is null)
            {
                mobileUserKey = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey")).ToString();
            }
            return Ok(await _accountService.GetUserDetails(mobileUserKey));
        }

        /// <summary>
        /// It will fetch the image from the server.
        /// </summary>
        [HttpGet("GetProfileImage")]
        [AllowAnonymous]
        public IActionResult GetProfileImage(string profileImage)
        {
            if (string.IsNullOrEmpty(profileImage))
            {
                return BadRequest("Image name not given");
            }
            var rootDirectory = Directory.GetCurrentDirectory();
            var imagePath = Path.Combine(rootDirectory, "Assets", "profile-images", profileImage);

            if (imagePath is not null && System.IO.File.Exists(imagePath))
            {
                var imageFileName = Path.GetFileName(imagePath);
                var imageExtension = Path.GetExtension(imageFileName);
                var imageBytes = System.IO.File.ReadAllBytes(imagePath);
                return File(imageBytes, $"image/{imageExtension.TrimStart('.').ToLower()}");
            }
            else
            {
                return NotFound($"Image not found.");
            }
        }

        /// <summary>
        /// To fetch the discount image from the server.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("GetDiscountImage")]
        public IActionResult GetDiscountImage(string imageName)
        {
            if (string.IsNullOrEmpty(imageName))
            {
                return BadRequest("Image name not given");
            }
            var rootDirectory = Directory.GetCurrentDirectory();
            var imagePath = Path.Combine(rootDirectory, "Assets", "Discount-Images", imageName);

            if (imagePath is not null && System.IO.File.Exists(imagePath))
            {
                var imageFileName = Path.GetFileName(imagePath);
                var imageExtension = Path.GetExtension(imageFileName);
                var imageBytes = System.IO.File.ReadAllBytes(imagePath);
                return File(imageBytes, $"image/{imageExtension.TrimStart('.').ToLower()}");
            }
            else
            {
                return NotFound($"Image not found.");
            }
        }

        /// <summary>
        /// To delet the user.
        /// The users data will be removed after 30 days for the current date.
        /// If the user logs in within 30 days then it will continue to use the application as before.
        /// </summary>
        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> Delete(string userKey)
        {
            return Ok(await _accountService.DeleteUser(userKey));
        }

        /// <summary>
        /// To get the topics of the products the user has purchased.
        /// </summary>
        [HttpGet("GetSubscriptionTopics")]
        public async Task<IActionResult> GetSubscriptionTopics(string userKey, string? updateVersion)
        {
            return Ok(await _accountService.GetSubscriptionTopics(userKey, updateVersion));
        }


        /// <summary>
        /// This is to update the partner account details of the user like zerodha, aliceblue, etc.
        /// </summary>
        [HttpPost("ManagePartnerAccount")]
        public async Task<IActionResult> ManagePartnerAccount([FromBody] PartnerAccountsMDetailRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            return Ok(await _accountService.ManagePartnerAccount(request));
        }

        /// <summary>
        /// To fetch the Demat account account names.
        /// </summary>
        [HttpGet("GetDematAccount")]
        public async Task<IActionResult> GetDematAccount()
        {
            return Ok(await _accountService.GetDematAccount());
        }

        /// <summary>
        /// To fetch the partner names like zerodha, aliceblue, etc.
        /// </summary>
        [HttpGet("GetPartnerNames")]
        public async Task<IActionResult> GetPartnerNames()
        {
            return Ok(await _accountService.GetPartnerNames());
        }


        [HttpGet("GetDematAccountDetails")]
        public async Task<IActionResult> GetDematAccountDetails(Guid mobileUserKey)
        {
            return Ok(await _accountService.GetDematAccountDetails(mobileUserKey));
        }

        /// <summary>
        /// To logout the user from the application and make the fcm token null.
        /// </summary>
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestModel request)
        {
            return Ok(await _accountService.Logout(request.MobileUserKey, request.FcmToken));
        }

        /// <summary>
        /// 
        /// </summary>
        [AllowAnonymous]
        [HttpGet("GetNewApiVersionMessage")]
        public async Task<IActionResult> GetNewApiVersionMessage(string version)
        {
            return Ok(await _accountService.GetNewApiVersionMessage(version));
        }

        [AllowAnonymous]
        [HttpGet("GetFreeTrial")]
        public async Task<IActionResult> GetFreeTrial(Guid mobileUserKey)
        {
            return Ok(await _accountService.GetFreeTrial(mobileUserKey));
        }

        [HttpGet("ActivateFreeTrial")]
        public async Task<IActionResult> ActivateFreeTrial(Guid mobileUserKey)
        {
            return Ok(await _accountService.ActivateFreeTrial(mobileUserKey));
        }

        /// <summary>
        /// To get the list of active products for the user.
        /// </summary>
        [HttpGet("GetMyActiveSubscription")]
        public async Task<IActionResult> GetMyActiveSubscription(Guid mobileUserKey)
        {

            if (mobileUserKey == Guid.Empty)
            {
                mobileUserKey = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));
            }
            return Ok(await _accountService.GetMyActiveSubscription(mobileUserKey));
        }

        /// <summary>
        /// To update the fcm token for the user in case if the fcm token is null.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("UpdateFcmToken")]
        public async Task<IActionResult> UpdateFcmToken([FromBody] UpdateFcmTokenRequestModel request)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _accountService.UpdateFcmToken(request));
            }
            else
            {
                request.MobileUserKey = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));
                return Ok(await _accountService.UpdateFcmToken(request));
            }
        }

        /// <summary>
        /// When deleting the user will see a list of statements that will happen after deleting, this will return those statements.
        /// </summary>
        [HttpGet("GetDeleteStatement")]
        public async Task<IActionResult> GetDeleteStatement()
        {
            return Ok(await _accountService.GetDeleteStatement());
        }

        /// <summary>
        /// This method processes a request to delete a mobile user account.
        /// If the specified user is found, the method marks the user for self-deletion by setting a flag and a scheduled deletion date (30 days from the request).
        /// The deletion request and reason are saved to both SQL and MongoDB databases. 
        /// </summary>
        [AllowAnonymous]
        [HttpPost("AccountDelete")]
        public async Task<IActionResult> AccountDelete([FromBody] AccountDeleteRequestModel request)
        {
            return Ok(await _accountService.AccountDelete(request));
        }


        /// <summary>
        /// To delete any image from azure blob storage.
        /// </summary>
        [AllowAnonymous]
        [HttpDelete("DeleteImage")]
        public async Task<IActionResult> DeleteImage(string publickey)
        {
            return Ok(await _accountService.DeleteImage(publickey));
        }


        /// <summary>
        /// This will return a new access token to the user when this is called.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("RefreshToken")]
        public async Task<ActionResult<string>> RefreshToken([FromBody] RefreshTokenRequestModel request)
        {
            return Ok(await _accountService.RefreshToken(request.RefreshToken, request.MobileUserKey, request.DeviceType, request.Version));
        }
    }
}
