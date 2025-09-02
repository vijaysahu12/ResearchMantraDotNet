using Azure.Storage.Blobs;
using RM.API.Dtos;
using RM.API.Helpers;
using RM.API.Models;
using RM.API.Services;
using RM.CommonServices.Helpers;
using RM.CommonServices.Services;
using RM.Database.ResearchMantraContext;
using RM.Model.Common;
using RM.Model.Models;
using RM.Model.RequestModel;
using RM.Model.RequestModel.Notification;
using RM.Model.ResponseModel;
using RM.MService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using ApiCommonResponseModel = RM.Model.ApiCommonResponseModel;

namespace RM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MobileController : ControllerBase
    {
        private readonly IMobileService _mobileProductService;
        private readonly IConfiguration _config;
        private readonly ResearchMantraContext _dbContext;
        private readonly MongoDbService _mongoDbService;
       
        private readonly IBlogService _blogService;

        public MobileController(IMobileService mobileProductService, IConfiguration config, ResearchMantraContext dbContext,
            MongoDbService mongoDbService, IBlogService blogService)
        {
            _mobileProductService = mobileProductService;
            _config = config;
            _dbContext = dbContext;
            _mongoDbService = mongoDbService;
             _blogService = blogService;
        }

        [HttpPost]
        public async Task<IActionResult> ManageProduct([FromForm] MobileProductRequestModel requestModel)
        {
            return Ok(await _mobileProductService.ManageProduct(requestModel));
        }

        [HttpGet]
        public async Task<IActionResult> GetMobileProducts(string searchText = null)
        {
            return Ok(await _mobileProductService.GetMobileProducts(searchText));
        }

        [HttpGet("GetProductCategories")]
        public async Task<IActionResult> GetProductCategories()
        {
            return Ok(await _mobileProductService.GetProductCategories());
        }

        [HttpGet("GetProductContent/{productId}")]
        public async Task<IActionResult> GetProductContent(int productId)
        {
            return Ok(await _mobileProductService.GetProductContent(productId));
        }

        [HttpGet("GetProducts")]
        public async Task<IActionResult> GetProducts(string searchText = null)
        {
            return Ok(await _mobileProductService.GetMobileProducts(searchText));
        }
        [HttpGet("GetPromotionImage")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPromotionImage(string imageName)
        {
            if (string.IsNullOrEmpty(imageName))
                return BadRequest("Image name is required.");

            string connectionString = _config["Azure:StorageConnection"];
            string containerName = _config["Azure:ContainerNameFirst"];

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(imageName);

            if (!await blobClient.ExistsAsync())
                return NotFound($"Image '{imageName}' not found.");

            var downloadInfo = await blobClient.DownloadAsync();
            var contentType = downloadInfo.Value.ContentType ?? "application/octet-stream";

            return File(downloadInfo.Value.Content, contentType);
        }
        [HttpPost("GetFilteredProducts")]
        public async Task<IActionResult> GetFilteredProducts([FromBody] ProductSearchRequestModel filter)
        {
            return Ok(await _mobileProductService.GetFilteredProductsAsync(filter));
        }

        [HttpPost("GetCommunityMapping")]
        public async Task<IActionResult> GetProductCommunityMapping(QueryValues queryValues)
        {

            return Ok(await _mobileProductService.GetProductCommunityMappings(queryValues));

        }

        [HttpPost("GetBonusProductMapping")]
        public async Task<IActionResult> GetBonusProductMapping(QueryValues queryValues)
        {

            return Ok(await _mobileProductService.GetBonusProductMappings(queryValues));

        }

        //[HttpPost("CreateProductCommunity")]
        //public async Task<IActionResult> CreateProductCommunity(ProductCommunityMappingRequestModel request)
        //{
        //    request.CreatedBy = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));

        //    return Ok(await _mobileProductService.CreateProductCommunity(request));
        //}

        [HttpPost("AddProductCommunity")]
        public async Task<IActionResult> AddProductCommunity(ProductCommunityMappingRequestModel request)
        {
            request.CreatedBy = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));
            return Ok(await _mobileProductService.AddProductCommunity(request));
        }

        [HttpPut("EditProductCommunity")]
        public async Task<IActionResult> EditProductCommunity(ProductCommunityMappingRequestModel request)
        {
            request.CreatedBy = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));
            return Ok(await _mobileProductService.EditProductCommunity(request));
        }

        [HttpPatch("ToggleProductCommunityStatus/{id}")]
        public async Task<IActionResult> ToggleProductCommunityStatus(int id)
        {
            int userId = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));
            return Ok(await _mobileProductService.ToggleProductCommunityStatus(id, userId));
        }

        [HttpPost("CreateBonusProductMapping")]
        public async Task<IActionResult> CreateBonusProductMapping(BonusProductMappingRequestModel request)
        {
            request.CreatedBy = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));

            return Ok(await _mobileProductService.CreateBonusProductMapping(request));
        }

        [HttpPut("UpdateBonusProductMapping/{id}")]
        public async Task<IActionResult> UpdateBonusProductMapping(int id, BonusProductMappingRequestModel request)
        {
            request.Id = id;
            request.CreatedBy = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));

            var result = await _mobileProductService.UpdateBonusProductMapping(request);
            return Ok(result);
        }

        [HttpPatch("ToggleBonusProductMappingStatus/{id}")]
        public async Task<IActionResult> ToggleBonusProductMappingStatus(int id)
        {
            var modifiedBy = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));

            var result = await _mobileProductService.ToggleBonusProductMappingStatus(id, modifiedBy);
            return Ok(result);
        }

        [HttpPost("ManageProductContent")]
        public async Task<IActionResult> ManageProductContent([FromForm] ManageProductContentRequestModel request)
        {
            //TokenAnalyser tokenAnalyser = new();
            //TokenVariables tokenVariables = null;

            //if (HttpContext.User.Identity is ClaimsIdentity identity)
            //{
            //    tokenVariables = tokenAnalyser.FetchTokenValues(identity);
            //}
            //request.CreatedBy = System.Guid.Parse(tokenVariables.PublicKey);
            request.CreatedBy = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));

            return Ok(await _mobileProductService.ManageProductContent(request));
        }

        [HttpGet("GetProductListImage/{imageName}")]
        public async Task<IActionResult> GetProductListImage(string imageName)
        {
            var mobileAssetsFolderPath = _config["Mobile:RootDirectory"];

            var rootDirectory = mobileAssetsFolderPath;
            var imagePath = Path.Combine(rootDirectory, "Assets", "Products", imageName);

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

        [HttpGet("GetProductLandscapeImage/{imageName}")]
        public IActionResult GetProductLandscapeImage(string imageName)
        {
            var mobileAssetsFolderPath = _config["Mobile:RootDirectory"];

            var rootDirectory = mobileAssetsFolderPath;
            var imagePath = Path.Combine(rootDirectory, "Assets", "Products", "Landscape", imageName);

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

        [HttpGet("GetProductContentListImage/{imageName}")]
        public async Task<IActionResult> GetProductContentListImage(string imageName)
        {
            var mobileAssetsFolderPath = _config["Mobile:RootDirectory"];

            var rootDirectory = mobileAssetsFolderPath;
            var imagePath = Path.Combine(rootDirectory, "Assets", "Products", "ProductContent", imageName);

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

        [HttpGet("GetProductContentThumbnailImage/{imageName}")]
        public async Task<IActionResult> GetProductContentThumbnailImage(string imageName)
        {
            var mobileAssetsFolderPath = _config["Mobile:RootDirectory"];

            var rootDirectory = mobileAssetsFolderPath;
            var imagePath = Path.Combine(rootDirectory, "Assets", "Products", "ProductContent", "Thumbnail", imageName);

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

        [HttpPost("GetAdImagesCrm")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAdImagesCrm(GetAdImagesMRequestModel request)
        {
            return Ok(await _mobileProductService.GetAdImagesCrm(request.Type, request.SearchText));
        }
        [HttpGet("GetActiveAdImagesCrm")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveAdsImagesCRM()
        {
            return Ok(await _mobileProductService.GetActiveAdsImagesCRM());
        }

        [HttpPost("GetPromoImagesCrm")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPromoImagesCrm(GetPmImagesMRequestModel request)
        {
            return Ok(await _mobileProductService.GetPromoImagesCrm(request.SearchText));
        }



        [HttpPost("ManagePromotion")]
        public async Task<IActionResult> ManagePromotion([FromForm] PromotionRequestModel model)
        {
           _ = Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out Guid loggedInUser);

            var result = await _mobileProductService.ManagePromotionAsync(model, loggedInUser);
            return Ok(result);
            //return StatusCode((int)result.StatusCode, result);
        }




        [HttpDelete("DeletePrImage/{id}")]
    public async Task<IActionResult> DeletePrImage(int id)
    {
        return Ok(await _mobileProductService.DeletePrImage(id));
    }
    [HttpPut("EnableDisableImage/{id}")]
        public async Task<IActionResult> EnableDisableImage(int id)
        {
            return Ok(await _mobileProductService.EnableDisableImage(id));
        }

        [HttpPost("ManageAdvertisementImage")]
        public async Task<IActionResult> ManageAdvertisementImage([FromForm] PostAdvertisementRequestModel request)
        {
            return Ok(await _mobileProductService.ManageAdvertisementImages(request.ImageList, request.Type, request.Url, request.ExpireOn, request.ImageId, request.ProductId, request.ProductName));
        }

        [HttpGet("GetAdvertisementImage")]
        public async Task<IActionResult> GetAdvertisementImage(string imageName)
        {
            var mobileAssetsFolderPath = _config["Mobile:RootDirectory"];

            var imagePath = Path.Combine(mobileAssetsFolderPath, "Assets", "Advertisement", imageName);

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

        [HttpGet("DisableProduct/{productId}")]
        public async Task<IActionResult> DisableProduct(int productId)
        {
            return Ok(await _mobileProductService.DisableProduct(productId));
        }

        [HttpGet("GetAdvertisementImageType")]
        public async Task<IActionResult> GetAdvertisementImageType()
        {
            return Ok(await _mobileProductService.GetAdvertisementType());
        }

        [HttpDelete("DeleteProduct/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            return Ok(await _mobileProductService.DeleteProduct(id));
        }

        [HttpDelete("DeleteAdImage/{id}")]
        public async Task<IActionResult> DeleteAdImage(int id)
        {
            return Ok(await _mobileProductService.DeleteAdImage(id));
        }

        [HttpPost("ManageArticle")]
        public async Task<IActionResult> ManageArticle(ManageArticleModel request)
        {
            request.LoggedInUserKey = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));

            if (request.LoggedInUserKey == Guid.Empty)
            {
                request.LoggedInUserKey = Guid.Parse("1194d99e-c419-ef11-b261-88b133b31c8f");
            }
            return Ok(await _mobileProductService.ManageArticle(request));
        }

        [HttpGet("GetBaskets")]
        public async Task<IActionResult> GetBaskets([FromQuery] string? isActive)
        {
            return Ok(await _mobileProductService.GetBaskets(isActive));
        }

        [HttpGet("GetCompanies/{basketId}")]
        public async Task<IActionResult> GetCompanies(int basketId)
        {
            return Ok(await _mobileProductService.GetCompanies(basketId));
        }

        [HttpGet("GetCompanyDetails/{companyId}")]
        public async Task<IActionResult> GetCompanyDetails(int companyId)
        {
            return Ok(await _mobileProductService.GetCompanyDetails(companyId));
        }

        [HttpPost("ManageBaskets")]
        public async Task<IActionResult> ManageBaskets(ManageBasketsRequestModel request)
        {
            return Ok(await _mobileProductService.ManageBasketsAsync(request));
        }

        [HttpPut("UpdateBasketStatus")]
        public async Task<IActionResult> UpdateBasketStatus([FromBody] UpdateBasketStatusRequestModel request)
        {
            return Ok(await _mobileProductService.UpdateBasketStatusAsync(request));
        }

        [HttpPost("ImportCompanyReport")]
        public async Task<IActionResult> ImportCompanyReport([FromForm] CompanyReportExcelImportModal param)
        {
            return Ok(await _mobileProductService.GetCompanyReportDetailsFromExcel(param));
        }

        [HttpPost("Logs")]
        public async Task<IActionResult> GetAllLogs(QueryValues query)
        {
            var response = await _mobileProductService.GetLogsAsync(query);
            return Ok(response);
        }

        [HttpPost("UserVersionReport")]
        public async Task<IActionResult> GetUserVersionReport(QueryValues query)
        {
            var response = await _mongoDbService.GetUserVersionReportsAsync(query);

            return Ok(response);
        }

        [HttpGet]
        [Route("LogSources")]
        public async Task<IActionResult> GetUniqueSources()
        {
            try
            {
                var response = await _mongoDbService.GetUniqueSourcesAsync();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return Ok(response);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = ex.Message,
                    Data = null,
                    Total = 0
                });
            }
        }

        [HttpPost("Exceptions")]
        public async Task<IActionResult> GetAllExceptions(QueryValues query)
        {
            var response = await _mobileProductService.GetExceptionsAsync(query);
            return Ok(response);
        }

        [HttpDelete("log/{id}")]
        public async Task<IActionResult> DeleteLog(string id)
        {
            return Ok(await _mobileProductService.DeleteLogByIdAsync(id));// await _mongoDbService.DeleteLogByIdAsync(id));
        }

        [HttpDelete("exception/{id}")]
        public async Task<IActionResult> DeleteException(string id)
        {
            return Ok(await _mobileProductService.DeleteExceptionByIdAsync(id));
        }

        [HttpPost("logs/delete")]
        public async Task<IActionResult> DeleteLogs([FromBody] QueryValues request)
        {
            var re = await _mobileProductService.DeleteLogsInBulk(request);
            return Ok(re);
        }

        [HttpPost("delete-all-exceptions")]
        public async Task<IActionResult> DeleteAllExceptions([FromBody] QueryValues query)
        {
            var re = await _mobileProductService.DeleteExceptionsInBulk(query);
            return Ok(re);
        }

        [AllowAnonymous]
        [HttpPost("Coupon")]
        public async Task<IActionResult> Coupon([FromBody] QueryValues query)
        {
            return Ok(await _mobileProductService.Coupon(query));
        }

        [HttpPost("ManageCoupon")]
        public async Task<IActionResult> ManageCoupon([FromBody] ManageCouponModel request)
        {
            return Ok(await _mobileProductService.ManageCoupon(request));
        }

        [HttpDelete("DeleteCoupon/{id}")]
        public async Task<IActionResult> DeleteCoupon(int id)
        {
            var userKey = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));
            var response = await _mobileProductService.DeleteCoupon(id, userKey);
            return Ok(response);
        }

        [HttpPost("Ticket")]
        public async Task<IActionResult> Ticket(GetTicketReqeustModel request)
        {
            TokenAnalyser tokenAnalyser = new();
            TokenVariables tokenVariables = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                tokenVariables = tokenAnalyser.FetchTokenValues(identity);
            }

            request.ModifiedBy = Guid.Parse(tokenVariables.PublicKey);

            return Ok(await _mobileProductService.Ticket(request));
        }

        [HttpPost("ManageTicket")]
        public async Task<IActionResult> ManageTicket(ManageTicketsRequestModel request)
        {
            TokenAnalyser tokenAnalyser = new();
            TokenVariables tokenVariables = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                tokenVariables = tokenAnalyser.FetchTokenValues(identity);
            }

            request.ModifiedBy = Guid.Parse(tokenVariables.PublicKey);

            return Ok(await _mobileProductService.ManageTicket(request));
        }

        [HttpPost("GetPartnerDematAccount")]
        public async Task<IActionResult> GetPartnerDematAccounts([FromBody] QueryValues query)
        {
            var response = await _mobileProductService.GetPartnerDematAccounts(query);

            return Ok(response);
        }

        [HttpPost("ManagePartnerDematAccount")]
        public async Task<IActionResult> ManagePartnerDematAccount([FromBody] PartnerDematAccountRequest accountRequest)
        {
            var loggedUser = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));
            var response = await _mobileProductService.ManagePartnerDematAccount(accountRequest, loggedUser);

            return Ok(response);
        }

        [HttpPost("GetFilteredPurchaseOrders")]
        public async Task<IActionResult> GetFilteredPurchaseOrders(QueryValues queryValues)
        {
            ApiCommonResponseModel response = await _mobileProductService.GetFilteredPurchaseOrders(queryValues);
            return Ok(response);
        }

        [HttpPost("SendWhatsappTemplateMessage")]
        public async Task<IActionResult> SendWhatsappTemplateMessage(SendWhatsappMessageRequestModel param)
        {
            return Ok(await _mobileProductService.SendWhatsappTemplateMessage(param));
        }

        [HttpGet("gettemplates")]
        public async Task<IActionResult> GetTemplates()
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("apikey", "dd2noa4nnfdfmbchbch8mmtm0rr0kzdb");
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var response = await client.GetAsync("https://api.gupshup.io/wa/app/b396326e-e2ce-4c46-85a7-2a5bc08de800/template?templateStatus=APPROVED&templateCategory=MARKETING");

            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpGet("BlogDetails/{id}")]
        public async Task<IActionResult> GetBlogDetails(string id)
        {
            var response = await _mongoDbService.GetBlogDetails(id);
            return Ok(response);
        }

        [HttpPost("ManageUserPostPermission")]
        public async Task<IActionResult> ManageUserPostPermission([FromBody] RestrictUserRequestModel request)
        {
            request.LoggedInUser = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));

            return Ok(await _mongoDbService.ManageUserPostPermissionAsync(request));
        }

        [HttpDelete("Blog/{id}")]
        public async Task<IActionResult> DeleteBlog(string id)
        {
            TokenAnalyser tokenAnalyser = new();
            TokenVariables tokenVariables = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                tokenVariables = tokenAnalyser.FetchTokenValues(identity);
            }
            var loggedInUser = Guid.Parse(tokenVariables.PublicKey);
            var response = await _mongoDbService.DeleteBlogAsync(id, loggedInUser);
            return Ok(response);
        }

        [HttpPost("ManageBlogStatus")]
        public async Task<IActionResult> ManageBlogStatus([FromBody] UpdateBlogStatusRequestModel request)
        {
            request.LoggedInUser = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));
            return Ok(await _mongoDbService.ManageBlogStatusAsync(request));
        }

        [HttpPost("ManageBlogPinStatus")]
        public async Task<IActionResult> ManageBlogPinStatus([FromBody] UpdatePinnedStatusRequestModel request)
        {
            request.LoggedInUser = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));
            return Ok(await _mongoDbService.ManageBlogPinStatusAsync(request));
        }

        [HttpPost("CreateBlogPost")]
        public async Task<IActionResult> CreatePost([FromForm] CommunityPostRequestModel model)
        {
            var response = await _mongoDbService.CreateCommunityPostAsync(model);
            return Ok(response);
        }

        [HttpPost("GetUsers")]
        public async Task<IActionResult> GetUsers([FromBody] QueryValues query)
        {
            var response = await _mongoDbService.GetUserAsync(query);
            return Ok(response);
        }


        [HttpPut("UserBucketUpdate/{id}")]
        public async Task<IActionResult> UserMyBucketUpdate(UpdateMyBucketResponseModel model)
        {
            model.PublicKey = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));
            return Ok(await _mobileProductService.UpdateUserMyBucketAsync(model));
        }

        [HttpPost("AddPurchaseOrder")]
        public async Task<IActionResult> AddPurchaseOrder(AddPurchaseOrderDetailsRequestModel model)
        {
           var loggedInUser = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));
            return Ok(await _mobileProductService.AddPurchaseOrder(model, loggedInUser));
        }

        [HttpPut("UserUpdate/{id}")]
        public async Task<IActionResult> UserUpdate(UpdateBasketStatusRequestModel model)
        {
            return Ok(await _mobileProductService.UpdateBasketStatusAsync(model));
        }

        [HttpGet("GetPartnerNames")]
        public async Task<IActionResult> GetPartnerNames()
        {
            return Ok(await _mobileProductService.GetPartnerNamesAsync());
        }

        [HttpPost("SaveChartImageForMobile")]
        public async Task<IActionResult> SaveChartImageForMobile([FromForm] ImageUploadRequestModel request)
        {
            return Ok(await _mobileProductService.SaveChartImageForMobile(request));
        }

        [AllowAnonymous]
        [HttpPost("GetMobileNotifications")]
        public async Task<IActionResult> GetMobileNotification([FromBody] QueryValues query)
        {
            return Ok(await _mongoDbService.GetMobileNotificationsAsync(query));
        }

        [HttpPost("UpdateNotificationPinStatus")]
        public async Task<IActionResult> UpdatePinnedStatusAsync([FromBody] UpdatePinnedStatusRequestModel request)
        {
            return Ok(await _mongoDbService.UpdatePinnedStatusAsync(request.Id, request.IsPinned));
        }

        //[AllowAnonymous]
        //[HttpGet("GetKalkibaataaj")]
        //public async Task<IActionResult> GetSpecificTopic()
        //{
        //    return Ok(_mongoDbService.GetSpecificTopicNotification());
        //}

        [HttpDelete("BlogCommentOrReply")]
        public async Task<IActionResult> DeleteBlogCommentOrReply([FromQuery] string objectId, [FromQuery] string userObjectId, [FromQuery] string type)
        {
            var response = await _mongoDbService.DeleteCommentOrReply(objectId, userObjectId, type);
            var responseModel = new ApiCommonResponseModel();

            if (response)
            {
                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Message = type == "COMMENT"
                    ? "Comment deleted successfully!"
                    : "Reply deleted successfully!";
            }
            else
            {
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                responseModel.Message = type == "COMMENT"
                    ? "Failed to delete the comment. Please try again."
                    : "Failed to delete the reply. Please try again.";
            }

            return Ok(responseModel);
        }

        [HttpPost("ActiveFreeTrail")]
        public async Task<IActionResult> GetAllFreeTrail(QueryValues query)
        {
            var response = await _mobileProductService.GetFreeTrial(query);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("ToggleCouponVisibility/{couponId}")]
        public async Task<IActionResult> ToggleCouponVisibility([Required] int couponId)
        {
            var coupon = await _dbContext.CouponsM.Where(x => x.Id == couponId).FirstOrDefaultAsync();
            coupon.IsVisible = !coupon.IsVisible;
            await _dbContext.SaveChangesAsync();
            var responseModel = new ApiCommonResponseModel();
            responseModel.StatusCode = HttpStatusCode.OK;
            return Ok(responseModel);
        }

        [HttpPost("PhonePe")]
        public async Task<IActionResult> GetPhonePe(QueryValues query)
        {
            var response = await _mobileProductService.GetPhonePe(query);

            return Ok(response);
        }


        [HttpPost("PhonePeChartData")]
        public async Task<IActionResult> GetPhonePeChartData(QueryValues query)
        {

            var data = await _mobileProductService.GetPhonePeChartDataAsync(query);
            return Ok(data);

        }
        [HttpPost]
        [Route("GetPhonepeReportChart")]
        public async Task<IActionResult> GetPhonePePaymentReportChart(PhonePePaymentReportChartResponceModel query)
        {
            var data = await _mobileProductService.GetPhonePePaymentReportChartAsync(query);
            return Ok(data);
        }


        [HttpPost("AddComment")]
        public async Task<IActionResult> AddComment(PostCommentRequestModel request)
        {
            Guid loggedInUser = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));
            return Ok(await _blogService.AddBlogComment(request, loggedInUser));
        }

        [HttpPost("User/History")]
        public async Task<IActionResult> GetUserHistory(QueryValues query)
        {
            var response = await _mobileProductService.GetUserHistory(query);

            return Ok(response);
        }

        [HttpPost("GetSubscriptionDetails")]
        public async Task<IActionResult> GetSubscriptionDetails(QueryValues query)
        {
            var subscriptionDetails = await _mobileProductService.GetSubscriptionDetailsAsync(query);
            return Ok(subscriptionDetails);
        }

        [HttpGet("GetSubscriptionDuration")]
        public async Task<IActionResult> GetSubscriptionDuration()
        {
            var durations = await _mobileProductService.GetSubscriptionDurationAsync();
            return Ok(durations);
        }
        [HttpGet("GetSubscriptionPlan")]
        public async Task<IActionResult> GetSubscriptionPlan()
        {
            var plans = await _mobileProductService.GetSubscriptionPlanAsync();
            return Ok(plans);
        }

        [HttpPost("AddSubscriptionMapping")]
        public async Task<IActionResult> AddSubscriptionMapping([FromBody] SubscriptionModel.SubscriptionMappingRequestModel requestModel)
        {
            return Ok(await _mobileProductService.AddSubscriptionMappingAsync(requestModel));
        }

        [HttpPost("AddSubscriptionPlan")]
        public async Task<IActionResult> AddSubscriptionPlan([FromBody] SubscriptionModel.SubscriptionPlanRequest requestModel)
        {
            return Ok(await _mobileProductService.AddSubscriptionPlanAsync(requestModel));
        }
        [HttpPut("UpdateSubscriptionPlan/{id}")]
        public async Task<IActionResult> UpdateSubscriptionPlan(int id, [FromBody] SubscriptionModel.SubscriptionPlanRequest requestModel)
        {
            requestModel.ModifiedBy = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));
            return Ok(await _mobileProductService.UpdateSubscriptionPlanAsync(id, requestModel));
        }

        [HttpPost("UpdateSubscriptionMapping")]
        public async Task<IActionResult> UpdateSubscriptionMapping([FromBody] SubscriptionModel.SubscriptionMappingUpdateRequest requestModel)
        {
            requestModel.ModifiedBy = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));
            return Ok(await _mobileProductService.UpdateSubscriptionMappingAsync(requestModel));
        }

        [HttpGet("ToggleSubscriptionDurationStatus/{id}")]
        public async Task<IActionResult> ToggleSubscriptionDurationStatus(int id)
        {
            var loggedUser = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));
            var durations = await _mobileProductService.ToggleSubscriptionDurationStatusAsync(id, loggedUser);
            return Ok(durations);
        }
        [HttpGet("ToggleSubscriptionPlanStatus/{id}")]
        public async Task<IActionResult> ToggleSubscriptionPlanStatus(int id)
        {
            var loggedUser = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));

            var result = await _mobileProductService.ToggleSubscriptionPlanStatusAsync(id, loggedUser);
            return Ok(result);
        }
        [HttpGet("ToggleSubscriptionMappingStatus/{id}")]
        public async Task<IActionResult> ToggleSubscriptionMappingStatus(int id)
        {
            var loggedUser = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));
            var result = await _mobileProductService.ToggleSubscriptionMappingStatusAsync(id, loggedUser);
            return Ok(result);
        }


        [HttpGet("GetReasons/{bucketId:int}")]
        public async Task<IActionResult> GetReasons(int bucketId)
        {
            return Ok(await _mobileProductService.GetReasons(bucketId));
        }
        [HttpGet("GetReasonsPurchase/{purchaseId:int}")]
        public async Task<IActionResult> GetReasonsPurchase(int purchaseId)
        {
            return Ok(await _mobileProductService.GetReasonsPurchaseAsync(purchaseId));
        }

        [HttpGet("SendWhatsappMessages")]
        public async Task<IActionResult> SendWhatsappMessages()
        {
            return Ok(await _mobileProductService.SendWhatsappFromExcel());
        }

        [HttpPut("UpdatePurchaseOrder/{id}")]
        public async Task<IActionResult> UpdatePurchaseOrder(int id)
        {
            Guid loggedInUser = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));
            return Ok(await _mobileProductService.UpdatePurchaseOrderAsync(id, loggedInUser));
        }

        [HttpPost("GetUserNotifications")]
        public async Task<IActionResult> GetUserNotifications(GetNotificationRequestModel requestModel)
        {
            return Ok(await _mongoDbService.GetUserNotifications(requestModel));
        }

        [AllowAnonymous]
        [HttpPost("GetScheduleNotification")]
        public async Task<IActionResult> GetScheduleNotification(QueryValues queryValues)
        {
            return Ok(await _mobileProductService.GetScheduleNotification(queryValues));
        }

        [AllowAnonymous]
        [HttpPost("GetPerformance")]
        public async Task<IActionResult> Get([FromBody] GetPerformanceRequestModel requestModel)
        {
            var result = await _mobileProductService.GetPerformance(requestModel);
            return Ok(result);
        }

        [HttpDelete("Peformence")]
        public async Task<IActionResult> Delete([FromQuery] int ID) // Use [FromQuery] to read ID from the query parameter
        {
            var result = await _mobileProductService.DeletePerformance(ID);
            return Ok(result);
        }

        [HttpDelete("Basket")]
        public async Task<IActionResult> BasketDelete([FromQuery] int id) // Use [FromQuery] to read ID from the query parameter
        {
            // Validate the input object
            if (id == 0)
            {
                return BadRequest("Invalid data provided.");
            }

            // Find the BasketsM by ID
            var item = await _dbContext.BasketsM.FirstOrDefaultAsync(b => b.Id == id);
            if (item == null)
            {
                return NotFound("Basket not found.");
            }

            // Mark the item as deleted and inactive
            item.IsDelete = true;
            item.IsActive = false;
            await _dbContext.SaveChangesAsync();

            return NoContent(); // Return 204 No Content for successful deletion
        }

        [HttpPost("AddLeadFreeTrail")]
        public async Task<IActionResult> AddLeadFreeTrail([FromBody] LeadFreeTrialRequestModel requestModel)
        {
            Guid loggedInUser = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));
            return Ok(await _mobileProductService.AddLeadFreeTrailAsync(requestModel, loggedInUser));
        }


        [HttpGet("GetLeadFreeTrials")]
        public async Task<IActionResult> GetLeadFreeTrials(Guid LeadKey)
        {
            var response = await _mobileProductService.GetLeadFreeTrailsAsync(LeadKey);
            return Ok(response);
        }

        [HttpDelete("LeadFreeTrail")]
        public async Task<IActionResult> LeadFreeTrail([FromQuery] int id) // Use [FromQuery] to read ID from the query parameter
        {
            Guid loggedInUser = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));

            var result = await _mobileProductService.DeleteFreeTrail(id, loggedInUser);

            return Ok(result);
        }

        [HttpDelete("Company")]
        public async Task<IActionResult> Company([FromQuery] int id) // Use [FromQuery] to read ID from the query parameter
        {
            Guid loggedInUser = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));

            var result = await _mobileProductService.DeleteCompany(id, loggedInUser);

            return Ok(result);
        }

        [HttpDelete("DeleteCompanyDetailMessage")]
        public async Task<IActionResult> DeleteCompanyDetails([FromQuery] int id) // Use [FromQuery] to read ID from the query parameter
        {
            Guid loggedInUser = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));

            var result = await _mobileProductService.DeleteCompanyDetails(id, loggedInUser);

            return Ok(result);
        }

        [HttpDelete("DeleteNotification")]
        public async Task<IActionResult> DeleteNotification([FromQuery] int id) // Use [FromQuery] to read ID from the query parameter
        {
            int userPublicKey = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));

            var result = await _mobileProductService.DeleteNotification(id, userPublicKey);

            return Ok(result);
        }

        [HttpDelete("DeleteProductCommunityMapping")]
        public async Task<IActionResult> ProductCommunityMapping([FromQuery] int id) // Use [FromQuery] to read ID from the query parameter
        {
            int userPublicKey = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));

            var result = await _mobileProductService.DeleteProductCommunity(id, userPublicKey);

            return Ok(result);
        }

        [HttpDelete("DeleteBonusProductMapping")]
        public async Task<IActionResult> DeleteBonusProductMapping([FromQuery] int id) // Use [FromQuery] to read ID from the query parameter
        {
            int userPublicKey = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));

            var result = await _mobileProductService.DeleteBonusProductMapping(id, userPublicKey);

            return Ok(result);
        }

        [HttpPost("GetChaptersWithSubChapters")]
        public async Task<IActionResult> GetChaptersWithSubChapters([FromBody] ChapterRequestModel request)
        {
            var chapters = await _dbContext.Chapter
                .Where(c => (!request.ProductId.HasValue || c.ProductId == request.ProductId.Value) && !c.IsDelete)
                .Select(c => new Chapter
                {
                    Id = c.Id,
                    ProductId = c.ProductId,
                    ChapterTitle = c.ChapterTitle,
                    IsActive = c.IsActive,
                    SubChapters = c.SubChapters
                        .Where(sc => !sc.IsDelete) // ❗ filter out deleted sub-chapters
                        .ToList()
                })
                .ToListAsync();

            if (!string.IsNullOrEmpty(request.SearchText))
            {
                chapters = chapters
                    .Where(c =>
                        c.ChapterTitle.Contains(request.SearchText, StringComparison.OrdinalIgnoreCase) ||
                        c.SubChapters.Any(sc => sc.Title.Contains(request.SearchText, StringComparison.OrdinalIgnoreCase))
                    )
                    .Select(c => new Chapter
                    {
                        Id = c.Id,
                        ProductId = c.ProductId,
                        ChapterTitle = c.ChapterTitle,
                        IsActive = c.IsActive,
                        SubChapters = c.SubChapters
                            .Where(sc => sc.Title.Contains(request.SearchText, StringComparison.OrdinalIgnoreCase))
                            .ToList()
                    })
                    .ToList();
            }

            return Ok(chapters);
        }



        //[HttpPost("CreateChapterSubChapter")]
        //public async Task<IActionResult> CreateChapterSubChapter(SubChapterRequestModel request)
        //{
        //    request.CreatedBy = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));

        //    return Ok(await _mobileProductService.CreateChapterSubChapter(request));
        //}

        [HttpPost("AddSubChapter")]
        public async Task<IActionResult> AddSubChapter(SubChapterRequestModel request)
        {
            request.CreatedBy = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));
            var result = await _mobileProductService.AddSubChapter(request);
            return Ok(result);
        }

        [HttpPost("EditSubChapter")]
        public async Task<IActionResult> EditSubChapter(SubChapterRequestModel request)
        {
            request.CreatedBy = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));
            var result = await _mobileProductService.EditSubChapter(request);
            return Ok(result);
        }

        [HttpPost("EditChapter")]
        public async Task<IActionResult> EditChapter(SubChapterRequestModel request)
        {
            request.CreatedBy = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));
            var result = await _mobileProductService.EditChapter(request);
            return Ok(result);
        }

        [HttpPost("AddChapter")]
        public async Task<IActionResult> AddChapter(SubChapterRequestModel request)
        {
            request.CreatedBy = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));
            var result = await _mobileProductService.AddChapter(request);
            return Ok(result);
        }

        [HttpDelete("DeleteChapter")]
        public async Task<IActionResult> DeleteChapter([FromQuery] int id)
        {
            int userPublicKey = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));

            var result = await _mobileProductService.DeleteChapter(id, userPublicKey);

            return Ok(result);
        }

        [HttpDelete("DeleteSubChapter")]
        public async Task<IActionResult> DeleteSubChapter([FromQuery] int id)
        {
            int userPublicKey = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));

            var result = await _mobileProductService.DeleteSubChapter(id, userPublicKey);

            return Ok(result);
        }

        [HttpPut("UpdateChapterStatus")]
        public async Task<IActionResult> UpdateChapterStatus([FromBody] UpdateChapterStatusRequestModel request)
        {
            request.ModifiedBy = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));
            return Ok(await _mobileProductService.UpdateChapterStatus(request));
        }

        [HttpPut("UpdateSubChapterStatus")]
        public async Task<IActionResult> UpdateSubChapterStatus([FromBody] UpdateChapterStatusRequestModel request)
        {
            request.ModifiedBy = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));
            return Ok(await _mobileProductService.UpdateSubChapterStatus(request));
        }

        [HttpPost("GetQueryForm")]
        public async Task<IActionResult> GetQueryForm(QueryValues query)
        {
            var response = await _mobileProductService.GetQueryForms(query);

            return Ok(response);
        }

        [HttpPost("UpdateQueryForm")]
        public async Task<IActionResult> UpdateQueryForm([FromForm] QueryFormRequestModel request)
        {
            request.CreatedBy = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));

            var result = await _mobileProductService.UpdateQueryFormAsync(request);
            return Ok(result);
        }

        [HttpGet("GetQueryFormRemark/{queryFormId:int}")]
        public async Task<IActionResult> GetLeadFreeTrailReasons(int queryFormId)
        {
            return Ok(await _mobileProductService.GetQueryFormRemarks(queryFormId));
        }

        [HttpDelete("DeleteQueryForm")]
        public async Task<IActionResult> DeleteQueryForm([FromQuery] int id)
        {
            int userPublicKey = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));

            var result = await _mobileProductService.DeleteQueryForm(id, userPublicKey);

            return Ok(result);
        }

        [HttpGet("GetQueryCategories/{productId}")]
        public ApiCommonResponseModel GetQueryCategoriesByProduct(int productId)
        {
            // Define a default category list (used for fallback)
            var defaultCategories = new List<QueryCategoryModel>
            {
                new QueryCategoryModel { Id = 1, Name = "Entry Criteria" },
                new QueryCategoryModel { Id = 2, Name = "Exit Strategy" },
                new QueryCategoryModel { Id = 3, Name = "Risk Management" },
                new QueryCategoryModel { Id = 4, Name = "Live Market" },
                new QueryCategoryModel { Id = 5, Name = "Confusion" },
                new QueryCategoryModel { Id = 6, Name = "Other Issues" }
            };

            // You can define product-specific mappings here
            var categoryMap = new Dictionary<int, List<QueryCategoryModel>>
            {
                { 91, defaultCategories },
                // Add other product-specific lists as needed
            };

            // Use specific list if found, else fallback to default list
            var categories = categoryMap.ContainsKey(productId)
                ? categoryMap[productId]
                : defaultCategories;

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Categories fetched successfully.",
                Data = categories
            };
        }

        //[HttpPost("CalculateSIP")]
        //public IActionResult CalculateSIP([FromBody] SIPRequest request)
        //{
        //    if (request.MonthlyInvestment <= 0 || request.AnnualRate <= 0 || request.Years <= 0)
        //    {
        //        return BadRequest("All input values must be greater than zero.");
        //    }

        //    decimal monthlyRate = (decimal)(request.AnnualRate / 100) / 12;
        //    decimal inflationRate = request.ApplyInflation ? 0.06m : 0.00m;

        //    List<SIPProjection> projections = new List<SIPProjection>();
        //    decimal totalInvested = 0;
        //    decimal futureValue = 0;
        //    decimal inflationAdjustedFutureValue = 0;

        //    for (int year = 1; year <= request.Years; year++)
        //    {
        //        int months = year * 12;
        //        totalInvested += request.MonthlyInvestment * 12;

        //        decimal yearlyFutureValue = 0;
        //        decimal yearlyInflationAdjustedValue = 0;

        //        for (int month = 1; month <= months; month++)
        //        {
        //            decimal monthlyFV = request.MonthlyInvestment * (decimal)Math.Pow((double)(1 + monthlyRate), months - month);

        //            yearlyFutureValue += monthlyFV;

        //            if (request.ApplyInflation)
        //            {
        //                decimal inflationDiscount = (decimal)Math.Pow((double)(1 + inflationRate), (months - month) / 12.0);
        //                yearlyInflationAdjustedValue += monthlyFV / inflationDiscount;
        //            }
        //            else
        //            {
        //                yearlyInflationAdjustedValue += monthlyFV;
        //            }
        //        }

        //        if (year == request.Years)
        //        {
        //            futureValue = yearlyFutureValue;
        //            inflationAdjustedFutureValue = yearlyInflationAdjustedValue;
        //        }

        //        projections.Add(new SIPProjection
        //        {
        //            Duration = year,
        //            SIPAmount = request.MonthlyInvestment,
        //            FutureValue = Math.Round(yearlyFutureValue, 2),
        //            InflationAdjustedFutureValue = Math.Round(yearlyInflationAdjustedValue, 2)
        //        });
        //    }

        //    decimal expectedAmount = request.ApplyInflation ? inflationAdjustedFutureValue : futureValue;
        //    decimal wealthGain = expectedAmount - totalInvested;

        //    var response = new SIPResponse
        //    {
        //        ExpectedAmount = Math.Round(expectedAmount, 2),
        //        AmountInvested = totalInvested,
        //        WealthGain = Math.Round(wealthGain, 2),
        //        ApplyInflation = request.ApplyInflation,
        //        InflationRate = request.ApplyInflation ? 6.0m : 0.0m,
        //        ProjectedSipReturnsTable = projections
        //    };

        //    return Ok(new ApiCommonResponseModel
        //    {
        //        Data = response,
        //        StatusCode = HttpStatusCode.OK,
        //        Message = "SIP calculation completed. (Expected amount is the nominal future value, inflation adjustment applied only in the projection table)."
        //    });
        //}



        [HttpPost("GetMobileUserBucketDetails")]
        public async Task<IActionResult> GetMobileUserBucketDetails([FromBody] QueryValues query)
        {
            var response = await _mobileProductService.GetMobileUserBucketDetails(query);
            return Ok(response);
        }

    }

}