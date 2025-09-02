////using RM.BlobStorage;
////using RM.Database.Constants;
////using RM.Database.Extension;
////using RM.Database.ResearchMantraContext;
////using RM.Database.MongoDbContext;
////using RM.Model;
////using RM.Model.Common;
////using RM.Model.MongoDbCollection;
////using Microsoft.Data.SqlClient;
////using Microsoft.EntityFrameworkCore;
////using Microsoft.Extensions.Caching.Memory;
////using Microsoft.Extensions.Configuration;
////using Microsoft.Extensions.Options;
////using MongoDB.Driver;
////using RestSharp;
//using System.Data;
////using System.Net;

////namespace RM.MService.Services
////{
////    public class CommunityPostService
////    {
////        private readonly IMongoDatabase _database;
////        private readonly IMemoryCache _cache;
////        private readonly IAzureBlobStorageService _azureBlobStorageService;
////        private readonly IConfiguration _configuration;
////        private readonly ResearchMantraContext _context;
////        private readonly IMongoCollection<CommunityPost> _communityPost;
////        ApiCommonResponseModel _apiResponse = new();
////        public CommunityPostService(IOptions<MongoDBSettings> mongoDBSettings,
////           IMemoryCache cache,
////           IConfiguration configuration, ResearchMantraContext context,
////           IAzureBlobStorageService azureBlobStorageService)
////        {
////            MongoClient client = new(mongoDBSettings.Value.ConnectionURI);
////            _database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
////            _configuration = configuration;
////            _communityPost = _database.GetCollection<CommunityPost>("CommunityPost");
////            _context = context;
////            _azureBlobStorageService = azureBlobStorageService;
////            _cache = cache;
////        }
////        public async Task<ApiCommonResponseModel> Delete(string postId)
////        {
////            var deleteResult = await _communityPost.DeleteOneAsync(p => p.Id == postId);

////            if (deleteResult.DeletedCount > 0)
////            {
////                _apiResponse.StatusCode = HttpStatusCode.OK;
////                _apiResponse.Message = "Post deleted successfully";
////            }
////            else
////            {
////                _apiResponse.StatusCode = HttpStatusCode.InternalServerError;
////                _apiResponse.Message = "Failed to delete post";
////            }
////            return _apiResponse;
////        }
////        /// <summary>
////        /// query.Id = productId,
////        /// query.PrimaryKey = PostTypeId
////        /// query.loggedInuserId = MobileUserId
////        /// Check only if the user has access to the community product and if the subscription is within the date range
////        /// Check if the user has purchased product once before accessing the community
////        /// If all above conditions are met, retrieve the posts from MongoDB 
////        /// else ask for purchase the product first before accessing the community or subscribe to community directly 
////        /// </summary>
////        public async Task<ApiCommonResponseModel> Get(GetCommunityRequestModel query)
////        {
////            if (query.PageNumber <= 0 || query.PageSize <= 0)
////            {
////                query.PageNumber = 1;
////                query.PageSize = 20;
////            }

//            var responseModel = new CommunityPostDataResponse
//            {
//                TotalCount = 0,
//                TotalPages = 0,
//                PageNumber = 0,
//                PageSize = 0,
//                hasPurchasedProduct = false,
//                hasAccessToPremiumContent = false
//            };
//            _apiResponse.Data = responseModel;

//            var mobileUser = await _context.MobileUsers.FirstOrDefaultAsync(item => item.Id == (query.LoggedInUserId));
//            int communityProductId = query.CommunityProductId;
//            Guid mobileUserKey = mobileUser?.PublicKey ?? Guid.Empty;
////            var mobileUser = await _context.MobileUsers.FirstOrDefaultAsync(item => item.Id ==  (query.LoggedInUserId));
////            int communityProductId = query.CommunityProductId;
////            if (mobileUser == null)
////            {
////                _apiResponse.StatusCode = HttpStatusCode.NotFound;
////                _apiResponse.Data = "UserNotFound";
////                _apiResponse.Message = "User not found";
////                return _apiResponse;
////            }

////            var productCommunityMapping = await _context.ProductCommunityMappingM.FirstOrDefaultAsync(b => b.CommunityId == communityProductId && b.IsActive);

