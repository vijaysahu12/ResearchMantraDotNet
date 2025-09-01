using Azure;
// using iTextSharp.text;
using RM.BlobStorage;
using RM.CommonService.Helpers;
using RM.CommonServices;
using RM.CommonServices.Services;
using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.KingResearchContext;
using RM.Database.MongoDbContext;
using RM.Model;
using RM.Model.Common;
using RM.Model.MongoDbCollection;
using RM.Model.RequestModel;
using RM.Model.RequestModel.Notification;
using RM.Model.ResponseModel;
using LiteDB;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
// using Org.BouncyCastle.Asn1.Ocsp;
using sib_api_v3_sdk.Client;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using static Google.Apis.Requests.BatchRequest;

namespace RM.CommonService
{
    public class CommunityPostService
    {
        private readonly IMongoDatabase _database;
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        private readonly IConfiguration _configuration;
        private readonly KingResearchContext _context;
        private readonly IMongoRepository<Log> _log;
        private readonly IMongoRepository<CommunityPost> _communityRepo;
        private readonly IMongoRepository<CommunityComments> _communityCommentsRepo;
        private readonly IMongoRepository<Model.MongoDbCollection.User> _userCollectionRepo;

        private readonly IMongoCollection<CommunityPost> _communityPost;
        private readonly IMongoCollection<CommunityComments> _communityComments;
        private readonly IMongoCollection<Model.MongoDbCollection.User> _userCollection;
        private readonly IMongoCollection<Like> _likeCollection;
        private readonly IMongoCollection<Reply> _replyCollection;
        private readonly IMongoCollection<BlogReport> _blogReportCollection;
        private readonly IMongoCollection<CommunityReport> _communityReportCollection;
        private readonly IMongoCollection<UserBlock> _userBlockCollection;
        private readonly Lazy<IMobileNotificationService> _mobileNotificationService;
        private readonly ILogger _logger;


        ApiCommonResponseModel _apiResponse = new();
        public CommunityPostService(IOptions<MongoDBSettings> mongoDBSettings,
           IConfiguration configuration, KingResearchContext context,
           IAzureBlobStorageService azureBlobStorageService, IMongoRepository<Log> log, IMongoRepository<CommunityPost> communityPostRepo,
           Lazy<IMobileNotificationService> mobileNotificationService, ILogger<CommunityPostService> logger)
        {
            MongoClient client = new(mongoDBSettings.Value.ConnectionURI);
            _database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _configuration = configuration;
            _communityPost = _database.GetCollection<CommunityPost>("CommunityPost");
            _communityComments = _database.GetCollection<CommunityComments>("CommunityComments");
            _userCollection = _database.GetCollection<Model.MongoDbCollection.User>("User");
            _context = context;
            _azureBlobStorageService = azureBlobStorageService;
            _log = log;
            _communityRepo = communityPostRepo;
            _likeCollection = _database.GetCollection<Like>("Like");
            _replyCollection = _database.GetCollection<Reply>("Reply");
            _blogReportCollection = _database.GetCollection<BlogReport>("BlogReport");
            _communityReportCollection = _database.GetCollection<CommunityReport>("CommunityReport");
            _userBlockCollection = _database.GetCollection<UserBlock>("UserBlock");
            _mobileNotificationService = mobileNotificationService;
            _logger = logger;
        }

        /// <summary>
        /// query.Id = productId,
        /// query.PrimaryKey = PostTypeId
        /// query.loggedInuserId = MobileUserId
        /// Check only if the user has access to the community product and if the subscription is within the date range
        /// Check if the user has purchased product once before accessing the community
        /// If all above conditions are met, retrieve the posts from MongoDB 
        /// else ask for purchase the product first before accessing the community or subscribe to community directly 
        /// </summary>


        /// <summary>
        /// Calling this method from CRM - UI
        /// </summary>
        public async Task<ApiCommonResponseModel> Manage(CreateCommunityPostRequest request)
        {
            var now = DateTime.Now;

            // Validate image count early
            if (request.NewImages?.Count > 3)
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Maximum 3 images are allowed."
                };

            // Fetch user from SQL
            var user = await _context.Users
                .Where(u => u.Id == request.MobileUserId)
                .Select(u => new
                {
                    u.Id,
                    u.PublicKey,
                    FullName = $"{u.FirstName} {u.LastName}",
                    u.Gender,
                    u.UserImage
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "User not found"
                };

            // Check if user exists in MongoDB
            var mobileUser = await _userCollection.Find(x => x.PublicKey == user.PublicKey).FirstOrDefaultAsync();

            if (mobileUser == null)
            {
                mobileUser = new Model.MongoDbCollection.User
                {
                    CanCommunityPost = true,
                    FullName = user.FullName,
                    Gender = user.Gender,
                    ProfileImage = user.UserImage,
                    PublicKey = (Guid)user.PublicKey,
                    CreatedOn = now,
                    ModifiedOn = now,
                    IsCrmUser = true // Assuming CRM users are admins
                };
                await _userCollection.InsertOneAsync(mobileUser);
            }

            // Prepare new post
            var newPost = new CommunityPost
            {
                ProductId = request.ProductId,
                PostTypeId = (int)request.PostTypeId,
                Url = request.Url,
                Title = request.Title,
                Content = request.Content,
                CreatedBy = user.Id,
                ModifiedBy = user.Id,
                CreatedOn = now,
                ModifiedOn = now,
                UpComingEvent = request.UpComingEvent,
                IsQueryFormEnabled = request.IsQueryFormEnabled,
                IsJoinNowEnabled = request.IsJoinNowEnabled,
                IsActive = false,
                IsApproved = request.IsApproved,
                UserObjectId = mobileUser.ObjectId,
                Isadminposted = true,
                Likecount = 0,
                CommentsCount = 0,
                EnableComments = true,
                ReportsCount = 0,
                ImageUrls = new List<ImageModel>()
            };

            // Upload main image (if any)
            if (request.ImageUrl != null)
                newPost.ImageUrl = await _azureBlobStorageService.UploadImage(request.ImageUrl);

            // Upload additional images (if any)
            if (request.NewImages?.Any() == true)
            {
                for (int i = 0; i < request.NewImages.Count; i++)
                {
                    var aspectRatio = request.AspectRatios?.ElementAtOrDefault(i) ?? "auto";
                    var imageName = await _azureBlobStorageService.UploadImage(request.NewImages[i]);
                    newPost.ImageUrls.Add(new ImageModel { Name = imageName, AspectRatio = aspectRatio });
                }

                if (string.IsNullOrEmpty(newPost.ImageUrl))
                    newPost.ImageUrl = newPost.ImageUrls.First().Name;
            }

            await _communityPost.InsertOneAsync(newPost);

            // Fetch post type and product name
            var postTypes = GetCommunityPostTypes().ToDictionary(pt => pt.Id, pt => pt.Name);
            var product = await _context.ProductsM
                .Where(p => p.Id == newPost.ProductId)
                .Select(p => p.Name)
                .FirstOrDefaultAsync();

            // Construct response
            var response = new
            {
                newPost.Id,
                newPost.ProductId,
                ProductName = product ?? "N/A",
                newPost.PostTypeId,
                PostTypeName = postTypes.GetValueOrDefault(newPost.PostTypeId, "N/A"),
                newPost.Url,
                newPost.Title,
                newPost.Content,
                CreatedBy = user.FullName,
                ModifiedBy = user.FullName,
                newPost.CreatedOn,
                newPost.ModifiedOn,
                newPost.IsActive,
                newPost.UpComingEvent,
                newPost.IsJoinNowEnabled,
                newPost.IsQueryFormEnabled,
                newPost.IsApproved,
                newPost.UserObjectId,
                newPost.ImageUrls,
                newPost.ImageUrl,
                newPost.Isadminposted,
                newPost.Likecount,
                newPost.CommentsCount,
                newPost.EnableComments
            };

            return CreateResponseStatusCode(HttpStatusCode.OK, "Post Created successfully", response);
        }

        //  Get All Posts
        //public async Task<ApiCommonResponseModel> GetAllPostsAsync(QueryValues query)
        //{
        //    // 1️⃣ Fetch all community post types
        //    var postTypes = GetCommunityPostTypes().ToDictionary(pt => pt.Id, pt => pt.Name);

        //    // 2️⃣ Build MongoDB Filters
        //    var filters = new List<FilterDefinition<CommunityPost>>();
        //    var filterBuilder = Builders<CommunityPost>.Filter;

        //    filters.Add(filterBuilder.Ne(post => post.IsDelete, true));

        //    if (query.Id > 0)
        //    {
        //        filters.Add(filterBuilder.Eq(post => post.ProductId, query.Id));
        //    }

        //    if (query.PostTypeId > 0)
        //    {
        //        filters.Add(filterBuilder.Eq(post => post.PostTypeId, query.PostTypeId));
        //    }

        //    // 3️⃣ Date Range Filter (Ensure proper filtering)
        //    if (query.FromDate.HasValue && query.ToDate.HasValue)
        //    {
        //        filters.Add(filterBuilder.And(
        //            filterBuilder.Gte(post => post.CreatedOn, query.FromDate.Value.Date),
        //            filterBuilder.Lte(post => post.CreatedOn, query.ToDate.Value.Date.AddDays(1).AddTicks(-1)) // Include full ToDate
        //        ));
        //    }
        //    else if (query.FromDate.HasValue)
        //    {
        //        filters.Add(filterBuilder.Gte(post => post.CreatedOn, query.FromDate.Value.Date));
        //    }
        //    else if (query.ToDate.HasValue)
        //    {
        //        filters.Add(filterBuilder.Lte(post => post.CreatedOn, query.ToDate.Value.Date.AddDays(1).AddTicks(-1)));
        //    }

        //    // 4️⃣ Status Filters
        //    if (!string.IsNullOrEmpty(query.PrimaryKey))
        //    {
        //        switch (query.PrimaryKey.ToLower())
        //        {
        //            case "active":
        //                filters.Add(filterBuilder.Eq(post => post.IsActive, true));
        //                filters.Add(filterBuilder.Eq(post => post.IsDelete, false));
        //                break;
        //            case "inactive":
        //                filters.Add(filterBuilder.Eq(post => post.IsActive, false));
        //                filters.Add(filterBuilder.Eq(post => post.IsDelete, false));
        //                break;
        //                //case "deleted":
        //                //    filters.Add(filterBuilder.Eq(post => post.IsDelete, true));
        //                //    break;
        //        }
        //    }
        //    else
        //    {
        //        // Ensure all records (Active, Inactive, Deleted) are included
        //        filters.Add(filterBuilder.Or(
        //            filterBuilder.Eq(post => post.IsActive, true),
        //            filterBuilder.Eq(post => post.IsActive, false)

        //        ));
        //    }

        //    var finalFilter = filters.Any() ? filterBuilder.And(filters) : filterBuilder.Empty;

        //    // 5️⃣ Get **Total Count Before Pagination**
        //    var totalCount = await _communityPost.CountDocumentsAsync(finalFilter);

        //    // 6️⃣ Fetch Paginated Data (sorted by ModifiedOn DESC)
        //    var paginatedPosts = await _communityPost
        //        .Find(finalFilter) // Apply proper filter
        //        .SortByDescending(post => post.ModifiedOn) // Handle nulls
        //        .ToListAsync();

        //    if (!paginatedPosts.Any())
        //    {
        //        return new ApiCommonResponseModel
        //        {
        //            Data = new List<CommunityPostResponse>(),
        //            Total = Convert.ToInt32(totalCount),
        //            StatusCode = HttpStatusCode.OK
        //        };
        //    }

