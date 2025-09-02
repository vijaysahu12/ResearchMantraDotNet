using RM.CommonServices.Helpers;
using RM.Database.ResearchMantraContext;
using RM.Model;
using RM.Model.RequestModel;
using RM.Model.RequestModel.MobileApi;
using RM.Model.ResponseModel;
using RM.MService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;

namespace RM.MobileAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productsService;

        public ProductController(IProductService products)
        {
            _productsService = products;
        }

        [HttpGet]
        [Route("GetProducts/{userKey}")]
        public async Task<IActionResult> GetProducts(string userKey, string searchByCategory = "")
        {

            //var device = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublickey"));
            //var deviceType = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublickey"));


            return Ok(await _productsService.GetProducts(searchByCategory, userKey));
        }

        [HttpGet("GetProductById")]
        public async Task<IActionResult> GetProductById(string id, string mobileUserKey)
        {
            return Ok(await _productsService.GetProductById(id, mobileUserKey));
        }

        [HttpPost("LikeUnlikeProduct")]
        public async Task<IActionResult> LikeUnlikeProduct(LikeProductModel likedByRequest)
        {
            return Ok(await _productsService.LikeUnlikeProduct(likedByRequest));
        }

        [HttpGet("MyBucketContent")]
        public async Task<IActionResult> MyBucketContent([Required] Guid userKey)
        {
            return Ok(await _productsService.MyBucketContent(userKey));
        }

        [HttpPost("GetProductContent")]
        public async Task<IActionResult> GetProductContent(GetProductContentRequestModel request)
        {
            return Ok(await _productsService.GetProductContent(request.Userkey, request.Id));
        }

        [HttpPost("GetProductContentV2")]
        public async Task<IActionResult> GetProductContentV2(GetProductContentRequestModel request)
        {
            return Ok(await _productsService.GetProductContentV2(request.Userkey, request.Id));
        }

        [HttpPost("RateProduct")]
        public async Task<IActionResult> RateProduct([FromBody] RateProductRequestModel request)
        {
            return Ok(await _productsService.RateProduct(request.UserKey, request.ProductId, request.Rating));
        }

        [HttpGet("GetImage")]
        [AllowAnonymous]
        public IActionResult GetImage(string imageName)
        {
            var rootDirectory = Directory.GetCurrentDirectory();
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

        //[HttpGet("GetAdvertisementImage")]
        //public async Task<IActionResult> GetAdvertisementImage(string imageName)
        //{
        //    var rootDirectory = Directory.GetCurrentDirectory();
        //    var imagePath = Path.Combine(rootDirectory, "Assets",   "Advertisement", imageName);

        //    var imageFileName = Path.GetFileName(imagePath);
        //    var imageExtension = Path.GetExtension(imageFileName);

        //    if (System.IO.File.Exists(imagePath))
        //    {
        //        var imageBytes = System.IO.File.ReadAllBytes(imagePath);
        //        return File(imageBytes, $"image/{imageExtension.TrimStart('.').ToLower()}");
        //    }
        //    else
        //    {
        //        return NotFound($"Image '{imageName}' not found.");
        //    }
        //}
        //[HttpGet("GetAdvertisementImageList")]
        //public async Task<IActionResult> GetAdvertisementImageList()
        //{
        //}

        [HttpGet("GetTopGainers/{type}")]
        public async Task<IActionResult> GetTopGainers([Required] string type)
        {
            return Ok(await _productsService.GetTopGainers(type));
        }

        [HttpGet("GetProductCategories")]
        public async Task<IActionResult> GetProductCategories()
        {
            return Ok(await _productsService.GetProductCategories());
        }

        [HttpGet("GetLandscapeImage")]
        [AllowAnonymous]
        public IActionResult GetLandscapeImage(string imageName)
        {
            var rootDirectory = Directory.GetCurrentDirectory();
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

        [HttpGet("GetLearningMaterial")]
        public async Task<IActionResult> GetLearningMaterial()
        {
            //var mobileUserKey = User.GetClaimValue("userPublicKey");
            //Guid.Parse(mobileUserKey))
            return Ok(await _productsService.GetLearningMaterial(Guid.Empty));
        }

        [HttpPost("GetLearningContentList")]
        public async Task<IActionResult> GetLearningContentList([FromBody] GetLearningContentList request)
        {
            return Ok(await _productsService.GetLearningContentList(request));
        }

        [HttpGet("GetLearningContentById/{productContentId}")]
        public async Task<IActionResult> GetLearningContentById(int productContentId)
        {
            var mobileUserKey = UserClaimsHelper.GetClaimValue(User, "userPublicKey");//ToDo Check if claim value are working or not 
            if (!string.IsNullOrEmpty(mobileUserKey) && Guid.TryParse(mobileUserKey, out Guid mobileUser))
            {
                return Ok(await _productsService.GetLearningContentById(productContentId, mobileUser));
            }
            else
            {
                return Ok();
            }
        }

        [HttpGet("GetLearningContentExamples/{productContentId}")]
        public async Task<IActionResult> GetLearningContentExamples(int productContentId)
        {
            return Ok(await _productsService.GetLearningContentExamples(productContentId));
        }

        [HttpPost("LikeUnlikeLearningContent")]
        public async Task<IActionResult> LikeUnlikeLearningContent([FromBody] LikeUnlikeLearningContentRequestModel request)
        {
            return Ok(await _productsService.LikeUnlikeLearningContent(request));
        }

        [HttpPost("GetCoupons")]
        public async Task<IActionResult> GetCoupons([FromBody] GetCouponsRequestModel request)
        {
            Guid userKey = Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out var parsedKey) ? parsedKey : Guid.Empty;

            return Ok(await _productsService.GetCoupons(userKey, request.ProductId, request.SubscriptionDurationId));
        }

        [HttpPost("ActiveProductCount")]
        public async Task<IActionResult> ActiveProductCount()
        {
            var totalActiveProductCount = await _productsService.ActiveProductCount();
            return Ok(totalActiveProductCount);
        }


        [HttpPost("ActiveProducts")]
        public async Task<IActionResult> ActiveProductList()
        {
            Guid userKey = Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out var parsedKey) ? parsedKey : Guid.Empty;
            var result = await _productsService.ActiveProductStatus(userKey);
            return Ok(result);
        }



        [HttpPost("GetPlayList")]
        public async Task<ActionResult> GetPlaylist([FromBody] PlaylistRequestModel requestModel)
        {
            // Validate request
            if (requestModel == null || requestModel.ProductId <= 0)
            {
                return BadRequest(new ApiCommonResponseModel
                {
                    Data = null,
                    Exceptions = null,
                    Message = "Invalid request parameters.",
                    Total = 0,
                    StatusCode = HttpStatusCode.BadRequest
                });
            }

            int userPublicKey = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));


            //return Ok(new ApiCommonResponseModel
            //{
            //    Data = new List<Chapter>(),
            //    Exceptions = null,
            //    Message = "No active product subscription found.",
            //    Total = 0,
            //    StatusCode = HttpStatusCode.OK
            //});

            var chapters = await _productsService.GetPlayList(userPublicKey, requestModel.ProductId);

            if (chapters == null)
            {
                return Ok(new ApiCommonResponseModel
                {
                    Data = new List<Chapter>(),
                    Exceptions = null,
                    Message = "No chapters available for this product.",
                    Total = 0,
                    StatusCode = HttpStatusCode.OK
                });
            }

            // Return the correct chapters
            return Ok(chapters);
        }

        [HttpPost("AddQueryForm")]
        public async Task<IActionResult> AddQuery([FromForm] QueryFormRequestModel request)
        {
            request.CreatedBy = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));

            request.MobileUserId = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));

            var result = await _productsService.AddQueryFormAsync(request);
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

        //[HttpGet("GetQueryCategories/{productId}")]
        //public ApiCommonResponseModel GetQueryCategoriesByProduct(int productId)
        //{
        //    var categoryMap = new Dictionary<int, List<QueryCategoryModel>>
        //    {
        //        {
        //            91, new List<QueryCategoryModel>
        //             {
        //                new QueryCategoryModel { Id = 1, Name = "Entry Criteria" },
        //                new QueryCategoryModel { Id = 2, Name = "Exit Strategy" },
        //                new QueryCategoryModel { Id = 3, Name = "Risk Management" },
        //                new QueryCategoryModel { Id = 4, Name = "Live Market" },
        //                new QueryCategoryModel { Id = 5, Name = "Confusion" },
        //                new QueryCategoryModel { Id = 6, Name = "Other Issues" }
        //             }
        //        }
        //    };

        //    if (!categoryMap.ContainsKey(productId))
        //    {
        //        return new ApiCommonResponseModel
        //        {
        //            StatusCode = HttpStatusCode.NotFound,
        //            Message = "No categories mapped for this ProductId."
        //        };
        //    }

        //    return new ApiCommonResponseModel
        //    {
        //        StatusCode = HttpStatusCode.OK,
        //        Message = "Categories fetched successfully.",
        //        Data = categoryMap[productId]
        //    };
        //}

    }
}