////            if (productCommunityMapping == null || !Enum.IsDefined(typeof(CommunityPostTypeEnum), query.PostTypeId))
////            {
////                _apiResponse.Data = responseModel;
////                _apiResponse.StatusCode = HttpStatusCode.OK;
////                _apiResponse.Message = "Community not found or Provide valid PostTypeId";
////                return _apiResponse;
////            }


//            bool doesHePurchasedProductAlready = await _context.MyBucketM
//                .AnyAsync(b => b.ProductId == productCommunityMapping.ProductId && b.MobileUserKey == mobileUserKey);

//            // Check if the user has access to the community product in MyBucket and if the subscription is within the date range
//            var doesHeHasAccessToCommunityProudct = await _context.MyBucketM
//              .FirstOrDefaultAsync(b => b.ProductId == communityProductId && b.MobileUserKey == mobileUserKey && b.IsActive
//                && b.EndDate > DateTime.Now.AddDays(1));


//            var totalCount = 0;
//            var totalPages = 0;
//            if (doesHePurchasedProductAlready && doesHeHasAccessToCommunityProudct != null)// he has purchased the product once, also has the access to Community
//            {
//                responseModel.hasPurchasedProduct = true;
//                responseModel.hasAccessToPremiumContent = true;
//                responseModel.Posts = await GetCommunityPostDataBasedOnPostTypeId((CommunityPostTypeEnum)query.PostTypeId, communityProductId, query.PageNumber, query.PageSize, totalCount, totalPages);
////            bool hasPurchasedProductBeforeCommunityAccess = await _context.MyBucketM
////                .AnyAsync(b => b.ProductId == productCommunityMapping.ProductId && b.MobileUserKey == mobileUser.PublicKey && b.IsActive);

////            if (!hasPurchasedProductBeforeCommunityAccess)
////            {
////                _apiResponse.Data = new
////                {
////                    TotalCount = 0,
////                    TotalPages = 0,
////                    PageNumber = query.PageNumber,
////                    PageSize = query.PageSize,
////                    hasPurchasedProduct = false,
////                    hasAccessToPremiumContent = false
////                };

//                _apiResponse.Data = responseModel;
//                _apiResponse.Message = "You do not have purchased strategy. Please subscribe strategy first.";
//            }
//            else if (doesHePurchasedProductAlready && doesHeHasAccessToCommunityProudct == null)// he has purchased the product once, but his Community access has gone or expired 
//            {
//                if (query.PostTypeId == 1)
//                {
//                    responseModel.Posts = await GetCommunityPostDataBasedOnPostTypeId(CommunityPostTypeEnum.Post, communityProductId, query.PageNumber, query.PageSize, totalCount, totalPages);
//                }
//                else
//                {
//                    var prod = await _context.ProductsM.FirstOrDefaultAsync(item => item.Id == productCommunityMapping.CommunityId);
//                    responseModel.CommunityId = productCommunityMapping.CommunityId;
//                    responseModel.CommunityName = prod?.Name ?? string.Empty;
//                }
//                responseModel.hasPurchasedProduct = true;
//                responseModel.hasAccessToPremiumContent = false;

//                _apiResponse.Data = responseModel;
//                _apiResponse.Message = "Please subscribe the community first to get the access.";
//            }
//            else if (!doesHePurchasedProductAlready)
//            {
//                _apiResponse.Message = "Please get the product before access to the community.";
//            }
//            else
//            {
//                _apiResponse.Data = responseModel;
//                _apiResponse.Message = "Invalid Request, Please check with admin or raise the ticket for this.";
//            }
//            _apiResponse.StatusCode = HttpStatusCode.OK;
//            return _apiResponse;
//        }


//        private async Task<List<CommunityPost>> GetCommunityPostDataBasedOnPostTypeId(CommunityPostTypeEnum postTypeId, int communityProductId, int pageNumber, int pageSize, long totalCount = 0, int totalPages = 0)
//        {
//            // MongoDb Retrieval of Posts based on ProductId and PostTypeId
//            var filter = Builders<CommunityPost>.Filter.And(
//                Builders<CommunityPost>.Filter.Eq(p => p.ProductId, communityProductId),
//                Builders<CommunityPost>.Filter.Eq(p => p.PostTypeId, (int)postTypeId)
//            );

////            var skip = (pageNumber - 1) * pageSize;