        //    // 7️⃣ Fetch **Product Names from SQL**
        //    var productIds = paginatedPosts.Select(p => p.ProductId).Distinct().ToList();
        //    var productDictionary = await _context.ProductsM
        //        .Where(p => productIds.Contains(p.Id))
        //        .ToDictionaryAsync(p => p.Id, p => p.Name ?? "N/A");

        //    // 8️⃣ Fetch **User Details**
        //    var createdByIds = paginatedPosts.Select(p => (long)p.ModifiedBy).Distinct().ToList();
        //    var userDictionary = await _context.Users
        //        .Where(u => createdByIds.Contains((long)u.Id))
        //        .ToDictionaryAsync(u => (long)u.Id, u => $"{u.FirstName} {u.LastName}");

        //    // 9️⃣ Combine Data (MongoDB + SQL)
        //    var result = paginatedPosts.Select(post => new CommunityPostResponse
        //    {
        //        Id = post.Id,
        //        ProductId = post.ProductId,
        //        ProductName = productDictionary.GetValueOrDefault(post.ProductId, "N/A"),
        //        PostTypeId = post.PostTypeId,
        //        PostTypeName = postTypes.GetValueOrDefault(post.PostTypeId, "Unknown"),
        //        Title = post.Title,
        //        Content = post.Content,
        //        CreatedOn = post.CreatedOn,
        //        ModifiedOn = post.ModifiedOn,
        //        IsActive = post.IsActive,
        //        IsDelete = post.IsDelete,
        //        Url = post.Url,
        //        UpComingEvent = post.UpComingEvent ?? DateTime.Now,
        //        ImageUrls = post.ImageUrls ?? new List<ImageModel>(),
        //        ImageUrl = post.ImageUrl,
        //        IsQueryFormEnabled = post.IsQueryFormEnabled,
        //        IsJoinNowEnabled = post.IsQueryFormEnabled,
        //        IsApproved = post.IsApproved,
        //        UserObjectId = post.UserObjectId,
        //        CreatedBy = userDictionary.GetValueOrDefault((long)post.CreatedBy, "Unknown"),
        //        ModifiedBy = userDictionary.GetValueOrDefault((long)post.ModifiedBy, "Unknown"),
        //        Isadminposted = post.Isadminposted,
        //        Likecount = post.Likecount,
        //        CommentsCount = post.CommentsCount,
        //    }).ToList();

        //    // 🔄 Apply custom sort BEFORE search
        //    if (query.PostTypeId == 1)
        //    {
        //        result = result.OrderBy(post =>
        //            post.IsApproved == "Pending" ? 0 :
        //            post.IsApproved == "Approved" ? 1 : 2)
        //        //.ThenByDescending(post => post.ModifiedOn)
        //        .ToList();
        //    }
        //    else if (query.PostTypeId == 2 || query.PostTypeId == 3)
        //    {
        //        result = result.OrderByDescending(post => post.IsActive == true)
        //            .ThenByDescending(post => post.ModifiedOn)
        //            .ToList();
        //    }

        //    // 🔟 Apply Search Filter **AFTER** Adding Product/PostType Names
        //    if (!string.IsNullOrEmpty(query.SearchText))
        //    {
        //        string searchText = query.SearchText.ToLower();
        //        result = result.Where(post =>
        //            (post.Title?.ToLower().Contains(searchText) ?? false) ||
        //            (post.Content?.ToLower().Contains(searchText) ?? false) ||
        //            (post.ProductName?.ToLower().Contains(searchText) ?? false) ||
        //            (post.PostTypeName?.ToLower().Contains(searchText) ?? false)
        //        ).ToList();

        //        totalCount = result.Count; // **Update total count after search**
        //    }

        //    return new ApiCommonResponseModel
        //    {
        //        Data = result,
        //        Total = Convert.ToInt32(totalCount), // **Correct total count**
        //        StatusCode = HttpStatusCode.OK
        //    };
        //}

        public async Task<ApiCommonResponseModel> GetAllPostsAsync(QueryValues query)
        {
            // 1️⃣ Fetch all community post types once
            var postTypes = GetCommunityPostTypes().ToDictionary(pt => pt.Id, pt => pt.Name);

            var filterBuilder = Builders<CommunityPost>.Filter;
            var filters = new List<FilterDefinition<CommunityPost>>
            {
                filterBuilder.Ne(post => post.IsDelete, true)  // Exclude deleted posts by default
            };

            // 2️⃣ Add filters based on query parameters
            if (query.Id > 0)
                filters.Add(filterBuilder.Eq(post => post.ProductId, query.Id));

            if (query.PostTypeId > 0)
                filters.Add(filterBuilder.Eq(post => post.PostTypeId, query.PostTypeId));

            // 3️⃣ Date range filters
            if (query.FromDate.HasValue || query.ToDate.HasValue)
            {
                var fromDate = query.FromDate?.Date ?? DateTime.MinValue;
                var toDate = query.ToDate?.Date.AddDays(1).AddTicks(-1) ?? DateTime.MaxValue;
                filters.Add(filterBuilder.And(
                    filterBuilder.Gte(post => post.CreatedOn, fromDate),
                    filterBuilder.Lte(post => post.CreatedOn, toDate)
                ));
            }

            // 4️⃣ Status filters
            if (!string.IsNullOrEmpty(query.SecondaryKey))
            {
                switch (query.SecondaryKey.ToLower())
                {
                    case "approved":
                        filters.Add(filterBuilder.Eq(post => post.IsApproved, "Approved"));
                        break;
                    case "pending":
                        filters.Add(filterBuilder.Eq(post => post.IsApproved, "Pending"));
                        break;
                    // Uncomment if needed:
                    case "rejected":
                        filters.Add(filterBuilder.Eq(post => post.IsApproved, "Rejected"));
                        break;
                }
            }

             if (!string.IsNullOrEmpty(query.PrimaryKey))
            {
                switch (query.PrimaryKey.ToLower())
                {
                    case "active":
                        filters.Add(filterBuilder.Eq(post => post.IsActive, true));
                        filters.Add(filterBuilder.Eq(post => post.IsDelete, false));
                        break;
                    case "inactive":
                        filters.Add(filterBuilder.Eq(post => post.IsActive, false));
                        filters.Add(filterBuilder.Eq(post => post.IsDelete, false));
                        break;
                        // Uncomment if needed:
                        // case "deleted":
                        //     filters.Add(filterBuilder.Eq(post => post.IsDelete, true));
                        //     break;
                }
            }
            else
            {
                // Include all non-deleted posts regardless of active status
                filters.Add(filterBuilder.Eq(post => post.IsDelete, false));
            }

            var finalFilter = filters.Any() ? filterBuilder.And(filters) : filterBuilder.Empty;

            // 5️⃣ Get total count before pagination/search
            var totalCount = await _communityPost.CountDocumentsAsync(finalFilter);

            var skip = (query.PageNumber - 1) * query.PageSize;

            var posts = await _communityPost.Find(finalFilter).ToListAsync();

            var paginatedPosts = posts
                    .OrderByDescending(p => p.ModifiedOn.HasValue)   // Puts non-null ModifiedOn first
                    .ThenByDescending(p => p.ModifiedOn)             // Then sorts those by descending date
                    .Skip(skip)
                    .Take(query.PageSize)
                    .ToList();

            // 6️⃣ Fetch all matching posts sorted by ModifiedOn descending
            //var paginatedPosts = await _communityPost
            //    .Find(finalFilter)
            //    .SortByDescending(post => post.ModifiedOn)
            //    .ToListAsync();

            if (!paginatedPosts.Any())
            {
                return new ApiCommonResponseModel
                {
                    Data = new List<CommunityPostResponse>(),
                    Total = 0,
                    StatusCode = HttpStatusCode.OK
                };
            }

            // 7️⃣ Fetch related product names and user details in parallel
            // 1️⃣ Collect distinct IDs
            var productIds = paginatedPosts.Select(p => p.ProductId).Distinct().ToList();
            var createdByIds = paginatedPosts
           .Where(p => p.CreatedBy != 0)
           .Select(p => p.CreatedBy)
           .Distinct()
           .ToList();

            // 2️⃣ Load product dictionary
            var productDictionary = await _context.ProductsM
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Name ?? "N/A");

            // 3️⃣ Load SQL users with FullName + Gender
            // 1️⃣ Get valid string UserObjectIds
            var userObjectIdList = paginatedPosts
                .Where(p => !string.IsNullOrWhiteSpace(p.UserObjectId))
                .Select(p => p.UserObjectId.Trim())
                .Distinct()
                .ToList();

            // 2️⃣ Match against string ObjectId field in Mongo
            var mongoUserFilter = Builders<Model.MongoDbCollection.User>
                .Filter.In(u => u.ObjectId, userObjectIdList);

            var mongoUsers = await _userCollection.Find(mongoUserFilter).ToListAsync();


            // 3️⃣ Dictionary for quick lookup
            var mongoUserDictionary = mongoUsers.ToDictionary(
                u => u.ObjectId.ToString(), // Convert _id to string for matching with UserObjectId
                u => new {
                    FullName = u.FullName ?? "Unknown",
                    Gender = u.Gender ?? "N/A"
                }
            );

            // 4️⃣ Map to DTOs
            var result = paginatedPosts.Select(post =>
            {
                var userInfo = string.IsNullOrEmpty(post.UserObjectId) ? null : mongoUserDictionary.GetValueOrDefault(post.UserObjectId);

                return new CommunityPostResponse
                {
                    Id = post.Id,
                    ProductId = post.ProductId,
                    ProductName = "", // fill if needed
                    PostTypeId = post.PostTypeId,
                    PostTypeName = "", // fill if needed
                    Title = post.Title,
                    Content = post.Content,
                    CreatedOn = post.CreatedOn,
                    ModifiedOn = post.ModifiedOn,
                    IsActive = post.IsActive,
                    IsDelete = post.IsDelete,
                    Url = post.Url,
                    UpComingEvent = post.UpComingEvent,
                    ImageUrls = post.ImageUrls ?? new List<ImageModel>(),
                    ImageUrl = post.ImageUrl,
                    IsQueryFormEnabled = post.IsQueryFormEnabled,
                    IsJoinNowEnabled = post.IsJoinNowEnabled,
                    IsApproved = post.IsApproved,
                    UserObjectId = post.UserObjectId,
                    CreatedBy = userInfo?.FullName ?? "Unknown",
                    Gender = userInfo?.Gender ?? "N/A",
                    ModifiedBy = "", // optional
                    Isadminposted = post.Isadminposted,
                    Likecount = post.Likecount,
                    CommentsCount = post.CommentsCount
                };
            }).ToList();

            // 9️⃣ Custom sorting based on PostTypeId
            if (query.PostTypeId == 1)
            {
                result = result.OrderBy(post =>
                    post.IsApproved == "Pending" ? 0 :
                    post.IsApproved == "Approved" ? 1 : 2)
                    .ToList();
            }
            else if (query.PostTypeId == 2 || query.PostTypeId == 3)
            {
                result = result
                    .OrderByDescending(post => post.IsActive)
                    .ThenByDescending(post => post.ModifiedOn)
                    .ToList();
            }

            // 🔟 Apply search filter AFTER enriching with names
            if (!string.IsNullOrEmpty(query.SearchText))
            {
                var searchText = query.SearchText.ToLowerInvariant();
                result = result.Where(post =>
                    (post.Title?.ToLower().Contains(searchText) ?? false) ||
                    (post.Content?.ToLower().Contains(searchText) ?? false) ||
                    (post.ProductName?.ToLower().Contains(searchText) ?? false) ||
                    (post.PostTypeName?.ToLower().Contains(searchText) ?? false)
                ).ToList();

                totalCount = result.Count;
            }

            return CreateResponseStatusCode(HttpStatusCode.OK, "Data fetched successfully", result, (int)totalCount);
        }

