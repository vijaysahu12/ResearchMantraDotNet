using Azure.Storage.Blobs;
using RM.CommonServices.Helpers;
using RM.Model.RequestModel;
using RM.MService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RM.MobileAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly IConfiguration _configuration;


        public DashboardController(IDashboardService dashboardService , IConfiguration configuration)
        {
            _dashboardService = dashboardService;
            _configuration = configuration;
        }

        [HttpGet("GetAdvertisementImage")]
        [AllowAnonymous]
        public IActionResult GetAdvertisementImage(string imageName)

        {


            var mobileAssetsFolderPath = _configuration["Mobile:RootDirectory"];

            var imagePath = Path.Combine(mobileAssetsFolderPath, "Assets", "Advertisement", imageName);
            //var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Advertisement", imageName);

            var imageFileName = Path.GetFileName(imagePath);
            var imageExtension = Path.GetExtension(imageFileName);

            if (System.IO.File.Exists(imagePath))
            {
                var imageBytes = System.IO.File.ReadAllBytes(imagePath);
                return File(imageBytes, $"image/{imageExtension.TrimStart('.').ToLower()}");
            }
            else
            {
                return NotFound($"Image '{imageName}' not found.");
            }

            


        }
        [HttpGet("GetPromotionImage")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPromotionImage(string imageName)
        {
            if (string.IsNullOrEmpty(imageName))
                return BadRequest("Image name is required.");

            string connectionString = _configuration["Azure:StorageConnection"];
            string containerName = _configuration["Azure:ContainerNameFirst"];

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(imageName);

            if (!await blobClient.ExistsAsync())
                return NotFound($"Image '{imageName}' not found.");

            var downloadInfo = await blobClient.DownloadAsync();
            var contentType = downloadInfo.Value.ContentType ?? "application/octet-stream";

            return File(downloadInfo.Value.Content, contentType);
        }

        [HttpPost("GetPromoPopUp")]
        public async Task<IActionResult> GetPromoPopUp([FromBody] RequestPopUpModel model )
        {
            // Guid loggedInUser = Guid.Parse("d7502ca9-7005-f011-b3b9-c3128e7d2ed7"); [FromBody]  string? fcmToken = null, [FromQuery] string? deviceType = null,[FromQuery] string? version = null
            Guid loggedInUser = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));

            return Ok(await _dashboardService.GetPromoPopUpAsync(loggedInUser,model));
        }

        [HttpGet("GetAdvertisementImageList")]
        [AllowAnonymous]

        public async Task<IActionResult> GetAdvertisementImageList(string type)
        {
            return Ok(await _dashboardService.GetAdvertisementImageList(type));
        }



        [HttpDelete("DeleteAdvertisementImage")]
        public async Task<IActionResult> DeleteAdvertisementImage(int id)
        {
            return Ok(await _dashboardService.DeleteAdvertisementImage(id));

        }

        [HttpGet("ProfileScreenAdvertisementImage/{image}")]
        [AllowAnonymous]
        public IActionResult ProfileScreenAdvertisementImage(string image)
        {

            if (image is null)
            {
                return NotFound($"Image not found.");
            }

            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Advertisement", image);
            var imageFileName = Path.GetFileName(imagePath);
            var imageExtension = Path.GetExtension(imageFileName);

            if (System.IO.File.Exists(imagePath))
            {
                var imageBytes = System.IO.File.ReadAllBytes(imagePath);
                return File(imageBytes, $"image/{imageExtension.TrimStart('.').ToLower()}");
            }
            else
            {
                return NotFound($"Image not found.");
            }
        }

        [HttpGet("ProfileScreenImageDetails")]
        [AllowAnonymous]

        public async Task<IActionResult> ProfileScreenImage()
        {
            return Ok(await _dashboardService.ProfileScreenImage());
        }

        /// <summary>
        /// Get all the products where isImportant is true
        /// </summary>
        [HttpGet("GetTop3Strategies")]
        public async Task<IActionResult> GetTop3Strategies(Guid mobileUserKey)
        {
            return Ok(await _dashboardService.GetTop3Strategies(mobileUserKey));

        }

        /// <summary>
        /// Get all the products where isImportant is true
        /// </summary>
        [HttpGet("GetTop3Scanners")]
        public async Task<IActionResult> GetTop3Scanners(Guid mobileUserKey)
        {
            return Ok(await _dashboardService.GetTop3Scanners(mobileUserKey));

        }

        [HttpGet("GetTop3Products")]
        public async Task<IActionResult> GetTop3Products(Guid mobileUserKey)
        {
            return Ok(await _dashboardService.GetTop3Products(mobileUserKey));

        }

        [AllowAnonymous]
        [HttpGet("GetServices")]
        public async Task<IActionResult> GetServices()
        {
            return Ok(await _dashboardService.GetServiceAsync());
        }


    }
}