////            var posts = await _communityPost.Find(filter)
////                .Sort(Builders<CommunityPost>.Sort.Descending(p => p.CreatedOn)) // Order by latest
////                .Skip(skip)
////                .Limit(pageSize)
////                .ToListAsync();

////            totalCount = await _communityPost.CountDocumentsAsync(filter);
////            totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

//            //return posts;
//            //var result = new
//            //{
//            //    Posts = posts,
//            //    TotalCount = totalCount,
//            //    TotalPages = totalPages,
//            //    PageNumber = pageNumber,
//            //    PageSize = pageSize,
//            //    hasPurchasedProduct = doesHePurchasedProductAlready != null,
//            //    hasAccessToPremiumContent = doesHeHasAccessToCommunityProudct?.EndDate > DateTime.Now
//            //};
//        }



//        //// Only to get how many products community he has access to.
//        //// No Need to check the subscription date.
//        //public async Task<ApiCommonResponseModel> GetCommunityDetails(long mobileUserId)
//        //{
//        //    List<SqlParameter> sqlParameters =
//        //    [new SqlParameter { ParameterName = "MobileUserId", Value = mobileUserId, SqlDbType = SqlDbType.BigInt }];

////            var data = await _context.SqlQueryToListAsync<CommunityDetailsResponse>(ProcedureCommonSqlParametersText.GetCommunityDetails, sqlParameters.ToArray());

////            if (data is null)
////            {
////                _apiResponse.Message = "No Activity For This Search Found.";
////            }
////            else
////            {
////                _apiResponse.Data = new
////                {
////                    Data = data,
////                    PostTypeData = GetCommunityPostTypes()
////                };
////            }

////            _apiResponse.StatusCode = HttpStatusCode.OK;
////            _apiResponse.Message = "Data Fetched Successfully.";
////            return _apiResponse;
////        }

////        protected List<CommunityPostTypeResponse> GetCommunityPostTypes()
////        {
////            return Enum.GetValues(typeof(CommunityPostTypeEnum))
////                .Cast<CommunityPostTypeEnum>()
////                .Select(e => new CommunityPostTypeResponse
////                {
////                    Id = (int)e,
////                    Name = e.ToString()
////                })
////                .ToList();
////        }

////        public List<CommunityPostTypeResponse> FetchCommunityPostTypes()
////        {
////            return GetCommunityPostTypes();
////        }

////        public async Task<ApiCommonResponseModel> Manage(CreateCommunityPostRequest request)
////        {
////            var mobileUser = await _context.MobileUsers.FindAsync(request.MobileUserId);

////            //if (mobileUser == null)
////            //{
////            //    _apiResponse.StatusCode = System.Net.HttpStatusCode.NotFound;
////            //    _apiResponse.Message = "User not found";
////            //    return _apiResponse;
////            //}

////            // Example: Create a Recorded Video post (PostTypeId 2)
////            var newVideoPost = new CommunityPost
////            {
////                ProductId = request.ProductId, // Replace with your ProductId
////                PostTypeId = (int)request.PostTypeId,
////                Url = request.Url,
////                ImageUrl = "",
////                Title = request.Title,
////                Content = request.Content,
////                CreatedBy = request.MobileUserId,
////                CreatedOn = DateTime.Now
////            };
////            if (newVideoPost.PostTypeId == 3)
////            {
////                newVideoPost.ImageUrl = "https://communitypostdata.blob.core.windows.net/mobileapptest/20241128122140500.jpg";
////            }
////            await _communityPost.InsertOneAsync(newVideoPost);
////            _apiResponse.StatusCode = HttpStatusCode.OK;
////            _apiResponse.Message = "Post created successfully";
////            return _apiResponse;
////        }
////    }

//    public class CommunityPostDataResponse
//    {
//        public List<CommunityPost> Posts { get; set; }
//        public int CommunityId { get; set; } = 0;
//        public string CommunityName { get; set; }
//        public int TotalCount { get; set; } = 0;
//        public int TotalPages { get; set; } = 0;
//        public int PageNumber { get; set; } = 0;
//        public int PageSize { get; set; } = 0;
//        public bool hasPurchasedProduct { get; set; } = false;
//        public bool hasAccessToPremiumContent { get; set; } = false;
//    }
////}