        public async Task<CommunityPostResponse?> GetPostByIdAsync(string postId)
        {
            var post = await _communityPost.Find(p => p.Id == postId).FirstOrDefaultAsync();

            if (post == null) return null;

            // Fetch Product Name from SQL
            var productName = await _context.ProductsM
                .Where(p => p.Id == post.ProductId)
                .Select(p => p.Name)
                .FirstOrDefaultAsync() ?? "N/A";

            // Fetch CreatedBy User Details
            var createdByName = await _context.Users
                .Where(u => u.Id == post.ModifiedBy)
                .Select(u => u.FirstName + " " + u.LastName)
                .FirstOrDefaultAsync() ?? "Unknown";

            return new CommunityPostResponse
            {
                Id = post.Id,
                ProductId = post.ProductId,
                ProductName = productName,
                PostTypeId = post.PostTypeId,
                Title = post.Title,
                Content = post.Content,
                CreatedOn = post.CreatedOn,
                Url = post.Url,
                ImageUrl = post.ImageUrl,
                ImageUrls = post.ImageUrls,
                CreatedBy = createdByName,
                IsDelete = post.IsDelete,
                UpComingEvent = post.UpComingEvent,
                IsQueryFormEnabled = post.IsQueryFormEnabled,
                IsJoinNowEnabled = post.IsJoinNowEnabled,
                IsActive = post.IsActive,
                ModifiedOn = post.ModifiedOn,
                IsApproved = post.IsApproved,
                UserObjectId = post.UserObjectId,
            };

        }

        public async Task<ApiCommonResponseModel> UpdateCommunityPost(string postId, CommunityPostUpdateModel updateModel)
        {
            // Parse the incoming datetime string as local time
            //if (updateModel.UpComingEvent.HasValue)
            //{
            //    updateModel.UpComingEvent = updateModel.UpComingEvent?.ToUniversalTime();
            //}

            var filter = Builders<CommunityPost>.Filter.Eq(p => p.Id, postId);

            var existingPost = await _communityPost.Find(filter).FirstOrDefaultAsync();

            if (existingPost == null)
            {
                return CreateResponseStatusCode(HttpStatusCode.NotFound, "Post Not Found");
            }

            var updateDefinition = new List<UpdateDefinition<CommunityPost>>
            {
                Builders<CommunityPost>.Update.Set(p => p.Content, updateModel.Content),
                Builders<CommunityPost>.Update.Set(p => p.Title, updateModel.Title),
                Builders<CommunityPost>.Update.Set(p => p.Url, updateModel.Url),
                Builders<CommunityPost>.Update.Set(p => p.PostTypeId, updateModel.PostTypeId),
                Builders<CommunityPost>.Update.Set(p => p.ProductId, updateModel.ProductId),
                Builders<CommunityPost>.Update.Set(p => p.ModifiedOn, DateTime.Now), // Track modification timestamp
                Builders<CommunityPost>.Update.Set(p => p.ModifiedBy,updateModel.ModifiedBy),
                Builders<CommunityPost>.Update.Set(p => p.IsDelete,updateModel.IsDelete),
                Builders<CommunityPost>.Update.Set(p => p.UpComingEvent,updateModel.UpComingEvent),
                Builders<CommunityPost>.Update.Set(p => p.IsJoinNowEnabled,updateModel.IsJoinNowEnabled),
                Builders<CommunityPost>.Update.Set(p => p.IsQueryFormEnabled,updateModel.IsQueryFormEnabled),
                Builders<CommunityPost>.Update.Set(p => p.IsApproved,updateModel.IsApproved),
            };

            // Upload and update ImageUrl (single image)
            // Handle single image upload (ImageUrl)
            // Handle single image (main image if used separately)
            if (updateModel.ImageUrl != null)
            {
                // Assuming ImageUrl is a file to upload
                string uploadedSingleImage = await _azureBlobStorageService.UploadImage(updateModel.ImageUrl);
                updateDefinition.Add(Builders<CommunityPost>.Update.Set(p => p.ImageUrl, uploadedSingleImage));
            }

            // Handle multiple images (ImageUrls) with delete/retain logic
            var finalImageModels = new List<ImageModel>();

            if (updateModel.ImageUrls != null && updateModel.ImageUrls.Any())
            {
                finalImageModels.AddRange(existingPost.ImageUrls
                    .Where(img => updateModel.ImageUrls.Contains(img.Name))
                    .ToList());
            }

            var imagesToDelete = (existingPost.ImageUrls ?? new List<ImageModel>())
                .Where(img => updateModel.ImageUrls == null || !updateModel.ImageUrls.Contains(img.Name))
                .ToList();


            foreach (var image in imagesToDelete)
            {
                await _azureBlobStorageService.DeleteImage(image.Name);
            }

            if (updateModel.NewImages != null && updateModel.NewImages.Any())
            {
                for (int i = 0; i < updateModel.NewImages.Count; i++)
                {
                    var file = updateModel.NewImages[i];
                    string uploadedName = await _azureBlobStorageService.UploadImage(file);

                    finalImageModels.Add(new ImageModel
                    {
                        Name = uploadedName,
                        AspectRatio = (updateModel.AspectRatios != null && updateModel.AspectRatios.Count > i)
                            ? updateModel.AspectRatios[i]
                            : "auto"
                    });
                }
            }

            if (updateModel.ImageUrl == null)
            {
                if (finalImageModels.Count > 0)
                {
                    // Set ImageUrl to first of remaining images
                    updateDefinition.Add(Builders<CommunityPost>.Update.Set(p => p.ImageUrl, finalImageModels[0].Name));
                }
                else
                {
                    // No images left, remove ImageUrl field from MongoDB
                    updateDefinition.Add(Builders<CommunityPost>.Update.Set(p => p.ImageUrl, null));
                }
            }

            updateDefinition.Add(Builders<CommunityPost>.Update.Set(p => p.ImageUrls, finalImageModels));

            var update = Builders<CommunityPost>.Update.Combine(updateDefinition);

            var options = new FindOneAndUpdateOptions<CommunityPost>
            {
                ReturnDocument = ReturnDocument.After
            };

            var updatedPost = await _communityPost.FindOneAndUpdateAsync(filter, update, options);


            if (updatedPost == null)
            {
                return CreateResponseStatusCode(HttpStatusCode.NotFound, "Post Not Found");
            }

            var postTypes = GetCommunityPostTypes().ToDictionary(pt => pt.Id, pt => pt.Name);

            var product = await _context.ProductsM
                .Where(p => p.Id == updatedPost.ProductId)
                .Select(p => new { p.Id, p.Name })
                .FirstOrDefaultAsync();

            var user = await _context.Users
                .Where(u => u.Id == updatedPost.ModifiedBy)
                .Select(u => new { u.Id, FullName = $"{u.FirstName} {u.LastName}" })
                .FirstOrDefaultAsync();

            // 🔹 **Step 3: Return Response with Names**
            var responsePost = new
            {
                updatedPost.Id,
                updatedPost.ProductId,
                ProductName = product?.Name ?? "N/A", // Fetch product name
                updatedPost.PostTypeId,
                PostTypeName = postTypes.ContainsKey(updatedPost.PostTypeId) ? postTypes[updatedPost.PostTypeId] : "N/A",
                updatedPost.Url,
                updatedPost.Title,
                updatedPost.Content,
                updatedPost.CreatedOn,
                ModifiedBy = user?.FullName ?? "Unknown", // Fetch user full name
                updatedPost.ModifiedOn,
                updatedPost.IsActive,
                updatedPost.UpComingEvent,
                updatedPost.ImageUrls,
                updatedPost.IsQueryFormEnabled,
                updatedPost.IsJoinNowEnabled,
                updatedPost.IsApproved,
            };

            return CreateResponseStatusCode(HttpStatusCode.OK, "Post updated successfully", responsePost);
        }

        public async Task<ApiCommonResponseModel> UpdateCommunityPostStatus(string id, int userPublicKey)
        {
            var filter = Builders<CommunityPost>.Filter.Eq(p => p.Id, id);
            var existingPost = await _communityPost.Find(filter).FirstOrDefaultAsync();

            // 2️⃣ Check if the post exists
            if (existingPost == null)
            {
                return CreateResponseStatusCode(HttpStatusCode.NotFound, "Post Not Found");
            }

            // 3️ Toggle the IsActive value
            bool newStatus = !existingPost.IsActive;

            // 4️ Update the post with the toggled value
            var update = Builders<CommunityPost>.Update
                .Set(p => p.IsActive, newStatus) // Toggle the value
                .Set(p => p.ModifiedOn, DateTime.UtcNow)
                .Set(p => p.ModifiedBy, userPublicKey);

            var options = new FindOneAndUpdateOptions<CommunityPost>
            {
                ReturnDocument = ReturnDocument.After // Return the updated document
            };

            var updatedPost = await _communityPost.FindOneAndUpdateAsync(filter, update, options);

            if (updatedPost == null)
            {
                return CreateResponseStatusCode(HttpStatusCode.NotFound, "Post Not Found");
            }

            return CreateResponseStatusCode(HttpStatusCode.OK, "Status updated successfully", updatedPost);
        }

        public async Task<ApiCommonResponseModel> DeleteCommunityPost(string postId, int userPublicKey)
        {
            var filter = Builders<CommunityPost>.Filter.Eq(p => p.Id, postId);
            var update = Builders<CommunityPost>.Update
                .Set(p => p.IsDelete, true)  // Soft delete by setting IsActive to false
                .Set(p => p.ModifiedOn, DateTime.UtcNow) // Track deletion timestamp
                 .Set(p => p.ModifiedBy, userPublicKey);

            var options = new FindOneAndUpdateOptions<CommunityPost>
            {
                ReturnDocument = ReturnDocument.After // Return the updated document
            };

            var deletedPost = await _communityPost.FindOneAndUpdateAsync(filter, update, options);

            if (deletedPost == null)
            {
                return CreateResponseStatusCode(HttpStatusCode.NotFound, "Post Not Found");
            }

            return CreateResponseStatusCode(HttpStatusCode.OK, "Post Deleted successfully", deletedPost);
        }


        /// <summary>
        /// Mobile side API (Calling this method from Mobile)
        /// </summary>
        /// <returns></returns>
        /// 
        /// <summary>
        /// THis method getting call while user posting the content on community from mobile application
        /// </summary>
        public async Task<ApiCommonResponseModel> ManageBlogCommunity(CreateCommunity request, int MobileUserId)
        {
            if (string.IsNullOrEmpty(request.Id) && !request.IsDelete)
            {
                var mobileUser = await GetMobileUserAsync(MobileUserId);
                if (mobileUser == null)
                    return CreateResponseStatusCode(HttpStatusCode.NotFound, "User Not Found");

                var mongoUser = await GetMongoUserByPublicKeyAsync(mobileUser.PublicKey);
                var imageUrlSuffix = _configuration["Azure:ImageUrlSuffix"];

                var newPost = await BuildNewCommunityPostAsync(request, MobileUserId, mongoUser?.ObjectId);

                await _communityPost.InsertOneAsync(newPost);

                bool hasUserLiked = false;
                if (!string.IsNullOrEmpty(mongoUser.PublicKey.ToString()))
                {
                    hasUserLiked = await _likeCollection
                        .Find(x => x.CreatedBy == mongoUser.PublicKey.ToString() && x.BlogId == newPost.Id)
                        .AnyAsync();
                }

                var response = CreatePostResponse(newPost, mongoUser, imageUrlSuffix, hasUserLiked);

                return CreateResponseStatusCode(HttpStatusCode.OK, "Post created successfully", response);
            }

            else if (!string.IsNullOrEmpty(request.Id) && !request.IsDelete)
            {
                return await HandlePostUpdateAsync(request, MobileUserId);
            }
            else if (!string.IsNullOrEmpty(request.Id) && request.IsDelete)
            {
                return await HandlePostDeleteAsync(request.Id, MobileUserId);
            }

            return CreateResponseStatusCode(HttpStatusCode.BadRequest, "Invalid request parameters.");
        }
        public async Task<ApiCommonResponseModel> Get(GetCommunityRequestModel query, long LoggedInUserId)
        {
            if (query.PageNumber <= 0 || query.PageSize <= 0)
            {
                query.PageNumber = 1;
                query.PageSize = 20;
            }

            var mobileUser = await _context.MobileUsers.FirstOrDefaultAsync(item => item.Id == LoggedInUserId);
            if (mobileUser == null)
            {
                return CreateResponseStatusCode(HttpStatusCode.NotFound, "User Not Found");
            }

            var productCommunityMapping = await _context.ProductCommunityMappingM
                .FirstOrDefaultAsync(b => b.CommunityId == query.CommunityProductId && b.IsActive);

            if (productCommunityMapping == null)
            {
                return CreateResponseStatusCode(HttpStatusCode.NotFound, "Community Not Found");
            }

            // Check if the user has purchased a related product before accessing the community
            bool hasPurchasedProductBeforeCommunityAccess = await _context.MyBucketM
                .AnyAsync(b => b.ProductId == productCommunityMapping.ProductId &&
                               b.MobileUserKey == mobileUser.PublicKey &&
                               b.IsActive);

            if (!hasPurchasedProductBeforeCommunityAccess)
            {
                var community = await _context.ProductsM.FirstOrDefaultAsync(c => c.Id == query.CommunityProductId && c.IsActive);

                _apiResponse.Data = new
                {
                    TotalCount = 0,
                    TotalPages = 0,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    CommunityId = query.CommunityProductId,
                    CommunityName = community?.Name,
                    hasPurchasedProduct = false,
                    hasAccessToPremiumContent = false
                };

                return CreateResponseStatusCode(HttpStatusCode.OK, "You do not have purchased strategy. Please subscribe strategy first.");
            }

            // Check if the user has purchased the community product (even if expired)
            var hasPurchasedCommunityProduct = await _context.MyBucketM
                .FirstOrDefaultAsync(b => b.ProductId == query.CommunityProductId &&
                                           b.MobileUserKey == mobileUser.PublicKey &&
                                           b.IsActive);

            // Check for subscription validity
            bool hasSubscription = hasPurchasedCommunityProduct != null;
            bool hasValidSubscription = hasPurchasedCommunityProduct?.EndDate.Value.Date >= DateTime.Now.Date;

            // If user does not have any subscription at all, deny access
            if (!hasSubscription)
            {
                var community = await _context.ProductsM.FirstOrDefaultAsync(c => c.Id == query.CommunityProductId && c.IsActive);

                _apiResponse.Data = new
                {
                    TotalCount = 0,
                    TotalPages = 0,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    CommunityId = query.CommunityProductId,
                    CommunityName = community?.Name,
                    hasPurchasedProduct = false,
                    hasAccessToPremiumContent = false
                };

                return CreateResponseStatusCode(HttpStatusCode.OK, "You do not have access to this community. Please subscribe.");
            }

            bool hasAccessToPremiumContent = query.PostTypeId == (int)CommunityPostTypeEnum.Post
                ? hasSubscription
                : hasValidSubscription;

            // Validate PostTypeId
            if (!Enum.IsDefined(typeof(CommunityPostTypeEnum), query.PostTypeId))
            {
                return CreateResponseStatusCode(HttpStatusCode.BadRequest, "Provide a valid PostTypeId");
            }

            // Step 1: Check CanPost flag
            var canPost = await _context.ProductsM
                .Where(p => p.Id == query.CommunityProductId)
                .Select(p => p.CanPost)
                .FirstOrDefaultAsync();

            // Step 2: Resolve current user's ObjectId
            var currentUserObjectId = await GetMobileUserObjectIdAsync(query.LoggedInUserId);
            var blockerObjectId = new MongoDB.Bson.ObjectId(currentUserObjectId);

            // Step 3: Get blocked users list
            var blockedUserIds = await _userBlockCollection
                .Find(b => b.BlockerId == blockerObjectId && b.IsActive)
                .Project(b => b.BlockedId.ToString())
                .ToListAsync();

            var currentUserObjectIdBson = MongoDB.Bson.ObjectId.Parse(currentUserObjectId); // Convert string to ObjectId

            var reportedPostIds = await _communityReportCollection
                .Find(r => r.ReportedBy == currentUserObjectIdBson && r.Id != null)
                .Project(r => r.CommunityId)
                .ToListAsync();

            // Step 4: Build MongoDB filter
            FilterDefinition<CommunityPost> filter;

            var baseFilter = Builders<CommunityPost>.Filter.And(
                Builders<CommunityPost>.Filter.Eq(p => p.ProductId, query.CommunityProductId),
                Builders<CommunityPost>.Filter.Eq(p => p.PostTypeId, query.PostTypeId),
                Builders<CommunityPost>.Filter.Eq(p => p.IsActive, true)
            );

            var approvalFilter = string.IsNullOrEmpty(currentUserObjectId)
                ? Builders<CommunityPost>.Filter.Eq(p => p.IsApproved, "Approved")
                : Builders<CommunityPost>.Filter.Or(
                    Builders<CommunityPost>.Filter.Eq(p => p.IsApproved, "Approved"),
                    Builders<CommunityPost>.Filter.Eq(p => p.UserObjectId, currentUserObjectId)
                );

            var blockFilter = !blockedUserIds.Any()
                ? FilterDefinition<CommunityPost>.Empty
                : Builders<CommunityPost>.Filter.Nin(p => p.UserObjectId, blockedUserIds);

            var reportFilter = !reportedPostIds.Any()
                ? FilterDefinition<CommunityPost>.Empty
                : Builders<CommunityPost>.Filter.Nin(p => p.Id, reportedPostIds);

            // Final filter
            filter = Builders<CommunityPost>.Filter.And(baseFilter, approvalFilter, blockFilter, reportFilter);


            // Step 5: Pagination setup
            var skip = (query.PageNumber - 1) * query.PageSize;
            var imageUrlSuffix = _configuration["Azure:ImageUrlSuffix"];

            // Step 6: Get total count
            var totalCount = await _communityPost.CountDocumentsAsync(filter);
            var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

            // Step 7: Fetch posts
            List<CommunityPost> posts;

            if (query.PostTypeId == 3)
            {
                var allPosts = await _communityPost.Find(filter).ToListAsync();

                posts = allPosts
                    .OrderByDescending(p => p.UpComingEvent.HasValue)
                    .ThenByDescending(p => p.UpComingEvent)
                    .Skip(skip)
                    .Take(query.PageSize)
                    .ToList();
            }
            else
            {
                posts = await _communityPost.Find(filter)
                    .SortByDescending(p => p.CreatedOn)
                    .Skip(skip)
                    .Limit(query.PageSize)
                    .ToListAsync();
            }

            // Step 8: Fetch user info
            var sqlUserIds = posts.Where(p => string.IsNullOrEmpty(p.UserObjectId)).Select(p => p.CreatedBy).Distinct().ToList();
            var mongoUserIds = posts.Where(p => !string.IsNullOrEmpty(p.UserObjectId)).Select(p => p.UserObjectId).Distinct().ToList();

            var mobileUsers = await _context.Users
                .Where(u => sqlUserIds.Contains(u.Id))
                .Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName, u.Gender, u.UserImage })
                .ToListAsync();

            var mongoUsers = await _userCollection
                .Find(Builders<Model.MongoDbCollection.User>.Filter.In(u => u.ObjectId, mongoUserIds))
                .Project(u => new { u.ObjectId, u.FullName, u.Gender, u.ProfileImage })
                .ToListAsync();

            // Step 9: Reported & Liked posts

            var likedBlogIds = await _likeCollection
                .Find(l => l.CreatedBy == query.LoggedInUserId.ToString())
                .Project(l => l.BlogId.ToString())
                .ToListAsync();

            var likedSet = likedBlogIds.ToHashSet();

            // Step 10: Project to DTO
            var updatedPosts = posts.Select(post =>
            {
                var user = string.IsNullOrEmpty(post.UserObjectId)
                    ? mobileUsers.Where(u => u.Id == post.CreatedBy).Select(u => new UserDto
                    {
                        FullName = u.FullName,
                        Gender = u.Gender,
                        ProfileImage = u.UserImage
                    }).FirstOrDefault()
                    : mongoUsers.Where(u => u.ObjectId == post.UserObjectId).Select(u => new UserDto
                    {
                        FullName = u.FullName,
                        Gender = u.Gender,
                        ProfileImage = u.ProfileImage
                    }).FirstOrDefault();

                return new CommunityPostDto
                {
                    Id = post.Id,
                    ObjectId = post.UserObjectId,
                    ProductId = post.ProductId,
                    PostTypeId = post.PostTypeId,
                    Title = post.Title,
                    Content = post.Content,
                    CreatedBy = post.CreatedBy,
                    CreatedOn = post.CreatedOn,
                    ModifiedBy = post.ModifiedBy,
                    ModifiedOn = post.ModifiedOn,
                    Url = post.Url,
                    ImageUrl = string.IsNullOrEmpty(post.ImageUrl) ? null : imageUrlSuffix + post.ImageUrl.Trim(),
                    ImageUrls = post.ImageUrls?.Select(img => new ImageModel
                    {
                        Name = string.IsNullOrEmpty(img.Name) ? null : imageUrlSuffix + img.Name.Trim(),
                        AspectRatio = img.AspectRatio
                    }).ToList() ?? new List<ImageModel>(),
                    IsActive = post.IsActive,
                    IsDelete = post.IsDelete,
                    UpComingEvent = post.UpComingEvent.HasValue
                        ? TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(post.UpComingEvent.Value, DateTimeKind.Utc),
                        TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")).ToString("yyyy-MM-ddTHH:mm:ss")
                        : null,
                    IsQueryFormEnabled = post.IsQueryFormEnabled,
                    IsJoinNowEnabled = post.IsJoinNowEnabled,
                    IsApproved = post.IsApproved,
                    UserObjectId = post.UserObjectId,
                    FullName = user?.FullName,
                    Gender = user?.Gender,
                    ProfileImage = user?.ProfileImage,
                    Likecount = post.Likecount,
                    CommentsCount = post.CommentsCount,
                    EnableComments = post.EnableComments,
                    ReportsCount = post.ReportsCount,
                    Isadminposted = post.Isadminposted,
                    IsUserReported = reportedPostIds.Contains(post.Id),
                    UserHasLiked = likedSet.Contains(post.Id.ToString())
                };
            }).ToList();

            // Step 11: Final Response
            var result = new
            {
                CanPost = query.PostTypeId == 1 ? canPost : false,
                Posts = updatedPosts,
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                CommunityId = query.CommunityProductId,
                hasPurchasedProduct = hasSubscription,
                hasAccessToPremiumContent = hasAccessToPremiumContent
            };

            string message = hasAccessToPremiumContent
                     ? "Posts retrieved successfully."
                     : "Your subscription has expired. Please subscribe.";

            return CreateResponseStatusCode(HttpStatusCode.OK, message, result);
        }

        public async Task<ApiCommonResponseModel> GetV2(GetCommunityRequestModel query, long LoggedInUserId)
        {
            if (query.PageNumber <= 0 || query.PageSize <= 0)
            {
                query.PageNumber = 1;
                query.PageSize = 20;
            }

            var mobileUser = await _context.MobileUsers.FirstOrDefaultAsync(item => item.Id == LoggedInUserId);
            if (mobileUser == null)
            {
                return CreateResponseStatusCode(HttpStatusCode.NotFound, "User Not Found");
            }

            var productCommunityMapping = await _context.ProductCommunityMappingM
                .FirstOrDefaultAsync(b => b.CommunityId == query.CommunityProductId && b.IsActive);

            if (productCommunityMapping == null)
            {
                return CreateResponseStatusCode(HttpStatusCode.NotFound, "Community Not Found");
            }

            // Check if the user has purchased a related product before accessing the community
            bool hasPurchasedProductBeforeCommunityAccess = await _context.MyBucketM
                .AnyAsync(b => b.ProductId == productCommunityMapping.ProductId &&
                               b.MobileUserKey == mobileUser.PublicKey &&
                               b.IsActive);

            if (!hasPurchasedProductBeforeCommunityAccess)
            {
                var community = await _context.ProductsM.FirstOrDefaultAsync(c => c.Id == query.CommunityProductId && c.IsActive);

                _apiResponse.Data = new
                {
                    TotalCount = 0,
                    TotalPages = 0,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    CommunityId = query.CommunityProductId,
                    CommunityName = community?.Name,
                    hasPurchasedProduct = false,
                    hasAccessToPremiumContent = false
                };

                return CreateResponseStatusCode(HttpStatusCode.OK, "You do not have purchased strategy. Please subscribe strategy first.");
            }

            // Check if the user has purchased the community product (even if expired)
            var hasPurchasedCommunityProduct = await _context.MyBucketM
                .FirstOrDefaultAsync(b => b.ProductId == query.CommunityProductId &&
                                           b.MobileUserKey == mobileUser.PublicKey &&
                                           b.IsActive);

            // Check for subscription validity
            bool hasSubscription = hasPurchasedCommunityProduct != null;
            bool hasValidSubscription = hasPurchasedCommunityProduct?.EndDate.Value.Date >= DateTime.Now.Date;

            // If user does not have any subscription at all, deny access
            if (!hasSubscription)
            {
                var community = await _context.ProductsM.FirstOrDefaultAsync(c => c.Id == query.CommunityProductId && c.IsActive);

                _apiResponse.Data = new
                {
                    TotalCount = 0,
                    TotalPages = 0,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    CommunityId = query.CommunityProductId,
                    CommunityName = community?.Name,
                    hasPurchasedProduct = false,
                    hasAccessToPremiumContent = false
                };

                return CreateResponseStatusCode(HttpStatusCode.OK, "You do not have access to this community. Please subscribe.");
            }

            bool hasAccessToPremiumContent = query.PostTypeId == (int)CommunityPostTypeEnum.Post
                ? hasSubscription
                : hasValidSubscription;

            // Validate PostTypeId
            if (!Enum.IsDefined(typeof(CommunityPostTypeEnum), query.PostTypeId))
            {
                return CreateResponseStatusCode(HttpStatusCode.BadRequest, "Provide a valid PostTypeId");
            }

            // MongoDB Retrieval of Posts based on ProductId and PostTypeId
            // Step 1: Get CanPost from SQL ProductSM table
            var canPost = await _context.ProductsM
                .Where(p => p.Id == query.CommunityProductId)
                .Select(p => p.CanPost)
                .FirstOrDefaultAsync(); // This returns bool (true/false) or false if not found

            var currentUserObjectId = await GetMobileUserObjectIdAsync(query.LoggedInUserId); // Make sure to pass correct GUID

            // Step 2: Build MongoDB filter for fetching posts
            var filter = Builders<CommunityPost>.Filter.And(
                Builders<CommunityPost>.Filter.Eq(p => p.ProductId, query.CommunityProductId),
                Builders<CommunityPost>.Filter.Eq(p => p.PostTypeId, query.PostTypeId),
                Builders<CommunityPost>.Filter.Eq(p => p.IsActive, true),
                Builders<CommunityPost>.Filter.Eq(p => p.IsApproved, "Approved")
            );

            var skip = (query.PageNumber - 1) * query.PageSize;
            var imageUrlSuffix = _configuration["Azure:ImageUrlSuffix"];

            // Step 3: Fetch paginated posts from MongoDB
            var posts = await _communityPost.Find(filter)
                 .Sort(Builders<CommunityPost>.Sort.Descending(p => p.CreatedOn))
                 .Skip(skip)
                 .Limit(query.PageSize)
                 .ToListAsync();

            var sqlUserIds = posts.Where(p => string.IsNullOrEmpty(p.UserObjectId)).Select(p => p.CreatedBy).Distinct().ToList();
            var mongoUserIds = posts.Where(p => !string.IsNullOrEmpty(p.UserObjectId)).Select(p => p.UserObjectId).Distinct().ToList();

            var mobileUsers = await _context.Users
                 .Where(mu => sqlUserIds.Contains(mu.Id))
                 .Select(mu => new
                 {
                     mu.Id,
                     FullName = mu.FirstName + " " + mu.LastName,
                     mu.Gender,
                     mu.UserImage
                 })
                 .ToListAsync();

            var mongoUsers = await _userCollection
                .Find(Builders<Model.MongoDbCollection.User>.Filter.In(u => u.ObjectId, mongoUserIds))
                .Project(u => new
                {
                    u.ObjectId,
                    u.FullName,
                    u.Gender,
                    u.ProfileImage
                })
                .ToListAsync();

            var reportedBlogIds = await _blogReportCollection
            .Find(r => r.ReportedBy.ToString() == currentUserObjectId && r.Status)
                .Project(r => r.BlogId)
                .ToListAsync();
            var likedBlogs = await _likeCollection
                .Find(l => l.CreatedBy.ToString() == currentUserObjectId)
                .Project(l => l.BlogId)
                .ToListAsync();

            // Step 4: Project to DTO
            var updatedPosts = posts.Select(post =>
            {
                UserDto? user;

                if (string.IsNullOrEmpty(post.UserObjectId))
                {
                    var sqlUser = mobileUsers.FirstOrDefault(u => u.Id == post.CreatedBy);
                    user = sqlUser != null ? new UserDto
                    {
                        FullName = sqlUser.FullName,
                        Gender = sqlUser.Gender,
                        ProfileImage = sqlUser.UserImage
                    } : null;
                }
                else
                {
                    var mongoUser = mongoUsers.FirstOrDefault(u => u.ObjectId == post.UserObjectId);
                    user = mongoUser != null ? new UserDto
                    {
                        FullName = mongoUser.FullName,
                        Gender = mongoUser.Gender,
                        ProfileImage = mongoUser.ProfileImage
                    } : null;
                }

                var fullImageUrl = !string.IsNullOrEmpty(post.ImageUrl)
                    ? imageUrlSuffix + post.ImageUrl.Trim()
                    : null;

                var fullImageUrls = post.ImageUrls?.Select(img =>
                    new ImageModel
                    {
                        Name = !string.IsNullOrEmpty(img.Name) ? imageUrlSuffix + img.Name.Trim() : null,
                        AspectRatio = img.AspectRatio
                    }).ToList() ?? new List<ImageModel>();

                return new CommunityPostDto
                {
                    Id = post.Id,
                    ProductId = post.ProductId,
                    PostTypeId = post.PostTypeId,
                    Title = post.Title,
                    Content = post.Content,
                    CreatedBy = post.CreatedBy,
                    CreatedOn = post.CreatedOn,
                    ModifiedBy = post.ModifiedBy,
                    ModifiedOn = post.ModifiedOn,
                    Url = post.Url,
                    ImageUrls = fullImageUrls,    // use updated list with full URLs
                    //ImageUrl = fullImageUrl,
                    IsActive = post.IsActive,
                    IsDelete = post.IsDelete,
                    UpComingEvent = TimeZoneInfo.ConvertTimeFromUtc(
                        DateTime.SpecifyKind(post.UpComingEvent ?? DateTime.UtcNow, DateTimeKind.Utc),
                        TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"))
                        .ToString("yyyy-MM-ddTHH:mm:ss"),
                    IsQueryFormEnabled = post.IsQueryFormEnabled,
                    IsJoinNowEnabled = post.IsJoinNowEnabled,
                    IsApproved = post.IsApproved,
                    UserObjectId = string.IsNullOrEmpty(post.UserObjectId) ? null : post.UserObjectId,

                    // Add user details here
                    FullName = user?.FullName,
                    Gender = user?.Gender,
                    ProfileImage = user?.ProfileImage,
                    Likecount = post.Likecount,
                    CommentsCount = post.CommentsCount,
                    EnableComments = post.EnableComments,
                    Isadminposted = post.Isadminposted,
                    UserHasLiked = likedBlogs.Contains(post.Id), // Assuming you have a method to check if the user has liked the post
                };
            }).ToList();

            // Step 5: Count total posts
            var totalCount = await _communityPost.CountDocumentsAsync(filter);
            var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

            // Step 6: Final response
            var result = new
            {
                CanPost = query.PostTypeId == 1 ? canPost : false, // coming from SQL table
                Posts = updatedPosts,
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                CommunityId = query.CommunityProductId,
                hasPurchasedProduct = hasSubscription,
                hasAccessToPremiumContent = hasAccessToPremiumContent
            };

            string message = hasAccessToPremiumContent
                      ? "Posts retrieved successfully."
                      : "Your subscription has expired. Please subscribe.";

            return CreateResponseStatusCode(HttpStatusCode.OK, message, result);
        }
        // Only to get how many products community he has access to.
        // No Need to check the subscription date.
        public async Task<ApiCommonResponseModel> GetCommunityDetails(long mobileUserId)
        {
            List<SqlParameter> sqlParameters =
            [new SqlParameter { ParameterName = "MobileUserId", Value = mobileUserId, SqlDbType = SqlDbType.BigInt }];

            var data = await _context.SqlQueryToListAsync<CommunityDetailsResponse>(ProcedureCommonSqlParametersText.GetCommunityDetails, sqlParameters.ToArray());
            if (data is null)
            {
                return CreateResponseStatusCode(HttpStatusCode.OK, "No activity for this search found.");
            }

            var responseData = new
            {
                Data = data,
                PostTypeData = GetCommunityPostTypes()
            };

            return CreateResponseStatusCode(HttpStatusCode.OK, "Data fetched successfully.", responseData);
        }

        protected List<CommunityPostTypeResponse> GetCommunityPostTypes()
        {
            return Enum.GetValues(typeof(CommunityPostTypeEnum))
                .Cast<CommunityPostTypeEnum>()
                .Select(e => new CommunityPostTypeResponse
                {
                    Id = (int)e,
                    Name = e.ToString()
                })
                .ToList();
        }

        public List<CommunityPostTypeResponse> FetchCommunityPostTypes()
        {
            return GetCommunityPostTypes();
        }

        public async Task<ApiCommonResponseModel> LikeBlogCommunity(string userObjectId, string blogId, bool isLiked)
        {
            var blogLike = new Like
            {
                CreatedBy = userObjectId,
                CreatedOn = DateTime.UtcNow, // Prefer UTC for storage
                BlogId = blogId
            };

            return await LikeCommunityPost(blogLike, isLiked);
        }

        public async Task<ApiCommonResponseModel> LikeCommunityPost(Like blogLike, bool isLiked)
        {
            var postFilter = Builders<CommunityPost>.Filter.Eq(p => p.Id, blogLike.BlogId);
            var postExists = await _communityPost.Find(postFilter).AnyAsync();

            if (!postExists)
            {
                return CreateResponseStatusCode(HttpStatusCode.NotFound, "Blog doesn't exist", postExists);
            }

            var likeFilter = Builders<Like>.Filter.And(
                Builders<Like>.Filter.Eq(l => l.CreatedBy, blogLike.CreatedBy),
                Builders<Like>.Filter.Eq(l => l.BlogId, blogLike.BlogId)
            );

            var likeExists = await _likeCollection.Find(likeFilter).AnyAsync();

            if (isLiked)
            {
                if (likeExists)
                {
                    return CreateResponseStatusCode(HttpStatusCode.OK, "You have already liked this post.", true);
                }

                blogLike.CreatedOn = DateTime.UtcNow;
                await _communityPost.UpdateOneAsync(postFilter, Builders<CommunityPost>.Update.Inc(p => p.Likecount, 1));
                await _likeCollection.InsertOneAsync(blogLike);

                return CreateResponseStatusCode(HttpStatusCode.OK, "Post liked successfully.", blogLike);
            }

            // Unlike flow
            if (likeExists)
            {
                await _communityPost.UpdateOneAsync(postFilter, Builders<CommunityPost>.Update.Inc(p => p.Likecount, -1));
                var deleteResult = await _likeCollection.DeleteOneAsync(likeFilter);

                return CreateResponseStatusCode(HttpStatusCode.OK, "Like removed successfully.", deleteResult);
            }

            return CreateResponseStatusCode(HttpStatusCode.OK, "You haven't liked this post yet.", false);
        }

        public async Task<ApiCommonResponseModel> AddComment(PostCommentRequestModel obj)
        {
            // Check if blog exists and is active
            var blogFilter = Builders<CommunityPost>.Filter.And(
                Builders<CommunityPost>.Filter.Eq(b => b.Id, obj.BlogId),
                Builders<CommunityPost>.Filter.Eq(b => b.IsActive, true),
                Builders<CommunityPost>.Filter.Eq(b => b.IsDelete, false)
            );

            bool blogExists = await _communityPost.Find(blogFilter).AnyAsync();

            if (!blogExists)
            {
                return CreateResponseStatusCode(HttpStatusCode.NotFound, "Blog doesn't exist or is inactive/deleted.");
            }

            // Create the comment object
            var comment = new CommunityComments
            {
                ObjectId = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                CommunityPostId = obj.BlogId,
                Content = obj.Comment,
                Mention = obj.Mention,
                CreatedBy = obj.UserObjectId,
                CreatedOn = DateTime.Now,
                IsActive = true,
                IsDelete = false,
                ReplyCount = 0
            };

            // Insert comment
            await _communityComments.InsertOneAsync(comment);

            // Update comment count on post
            var update = Builders<CommunityPost>.Update.Inc(b => b.CommentsCount, 1);
            await _communityPost.UpdateOneAsync(blogFilter, update);

            // Return response
            return CreateResponseStatusCode(HttpStatusCode.OK, "Comment added successfully.");
        }

        public async Task<ApiCommonResponseModel> CommentReply(CommentReplyRequestModel request)
        {
            var commentFilter = Builders<CommunityComments>.Filter.Eq(c => c.ObjectId, request.CommentId);
            var comment = await _communityComments.Find(commentFilter).FirstOrDefaultAsync();

            if (comment == null)
            {
                return CreateResponseStatusCode(HttpStatusCode.InternalServerError, "An error occurred.");
            }

            var reply = new Reply
            {
                ObjectId = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                CommentId = request.CommentId,
                Content = request.Reply,
                Mention = request.Mention,
                CreatedBy = request.UserObjectId,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
                IsDelete = false
            };

            // Insert reply and update counts
            await _replyCollection.InsertOneAsync(reply);
            await _communityComments.UpdateOneAsync(commentFilter, Builders<CommunityComments>.Update.Inc(c => c.ReplyCount, 1));

            var postFilter = Builders<CommunityPost>.Filter.Eq(p => p.Id, comment.CommunityPostId);
            await _communityPost.UpdateOneAsync(postFilter, Builders<CommunityPost>.Update.Inc(p => p.CommentsCount, 1));

            return CreateResponseStatusCode(HttpStatusCode.OK, "Reply Added Successfully.");
        }

        public async Task<ApiCommonResponseModel> GetComments(string blogId, int pageNumber, int pageSize)
        {
            var data = await GetCommunityComments(blogId, pageNumber, pageSize);

            return CreateResponseStatusCode(HttpStatusCode.OK, "Data Fetched Successfully.", data);
        }

        public async Task<ApiCommonResponseModel> GetReplies(string commentId)
        {
            var data = await GetCommunityReplies(commentId);

            return CreateResponseStatusCode(HttpStatusCode.OK, "Replies fetched successfully", data);
        }

        public async Task<ApiCommonResponseModel> EditCommentOrReply(EditCommentOrReplyRequestModel request)
        {
            var editedInMongo = await EditCommunityCommentOrReply(request.ObjectId, request.UserObjectId, request.NewContent, request.NewMention, request.Type);

            if (!editedInMongo)
            {
                return CreateResponseStatusCode(HttpStatusCode.InternalServerError, "An error occurred.");
            }

            return CreateResponseStatusCode(HttpStatusCode.OK, $"{request.Type} edited successfully.");
        }

        public async Task<ApiCommonResponseModel> DeleteCommentOrReply(string objectId, string userObjectId, string type)
        {
            var saveToMongo = await DeleteCommunityCommentOrReply(objectId, userObjectId, type);
            if (saveToMongo)
            {
                return CreateResponseStatusCode(HttpStatusCode.NoContent, "Delete Successfull.");
            }
            else
            {
                return CreateResponseStatusCode(HttpStatusCode.InternalServerError, "Couldn't delete.");
            }
        }

        public async Task<ApiCommonResponseModel> BlockUser(BlockUserRequestModel request)
        {
            ApiCommonResponseModel blockStatus = await BlockCommunityUser(request);
            return blockStatus;
        }

        public async Task<ApiCommonResponseModel> DisableBlogComment(DisableBlogCommentRequestModel request)
        {
            var mongoOperation = await DisableCommunityComment(request);

            if (mongoOperation)
            {
                return CreateResponseStatusCode(HttpStatusCode.OK, "Successful");
            }
            else
            {
                return CreateResponseStatusCode(HttpStatusCode.InternalServerError, "An error occurred while disabling comments");
            }
        }

        public async Task<ApiCommonResponseModel> ReportBlog(ReportBlogRequestModel request, int mobileUserId)
        {
            var reportStatus = await ReportCommunity(request, mobileUserId);

            if (reportStatus.StatusCode == HttpStatusCode.OK)
            {
                return CreateResponseStatusCode(HttpStatusCode.OK, "Report Successful.");
            }
            else if (reportStatus.StatusCode == HttpStatusCode.Accepted)
            {
                // Disabled by instruction from Vijay sir
                // await DisableCommunityPostForUser(Guid.Parse(reportStatus.Data.ToString()!));

                MobileUser? user = await _context.MobileUsers.FirstOrDefaultAsync(u => u.PublicKey == Guid.Parse(reportStatus.Data.ToString()!));

                if (user != null)
                {
                    var notificationToMobileRequestModel = new NotificationToMobileRequestModel
                    {
                        Body = "You have been restricted from using the community due to excessive reports.",
                        Mobile = user.Mobile,
                        Title = "Community Post Access Restricted",
                        Topic = "Announcement"
                    };

                    _ = await _mobileNotificationService.Value.SendNotificationToMobile(notificationToMobileRequestModel);
                }

                return CreateResponseStatusCode(HttpStatusCode.OK, "Blog Reported Successfully.");
            }

            return CreateResponseStatusCode(reportStatus.StatusCode, reportStatus.Message);
        }

        public async Task<ApiCommonResponseModel> DisableCommunityPostForUser(Guid userKey)
        {
            MobileUser? user = await _context.MobileUsers.FirstOrDefaultAsync(u => u.PublicKey == userKey);

            if (user == null)
            {
                return CreateResponseStatusCode(HttpStatusCode.NotFound, "User Not Found.");
            }

            user.CanCommunityPost = false;
            user.ModifiedOn = DateTime.Now;
            _ = await _context.SaveChangesAsync();

            return CreateResponseStatusCode(HttpStatusCode.OK, "Successful.");
        }

        /// <summary>
        /// Helper Methods for Community Post
        /// </summary>

        private async Task<string?> GetMobileUserObjectIdAsync(Guid? loggedInUser)
        {
            var mobile = await _context.MobileUsers
                .Where(b => b.PublicKey == loggedInUser)
                .Select(b => b.Mobile)
                .FirstOrDefaultAsync();

            loggedInUser = await _context.MobileUsers
                .Where(b => b.Mobile == mobile)
                .Select(b => b.PublicKey)
                .FirstOrDefaultAsync();

            var user = await _userCollection
               .Find(x => x.PublicKey == loggedInUser)
               .FirstOrDefaultAsync();

            return user?.ObjectId;
        }

        private async Task<MobileUser> GetMobileUserAsync(long mobileUserId)
        {
            return await _context.MobileUsers.FindAsync(mobileUserId);
        }

        private async Task<Model.MongoDbCollection.User> GetMongoUserByPublicKeyAsync(Guid publicKey)
        {
            return await _userCollection.Find(x => x.PublicKey == publicKey).FirstOrDefaultAsync();
        }

        private async Task<CommunityPost> BuildNewCommunityPostAsync(CreateCommunity request, int mobileUserId, string objectId)
        {
            var newPost = new CommunityPost
            {
                ProductId = request.ProductId,
                PostTypeId = 1,
                Url = request.Url ?? " ",
                Title = request.Title,
                Content = request.Content,
                CreatedBy = mobileUserId,
                CreatedOn = DateTime.Now,
                ModifiedOn = DateTime.Now,
                ModifiedBy = mobileUserId,
                IsActive = true,
                IsApproved = "Pending",
                IsDelete = false,
                UserObjectId = objectId ?? string.Empty,
                Likecount = 0,
                Isadminposted = false,
                CommentsCount = 0,
                ReportsCount = 0,
                IsJoinNowEnabled = false,
                IsQueryFormEnabled = false,
                EnableComments = true
            };

            // Upload main image if present
            if (request.ImageUrl != null)
                newPost.ImageUrl = await _azureBlobStorageService.UploadImage(request.ImageUrl);

            // Upload image gallery
            if (request.Images?.Any() == true)
            {
                for (int i = 0; i < request.Images.Count; i++)
                {
                    var imageFile = request.Images[i];
                    var aspectRatio = (request.AspectRatios != null && request.AspectRatios.Count > i)
                        ? request.AspectRatios[i]
                        : "auto";

                    var uploadedFileName = await _azureBlobStorageService.UploadImage(imageFile);
                    newPost.ImageUrls.Add(new ImageModel
                    {
                        Name = uploadedFileName,
                        AspectRatio = aspectRatio
                    });

                    if (i == 0 && string.IsNullOrEmpty(newPost.ImageUrl))
                        newPost.ImageUrl = uploadedFileName;
                }
            }

            return newPost;
        }

        private object CreatePostResponse(CommunityPost post, Model.MongoDbCollection.User mongoUser, string imageUrlSuffix, bool? hasUserLiked)
        {
            return new
            {
                Id = post.Id,
                post.Title,
                post.Content,
                post.Url,
                ImageUrl = !string.IsNullOrEmpty(post.ImageUrl)
                     ? imageUrlSuffix + post.ImageUrl.Trim()
                     : null,
                ImageUrls = post.ImageUrls?.Select(img => new
                {
                    Name = imageUrlSuffix + img.Name.Trim(),
                    img.AspectRatio
                }),
                post.CreatedOn,
                post.CreatedBy,
                post.ModifiedOn,
                post.ModifiedBy,
                post.UserObjectId,
                post.ProductId,
                post.PostTypeId,
                post.IsApproved,
                post.CommentsCount,
                post.Likecount,
                post.Isadminposted,
                post.IsActive,
                post.IsDelete,
                post.EnableComments,
                post.IsQueryFormEnabled,
                post.IsJoinNowEnabled,
                FullName = mongoUser?.FullName,
                ProfileImage = mongoUser?.ProfileImage,
                Gender = mongoUser?.Gender,
                UserHasLiked = hasUserLiked
            };
        }

        private async Task<ApiCommonResponseModel> HandlePostUpdateAsync(CreateCommunity request, long mobileUserId)
        {
            var filter = Builders<CommunityPost>.Filter.Eq(p => p.Id, request.Id);
            var updates = new List<UpdateDefinition<CommunityPost>>();

            // Conditional updates
            AddIfNotEmpty(updates, request.Title, p => p.Title);
            AddIfNotEmpty(updates, request.Content, p => p.Content);
            AddIfNotEmpty(updates, request.Url, p => p.Url);

            // Mandatory metadata updates
            updates.Add(Builders<CommunityPost>.Update.Set(p => p.ModifiedOn, DateTime.UtcNow));
            updates.Add(Builders<CommunityPost>.Update.Set(p => p.ModifiedBy, mobileUserId));

            // If nothing to update
            if (updates.Count == 2) // only ModifiedOn & ModifiedBy were added
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "No valid fields to update."
                };
            }

            var combinedUpdate = Builders<CommunityPost>.Update.Combine(updates);

            var updatedPost = await _communityPost.FindOneAndUpdateAsync(
                filter,
                combinedUpdate,
                new FindOneAndUpdateOptions<CommunityPost>
                {
                    ReturnDocument = ReturnDocument.After
                });

            if (updatedPost == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Post not found"
                };
            }

            // Prepare user data (if available)
            var mongoUser = !string.IsNullOrEmpty(updatedPost.UserObjectId)
                ? await _userCollection.Find(u => u.ObjectId == updatedPost.UserObjectId).FirstOrDefaultAsync()
                : null;

            var imageUrlSuffix = _configuration["Azure:ImageUrlSuffix"];
            var currentMobileUser = await GetMobileUserAsync((int)mobileUserId);
            var currentMongoUser = await GetMongoUserByPublicKeyAsync(currentMobileUser.PublicKey);
            var currentObjectId = currentMongoUser?.PublicKey;

            bool hasUserLiked = false;
            if (!string.IsNullOrEmpty(currentObjectId.ToString()))
            {
                hasUserLiked = await _likeCollection
                    .Find(x => x.CreatedBy == currentObjectId.ToString() && x.BlogId == updatedPost.Id)
                    .AnyAsync();
            }

            var response = CreatePostResponse(updatedPost, mongoUser, imageUrlSuffix, hasUserLiked);

            return CreateResponseStatusCode(HttpStatusCode.OK, "Post updated successfully",response);
        }

        private async Task<ApiCommonResponseModel> HandlePostDeleteAsync(string postId, long mobileUserId)
        {
            var filter = Builders<CommunityPost>.Filter.Eq(p => p.Id, postId);
            var update = Builders<CommunityPost>.Update
                .Set(p => p.IsDelete, true)
                .Set(p => p.IsActive, false)
                .Set(p => p.ModifiedOn, DateTime.Now)
                .Set(p => p.ModifiedBy, mobileUserId);

            await _communityPost.UpdateOneAsync(filter, update);

            return CreateResponseStatusCode(HttpStatusCode.OK, "Post deleted successfully");
        }

        private void AddIfNotEmpty(List<UpdateDefinition<CommunityPost>> updates, string value, Expression<Func<CommunityPost, string>> field)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                updates.Add(Builders<CommunityPost>.Update.Set(field, value));
            }
        }

        private async Task<ApiCommonResponseModel> ReportCommunity(ReportBlogRequestModel request, int mobileUserId)
        {
            try
            {
                var community = await _communityPost.Find(b => b.Id == request.BlogId).FirstOrDefaultAsync();
                if (community == null)
                {
                    return CreateResponseStatusCode(HttpStatusCode.NotFound, "Community post not found");
                }

                var report = new CommunityReport
                {
                    Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                    CommunityId = request.BlogId,
                    ReportedBy = MongoDB.Bson.ObjectId.Parse(request.ReportedBy),
                    ReasonId = MongoDB.Bson.ObjectId.Parse(request.ReasonId),
                    Description = request.Description,
                    CreatedOn = DateTime.UtcNow,
                    Status = true
                };

                await _communityReportCollection.InsertOneAsync(report);

                var update = Builders<CommunityPost>.Update
                    .Inc(b => b.ReportsCount, 1)
                    .Set(b => b.ModifiedBy, mobileUserId)
                    .Set(b => b.ModifiedOn, DateTime.Now);

                var updateResult = await _communityPost.UpdateOneAsync(
                    b => b.Id == request.BlogId,
                    update
                );

                if (updateResult.ModifiedCount == 0)
                {
                    _logger.LogWarning("Blog {BlogId} not found or not updated", request.BlogId);
                    return CreateResponseStatusCode(HttpStatusCode.NotFound, "Blog not found or could not be updated.");
                }

                // Get all blogs by the creator
                var creatorBlogs = await _communityPost
                    .Find(b => b.UserObjectId == request.ReportedBy)
                    .ToListAsync();

                // Count total reports across all blogs by the creator
                var creatorId = community.UserObjectId;
                var blogIds = await _communityPost
                    .Find(b => b.UserObjectId == creatorId)
                    .Project(b => b.Id)
                    .ToListAsync();

                if (!blogIds.Any())
                    return CreateResponseStatusCode(HttpStatusCode.OK, "Report successful");

                var totalReports = await _communityReportCollection
                    .CountDocumentsAsync(r => blogIds.Contains(r.CommunityId.ToString()));

                if (totalReports >= 5 && totalReports < 6)
                {
                    // get the public key of the user
                    var user = await _userCollection
                        .Find(b => b.ObjectId == creatorId)
                        .FirstOrDefaultAsync();

                    _logger.LogInformation(
                        "Blog creator has received 5 or more total reports across all blogs. BlogId: {BlogId}, CreatorId: {CreatorId}",
                        request.BlogId, community.CreatedBy);
                    return CreateResponseStatusCode(HttpStatusCode.Accepted, "Excessive reports detected", user?.PublicKey);
                }

                return CreateResponseStatusCode(HttpStatusCode.OK, "Report successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reporting blog {BlogId} by user {UserId}", request.BlogId,
                    request.ReportedBy);
                return CreateResponseStatusCode(HttpStatusCode.InternalServerError, "An error occurred while reporting.");
            }
        }

        private async Task<bool> DisableCommunityComment(DisableBlogCommentRequestModel request)
        {
            var filter = Builders<CommunityPost>.Filter.And(
                Builders<CommunityPost>.Filter.Eq("userObjectId", MongoDB.Bson.ObjectId.Parse(request.CreatedByObjectId)),
                Builders<CommunityPost>.Filter.Eq("_id", MongoDB.Bson.ObjectId.Parse(request.BlogId))
            );

            var existingBlog = await _communityPost.Find(filter).FirstOrDefaultAsync();

            if (existingBlog != null)
            {
                var currentEnableComments = existingBlog.EnableComments;
                var invertedEnableComments = !currentEnableComments;

                var update = Builders<CommunityPost>.Update
                    .Set("EnableComments", invertedEnableComments)
                    .Set("ModifiedOn", DateTime.Now);

                var result = await _communityPost.UpdateOneAsync(filter, update);

                return result.ModifiedCount > 0;
            }
            else return false;
        }

        private async Task<ApiCommonResponseModel> BlockCommunityUser(BlockUserRequestModel request)
        {
            var user = await _userCollection.Find(x => x.PublicKey == request.UserKey).FirstOrDefaultAsync();
            var userId = user?.ObjectId;

            if (userId == request.BlockedId)
            {
                return CreateResponseStatusCode(HttpStatusCode.Forbidden, "Cannot block themselves");
            }

            if (string.Equals(request.Type, "BLOCK", StringComparison.OrdinalIgnoreCase))
            {
                var existingBlock = await _userBlockCollection.Find(x =>
                    x.BlockerId == MongoDB.Bson.ObjectId.Parse(userId) &&
                    x.BlockedId == MongoDB.Bson.ObjectId.Parse(request.BlockedId) &&
                    x.IsActive == true).FirstOrDefaultAsync();

                if (existingBlock != null)
                {
                    return CreateResponseStatusCode(HttpStatusCode.BadRequest, "User is already blocked");
                }

                var block = new UserBlock
                {
                    BlockerId = MongoDB.Bson.ObjectId.Parse(userId),
                    BlockedId = MongoDB.Bson.ObjectId.Parse(request.BlockedId),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _userBlockCollection.InsertOneAsync(block);
                return CreateResponseStatusCode(HttpStatusCode.OK, "User blocked successfully");
            }

            if (string.Equals(request.Type, "UNBLOCK", StringComparison.OrdinalIgnoreCase))
            {
                var filter = Builders<UserBlock>.Filter.And(
                    Builders<UserBlock>.Filter.Eq(x => x.BlockerId, MongoDB.Bson.ObjectId.Parse(userId)),
                    Builders<UserBlock>.Filter.Eq(x => x.BlockedId, MongoDB.Bson.ObjectId.Parse(request.BlockedId)),
                    Builders<UserBlock>.Filter.Eq(x => x.IsActive, true)
                );

                var update = Builders<UserBlock>.Update.Set(x => x.IsActive, false);
                var result = await _userBlockCollection.UpdateOneAsync(filter, update);

                return result.ModifiedCount > 0
                    ? CreateResponseStatusCode(HttpStatusCode.OK, "User unblocked successfully")
                    : CreateResponseStatusCode(HttpStatusCode.NotFound, "No active block found for this user");
            }

            return CreateResponseStatusCode(HttpStatusCode.BadRequest, "Invalid block type specified");
        }

        private async Task<bool> DeleteCommunityCommentOrReply(string objectId, string userObjectId, string type)
        {
            if (string.IsNullOrWhiteSpace(type)) return false;

            type = type.Trim().ToUpperInvariant();

            if (type == "COMMENT")
            {
                var commentFilter = Builders<CommunityComments>.Filter.And(
                    Builders<CommunityComments>.Filter.Eq(c => c.ObjectId, objectId),
                    Builders<CommunityComments>.Filter.Eq(c => c.CreatedBy, userObjectId)
                );

                var update = Builders<CommunityComments>.Update
                    .Set(c => c.IsActive, false)
                    .Set(c => c.IsDelete, true)
                    .Set(c => c.ModifiedOn, DateTime.Now);

                var comment = await _communityComments.FindOneAndUpdateAsync(commentFilter, update);

                if (comment == null) return false;

                var totalDecrement = 1;

                if (comment.ReplyCount > 0)
                {
                    var replyFilter = Builders<Reply>.Filter.Eq(r => r.CommentId, comment.ObjectId);
                    var replyUpdate = Builders<Reply>.Update
                        .Set(r => r.IsActive, false)
                        .Set(r => r.IsDelete, true)
                        .Set(r => r.ModifiedOn, DateTime.Now);

                    await _replyCollection.UpdateManyAsync(replyFilter, replyUpdate);

                    totalDecrement += comment.ReplyCount;

                    // Reset reply count in comment
                    await _communityComments.UpdateOneAsync(
                        Builders<CommunityComments>.Filter.Eq(c => c.ObjectId, comment.ObjectId),
                        Builders<CommunityComments>.Update.Set(c => c.ReplyCount, 0)
                    );
                }

                await DecrementPostCommentCount(comment.CommunityPostId, totalDecrement);
                return true;
            }
            else if (type == "REPLY")
            {
                var replyFilter = Builders<Reply>.Filter.And(
                    Builders<Reply>.Filter.Eq(r => r.ObjectId, objectId),
                    Builders<Reply>.Filter.Eq(r => r.CreatedBy, userObjectId)
                );

                var replyUpdate = Builders<Reply>.Update
                    .Set(r => r.IsActive, false)
                    .Set(r => r.IsDelete, true)
                    .Set(r => r.ModifiedOn, DateTime.Now);

                var reply = await _replyCollection.FindOneAndUpdateAsync(replyFilter, replyUpdate);
                if (reply == null) return false;

                // Decrease reply count in comment
                var commentFilter = Builders<CommunityComments>.Filter.Eq(c => c.ObjectId, reply.CommentId);

                var comment = await _communityComments.FindOneAndUpdateAsync(
                    commentFilter,
                    Builders<CommunityComments>.Update.Inc(c => c.ReplyCount, -1)
                );

                if (comment != null)
                {
                    await DecrementPostCommentCount(comment.CommunityPostId, 1);
                }

                return true;
            }

            return false;
        }

        // Shared method to safely decrement post comment count
        private async Task DecrementPostCommentCount(string postId, int count)
        {
            var postFilter = Builders<CommunityPost>.Filter.Eq(p => p.Id, postId);
            var post = await _communityPost.FindOneAndUpdateAsync(
                postFilter,
                Builders<CommunityPost>.Update.Inc(p => p.CommentsCount, -count)
            );

            // Ensure count doesn't go negative
            if (post != null && post.CommentsCount - count < 0)
            {
                await _communityPost.UpdateOneAsync(
                    postFilter,
                    Builders<CommunityPost>.Update.Set(p => p.CommentsCount, 0)
                );
            }
        }

        private async Task<bool> EditCommunityCommentOrReply(string objectId, string userObjectId, string newContent, string newMention, string type)
        {
            var normalizedType = type?.Trim().ToUpperInvariant();
            var timeThreshold = DateTime.UtcNow.AddMinutes(-30);
            DateTime modifiedOn = DateTime.Now;

            if (normalizedType == "COMMENT")
            {
                var filter = Builders<CommunityComments>.Filter.And(
                    Builders<CommunityComments>.Filter.Eq(c => c.ObjectId, objectId),
                    Builders<CommunityComments>.Filter.Eq(c => c.CreatedBy, userObjectId),
                    Builders<CommunityComments>.Filter.Eq(c => c.IsActive, true),
                    Builders<CommunityComments>.Filter.Gte(c => c.CreatedOn, timeThreshold)
                );

                var update = Builders<CommunityComments>.Update
                    .Set(c => c.Content, newContent)
                    .Set(c => c.Mention, newMention)
                    .Set(c => c.ModifiedOn, modifiedOn);

                var result = await _communityComments.FindOneAndUpdateAsync(filter, update);
                return result != null;
            }

            if (normalizedType == "REPLY")
            {
                var filter = Builders<Reply>.Filter.And(
                    Builders<Reply>.Filter.Eq(r => r.ObjectId, objectId),
                    Builders<Reply>.Filter.Eq(r => r.CreatedBy, userObjectId),
                    Builders<Reply>.Filter.Eq(r => r.IsActive, true),
                    Builders<Reply>.Filter.Gte(r => r.CreatedOn, timeThreshold)
                );

                var update = Builders<Reply>.Update
                    .Set(r => r.Content, newContent)
                    .Set(r => r.Mention, newMention)
                    .Set(r => r.ModifiedOn, modifiedOn);

                var result = await _replyCollection.FindOneAndUpdateAsync(filter, update);
                return result != null;
            }

            return false;
        }

        public ApiCommonResponseModel CreateResponseStatusCode(HttpStatusCode statusCode, string message, object? data = null, int? total = null)
        {
            return new ApiCommonResponseModel
            {
                StatusCode = statusCode,
                Message = message,
                Data = data,
                Total = total ?? 0
            };
        }

        private async Task<List<GetRepliesResponseModel>?> GetCommunityReplies(string commentId)
        {

            var aggregationPipeline = await _replyCollection.Aggregate()
             .Match(new MongoDB.Bson.BsonDocument
             {
                { "CommentId", new MongoDB.Bson.ObjectId(commentId) },
                { "IsDelete", false },
                { "IsActive", true }
             })
             .Sort(new MongoDB.Bson.BsonDocument("CreatedOn", -1))
             .Lookup("User", "CreatedBy", "_id", "userData")
             .Unwind("userData")

             // Join with CommunityComments to get CommunityPostId
             .Lookup("CommunityComments", "CommentId", "_id", "commentData")
             .Unwind("commentData")

             // Join with CommunityPost to get the BlogId
             .Lookup("CommunityPost", "commentData.CommunityPostId", "_id", "postData")
             .Unwind("postData")

             .Project(new MongoDB.Bson.BsonDocument
             {
                { "_id", 0 },
                { "ObjectId", new MongoDB.Bson.BsonDocument("$toString", "$_id") },
                { "CommentId", new MongoDB.Bson.BsonDocument("$toString", "$CommentId") },
                { "Content", 1 },
                { "Mention", 1 },
                { "CreatedBy", new MongoDB.Bson.BsonDocument("$toString", "$CreatedBy") },
                { "CreatedOn", new MongoDB.Bson.BsonDocument {
                    { "$dateToString", new MongoDB.Bson.BsonDocument {
                        { "format", "%d-%m-%Y %H:%M:%S" },
                        { "date", "$CreatedOn" }
                    } }
                }},
                { "UserFullName", "$userData.FullName" },
                { "UserProfileImage", "$userData.ProfileImage" },
                { "Gender", "$userData.Gender" },

                //  Include BlogId from CommunityPost
                { "BlogId", new MongoDB.Bson.BsonDocument("$toString", "$postData._id") }
             })
             .ToListAsync();

            if (aggregationPipeline.Count == 0)
            {
                return new List<GetRepliesResponseModel>();
            }

            var aggregationJson =
                System.Text.Json.JsonSerializer
                    .Deserialize<List<RepliesAggregationModel>>(aggregationPipeline.ToJson());

            string format = "dd-MM-yyyy HH:mm:ss";

            var repliesList = new List<GetRepliesResponseModel>();

            foreach (var item in aggregationJson)
            {
                //var user = await _userCollection.Find(u => u.ObjectId == item.CreatedBy)
                //                                 .Project(u => new { u.FullName, u.ProfileImage, u.Gender })
                //                                 .FirstOrDefaultAsync();
                var combinedReply = new GetRepliesResponseModel()
                {
                    ObjectId = item.ObjectId,
                    BlogId = item.BlogId,
                    CommentId = item.CommentId,
                    Content = item.Content,
                    Mention = item.Mention,
                    CreatedBy = item.CreatedBy,
                    PostedAgo = GetRelativeTimeSincePost.GetRelativeTimeSincePostedForCommentsAndReply(
                        DateTime.ParseExact(item.CreatedOn, format, null)),
                    CreatedOn = UtcToIstDateTime.UtcStringToIst(item.CreatedOn),
                    UserFullName = item?.UserFullName,
                    Gender = item?.Gender,
                    UserProfileImage = _configuration["Azure:ImageUrlSuffix"] + item?.UserProfileImage,
                };

                // Add the custom object to the list
                repliesList.Add(combinedReply);
            }

            return repliesList;
        }

        private async Task<Dictionary<string, Model.MongoDbCollection.User>> GetUserDetailsByObjectIdsAsync(IEnumerable<string> objectIds)
        {
            var filter = Builders<Model.MongoDbCollection.User>.Filter.In(u => u.ObjectId, objectIds);
            var users = await _userCollection.Find(filter).ToListAsync();

            var distinctUsers = users
                .GroupBy(u => u.ObjectId) // Use ObjectId for grouping
                .Select(g => g.First())
                .ToDictionary(u => u.ObjectId, u => u);

            return distinctUsers;
        }

        private async Task<List<GetCommentsResponseModel>?> GetCommunityComments(string blogId, int pageNumber, int pageSize)
        {
            //WITH AGGREGATION PIPELINE to
            var aggregationPipeline = await _communityComments.Aggregate()
                .Match(new MongoDB.Bson.BsonDocument
                {
                    {
                        "CommunityPostId",
                        new MongoDB.Bson.ObjectId(blogId)
                    },
                    {
                        "IsDelete",
                        false
                    },
                    {
                        "IsActive",
                        true
                    }
                })
                .Sort(new MongoDB.Bson.BsonDocument
                {
                    {
                        "CreatedOn",
                        -1
                    }
                })
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .Lookup("User", "CreatedBy", "_id", "userData")
                .Project(
                    new MongoDB.Bson.BsonDocument
                    {
                        {
                            "_id",
                            0
                        },
                        {
                            "BlogId",
                            new MongoDB.Bson.BsonDocument("$toString", "$CommunityPostId")
                        },
                        {
                            "ObjectId",
                            new MongoDB.Bson.BsonDocument("$toString", "$_id")
                        },
                        {
                            "Content",
                            1
                        },
                        {
                            "Mention",
                            1
                        },
                        {
                            "CreatedBy",
                            new MongoDB.Bson.BsonDocument("$toString", "$CreatedBy")
                        },
                        {
                            "CreatedOn",
                            new MongoDB.Bson.BsonDocument
                            {
                                {
                                    "$dateToString",
                                    new MongoDB.Bson.BsonDocument
                                    {
                                        {
                                            "format",
                                            "%d-%m-%Y %H:%M:%S"
                                        },
                                        {
                                            "date",
                                            "$CreatedOn"
                                        }
                                    }
                                }
                            }
                        },
                        {
                            "ReplyCount",
                            new MongoDB.Bson.BsonDocument("$toInt", "$ReplyCount")
                        },
                        {
                            "UserFullName",
                            new MongoDB.Bson.BsonDocument("$arrayElemAt", new MongoDB.Bson.BsonArray
                            {
                                "$userData.FullName",
                                0
                            })
                        },
                        {
                            "Gender",
                            new MongoDB.Bson.BsonDocument("$arrayElemAt", new MongoDB.Bson.BsonArray
                            {
                                "$userData.Gender",
                                0
                            })
                        },
                        {
                            "UserProfileImage",
                            new MongoDB.Bson.BsonDocument("$arrayElemAt", new MongoDB.Bson.BsonArray
                            {
                                "$userData.ProfileImage",
                                0
                            })
                        },
                    })
                .ToListAsync();

            if (aggregationPipeline.Count == 0)
            {
                return new List<GetCommentsResponseModel>();
            }

            var aggregationJson =
                System.Text.Json.JsonSerializer
                    .Deserialize<List<CommentAggregationModel>>(aggregationPipeline.ToJson());

            var publicKeys = aggregationJson.Select(a => a.CreatedBy).Distinct().ToList();
            var userMap = await GetUserDetailsByObjectIdsAsync(publicKeys);

            string format = "dd-MM-yyyy HH:mm:ss";

            var commentsList = new List<GetCommentsResponseModel>();

            foreach (var item in aggregationJson)
            {
                userMap.TryGetValue(item.CreatedBy, out var user);

                var combinedComment = new GetCommentsResponseModel()
                {
                    ObjectId = item.ObjectId,
                    BlogId = item.BlogId,
                    Content = item.Content,
                    Mention = item.Mention,
                    CreatedBy = user?.ObjectId,
                    PostedAgo = GetRelativeTimeSincePost.GetRelativeTimeSincePostedForCommentsAndReply(DateTime.ParseExact(item.CreatedOn, format, null)),
                    CreatedOn = UtcToIstDateTime.UtcStringToIst(item.CreatedOn),
                    ReplyCount = item.ReplyCount,
                    UserFullName = user?.FullName,
                    Gender = user?.Gender,
                    UserProfileImage = user != null
                            ? _configuration["Azure:ImageUrlSuffix"] + user.ProfileImage
                            : null
                };

                // Add the custom object to the list
                commentsList.Add(combinedComment);
            }

            return commentsList;
        }

    }
}
