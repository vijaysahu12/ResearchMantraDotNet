using RM.BlobStorage;
using RM.CommonService;
using RM.CommonService.Helpers;
using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.ResearchMantraContext;
using RM.Database.ResearchMantraContext.Tables;
using RM.Database.MongoDbContext;
using RM.Model;
using RM.Model.Common;
using RM.Model.MongoDbCollection;
using RM.Model.RequestModel;
using RM.Model.RequestModel.Notification;
using RM.Model.ResponseModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using System.Net;
using Blog = RM.Model.MongoDbCollection.Blog;
using Log = RM.Model.MongoDbCollection.Log;
using PushNotification = RM.Model.MongoDbCollection.PushNotification;
using User = RM.Model.MongoDbCollection.User;
using UserActivity = RM.Model.MongoDbCollection.UserActivity;

namespace RM.CommonServices.Services
{
    public class MongoDbService
    {
        //private readonly IMongoCollection<MobileUserNotificationCollection> _userNotificationCollection;
        private readonly IMongoDatabase _database;
        private readonly IMemoryCache _cache;
        private const int CACHE_DURATION_MINUTES = 15;

        private readonly IAzureBlobStorageService _azureBlobStorageService;
        private readonly ILogger<MongoDbService> _logger;
        private readonly IMongoCollection<Blog> _blogCollection;
        private readonly IMongoCollection<CommunityPost> _commnunitypostCollection;
        private readonly IMongoCollection<MobileUserSelfDeleteData> _mobileUserSelfDeleteDataCollection;
        private readonly IMongoCollection<Comment> _commentCollection;
        private readonly IMongoCollection<CommunityComments> _communityComments;
        private readonly IMongoCollection<Reply> _replyCollection;
        private readonly IMongoCollection<Like> _likeCollection;
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<PushNotification> _pushNotificationCollection;
        private readonly IMongoCollection<PushNotificationReceiver> _pushNotificationCollectionReceiver;
        private readonly IMongoCollection<Log> _log;
        private readonly IMongoCollection<UserActivity> _userActivityCollection;
        private readonly IMongoCollection<BlogReport> _blogReportCollection;
        private readonly IMongoCollection<ReportReason> _reportReasonCollection;
        private readonly IMongoCollection<UserBlock> _userBlockCollection;
        private readonly IMongoCollection<UserVersionReport> _userVersion;
        private readonly ResearchMantraContext _context;
        private readonly CommunityPostService _communityPostService;

        private readonly IConfiguration _configuration;

        public MongoDbService(IOptions<MongoDBSettings> mongoDBSettings,
            IMemoryCache cache,
            ILogger<MongoDbService> logger, IConfiguration configuration, ResearchMantraContext context,
            IAzureBlobStorageService azureBlobStorageService, CommunityPostService communityPostService)
        {
            MongoClient client = new(mongoDBSettings.Value.ConnectionURI);
            _database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _logger = logger;
            _cache = cache;
            _configuration = configuration;
            _blogCollection = _database.GetCollection<Blog>("Blog");
            _commnunitypostCollection = _database.GetCollection<CommunityPost>("CommunityPost");
            _mobileUserSelfDeleteDataCollection = _database.GetCollection<MobileUserSelfDeleteData>("MobileUserSelfDeleteData");
            _replyCollection = _database.GetCollection<Reply>("Reply");
            _commentCollection = _database.GetCollection<Comment>("Comment");
            _communityComments = _database.GetCollection<CommunityComments>("CommunityComments");
            _likeCollection = _database.GetCollection<Like>("Like");
            _userCollection = _database.GetCollection<User>("User");
            _pushNotificationCollection = _database.GetCollection<PushNotification>("PushNotification");
            _pushNotificationCollectionReceiver = _database.GetCollection<PushNotificationReceiver>("PushNotificationReceiver");
            _database.GetCollection<PushNotificationReceiver>("PushNotificationReceiver");
            _userActivityCollection = _database.GetCollection<UserActivity>("UserActivity");
            _blogReportCollection = _database.GetCollection<BlogReport>("BlogReport");
            _reportReasonCollection = _database.GetCollection<ReportReason>("ReportReason");
            _log = _database.GetCollection<Log>("Log");
            _userBlockCollection = _database.GetCollection<UserBlock>("UserBlock");
            _userVersion = _database.GetCollection<UserVersionReport>("UserVersionReport");
            _context = context;
            _communityPostService = communityPostService;
            _azureBlobStorageService = azureBlobStorageService;

        }

        public async Task<bool> InsertPushNotification(CRMPushNotificationCollection pushNotification)
        {
            try
            {
                var collection =
                    _database.GetCollection<CRMPushNotificationCollection>(
                        _configuration["MongoDB:CRMPushNotificationCollection"]);
                await collection.InsertOneAsync(pushNotification);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //public async Task<ApiCommonResponseModel> GetException(QueryValues query)
        //{
        //    // Calculate the number of documents to skip
        //    int skip = (query.PageNumber - 1) * query.PageSize;

        //    // Query the database with skip and limit
        //    var collection = _database.GetCollection<CRMPushNotificationCollection>(_configuration["MongoDB:CRMExceptions"]);
        //    _ = collection.Find(new BsonDocument()).Skip(skip).Limit(query.PageSize).ToListAsync();

        //    var apiCommon = new ApiCommonResponseModel
        //    {
        //        Data = collection
        //    };

        //    return apiCommon;
        //}
        public async Task<ApiCommonResponseModel> GetUniqueSourcesAsync()
        {
            var allSources = await _log.Distinct<string>("Source", FilterDefinition<Log>.Empty).ToListAsync();
            var uniqueSources = allSources.Where(source => !string.IsNullOrWhiteSpace(source)).ToList();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Unique sources retrieved successfully",
                Data = uniqueSources,
                Total = uniqueSources.Count
            };
        }

        public async Task<ApiCommonResponseModel> GetUserAsync(QueryValues queryValues)
        {
            var apiCommonResponse = new ApiCommonResponseModel();

            List<SqlParameter> sqlParameters = ProcedureCommonSqlParameters.GetCommonSqlParameters(queryValues);
            SqlParameter parameterOutValue = new()
            {
                ParameterName = "TotalCount",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output,
            };

            sqlParameters.AddRange(new SqlParameter[]
               {
                    new()
                    {
                        ParameterName = "PrimaryKey",
                        Value = string.IsNullOrEmpty(queryValues.PrimaryKey) ? DBNull.Value : queryValues.PrimaryKey,
                        SqlDbType = SqlDbType.NVarChar
                    },
                    parameterOutValue
               });

            List<MobileUserResponseModel> mobileUsers = await _context.SqlQueryToListAsync<MobileUserResponseModel>(
                "exec GetMobileUsers @IsPaging , @PageSize, @PageNumber , @SortExpression , @SortOrder , @RequestedBy,  @FromDate,@ToDate,@SearchText, @PrimaryKey, @TotalCount  OUTPUT",
                sqlParameters.ToArray());

            object totalRecords = parameterOutValue.Value;

            apiCommonResponse.Data = mobileUsers;
            apiCommonResponse.StatusCode = HttpStatusCode.OK;
            apiCommonResponse.Total = Convert.ToInt32(totalRecords);

            return apiCommonResponse;
        }

        #region CRM Logs & Exception Methods

        public async Task<ApiCommonResponseModel> GetUserVersionReportsAsync(QueryValues query)
        {
            const string baseDataKey = "UserVersionReports_BaseData";
            List<UserVersionReportResponseModel> baseData;

            if (!_cache.TryGetValue(baseDataKey, out baseData))
            {
                baseData = await FetchBaseDataFromDatabase();
                _cache.Set(baseDataKey, baseData, TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));
            }

            var filteredData = baseData.AsQueryable();

            if (!string.IsNullOrEmpty(query.SearchText))
            {
                var searchTextLower = query.SearchText.ToLower();
                filteredData = filteredData.Where(x =>
                    (x.FullName != null && x.FullName.ToLower().Contains(searchTextLower)) ||
                    (x.MobileNumber != null && x.MobileNumber.Contains(query.SearchText)));
            }

            if (!string.IsNullOrEmpty(query.PrimaryKey))
            {
                var primaryKeyLower = query.PrimaryKey.ToLower();
                filteredData = filteredData.Where(x =>
                    x.DeviceType != null && x.DeviceType.ToLower().Contains(primaryKeyLower));
            }

            if (query.FromDate.HasValue)
            {
                filteredData = filteredData.Where(x =>
                    DateTime.Parse(x.CreatedOn).ToUniversalTime() >= query.FromDate.Value);
            }

            if (query.ToDate.HasValue)
            {
                var toDateEnd = query.ToDate.Value.AddDays(1).AddTicks(-1);
                filteredData = filteredData.Where(x =>
                    DateTime.Parse(x.CreatedOn).ToUniversalTime() <= toDateEnd);
            }

            var totalCount = filteredData.Count();

            var pagedData = filteredData
                .OrderByDescending(x => DateTime.Parse(x.CreatedOn))
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();

            var response = new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "User version reports retrieved successfully",
                Data = pagedData,
                Total = totalCount
            };

            return response;
        }

        private async Task<List<UserVersionReportResponseModel>> FetchBaseDataFromDatabase()
        {
            var pipeline = new List<BsonDocument>
            {
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "User" },
                    { "localField", "MobileUserKey" },
                    { "foreignField", "PublicKey" },
                    { "as", "userDetails" }
                }),
                new("$unwind", new BsonDocument
                {
                    { "path", "$userDetails" },
                    { "preserveNullAndEmptyArrays", true }
                }),
                new BsonDocument("$addFields", new BsonDocument
                    {
                      {
                        "sortDate", new BsonDocument("$dateFromString", new BsonDocument
                        {
                            { "dateString", "$CreatedOn" },
                            { "onError", new DateTime(0) }
                        })
                      }
                    }),
                new BsonDocument("$sort", new BsonDocument
                {
                    { "sortDate", -1 }
                }),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$MobileUserKey" },
                    { "MobileUserKey", new BsonDocument("$first", "$MobileUserKey") },
                    { "MobileUserId", new BsonDocument("$first", "$MobileUserId") },
                    { "DeviceType", new BsonDocument("$first", "$DeviceType") },
                    { "Version", new BsonDocument("$first", "$Version") },
                    { "Description", new BsonDocument("$first", "$Description") },
                    { "CreatedOn", new BsonDocument("$first", "$CreatedOn") },
                    { "FullName", new BsonDocument("$first", "$userDetails.FullName") }
                }),
                new BsonDocument("$project", new BsonDocument
                {
                    { "_id", 0 },
                    { "MobileUserKey", 1 },
                    { "MobileUserId", 1 },
                    { "DeviceType", 1 },
                    { "Version", 1 },
                    { "Description", 1 },
                    { "CreatedOn", 1 },
                    { "FullName", 1 }
                })
            };

            var mongoResult = await _userVersion
                .Aggregate<BsonDocument>(pipeline)
                .ToListAsync();

            var publicKeys = mongoResult
                .Select(r => Guid.Parse(r["MobileUserKey"].AsString))
                .ToList();

            var mobileNumbers = await GetMobileNumbersAsync(publicKeys);

            return mongoResult.Select(r => new UserVersionReportResponseModel
            {
                MobileUserKey = GetSafeString(r, "MobileUserKey"),
                MobileUserId = GetSafeLong(r, "MobileUserId"),
                DeviceType = GetSafeString(r, "DeviceType"),
                Version = GetSafeString(r, "Version"),
                Description = GetSafeString(r, "Description"),
                CreatedOn = GetSafeString(r, "CreatedOn"),
                FullName = GetSafeString(r, "FullName"),
                MobileNumber = GetMobileNumber(r, mobileNumbers)
            }).ToList();
        }

        private string GetSafeString(BsonDocument document, string fieldName)
        {
            if (!document.Contains(fieldName) || document[fieldName] == BsonNull.Value)
                return null;
            return document[fieldName].AsString;
        }

        private long? GetSafeLong(BsonDocument document, string fieldName)
        {
            if (!document.Contains(fieldName) || document[fieldName] == BsonNull.Value)
                return null;
            return document[fieldName].AsInt64;
        }

        private string GetMobileNumber(BsonDocument document, Dictionary<Guid, string> mobileNumbers)
        {
            var mobileUserKeyString = GetSafeString(document, "MobileUserKey");
            if (string.IsNullOrEmpty(mobileUserKeyString))
                return null;

            var mobileUserGuid = Guid.Parse(mobileUserKeyString);
            return mobileNumbers.ContainsKey(mobileUserGuid) ? mobileNumbers[mobileUserGuid] : null;
        }

        #endregion CRM Logs & Exception Methods

        #region CRM Blog Methods

        //public async Task<ApiCommonResponseModel> GetAllBlogs(QueryValues query)
        //{
        //    int skip = (query.PageNumber - 1) * query.PageSize;
        //    var adjustedToDate = query.ToDate?.AddDays(1).AddTicks(-1);

        //    var combinedFilter = await BuildCombinedFilter(query, adjustedToDate);
        //    int totalBlogsCount = (int)await _blogCollection.CountDocumentsAsync(combinedFilter);

        //    var blogs = await GetBlogs(combinedFilter, skip, query.PageSize);
        //    var comments = await GetComments(blogs);
        //    var replies = await GetReplies(comments);
        //    var allReports = await GetReports(blogs);

        //    var userDict = await GetUsersDictionary(blogs, allReports, comments, replies);
        //    var reasonDict = await GetReasonsDictionary(allReports);
        //    var mobileUsersDict = _context.MobileUsers.ToDictionary(mu => mu.PublicKey, mu => mu.Mobile);

        //    var result = BuildResult(blogs, comments, replies, allReports, userDict, reasonDict, mobileUsersDict);

        //    return new ApiCommonResponseModel
        //    { StatusCode = HttpStatusCode.OK, Data = result, Total = totalBlogsCount };
        //}

        public async Task<ApiCommonResponseModel> DeleteBlogAsync(string id, Guid loggedInUser)
        {
            var deletedBy = await GetMobileUsersNameAsync(loggedInUser);
            var blog = await _blogCollection.Find(b => b.ObjectId == id && !b.IsDelete).FirstOrDefaultAsync();

            if (blog == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Blog not found",
                    Data = null
                };
            }

            blog.Status = "Deleted";
            blog.IsDelete = true;
            blog.IsActive = false;
            blog.ModifiedOn = DateTime.Now;
            blog.ModifiedBy = deletedBy;
            await _blogCollection.ReplaceOneAsync(b => b.ObjectId == id, blog);
            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Blog deleted successfully",
                Data = blog
            };
        }

        public async Task<ApiCommonResponseModel> ManageBlogStatusAsync(UpdateBlogStatusRequestModel request)
        {
            var modifiedBy = await GetMobileUsersNameAsync(request.LoggedInUser);

            var blog = await _blogCollection.Find(b => b.ObjectId == request.BlogId).FirstOrDefaultAsync();

            if (blog == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Blog not found",
                    Data = null
                };
            }

            if (request.BlogStatus)
            {
                var relatedReports = await _blogReportCollection
                    .Find(r => r.BlogId == request.BlogId && r.Status == true)
                    .ToListAsync();

                foreach (var report in relatedReports)
                {
                    report.Status = false;
                    await _blogReportCollection.ReplaceOneAsync(r => r.Id == report.Id, report);
                }

                blog.IsActive = false;
                blog.Status = "Blocked";
            }
            else
            {
                blog.IsActive = true;
                blog.Status = "Clean";
            }

            blog.ModifiedBy = modifiedBy;
            blog.ModifiedOn = DateTime.Now;
            await _blogCollection.ReplaceOneAsync(b => b.ObjectId == request.BlogId, blog);

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = request.BlogStatus ? "Blog blocked successfully" : "Blog unblocked successfully",
                Data = blog
            };
        }

        public async Task<ApiCommonResponseModel> ManageBlogPinStatusAsync(UpdatePinnedStatusRequestModel request)
        {
            var modifiedBy = await GetMobileUsersNameAsync(request.LoggedInUser);

            var blog = await _blogCollection.Find(b => b.ObjectId == request.Id).FirstOrDefaultAsync();

            if (blog == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Blog not found",
                    Data = null
                };
            }

            if (request.IsPinned)
            {
                blog.IsPinned = false;
            }
            else
            {
                blog.IsPinned = true;
            }

            blog.ModifiedBy = modifiedBy;
            blog.ModifiedOn = DateTime.Now;
            await _blogCollection.ReplaceOneAsync(b => b.ObjectId == request.Id, blog);

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = request.IsPinned ? "Blog unpinned successfully" : "Blog pinned successfully",
                Data = blog
            };
        }

        public async Task<ApiCommonResponseModel> ManageUserPostPermissionAsync(RestrictUserRequestModel request)
        {
            var modifiedBy = await GetMobileUsersNameAsync(request.LoggedInUser);

            var blog = await _blogCollection.Find(b => b.ObjectId == request.BlogId).FirstOrDefaultAsync();

            if (blog == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Blog not found",
                    Data = null
                };
            }

            var user = await _userCollection.Find(u => u.ObjectId == request.CreatedBy).FirstOrDefaultAsync();
            var mobileUser = await _context.MobileUsers.FirstOrDefaultAsync(u => u.PublicKey == user.PublicKey);

            if (user == null || mobileUser == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "User not found",
                    Data = null
                };
            }

            user.CanCommunityPost = request.CanCommunityPost;
            mobileUser.CanCommunityPost = request.CanCommunityPost;
            user.ModifiedOn = DateTime.Now;
            mobileUser.ModifiedOn = DateTime.Now;

            await _userCollection.ReplaceOneAsync(u => u.ObjectId == request.CreatedBy, user);

            blog.ModifiedBy = modifiedBy;
            blog.ModifiedOn = DateTime.Now;
            await _blogCollection.ReplaceOneAsync(b => b.ObjectId == request.BlogId, blog);

            await _context.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = request.CanCommunityPost
                    ? "User unrestricted from posting successfully."
                    : "User restricted from posting successfully.",
                Data = null
            };
        }

        public async Task<ApiCommonResponseModel> CreateCommunityPostAsync(CommunityPostRequestModel blogPost)
        {
            var responseModel = new ApiCommonResponseModel();
            var publicKey = _context.MobileUsers.Where(b => b.Mobile == blogPost.UserMobileNumber)
                .Select(b => b.PublicKey).FirstOrDefault();
            var createdBy = _userCollection.AsQueryable()
                .Where(b => b.PublicKey == publicKey)
                .Select(b => b.ObjectId)
                .FirstOrDefault();
            var createdByName = _userCollection.AsQueryable()
                .Where(b => b.PublicKey == publicKey)
                .Select(b => b.FullName)
                .FirstOrDefault();
            List<ImageModel> imageList = new();
            //  string notificationImageUrl = null;
            //  var imageUrlSuffix = _configuration["Azure:ImageUrlSuffix"] ?? "";

            if (blogPost.Images != null && blogPost.Images.Any())
            {
                for (int i = 0; i < blogPost.Images.Count; i++)
                {
                    var item = blogPost.Images[i];
                    try
                    {
                        var name = await _azureBlobStorageService.UploadImage(item);

                        // Use the first uploaded image as notification image
                        //if (i == 0)
                        //{
                        //    notificationImageUrl = imageUrlSuffix + name;
                        //}

                        var aspectRatio = (blogPost.AspectRatios != null && blogPost.AspectRatios.Count > i)
                            ? blogPost.AspectRatios[i]
                            : "auto";

                        imageList.Add(new ImageModel
                        {
                            Name = name,
                            AspectRatio = aspectRatio
                        });
                    }
                    catch (Exception ex)
                    {
                        // Log exception, continue with next image or handle accordingly
                        // await _exception.AddAsync(...);
                        // For now just continue
                        continue;
                    }
                }
            }

            // 4. Create blog object
            Blog blogCollectionItem = new()
            {
                Content = blogPost.Content,
                CreatedBy = createdBy,
                //Hashtag = blogPost.Hashtag,
                CreatedOn = DateTime.Now,
                ModifiedOn = null,
                IsDelete = false,
                IsActive = true,
                Image = imageList,
                EnableComments = true,
                Status = "Posted",
                ModifiedBy = createdByName
            };

            // 5. Save blog to DB
            var (success, message, blogId) = await SaveBlog(blogCollectionItem);

            if (!success)
            {
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                responseModel.Message = message;
                return responseModel;
            }

            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Blog Created Successfully.";
            responseModel.Data = blogId;

            ////  Notify all active mobile users
            //var mobileUsers = await _context.MobileUsers
            //    .Where(mu => mu.IsActive == true && mu.IsDelete != true && !string.IsNullOrEmpty(mu.FirebaseFcmToken))
            //    .Select(mu => mu.Mobile)
            //    .ToListAsync();

            // if (mobileUsers != null && mobileUsers.Any())
            //        {
            //    var allMobilesUsers = string.Join(",", mobileUsers);

            //    var notificationModel = new NotificationToMobileRequestModel
            //    {
            //        Title = "New Blog Posted!",
            //        Body = $"{createdByName} posted:",
            //        Mobile = allMobilesUsers,
            //        Topic = "ANNOUNCEMENT",
            //        ScreenName = "getAllBlogs",
            //        NotificationImage = notificationImageUrl ?? "" // fallback empty string
            //    };

            //    try
            //    {
            //        await _mobileNotificationService.SendNotificationToMobile(notificationModel);
            //    }
            //    catch (Exception ex)
            //    {
            //        // Log full exception details including stack trace
            //        Console.WriteLine($"Error sending notification: {ex.Message}");
            //        Console.WriteLine(ex.StackTrace);
            //        if (ex.InnerException != null)
            //        {
            //            Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            //            Console.WriteLine(ex.InnerException.StackTrace);
            //        }

            //        throw;  // optionally rethrow to debug in debugger
            //    }

            return responseModel;
        }



        #endregion CRM Blog Methods

        #region Helper Methods










        private async Task<string?> GetMobileUsersNameAsync(Guid? loggedInUser)
        {
            var mobile = await _context.Users
                .Where(b => b.PublicKey == loggedInUser)
                .Select(b => b.MobileNumber)
                .FirstOrDefaultAsync();

            return await _context.MobileUsers
                .Where(b => b.Mobile == mobile)
                .Select(b => b.FullName)
                .FirstOrDefaultAsync();
        }

        private async Task<string?> GetMobileUserObjectIdAsync(Guid? loggedInUser)
        {
            var mobile = await _context.Users
                .Where(b => b.PublicKey == loggedInUser)
                .Select(b => b.MobileNumber)
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

        private async Task<Dictionary<string, User>> GetUserDetailsByObjectIdsAsync(IEnumerable<string> objectIds)
        {
            var filter = Builders<User>.Filter.In(u => u.ObjectId, objectIds);
            var users = await _userCollection.Find(filter).ToListAsync();

            var distinctUsers = users
                .GroupBy(u => u.ObjectId) // Use ObjectId for grouping
                .Select(g => g.First())
                .ToDictionary(u => u.ObjectId, u => u);

            return distinctUsers;
        }

        private async Task<string> SaveImageToAssetsFolderAsync(IFormFile profileImage, string fileName)
        {
            string assetsDirectory = Path.Combine("C:\\inetpub\\wwwroot\\Mobile\\TestApi", "Assets", "Blog-images");
            if (!Directory.Exists(assetsDirectory))
            {
                Directory.CreateDirectory(assetsDirectory);
            }

            string filePath = Path.Combine(assetsDirectory, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await profileImage.CopyToAsync(fileStream);
            }

            return fileName;
        }

        private async Task<FilterDefinition<Blog>> BuildCombinedFilter(QueryValues query, DateTime? adjustedToDate)
        {
            var filterBuilder = Builders<Blog>.Filter;
            var baseFilter = GetStatusBasedFilter(query, adjustedToDate, filterBuilder);

            if (!string.IsNullOrWhiteSpace(query.SearchText))
            {
                var contentFilter = filterBuilder.Regex(b => b.Content,
                    new BsonRegularExpression(query.SearchText, "i"));

                var fullNameMatchingUserIds = await _userCollection
                    .Find(u => u.FullName.ToLower().Contains(query.SearchText.ToLower()))
                    .Project(u => u.ObjectId)
                    .ToListAsync();

                var matchingMobileUsers = await _context.MobileUsers
                    .Where(mu => mu.Mobile.Contains(query.SearchText))
                    .Select(mu => mu.PublicKey)
                    .ToListAsync();

                var mobileNumberMatchingUserIds = await _userCollection
                    .Find(u => matchingMobileUsers.Contains(u.PublicKey))
                    .Project(u => u.ObjectId)
                    .ToListAsync();

                var allMatchingUserIds = fullNameMatchingUserIds
                    .Union(mobileNumberMatchingUserIds)
                    .Distinct();

                var creatorFilter = filterBuilder.In(b => b.CreatedBy, allMatchingUserIds);

                var searchFilter = filterBuilder.Or(contentFilter, creatorFilter);

                return filterBuilder.And(baseFilter, searchFilter);
            }

            return baseFilter;
        }

        private FilterDefinition<Blog> GetStatusBasedFilter(QueryValues query, DateTime? adjustedToDate,
            FilterDefinitionBuilder<Blog> filterBuilder)
        {
            switch (query.PrimaryKey?.ToLower())
            {
                case "reported":
                    return filterBuilder.Gte(b => b.ModifiedOn, query.FromDate) &
                           filterBuilder.Lte(b => b.ModifiedOn, adjustedToDate) &
                           filterBuilder.Eq(b => b.Status, "Reported") &
                           filterBuilder.Ne(b => b.IsDelete, true);

                case "clean":
                    return filterBuilder.Gte(b => b.ModifiedOn, query.FromDate) &
                           filterBuilder.Lte(b => b.ModifiedOn, adjustedToDate) &
                           filterBuilder.Eq(b => b.Status, "Clean") &
                           filterBuilder.Ne(b => b.IsDelete, true);

                case "posted":
                    return filterBuilder.Gte(b => b.CreatedOn, query.FromDate) &
                           filterBuilder.Lte(b => b.CreatedOn, adjustedToDate) &
                           filterBuilder.Eq(b => b.Status, "Posted") &
                           filterBuilder.Ne(b => b.IsDelete, true);

                case "blocked":
                    return filterBuilder.Gte(b => b.ModifiedOn, query.FromDate) &
                           filterBuilder.Lte(b => b.ModifiedOn, adjustedToDate) &
                           filterBuilder.Eq(b => b.Status, "Blocked") &
                           filterBuilder.Ne(b => b.IsDelete, true);

                case "deleted":
                    return filterBuilder.Gte(b => b.ModifiedOn, query.FromDate) &
                           filterBuilder.Lte(b => b.ModifiedOn, adjustedToDate) &
                           filterBuilder.Eq(b => b.Status, "Deleted") &
                           filterBuilder.Eq(b => b.IsDelete, true);

                default:
                    return BuildDefaultFilter(filterBuilder, query.FromDate, adjustedToDate);
            }
        }

        private FilterDefinition<Blog> BuildDefaultFilter(FilterDefinitionBuilder<Blog> filterBuilder,
            DateTime? fromDate, DateTime? toDate)
        {
            var modifiedFilter = filterBuilder.And(
                filterBuilder.Ne(b => b.ModifiedOn, null),
                filterBuilder.Gte(b => b.ModifiedOn, fromDate),
                filterBuilder.Lte(b => b.ModifiedOn, toDate)
            );

            var createdFilter = filterBuilder.And(
                filterBuilder.Eq(b => b.ModifiedOn, null),
                filterBuilder.Gte(b => b.CreatedOn, fromDate),
                filterBuilder.Lte(b => b.CreatedOn, toDate)
            );

            return filterBuilder.Or(modifiedFilter, createdFilter);
        }


        public async Task<ApiCommonResponseModel> GetUserNotifications(GetNotificationRequestModel request)
        {
            var skip = (request.PageNumber - 1) * request.PageSize;

            var pipeline = _pushNotificationCollectionReceiver.Aggregate()
                .Match(new BsonDocument
                {
            {
                "ReceivedBy",
                request.MobileUserKey.ToString().ToLower()
            }
                })
                .Lookup("PushNotification", "NotificationId", "_id", "notificationTempTable")
                .Unwind("notificationTempTable");

            // Since CreatedOn is now DateTime, we can use it directly instead of converting from string
            if (request.StartDate.HasValue || request.EndDate.HasValue)
            {
                var dateRangeFilter = new BsonDocument();
                if (request.StartDate.HasValue)
                    dateRangeFilter.Add("$gte", request.StartDate);
                if (request.EndDate.HasValue)
                    // Set the time to end of day for the end date
                    dateRangeFilter.Add("$lte", request.EndDate.Value.Date.AddDays(1).AddTicks(-1));
                pipeline = pipeline.Match(new BsonDocument
        {
            { "notificationTempTable.CreatedOn", dateRangeFilter }
        });
            }

            var totalPipeline = pipeline.AppendStage<BsonDocument>("{ $count: 'TotalCount' }");
            var totalResult = await totalPipeline.FirstOrDefaultAsync();
            var totalCount = totalResult != null ? totalResult["TotalCount"].AsInt32 : 0;

            var result = await pipeline
                .Sort(new BsonDocument
                {
            { "notificationTempTable.IsPinned", -1 },
            { "notificationTempTable.CreatedOn", -1 }
                })
                .Skip(skip)
                .Limit(request.PageSize)
                .Project(new BsonDocument
                {
            { "_id", 0 },
            { "ObjectId", new BsonDocument("$toString", "$_id") },
            { "NotificationId", new BsonDocument("$toString", "$NotificationId") },
            { "ReceivedBy", 1 },
            { "Message", "$notificationTempTable.Message" },
            { "Title", "$notificationTempTable.Title" },
            { "CreatedBy", "$notificationTempTable.CreatedBy" },
            { "EnableTradingButton", "$notificationTempTable.EnableTradingButton" },
            { "AppCode", "$notificationTempTable.AppCode" },
            { "Exchange", "$notificationTempTable.Exchange" },
            { "TradingSymbol", "$notificationTempTable.TradingSymbol" },
            { "TransactionType", "$notificationTempTable.TransactionType" },
            { "OrderType", "$notificationTempTable.OrderType" },
            { "Price", new BsonDocument("$toInt", "$notificationTempTable.Price") },
            { "ProductId", "$notificationTempTable.ProductId" },
            { "Complexity", "$notificationTempTable.Complexity" },
            { "CategoryId", "$notificationTempTable.CategoryId" },
            { "IsRead", 1 },
            { "IsDelete", "$notificationTempTable.IsDelete" },
            { "ReadDate", 1 },
            { "IsPinned", "$notificationTempTable.IsPinned" },
            { "Topic", "$notificationTempTable.Topic" },
            { "ScreenName", "$notificationTempTable.ScreenName" },
            {
                "CreatedOn",
                new BsonDocument("$dateToString",
                    new BsonDocument
                    {
                        { "date", "$notificationTempTable.CreatedOn" }
                    })
            }
                })
                .ToListAsync();

            var aggregationJson = System.Text.Json.JsonSerializer.Deserialize<List<PushNotificationResponse>>(result.ToJson());

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Notifications fetched successfully.",
                Data = aggregationJson,
                Total = totalCount,
                Exceptions = null
            };
        }

        //private async Task<List<Blog>> GetBlogsOld(FilterDefinition<Blog> combinedFilter, int skip, int pageSize)
        //{
        //    var pipeline = new[]
        //    {
        //new BsonDocument("$match", combinedFilter.Render(_blogCollection.DocumentSerializer,_blogCollection.Settings.SerializerRegistry)),

        //new BsonDocument("$addFields",
        //        new BsonDocument
        //        {
        //            { "LatestDate", new BsonDocument("$max", new BsonArray { "$ModifiedOn", "$CreatedOn" }) },
        //            {
        //                "PinnedPriority", new BsonDocument("$cond",
        //                    new BsonArray
        //                    {
        //                        new BsonDocument("$and", new BsonArray
        //                        {
        //                            "$IsPinned",
        //                            "$IsActive",
        //                            new BsonDocument("$not", "$IsDelete")
        //                        }),
        //                        1,
        //                        0
        //                    })
        //            }
        //        }),

        //        new BsonDocument("$sort", new BsonDocument
        //        {
        //            { "PinnedPriority", -1 },
        //            { "LatestDate", -1 }
        //        }),

        //        new BsonDocument("$skip", skip),
        //        new BsonDocument("$limit", pageSize),

        //            new BsonDocument("$project",
        //                new BsonDocument
        //                {
        //                    { "PinnedPriority", 0 },
        //                    { "LatestDate", 0 }
        //                })
        //    };

        //    return (List<Blog>)await _blogCollection.AggregateAsync<Blog>(pipeline);
        //}

        public async Task<ApiCommonResponseModel> GetAllBlogs(QueryValues query)
        {
            int skip = (query.PageNumber - 1) * query.PageSize;
            var adjustedToDate = query.ToDate.HasValue
                ? DateTime.SpecifyKind(query.ToDate.Value.AddDays(1).AddTicks(-1), DateTimeKind.Utc)
                : (DateTime?)null;

            var combinedFilter = await BuildCombinedFilter(query, adjustedToDate);

            // Fetch total count and blogs in parallel
            var totalBlogsTask = _blogCollection.CountDocumentsAsync(combinedFilter);
            var blogsTask = GetBlogs(combinedFilter, skip, query.PageSize);

            await Task.WhenAll(totalBlogsTask, blogsTask);
            var blogs = await blogsTask;
            int totalBlogsCount = (int)await totalBlogsTask;

            var userIds = blogs.Select(b => b.CreatedBy.ToString()).Distinct().ToList();
            var users = await _userCollection.Find(u => userIds.Contains(u.ObjectId)).ToListAsync();
            var userDict = users.ToDictionary(u => u.ObjectId);

            var mobileUsersDict = _context.MobileUsers
                .Select(mu => new { mu.PublicKey, mu.Mobile })
                .AsEnumerable()
                .ToDictionary(mu => mu.PublicKey, mu => mu.Mobile);

            var result = BuildResult(blogs, userDict, mobileUsersDict);

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Data = result,
                Total = totalBlogsCount
            };
        }


        public async Task<ApiCommonResponseModel> GetAllBlogsNew(QueryValues query)
        {
            int skip = (query.PageNumber - 1) * query.PageSize;
            var adjustedToDate = query.ToDate.HasValue
            ? query.ToDate.Value.AddDays(1).AddTicks(-1)  // End of the day in local time
            : (DateTime?)null;


            var pipeline = new[]
        {
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "User" },
                { "localField", "CreatedBy" },
                { "foreignField", "_id" },
                { "as", "authorDetails" }
            }),
            new BsonDocument("$project", new BsonDocument
            {
                { "Content", 1 },
                { "Author", new BsonDocument("$arrayElemAt", new BsonArray { "$authorDetails.Name", 0 }) },
                { "AuthorMobile", new BsonDocument("$arrayElemAt", new BsonArray { "$authorDetails.Mobile", 0 }) },
                { "CreatedOn", new BsonDocument("$dateToString", new BsonDocument
                    {
                        { "format", "%Y-%m-%d %H:%M:%S" },
                        { "date", "$CreatedOn" }
                    })
                },
                { "ModifiedOn", new BsonDocument("$dateToString", new BsonDocument
                    {
                        { "format", "%Y-%m-%d %H:%M:%S" },
                        { "date", "$ModifiedOn" }
                    })
                },
                { "LikesCount", 1 },
                { "CommentsCount", 1 },
                { "ReportsCount", 1 },
                { "ModifiedBy", 1 },
                { "IsPinned", 1 }
            }),
            new BsonDocument("$sort", new BsonDocument("CreatedOn", -1)), // Sort by most recent posts
            new BsonDocument("$skip", skip),  // Apply pagination
            new BsonDocument("$limit", query.PageSize)  // Apply pagination
        };

            var result = await _blogCollection.Aggregate<BsonDocument>(pipeline).ToListAsync();


            foreach (var blog in result)
            {
                Console.WriteLine(blog.ToJson());
            }
            //var totalBlogsCount = await _blogCollection.CountDocumentsAsync(filters);

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Data = result,
                Total = 0
            };
        }

        private async Task<List<Blog>> GetBlogs(FilterDefinition<Blog> combinedFilter, int skip, int pageSize)
        {
            var blogsWithModified = await _blogCollection.Find(
                    (combinedFilter ?? FilterDefinition<Blog>.Empty) & Builders<Blog>.Filter.Ne(b => b.ModifiedOn, null))
                .Sort(Builders<Blog>.Sort.Descending(b => b.IsPinned).Descending(b => b.ModifiedOn))
                .ToListAsync();

            var blogsWithoutModified = await _blogCollection.Find(
                    (combinedFilter ?? FilterDefinition<Blog>.Empty) & Builders<Blog>.Filter.Eq(b => b.ModifiedOn, null))
                .Sort(Builders<Blog>.Sort.Descending(b => b.IsPinned).Descending(b => b.CreatedOn))
                .ToListAsync();

            return blogsWithModified
                .Concat(blogsWithoutModified)
                .OrderByDescending(b => b.IsPinned && (b.Status == "Posted" || b.Status == "Clean") ? 1 : 0)
                .ThenByDescending(b => b.ModifiedOn ?? b.CreatedOn)
                .Skip(skip)
                .Take(pageSize)
                .ToList();
        }


        public async Task<ApiCommonResponseModel> GetBlogDetails(string blogId)
        {
            var blog = await _blogCollection.Find(b => b.ObjectId == blogId).FirstOrDefaultAsync();
            if (blog == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Blog not found",
                    Data = null
                };
            }
            var comments = await _commentCollection.Find(c => c.BlogId == blogId).ToListAsync();
            var replies = await _replyCollection.Find(r => comments.Select(c => c.ObjectId).Contains(r.CommentId)).ToListAsync();
            var allReports = await _blogReportCollection.Find(r => r.BlogId == blogId).ToListAsync();
            var userDict = await GetUsersDictionary(blog.CreatedBy, comments, replies, allReports);
            var reasonDict = await GetReasonsDictionary(allReports);
            var commentsList = BuildCommentsList(blogId, comments, replies, userDict);
            var reportDetails = BuildReportDetails(allReports, userDict, reasonDict);
            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Data = new
                {
                    Comments = commentsList,
                    Reports = reportDetails
                }
            };
        }

        private async Task<Dictionary<string, User>> GetUsersDictionary(string blogCreatedBy, List<Comment> comments, List<Reply> replies, List<BlogReport> allReports)
        {
            var userIds = new List<string> { blogCreatedBy }
                .Concat(comments.Select(c => c.CreatedBy.ToString()))
                .Concat(replies.Select(r => r.CreatedBy.ToString()))
                .Concat(allReports.Select(r => r.ReportedBy.ToString()))
                .Distinct()
                .ToList();

            if (!userIds.Any())
                return new Dictionary<string, User>();

            var users = await _userCollection.Find(u => userIds.Contains(u.ObjectId)).ToListAsync();

            return users.ToDictionary(u => u.ObjectId);
        }


        private async Task<Dictionary<string, ReportReason>> GetReasonsDictionary(List<BlogReport> allReports)
        {
            var reasonIds = allReports.Select(r => r.ReasonId.ToString()).Distinct().ToList();

            var reasonsTask = _reportReasonCollection.Find(r => reasonIds.Contains(r.Id)).ToListAsync();
            await reasonsTask;

            return reasonsTask.Result.ToDictionary(r => r.Id);
        }

        private List<object> BuildResult(List<Blog> blogs,
            Dictionary<string, User> userDict,
            Dictionary<Guid, string> mobileUsersDict)
        {
            return blogs.Select(blog =>
            {

                userDict.TryGetValue(blog.CreatedBy.ToString(), out var creatorUser);

                return new
                {
                    blog.ObjectId,
                    blog.Content,
                    blog.Hashtag,
                    BlogImages = blog.Image.Select(b => b.Name).ToList(),
                    CreatorId = creatorUser?.ObjectId,
                    CreatorName = creatorUser?.FullName ?? "Unknown User",
                    CreatorCanCommunityPost = creatorUser?.CanCommunityPost ?? false,
                    CreatorMobileNum = creatorUser != null && mobileUsersDict.TryGetValue(creatorUser.PublicKey, out var mobile) ? mobile : null,
                    blog.CreatedOn,
                    CreatorPublicKey = creatorUser?.PublicKey,
                    blog.ModifiedOn,
                    blog.EnableComments,
                    blog.IsActive,
                    blog.IsDelete,
                    blog.LikesCount,
                    blog.CommentsCount,
                    blog.ReportsCount,
                    blog.Status,
                    blog.ModifiedBy,
                    blog.IsPinned,
                } as object;
            }).ToList();
        }

        private List<object> BuildCommentsList(string blogId,
            List<Comment> comments,
            List<Reply> replies,
            Dictionary<string, User> userDict)
        {
            var commentList = comments.Where(c => c.BlogId == blogId).Select(comment =>
            {
                userDict.TryGetValue(comment.CreatedBy.ToString(), out var commentUser);

                var commentReplies = replies.Where(r => r.CommentId == comment.ObjectId).ToList();

                var commentObject = new
                {
                    comment.ObjectId,
                    comment.Content,
                    comment.Mention,
                    CreatorName = commentUser?.FullName ?? "Unknown User",
                    comment.IsActive,
                    comment.IsDelete,
                    comment.ModifiedOn,
                    comment.CreatedOn,
                    CreatorId = comment.CreatedBy,
                    RepliesCount = comment.ReplyCount,
                    Replies = commentReplies.Select(reply =>
                    {
                        userDict.TryGetValue(reply.CreatedBy.ToString(), out var replyUser);

                        return new
                        {
                            reply.ObjectId,
                            reply.Content,
                            reply.Mention,
                            CreatorName = replyUser?.FullName ?? "Unknown User",
                            reply.IsActive,
                            reply.IsDelete,
                            reply.ModifiedOn,
                            CreatorId = reply.CreatedBy,
                            reply.CreatedOn
                        };
                    }).OrderByDescending(c => c.CreatedOn).ThenByDescending(c => c.ModifiedOn).OrderBy(c => c.IsDelete).ToList()
                };

                return commentObject;
            }).ToList();

            return commentList.OrderByDescending(c => c.CreatedOn)
                .ThenByDescending(c => c.ModifiedOn)
                .OrderBy(c => c.IsDelete)
                .Cast<object>()
                .ToList();
        }

        private List<object> BuildReportDetails(List<BlogReport> blogReports,
            Dictionary<string, User> userDict,
            Dictionary<string, ReportReason> reasonDict)
        {
            return blogReports.Select(report =>
            {
                userDict.TryGetValue(report.ReportedBy.ToString(), out var reportUser);
                reasonDict.TryGetValue(report.ReasonId.ToString(), out var reason);

                return new
                {
                    ReportId = report.Id,
                    ReportedByUserName = reportUser?.FullName ?? "Unknown Reporter",
                    ReportReason = reason?.Reason ?? "Unknown Reason",
                    ReportCreatedOn = report.CreatedOn,
                    ReportStatus = report.Status
                } as object;
            }).ToList();
        }

        private FilterDefinition<T> BuildDateRangeFilter<T>(QueryValues query, string sourceField = null,
            string sourceValue = null)
        {
            var builder = Builders<T>.Filter;
            var filter = builder.Empty;

            if (query.FromDate.HasValue)
            {
                filter &= builder.Gte("CreatedOn", query.FromDate);
            }

            if (query.ToDate.HasValue)
            {
                filter &= builder.Lte("CreatedOn", query.ToDate?.AddDays(1).AddTicks(-1));
            }

            if (!string.IsNullOrEmpty(sourceField) && !string.IsNullOrEmpty(sourceValue))
            {
                filter &= builder.Eq(sourceField, sourceValue);
            }

            if (!string.IsNullOrEmpty(query.SearchText))
            {
                filter &= builder.Regex("Message", new BsonRegularExpression(query.SearchText, "i"));
            }

            return filter;
        }

        private async Task<Dictionary<Guid, string>> GetMobileNumbersAsync(List<Guid> publicKeys)
        {
            if (publicKeys == null || !publicKeys.Any())
            {
                return new Dictionary<Guid, string>();
            }

            // Convert to List before the query to avoid multiple enumeration
            var results = await _context.MobileUsers
                .Where(mu => publicKeys.Contains(mu.PublicKey))
                .Select(mu => new { mu.PublicKey, mu.Mobile })
                .ToListAsync();

            return results.ToDictionary(r => r.PublicKey, r => r.Mobile);
        }

        #endregion Helper Methods

        #region Logs & Exceptions



        #endregion Logs & Exceptions

        #region Comments Related Methods

        public async Task<bool> AddComment(Comment blogComment)
        {
            var filter = Builders<Blog>.Filter.And(
                Builders<Blog>.Filter.Eq(b => b.ObjectId, blogComment.BlogId),
                Builders<Blog>.Filter.Eq(b => b.IsActive, true),
                Builders<Blog>.Filter.Eq(b => b.IsDelete, false)
            );

            var blogExists = await _blogCollection.Find(filter).AnyAsync();

            if (!blogExists)
            {
                return false;
            }

            await _blogCollection.UpdateOneAsync(filter, Builders<Blog>.Update.Inc(b => b.CommentsCount, +1));

            await _commentCollection.InsertOneAsync(blogComment);

            return true;
        }

        public async Task<bool> CommentReply(Reply reply)
        {
            var filter = Builders<Comment>.Filter.Eq(b => b.ObjectId, reply.CommentId);

            var commentExists = await _commentCollection.Find(filter).FirstOrDefaultAsync();

            if (commentExists == null)
            {
                return false;
            }

            await _commentCollection.UpdateOneAsync(filter, Builders<Comment>.Update.Inc(b => b.ReplyCount, +1));
            await _replyCollection.InsertOneAsync(reply);

            var blogFilter = Builders<Blog>.Filter.Eq(b => b.ObjectId, commentExists.BlogId);

            await _blogCollection.UpdateOneAsync(blogFilter, Builders<Blog>.Update.Inc(b => b.CommentsCount, +1));

            return true;
        }

        public async Task<List<GetCommentsResponseModel>?> GetComments(string blogId, int pageNumber, int pageSize)
        {
            //WITH AGGREGATION PIPELINE to

            var aggregationPipeline = await _commentCollection.Aggregate()
                .Match(new BsonDocument
                {
                    {
                        "BlogId",
                        new ObjectId(blogId)
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
                .Sort(new BsonDocument
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
                    new BsonDocument
                    {
                        {
                            "_id",
                            0
                        },
                        {
                            "BlogId",
                            new BsonDocument("$toString", "$BlogId")
                        },
                        {
                            "ObjectId",
                            new BsonDocument("$toString", "$_id")
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
                            new BsonDocument("$toString", "$CreatedBy")
                        },
                        {
                            "CreatedOn",
                            new BsonDocument
                            {
                                {
                                    "$dateToString",
                                    new BsonDocument
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
                            new BsonDocument("$toInt", "$ReplyCount")
                        },
                        {
                            "UserFullName",
                            new BsonDocument("$arrayElemAt", new BsonArray
                            {
                                "$userData.FullName",
                                0
                            })
                        },
                        {
                            "Gender",
                            new BsonDocument("$arrayElemAt", new BsonArray
                            {
                                "$userData.Gender",
                                0
                            })
                        },
                        {
                            "UserProfileImage",
                            new BsonDocument("$arrayElemAt", new BsonArray
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

            string format = "dd-MM-yyyy HH:mm:ss";

            var commentsList = new List<GetCommentsResponseModel>();

            foreach (var item in aggregationJson)
            {
                var combinedComment = new GetCommentsResponseModel()
                {
                    ObjectId = item.ObjectId,
                    BlogId = item.BlogId,
                    Content = item.Content,
                    Mention = item.Mention,
                    CreatedBy = item.CreatedBy,
                    PostedAgo = GetRelativeTimeSincePost.GetRelativeTimeSincePostedForCommentsAndReply(DateTime.ParseExact(item.CreatedOn, format, null)),
                    CreatedOn = UtcToIstDateTime.UtcStringToIst(item.CreatedOn),
                    ReplyCount = item.ReplyCount,
                    UserFullName = item.UserFullName,
                    Gender = item.Gender,
                    UserProfileImage = _configuration["Azure:ImageUrlSuffix"] + item.UserProfileImage,
                };

                // Add the custom object to the list
                commentsList.Add(combinedComment);
            }

            return commentsList;
        }

        public async Task<List<GetRepliesResponseModel>?> GetReplies(string commentId)
        {
            var aggregationPipeline = await _replyCollection.Aggregate()
                .Match(new BsonDocument
                {
                    {
                        "CommentId",
                        new ObjectId(commentId)
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
                .Sort(new BsonDocument("CreatedOn", -1))
                .Lookup("Comment", "CommentId", "_id", "commentData")
                .Unwind("commentData")
                .Lookup("User", "CreatedBy", "_id", "userData")
                .Unwind("userData")
                .Project(new BsonDocument
                {
                    {
                        "_id",
                        0
                    },
                    {
                        "ObjectId",
                        new BsonDocument("$toString", "$_id")
                    },
                    {
                        "BlogId",
                        new BsonDocument("$toString", "$commentData.BlogId")
                    },
                    {
                        "CommentId",
                        new BsonDocument("$toString", "$CommentId")
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
                        new BsonDocument("$toString", "$CreatedBy")
                    },
                    {
                        "CreatedOn",
                        new BsonDocument
                        {
                            {
                                "$dateToString",
                                new BsonDocument
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
                        "UserFullName",
                        "$userData.FullName"
                    },
                    {
                        "UserProfileImage",
                        "$userData.ProfileImage"
                    },
                    {
                        "Gender",
                        "$userData.Gender"
                    }
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

        public async Task<bool> DeleteCommentOrReply(string objectId, string userObjectId, string type)
        {
            if (type.Trim().ToUpper() == "COMMENT")
            {
                var commentFilter = Builders<Comment>.Filter.And(
                    Builders<Comment>.Filter.Eq(c => c.ObjectId, objectId),
                    Builders<Comment>.Filter.Eq(c => c.CreatedBy, userObjectId)
                );

                var comment = await _commentCollection.FindOneAndUpdateAsync(
                    commentFilter,
                    Builders<Comment>.Update
                        .Set(c => c.IsActive, false)
                        .Set(c => c.IsDelete, true)
                        .Set(c => c.ModifiedOn, DateTime.Now)
                );

                if (comment == null)
                {
                    return false;
                }

                // check if the comment has any repies, if it does then delete it
                if (comment.ReplyCount > 0)
                {
                    var replyFilter = Builders<Reply>.Filter.And(
                        Builders<Reply>.Filter.Eq(c => c.CommentId, comment.ObjectId)
                    );

                    var reply = await _replyCollection.UpdateManyAsync(
                        replyFilter,
                        Builders<Reply>.Update
                            .Set(c => c.IsActive, false)
                            .Set(c => c.IsDelete, true)
                            .Set(c => c.ModifiedOn, DateTime.Now)
                    );
                }

                var updateCommentResult = await _commentCollection.UpdateOneAsync(
                   commentFilter,
                   Builders<Comment>.Update.Set(c => c.ReplyCount, 0)
                );

                var blogFilter = Builders<Blog>.Filter.Eq(b => b.ObjectId, comment.BlogId);
                var updateBlog = Builders<Blog>.Update.Inc(b => b.CommentsCount, -comment.ReplyCount - 1);
                var updateResult = await _blogCollection.UpdateOneAsync(blogFilter, updateBlog);


                return true;
            }
            else if (type.Trim().ToUpper() == "REPLY")
            {
                var replyFilter = Builders<Reply>.Filter.And(
                    Builders<Reply>.Filter.Eq(r => r.ObjectId, objectId),
                    Builders<Reply>.Filter.Eq(r => r.CreatedBy, userObjectId)
                );

                var reply = await _replyCollection.FindOneAndUpdateAsync(
                    replyFilter,
                    Builders<Reply>.Update
                        .Set(r => r.IsActive, false)
                        .Set(r => r.IsDelete, true)
                        .Set(c => c.ModifiedOn, DateTime.Now)
                );

                if (reply == null)
                {
                    return false;
                }

                // Decrement reply count in the associated comment and blog
                var commentFilter = Builders<Comment>.Filter.Eq(c => c.ObjectId, reply.CommentId);
                var updateCommentResult = await _commentCollection.UpdateOneAsync(
                    commentFilter,
                    Builders<Comment>.Update.Inc(c => c.ReplyCount, -1)
                );

                if (updateCommentResult.ModifiedCount > 0)
                {
                    var comment = await _commentCollection.Find(commentFilter).FirstOrDefaultAsync();

                    if (comment != null)
                    {
                        var blogFilter = Builders<Blog>.Filter.Eq(b => b.ObjectId, comment.BlogId);
                        var updateBlogResult = await _blogCollection.UpdateOneAsync(
                            blogFilter,
                            Builders<Blog>.Update.Inc(b => b.CommentsCount, -1)
                        );
                    }
                }

                return true;
            }
            else return false;
        }

        public async Task<List<PushNotification>?> GetScanner(GetNotificationRequestModel request)
        {
            try
            {
                int skip = (request.PageNumber - 1) * request.PageSize;
                var filterBuilder = Builders<PushNotificationReceiver>.Filter;
                var filter = filterBuilder.Eq(b => b.ReceivedBy, request.MobileUserKey);
                var notificationsQuery = _pushNotificationCollectionReceiver.Find(filter);

                var unreadCount = await notificationsQuery.CountDocumentsAsync();

                //var filterBuilderNotification = Builders<PushNotification>.Filter;

                var notificationsReceiver = await notificationsQuery
                    .Skip(skip)
                    .Limit(request.PageSize)
                    .ToListAsync();

                var result = await _pushNotificationCollectionReceiver.Aggregate()
                    .Match(new BsonDocument
                    {
                        {
                            "ReceivedBy",
                            request.MobileUserKey.ToString()
                        },
                    }).Sort(new BsonDocument("CreatedOn", -1))
                    .Skip(skip).Limit(request.PageSize)
                    .Lookup("PushNotification", "NotificationId", "_id", "notificationTempTable")
                    .Unwind("notificationTempTable")
                    .Match(new BsonDocument
                    {
                        {
                            "notificationTempTable.Scanner",
                            true
                        },
                    })
                    .Project(new BsonDocument
                    {
                        {
                            "_id",
                            "$notificationTempTable._Id"
                        },
                        {
                            "Message",
                            "$notificationTempTable.Message"
                        },
                        {
                            "Title",
                            "$notificationTempTable.Title"
                        },
                        {
                            "CreatedBy",
                            "$notificationTempTable.CreatedBy"
                        },
                        {
                            "EnableTradingButton",
                            "$notificationTempTable.EnableTradingButton"
                        },
                        {
                            "AppCode",
                            "$notificationTempTable.AppCode"
                        },
                        {
                            "Exchange",
                            "$notificationTempTable.Exchange"
                        },
                        {
                            "TradingSymbol",
                            "$notificationTempTable.TradingSymbol"
                        },
                        {
                            "TransactionType",
                            "$notificationTempTable.TransactionType"
                        },
                        {
                            "OrderType",
                            "$notificationTempTable.OrderType"
                        },
                        {
                            "Price",
                            new BsonDocument("$toInt", "$notificationTempTable.Price")
                        },
                        {
                            "ProductId",
                            "$notificationTempTable.ProductId"
                        },
                        {
                            "Complexity",
                            "$notificationTempTable.Complexity"
                        },
                        {
                            "CategoryId",
                            "$notificationTempTable.CategoryId"
                        },
                        {
                            "IsRead",
                            "$notificationTempTable.IsRead"
                        },
                        {
                            "IsDelete",
                            "$notificationTempTable.IsDelete"
                        },
                        {
                            "ReadDate",
                            "$notificationTempTable.ReadDate"
                        },
                        {
                            "Topic",
                            "$notificationTempTable.Topic"
                        },
                        {
                            "CreatedOn",
                            new BsonDocument
                            {
                                {
                                    "$dateToString",
                                    new BsonDocument
                                    {
                                        {
                                            "format",
                                            "%d-%m-%Y %H:%M:%S"
                                        },
                                        {
                                            "date",
                                            "$notificationTempTable.CreatedOn"
                                        }
                                    }
                                }
                            }
                        },
                    })
                    .ToListAsync();
                var aggregationJson =
                    System.Text.Json.JsonSerializer.Deserialize<List<PushNotification>>(result.ToJson());

                return aggregationJson;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<bool> EditCommentOrReply(string objectId, string userObjectId, string newContent,
            string newMention, string type)
        {
            if (type.Trim().ToUpper() == "COMMENT")
            {
                var timeThreshold = DateTime.UtcNow.AddMinutes(-30);
                var filter = Builders<Comment>.Filter.And(
                    Builders<Comment>.Filter.Eq(b => b.ObjectId, objectId),
                    Builders<Comment>.Filter.Eq(b => b.CreatedBy, userObjectId),
                    Builders<Comment>.Filter.Eq(b => b.IsActive, true),
                    Builders<Comment>.Filter.Gte(b => b.CreatedOn, timeThreshold)
                );

                var update = await _commentCollection.FindOneAndUpdateAsync(
                    filter,
                    Builders<Comment>.Update
                        .Set(c => c.Content, newContent)
                        .Set(c => c.Mention, newMention)
                        .Set(c => c.ModifiedOn, DateTime.Now)
                );

                if (update == null)
                {
                    return false;
                }

                return true;
            }

            if (type.Trim().ToUpper() == "REPLY")
            {
                var timeThreshold = DateTime.UtcNow.AddMinutes(-30);
                var filter = Builders<Reply>.Filter.And(
                    Builders<Reply>.Filter.Eq(b => b.ObjectId, objectId),
                    Builders<Reply>.Filter.Eq(b => b.CreatedBy, userObjectId),
                    Builders<Reply>.Filter.Eq(b => b.IsActive, true),
                    Builders<Reply>.Filter.Gte(b => b.CreatedOn, timeThreshold)
                );

                var update = await _replyCollection.FindOneAndUpdateAsync(
                    filter,
                    Builders<Reply>.Update
                        .Set(c => c.Content, newContent)
                        .Set(c => c.Mention, newMention)
                        .Set(c => c.ModifiedOn, DateTime.Now)
                );

                if (update == null)
                {
                    return false;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<ApiCommonResponseModel> AddBlogComment(PostCommentRequestModel request, Guid loggedInUser)
        {
            var createdBy = await GetMobileUserObjectIdAsync(loggedInUser);

            var comment = new Comment
            {
                ObjectId = ObjectId.GenerateNewId().ToString(),
                BlogId = request.BlogId,
                Content = request.Comment,
                Mention = request.Mention,
                CreatedBy = createdBy,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
                IsDelete = false
            };

            bool success = await AddComment(comment);

            string message = success ? "Comment Added Successfully." : "Couldn't Add Comment.";
            var statusCode = success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;

            return _communityPostService.CreateResponseStatusCode(statusCode, message, success ? comment : null);
        }


        public async Task<bool> DisableBlogComment(DisableBlogCommentRequestModel request)
        {
            var filter = Builders<Blog>.Filter.And(
                Builders<Blog>.Filter.Eq("CreatedBy", ObjectId.Parse(request.CreatedByObjectId)),
                Builders<Blog>.Filter.Eq("_id", ObjectId.Parse(request.BlogId))
            );

            var existingBlog = await _blogCollection.Find(filter).FirstOrDefaultAsync();

            if (existingBlog != null)
            {
                var currentEnableComments = existingBlog.EnableComments;
                var invertedEnableComments = !currentEnableComments;

                var update = Builders<Blog>.Update
                    .Set("EnableComments", invertedEnableComments)
                    .Set("ModifiedOn", DateTime.Now);

                var result = await _blogCollection.UpdateOneAsync(filter, update);

                return result.ModifiedCount > 0;
            }
            else return false;
        }

        #endregion Comments Related Methods

        #region Blogs Related Method

        public async Task<(bool Success, string Message, string BlogId)> SaveBlog(Blog blog)
        {
            try
            {
                var postedBy = await _userCollection
                    .Find(u => u.ObjectId == blog.CreatedBy)
                    .Project(u => u.FullName)
                    .FirstOrDefaultAsync();
                blog.ModifiedBy = postedBy;
                await _blogCollection.InsertOneAsync(blog);



                return (true, "Blog saved successfully", blog.ObjectId.ToString());

            }
            catch (Exception ex)
            {
                return (false, $"An unexpected error occurred: {ex.Message}", null);
            }
        }

        //ToDo Check Code Comparision with above method
        //public async Task<(bool Success, string Message, string BlogId)> SaveBlog(Blog blog)
        //{
        //    try
        //    {
        //        await _blogCollection.InsertOneAsync(blog);

        //        return (true, "Blog saved successfully", blog.ObjectId.ToString());
        //    }
        //    catch (Exception ex)
        //    {
        //        return (false, $"An unexpected error occurred: {ex.Message}", null);
        //    }
        //}

        public async Task<List<GetBlogsResponseModel>?> GetBlogs(int pageNumber, int pageSize, string userObjectId)
        {
            PipelineDefinition<Blog, BsonDocument> pipeline = new BsonDocument[]
            {
                new BsonDocument("$match",
                    new BsonDocument
                    {
                        { "IsDelete", false },
                        { "IsActive", true }
                    }),
                 new BsonDocument("$sort",
                    new BsonDocument
                    {
                        { "IsPinned", -1 },
                        { "CreatedOn", -1 }
                    }),
                new BsonDocument("$lookup",
                    new BsonDocument
                    {
                        { "from", "UserBlock" },
                        {
                            "let",
                            new BsonDocument("createdById", "$CreatedBy")
                        },
                        {
                            "pipeline",
                            new BsonArray
                            {
                                new BsonDocument("$match",
                                    new BsonDocument("$expr",
                                        new BsonDocument("$and",
                                            new BsonArray
                                            {
                                                new BsonDocument("$eq",
                                                    new BsonArray
                                                    {
                                                        "$BlockerId",
                                                        new ObjectId(userObjectId)
                                                    }),
                                                new BsonDocument("$eq",
                                                    new BsonArray
                                                    {
                                                        "$BlockedId",
                                                        "$$createdById"
                                                    }),
                                                new BsonDocument("$eq",
                                                    new BsonArray
                                                    {
                                                        "$IsActive",
                                                        true
                                                    })
                                            })))
                            }
                        },
                        { "as", "blockedByUser" }
                    }),
                new BsonDocument("$lookup",
                    new BsonDocument
                    {
                        { "from", "UserBlock" },
                        {
                            "let",
                            new BsonDocument("createdById", "$CreatedBy")
                        },
                        {
                            "pipeline",
                            new BsonArray
                            {
                                new BsonDocument("$match",
                                    new BsonDocument("$expr",
                                        new BsonDocument("$and",
                                            new BsonArray
                                            {
                                                new BsonDocument("$eq",
                                                    new BsonArray
                                                    {
                                                        "$BlockedId",
                                                        new ObjectId(userObjectId)
                                                    }),
                                                new BsonDocument("$eq",
                                                    new BsonArray
                                                    {
                                                        "$BlockerId",
                                                        "$$createdById"
                                                    }),
                                                new BsonDocument("$eq",
                                                    new BsonArray
                                                    {
                                                        "$IsActive",
                                                        true
                                                    })
                                            })))
                            }
                        },
                        { "as", "userBlocked" }
                    }),
                new BsonDocument("$match",
                    new BsonDocument("$and",
                        new BsonArray
                        {
                            new BsonDocument("blockedByUser",
                                new BsonDocument("$size", 0)),
                            new BsonDocument("userBlocked",
                                new BsonDocument("$size", 0))
                        })),
                new BsonDocument("$skip", (pageNumber - 1) * pageSize),
                new BsonDocument("$limit", pageSize),
                new BsonDocument("$lookup",
                    new BsonDocument
                    {
                        { "from", "User" },
                        { "localField", "CreatedBy" },
                        { "foreignField", "_id" },
                        { "as", "user" }
                    }),
                new BsonDocument("$unwind", "$user"),
                new BsonDocument("$lookup",
                    new BsonDocument
                    {
                        { "from", "Like" },
                        { "localField", "_id" },
                        { "foreignField", "BlogId" },
                        { "as", "likes" }
                    }),
                new BsonDocument("$lookup",
                    new BsonDocument
                    {
                        { "from", "BlogReport" },
                        {
                            "let",
                            new BsonDocument("blogId", "$_id")
                        },
                        {
                            "pipeline",
                            new BsonArray
                            {
                                new BsonDocument("$match",
                                    new BsonDocument("$expr",
                                        new BsonDocument("$and",
                                            new BsonArray
                                            {
                                                new BsonDocument("$eq",
                                                    new BsonArray
                                                    {
                                                        "$BlogId",
                                                        "$$blogId"
                                                    }),
                                                new BsonDocument("$eq",
                                                    new BsonArray
                                                    {
                                                        "$ReportedBy",
                                                        new ObjectId(userObjectId)
                                                    })
                                            })))
                            }
                        },
                        { "as", "reports" }
                    }),
                new BsonDocument("$project",
                    new BsonDocument
                    {
                        { "_id", 0 },
                        { "EnableComments", 1 },
                        {
                            "ObjectId",
                            new BsonDocument("$toString", "$_id")
                        },
                        { "Content", 1 },
                        { "Hashtag", 1 },
                        {
                            "CreatedBy",
                            new BsonDocument("$toString", "$CreatedBy")
                        },
                        {
                            "CreatedOn",
                            new BsonDocument("$dateToString",
                                new BsonDocument
                                {
                                    { "format", "%d-%m-%Y %H:%M:%S" },
                                    { "date", "$CreatedOn" }
                                })
                        },
                        {
                            "LikesCount",
                            new BsonDocument("$size", "$likes")
                        },
                        {
                            "CommentsCount",
                            new BsonDocument("$toInt", "$CommentsCount")
                        },
                        { "Image", 1 },
                        { "IsPinned", 1 },
                        { "UserFullName", "$user.FullName" },
                        { "UserProfileImage", "$user.ProfileImage" },
                        { "Gender", "$user.Gender" },
                        {
                            "UserHasLiked",
                            new BsonDocument("$in",
                                new BsonArray
                                {
                                    userObjectId,
                                    "$likes.CreatedBy"
                                })
                        },
                        {
                            "isUserReported",
                            new BsonDocument("$gt",
                                new BsonArray
                                {
                                    new BsonDocument("$size", "$reports"),
                                    0
                                })
                        }
                    })
            };

            var aggregationResult = await _blogCollection.Aggregate(pipeline).ToListAsync();
            var aggregationJson = JsonConvert.DeserializeObject<List<BlogAggregationModel>>(aggregationResult.ToJson());

            //aggregationJson?.ForEach(
            //    x => x.Image.ForEach(x => x.Name = _configuration["Azure:ImageUrlSuffix"] + x.Name));

            const string format = "dd-MM-yyyy HH:mm:ss";

            List<GetBlogsResponseModel> blogsList = new();

            if (aggregationJson is { Count: 0 })
            {
                return blogsList;
            }

            if (aggregationJson != null)
            {
                blogsList.AddRange(aggregationJson.Select(item => new GetBlogsResponseModel
                {
                    ObjectId = item.ObjectId,
                    Content = item.Content,
                    Hashtag = item.Hashtag,
                    CreatedBy = item.CreatedBy,
                    EnableComments = item.EnableComments,
                    PostedAgo = GetRelativeTimeSincePost.GetRelativeTimeSincePosted(
                        DateTime.ParseExact(item.CreatedOn, format, null)),
                    CreatedOn = UtcToIstDateTime.UtcStringToIst(item.CreatedOn),
                    LikesCount = item.LikesCount,
                    CommentsCount = item.CommentsCount,
                    Image = item.Image.Select(x => new ImageModel
                    { Name = _configuration["Azure:ImageUrlSuffix"] + x.Name, AspectRatio = x.AspectRatio, })
                        .ToList(),
                    UserFullName = item?.UserFullName,
                    UserProfileImage = item?.UserProfileImage is not null
                        ? _configuration["Azure:ImageUrlSuffix"] + item?.UserProfileImage
                        : null,
                    Gender = item?.Gender,
                    UserHasLiked = item?.UserHasLiked is not null and not false,
                    IsUserReported = item is { IsUserReported: true },
                    IsPinned = item is { IsPinned: true }
                }));
            }

            var reportedBlogIds = await _blogReportCollection
                .Find(x => x.ReportedBy == new ObjectId(userObjectId) && x.Status)
                .Project(g => g.BlogId)
                .ToListAsync()
                .ContinueWith(task => task.Result.ToHashSet());

            var filteredBlogsList = blogsList
                .Where(blog => !reportedBlogIds.Contains(blog.ObjectId)).OrderByDescending(blog => blog.IsPinned)
                .ToList();

            return filteredBlogsList;
        }

        public async Task<bool> DeleteBlog(string blogId, string userObjectId)
        {
            var filter = Builders<Blog>.Filter.And(
                Builders<Blog>.Filter.Eq(b => b.ObjectId, blogId),
                Builders<Blog>.Filter.Eq(b => b.CreatedBy, userObjectId)
            );
            var deletedBy = await _userCollection
                .Find(u => u.ObjectId == userObjectId)
                .Project(u => u.FullName)
                .FirstOrDefaultAsync();
            var update = Builders<Blog>.Update
                .Set(b => b.IsActive, false)
                .Set(b => b.IsDelete, true)
                .Set(b => b.Status, "Deleted")
                .Set(b => b.ModifiedBy, deletedBy)
                .Set(b => b.ModifiedOn, DateTime.Now);

            var updateResult = await _blogCollection.UpdateOneAsync(filter, update);

            return updateResult.ModifiedCount > 0;
        }

        public async Task<bool> EditBlog(string blogId, string userObjectId, string newContent, string newHashtag)
        {
            var timeThreshold = DateTime.UtcNow.AddMinutes(-30);
            var editedBy = await _userCollection
                .Find(u => u.ObjectId == userObjectId)
                .Project(u => u.FullName)
                .FirstOrDefaultAsync();
            var filter = Builders<Blog>.Filter.And(
                Builders<Blog>.Filter.Eq(b => b.ObjectId, blogId),
                Builders<Blog>.Filter.Eq(b => b.CreatedBy, userObjectId),
                Builders<Blog>.Filter.Eq(b => b.IsActive, true),
                Builders<Blog>.Filter.Gte(b => b.CreatedOn, timeThreshold)
            );

            var update = await _blogCollection.FindOneAndUpdateAsync(
                filter,
                Builders<Blog>.Update
                    .Set(c => c.Content, newContent)
                    .Set(c => c.Hashtag, newHashtag)
                    .Set(c => c.ModifiedOn, DateTime.Now)
            );

            if (update == null)
            {
                return false;
            }

            return true;
        }

        public async Task<ApiCommonResponseModel> ReportBlog(ReportBlogRequestModel request)
        {
            var responseModel = new ApiCommonResponseModel();
            try
            {
                var blog = await _blogCollection.Find(b => b.ObjectId == request.BlogId).FirstOrDefaultAsync();
                if (blog == null)
                {
                    responseModel.StatusCode = System.Net.HttpStatusCode.NotFound;
                    return responseModel;
                }

                // Check if the user has already reported this blog
                var existingReport = await _blogReportCollection.Find(r =>
                    r.BlogId == request.BlogId &&
                    r.ReportedBy == ObjectId.Parse(request.ReportedBy)
                ).FirstOrDefaultAsync();
                //if (existingReport != null)
                //{
                //    responseModel.StatusCode = System.Net.HttpStatusCode.Conflict;
                //    responseModel.Message = "You have already reported this blog.";
                //    return responseModel;
                //}

                var report = new BlogReport
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    BlogId = request.BlogId,
                    ReportedBy = ObjectId.Parse(request.ReportedBy),
                    ReasonId = ObjectId.Parse(request.ReasonId),
                    Description = null,
                    CreatedOn = DateTime.UtcNow,
                    Status = true
                };

                await _blogReportCollection.InsertOneAsync(report);
                var reportedByName = await _userCollection
                    .Find(u => u.ObjectId == request.ReportedBy)
                    .Project(u => u.FullName)
                    .FirstOrDefaultAsync();
                var update = Builders<Blog>.Update
                    .Inc(b => b.ReportsCount, 1)
                    .Set(b => b.Status, "Reported")
                    .Set(b => b.ModifiedBy, reportedByName)
                    .Set(b => b.ModifiedOn, DateTime.Now);

                var updateResult = await _blogCollection.UpdateOneAsync(
                    b => b.ObjectId == request.BlogId,
                    update
                );

                if (updateResult.ModifiedCount == 0)
                {
                    _logger.LogWarning("Blog {BlogId} not found or not updated", request.BlogId);
                    responseModel.StatusCode = System.Net.HttpStatusCode.NotFound;
                    responseModel.Message = "Blog not found or could not be updated.";
                    return responseModel;
                }

                // Get all blogs by the creator
                var creatorBlogs = await _blogCollection
                    .Find(b => b.CreatedBy == blog.CreatedBy)
                    .ToListAsync();

                // Count total reports across all blogs by the creator
                var blogIds = creatorBlogs.Select(b => b.ObjectId).ToList();

                var creatorTotalReports = await _blogReportCollection
                    .CountDocumentsAsync(r => blogIds.Contains(r.BlogId.ToString()));

                if (creatorTotalReports > 4 && creatorTotalReports < 6)
                {
                    // get the public key of the user
                    var user = await _userCollection
                        .Find(b => b.ObjectId == blog.CreatedBy)
                        .FirstOrDefaultAsync();

                    _logger.LogInformation(
                        "Blog creator has received 5 or more total reports across all blogs. BlogId: {BlogId}, CreatorId: {CreatorId}",
                        request.BlogId, blog.CreatedBy);
                    responseModel.StatusCode = System.Net.HttpStatusCode.Accepted;
                    responseModel.Data = user.PublicKey;
                    return responseModel;
                }

                if (updateResult.ModifiedCount == 0)
                {
                    _logger.LogWarning("Blog {BlogId} not found or not updated", request.BlogId);
                    responseModel.StatusCode = System.Net.HttpStatusCode.NotFound;
                    return responseModel;
                }

                responseModel.StatusCode = System.Net.HttpStatusCode.OK;
                return responseModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reporting blog {BlogId} by user {UserId}", request.BlogId,
                    request.ReportedBy);
                responseModel.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                return responseModel;
            }
        }

        public async Task<List<GetReportDto>> GetReportReason()
        {
            return await _reportReasonCollection
                .Find(r => r.IsActive)
                .Project(r => new GetReportDto
                {
                    Id = r.Id,
                    Reason = r.Reason
                })
                .ToListAsync();
        }

        public async Task<bool> LikeBlog(Like blogLike, bool isLiked)
        {
            var filter = Builders<Blog>.Filter.Eq(b => b.ObjectId, blogLike.BlogId);
            var blogExists = await _blogCollection.Find(filter).AnyAsync();

            if (!blogExists)
            {
                return false;
            }

            var likeExistsFilter = Builders<Like>.Filter.And(
                Builders<Like>.Filter.Eq(l => l.CreatedBy, blogLike.CreatedBy),
                Builders<Like>.Filter.Eq(l => l.BlogId, blogLike.BlogId));

            var existingLike = await _likeCollection.Find(likeExistsFilter).AnyAsync();

            if (isLiked)
            {
                if (existingLike == true)
                {
                    return true;
                }
                else
                {
                    await _blogCollection.UpdateOneAsync(filter, Builders<Blog>.Update.Inc(b => b.LikesCount, +1));

                    await _likeCollection.InsertOneAsync(blogLike);
                }
            }
            else if (isLiked == false)
            {
                if (existingLike == true)
                {
                    // Decrease the like count
                    await _blogCollection.UpdateOneAsync(filter, Builders<Blog>.Update.Inc(b => b.LikesCount, -1));

                    // Remove like record  from likes collection
                    var deleteResult = await _likeCollection.DeleteOneAsync(likeExistsFilter);

                    return true;
                }
                else
                {
                    return true;
                }
            }

            return true;
        }

        #endregion Blogs Related Method

        #region Notification Related Methods

        public async Task<bool> ReadPushNotification(string receiverId)
        {
            try
            {
                var filter = Builders<PushNotificationReceiver>.Filter.Eq(p => p.ObjectId, receiverId);
                var update = Builders<PushNotificationReceiver>.Update
                    .Set(p => p.IsRead, true)
                    .Set(p => p.ReadDate, DateTime.Now.ToString());

                var updateResult = await _pushNotificationCollectionReceiver.UpdateOneAsync(filter, update);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<bool> MarkAllNotificationAsRead(Guid mobileUserKey)
        {
            try
            {
                var userKeyString = mobileUserKey.ToString().ToLower();

                var filter = Builders<PushNotificationReceiver>.Filter.And(
                    Builders<PushNotificationReceiver>.Filter.Eq("ReceivedBy", mobileUserKey.ToString().ToLower()),
                    Builders<PushNotificationReceiver>.Filter.Eq("IsRead", false)
                );


                var update = Builders<PushNotificationReceiver>.Update
                                .Set("IsRead", true)
                                .Set("ReadDate", DateTime.Now.ToString());

                var result = await _pushNotificationCollectionReceiver.UpdateManyAsync(filter, update);
                return result.ModifiedCount > 0;
            }

            catch (Exception ex)
            {
                //await LogExceptions(ex, source: "FcmToken");
                return false;
            }
        }
        public async Task<long> GetUnreadNotificationCount(Guid mobileUserKey)
        {
            PipelineDefinition<PushNotificationReceiver, BsonDocument> pipeline = new BsonDocument[]
            {
                new BsonDocument("$match", new BsonDocument("ReceivedBy", mobileUserKey.ToString().ToLower())),
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "PushNotification" },
                    { "localField", "NotificationId" },
                    { "foreignField", "_id" },
                    { "as", "notificationTempTable" }
                }),
                new BsonDocument("$unwind", "$notificationTempTable"),
                new BsonDocument("$match", new BsonDocument
                {
                    { "IsRead", false },
                    {"IsDelete",false },
                    { "notificationTempTable.Scanner", false }
                }),
                new BsonDocument("$count", "UnreadCount")
            };
            var aggregationResult = await _pushNotificationCollectionReceiver.Aggregate(pipeline).ToListAsync();
            var aggregationJson =
                System.Text.Json.JsonSerializer.Deserialize<List<UnreadNotificationCountModel>>(
                    aggregationResult.ToJson());

            if (aggregationResult == null || !aggregationResult.Any())
            {
                return 0; // Return 0 as the unread count when no documents match
            }
            return aggregationJson[0].UnreadCount;
        }
        /// If the date range is more than 1 day, fetch data from SQL, becase he is requesting for historical data
        /// else only fetch for todays data
        public async Task<List<ScannerNotification>?> GetScannerNotificationOld(QueryValues request)
        {
            var skip = (request.PageNumber - 1) * request.PageSize;

            TimeSpan dateDiff;
            if (request.FromDate != null && request.ToDate != null)
            {
                dateDiff = request.ToDate.Value - request.FromDate.Value;
            }
            else
            {
                dateDiff = TimeSpan.Zero;
            }

            // Step 1: Build Filters for MongoDB
            var fromDate = Convert.ToDateTime(request.FromDate).Date;
            var toDate = Convert.ToDateTime(request.ToDate).Date.AddDays(1).AddTicks(-1); // End of the day

            var filters = new List<FilterDefinition<PushNotification>>
            {
                Builders<PushNotification>.Filter.Gte(p => p.CreatedOn, fromDate),
                Builders<PushNotification>.Filter.Lte(p => p.CreatedOn, toDate),
            };

            // Handle null/missing topic
            var announcementTopicFilter = Builders<PushNotification>.Filter.Or(
                Builders<PushNotification>.Filter.Exists(p => p.Topic, false),
                Builders<PushNotification>.Filter.Not(Builders<PushNotification>.Filter.Regex(p => p.Topic, new BsonRegularExpression("^announcement$", "i")))
            );

            filters.Add(announcementTopicFilter);
            var projection = Builders<PushNotification>.Projection.Exclude("exchange");

            if (dateDiff.Days > 1 && request.LoggedInUser != null)
            {
                if (!string.IsNullOrEmpty(request.PrimaryKey))
                {
                    filters.Add(Builders<PushNotification>.Filter.Eq(p => p.Topic, request.PrimaryKey));
                }

                // Combine filters
                var filter = Builders<PushNotification>.Filter.And(filters);

                // Step 2: Query MongoDB
                var mongoNotifications = await _pushNotificationCollection
                .Find(filter)
                .Sort(Builders<PushNotification>.Sort.Descending(p => p.CreatedOn))
                .Project<PushNotification>(projection)
                .Skip(skip)
                .Limit(request.PageSize).ToListAsync();


                // Step 3: Map MongoDB Results
                var scannerNotifications = mongoNotifications.Select(x => new ScannerNotification
                {
                    IsRead = false,
                    ObjectId = x.ObjectId,
                    TradingSymbol = x.TradingSymbol,
                    CreatedOn = (x.CreatedOn).ToString("HH:mm, MMM dd"),
                    Title = x.Title,
                    Message = x.Message,
                    Price = (int)x.Price,
                    TransactionType = x.TransactionType,
                    Topic = x.Topic,
                    ViewChart = ""
                }).OrderByDescending(c => c.Created).ToList();


                var sqlNotifications = await _context.ScannerPerformanceM
                   .Where(x =>

                       x.SentAt.Date >= request.FromDate &&
                       x.SentAt.Date <= request.ToDate)
                   .Select(x => new ScannerNotification
                   {
                       IsRead = false,
                       ObjectId = "",
                       TradingSymbol = x.TradingSymbol,
                       CreatedOn = x.SentAt.ToString("HH:mm, MMM dd"),
                       Title = "",
                       Message = x.Message,
                       Price = Convert.ToInt32(x.Ltp ?? 0),
                       TransactionType = "",
                       Topic = x.Topic,
                       ViewChart = x.ViewChart,
                       Created = x.SentAt.ToString()
                   })
                   .OrderByDescending(c => c.Created)
                   .ToListAsync();

                if (!string.IsNullOrEmpty(request.PrimaryKey))
                {
                    sqlNotifications = sqlNotifications.Where(x => x.Topic == request.PrimaryKey).ToList();
                    scannerNotifications.AddRange(sqlNotifications);
                }
                return scannerNotifications.Skip(skip).Take(request.PageSize).OrderByDescending(x => string.IsNullOrEmpty(x.Created) ? DateTime.MinValue : DateTime.Parse(x.Created)).ToList();
            }
            else
            {
                if (!string.IsNullOrEmpty(request.PrimaryKey))
                {
                    var hasProudctAccess = await _context.MyBucketM.Where(item => item.ProductName == request.PrimaryKey && DateTime.Now > item.EndDate && item.IsActive).AnyAsync();

                    if (hasProudctAccess)
                    {
                        filters.Add(Builders<PushNotification>.Filter.Eq(p => p.Topic, request.PrimaryKey));
                    }
                    else
                    {
                        filters.Add(Builders<PushNotification>.Filter.Eq(p => p.Topic, "__NO_MATCH__"));
                    }
                }


                // Combine filters
                var filter = Builders<PushNotification>.Filter.And(filters);

                // Step 2: Query MongoDB
                var mongoNotifications = await _pushNotificationCollection
                .Find(filter)
                .Sort(Builders<PushNotification>.Sort.Descending(p => p.CreatedOn))
                .Project<PushNotification>(projection)
                .Skip(skip)
                .Limit(request.PageSize).ToListAsync();


                // Step 3: Map MongoDB Results
                var scannerNotifications = mongoNotifications.Select(x => new ScannerNotification
                {
                    IsRead = false,
                    ObjectId = x.ObjectId,
                    TradingSymbol = x.TradingSymbol,
                    CreatedOn = (x.CreatedOn).ToString("HH:mm, MMM dd"),
                    Title = x.Title,
                    Message = x.Message,
                    Price = (int)x.Price,
                    TransactionType = x.TransactionType,
                    Topic = x.Topic,
                    ViewChart = ""
                }).OrderByDescending(c => c.Created).ToList();


                return scannerNotifications;
            }
        }
        public async Task<List<ScannerNotification>?> GetScannerNotification(QueryValues request)
        {
            var skip = (request.PageNumber - 1) * request.PageSize;

            // Convert dates safely
            var fromDate = request.FromDate?.Date ?? DateTime.MinValue;
            var toDate = (request.ToDate?.Date ?? DateTime.UtcNow).AddDays(1).AddTicks(-1);

            // Debugging logs (optional)
            Console.WriteLine($"Filtering Data From: {fromDate:o} To: {toDate:o}");

            // Base filters
            var filters = new List<FilterDefinition<PushNotification>>
            {
                Builders<PushNotification>.Filter.Gte(p => p.CreatedOn, fromDate),
                Builders<PushNotification>.Filter.Lte(p => p.CreatedOn, toDate),
            };

            // Handle announcement topic
            var announcementTopicFilter = Builders<PushNotification>.Filter.Or(
                Builders<PushNotification>.Filter.Exists(p => p.Topic, false),
                Builders<PushNotification>.Filter.Not(Builders<PushNotification>.Filter.Regex(p => p.Topic, new BsonRegularExpression("^announcement$", "i")))
            );

            filters.Add(announcementTopicFilter);

            List<string> subscribedProductNames = new List<string>();
            bool hasActiveSubscription = false;
            if (request.LoggedInUser != null)
            {
                var subscribedProductCodesRaw = await _context.MyBucketM
                    .Where(item => item.MobileUserKey == Guid.Parse(request.LoggedInUser) &&
                                   item.EndDate.HasValue &&
                                   DateTime.UtcNow <= item.EndDate.Value.AddDays(1).AddSeconds(-1) &&
                                   item.IsActive)
                    .Join(
                        _context.ProductsM,
                        bucketItem => bucketItem.ProductId,
                        product => product.Id,
                        (bucketItem, product) => product.Code
                    )
                    .ToListAsync();
                subscribedProductNames = subscribedProductCodesRaw
                .Select(code => code.Replace(" ", "").ToUpper())
                .ToList();

                hasActiveSubscription = subscribedProductNames.Any();
            }


            if (fromDate.Date == DateTime.Now.Date && toDate.Date == DateTime.Now.Date)
            {
                if (hasActiveSubscription)
                {
                    if (subscribedProductNames.Any())
                    {
                        filters.Add(Builders<PushNotification>.Filter.In(p => p.Topic, subscribedProductNames));
                    }
                }
                else
                {
                    filters.Add(Builders<PushNotification>.Filter.Eq(p => p.Topic, "__NO_MATCH__"));
                }
            }
            else
            {
                if (request.PrimaryKey != null)
                {
                    filters.Add(Builders<PushNotification>.Filter.Eq(p => p.Topic, request.PrimaryKey.ToUpper()));
                }

            }


            // Final MongoDB filter
            var filter = Builders<PushNotification>.Filter.And(filters);

            // Query MongoDB
            var mongoNotifications = (await _pushNotificationCollection
                .Find(filter)
                .SortByDescending(p => p.CreatedOn)
                .Skip(skip)
                .Limit(request.PageSize)
                .ToListAsync()) // Fetch data first
                .Select(p => new ScannerNotification
                {
                    IsRead = false,
                    ObjectId = p.ObjectId,
                    TradingSymbol = p.TradingSymbol,
                    CreatedOn = p.CreatedOn.ToString("HH:mm, MMM dd"), // Format after fetching
                    Title = p.Title,
                    Message = p.Message,
                    Price = (int)p.Price,
                    TransactionType = p.TransactionType,
                    Topic = p.Topic,
                    ViewChart = ""
                })
                .ToList();


            // If date range is large, merge with SQL notifications
            if (fromDate.Date != DateTime.Now.Date && toDate.Date != DateTime.Now.Date && request.LoggedInUser != null)
            {
                var sqlNotifications = await _context.ScannerPerformanceM
                    .Where(x => x.SentAt.Date >= fromDate && x.SentAt.Date <= toDate)
                    .Select(x => new ScannerNotification
                    {
                        IsRead = false,
                        ObjectId = "",
                        TradingSymbol = x.TradingSymbol,
                        CreatedOn = x.SentAt.ToLocalTime().ToString("HH:mm, MMM dd"),
                        Title = "",
                        Message = x.Message,
                        Price = Convert.ToInt32(x.Ltp ?? 0),
                        TransactionType = "",
                        Topic = x.Topic,
                        ViewChart = x.ViewChart,
                        Created = x.SentAt.ToString()
                    })
                    .ToListAsync();

                // Filter SQL notifications by PrimaryKey if provided
                if (!string.IsNullOrEmpty(request.PrimaryKey))
                {
                    sqlNotifications = sqlNotifications.Where(x => x.Topic == request.PrimaryKey.ToUpper()).ToList();
                }

                // Merge MongoDB & SQL results, apply final pagination & sorting
                mongoNotifications.AddRange(sqlNotifications);
                return mongoNotifications
                .OrderByDescending(x => DateTime.TryParse(x.CreatedOn, out var date) ? date : DateTime.MinValue)
                .Skip(skip)
                .Take(request.PageSize)
                .ToList();
            }

            return mongoNotifications;
        }
        //public async Task<List<ScannerNotification>?> GetScannerNotification(QueryValues request)
        //{
        //    var skip = (request.PageNumber - 1) * request.PageSize;

        //    // Convert dates safely
        //    var fromDate = request.FromDate?.Date ?? DateTime.MinValue;
        //    var toDate = (request.ToDate?.Date ?? DateTime.UtcNow).AddDays(1).AddTicks(-1);


        //    // Get SQL data
        //    var sqlNotifications = await _context.ScannerPerformanceM
        //        .Where(x => x.SentAt.Date >= fromDate && x.SentAt.Date <= toDate)
        //        .Select(x => new ScannerNotification
        //        {
        //            IsRead = false,
        //            ObjectId = "",
        //            TradingSymbol = x.TradingSymbol,
        //            CreatedOn = x.SentAt.ToString("HH:mm, MMM dd"),
        //            Title = "",
        //            Message = x.Message,
        //            Price = Convert.ToInt32(x.Ltp ?? 0),
        //            TransactionType = "",
        //            Topic = x.Topic,
        //            ViewChart = x.ViewChart,
        //            Created = x.SentAt.ToString()
        //        })
        //        .ToListAsync();

        //    // Apply topic filter to SQL results if needed
        //    if (!string.IsNullOrEmpty(request.PrimaryKey))
        //    {
        //        sqlNotifications = sqlNotifications.Where(x => x.Topic == request.PrimaryKey).ToList();
        //    }

        //    // Combine and apply pagination at the end

        //    return sqlNotifications
        //        .OrderByDescending(x => DateTime.TryParse(x.Created, out var date) ? date : DateTime.MinValue)
        //        .Skip(skip)
        //        .Take(request.PageSize)
        //        .ToList();
        //}

        public async Task<Dictionary<string, List<ScannerNotification>>> GetTodayScannerNotification(Guid loggedInUser, int[]? Ids)
        {
            var fromDateUtc = DateTime.UtcNow.Date;
            var toDateUtc = fromDateUtc.AddDays(1).AddTicks(-1);

            Console.WriteLine($"Filtering Data From (UTC): {fromDateUtc:o} To (UTC): {toDateUtc:o}");

            Dictionary<string, string> productCodeToTitleCaseMap = new Dictionary<string, string>();
            bool hasActiveSubscription = false;

            if (Ids != null & Ids.Any())
            {
                // Get the product code for the specific product
                var productCodes = await _context.ProductsM
                    .Where(p => Ids.Contains(p.Id) && p.IsActive).Select(p => p.Code)
                    .ToListAsync();

                if (productCodes.Any())
                {
                    hasActiveSubscription = true;
                    TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
                    productCodeToTitleCaseMap = productCodes
                        .Select(code => code.Replace(" ", "").ToUpperInvariant())
                        .Distinct()
                        .ToDictionary(
                            upperCaseCode => upperCaseCode,
                            upperCaseCode => textInfo.ToTitleCase(upperCaseCode.ToLowerInvariant())
                        );
                }
            }
            else if (loggedInUser != Guid.Empty)
            {
                var subscribedProductCodesRaw = await (
                    from bucket in _context.MyBucketM
                    join product in _context.ProductsM on bucket.ProductId equals product.Id
                    join category in _context.ProductCategoriesM on product.CategoryID equals category.Id
                    where bucket.MobileUserKey == loggedInUser &&
                          bucket.EndDate.HasValue &&
                          DateTime.UtcNow <= bucket.EndDate.Value.AddDays(1).AddSeconds(-1) &&
                          bucket.IsActive &&
                          category.Code == "LIVETRADING"
                    select product.Code
                ).ToListAsync();

                if (subscribedProductCodesRaw.Any())
                {
                    hasActiveSubscription = true;
                    TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
                    productCodeToTitleCaseMap = subscribedProductCodesRaw
                        .Select(code => code.Replace(" ", "").ToUpperInvariant())
                        .Distinct()
                        .ToDictionary(
                            upperCaseCode => upperCaseCode,
                            upperCaseCode => textInfo.ToTitleCase(upperCaseCode.ToLowerInvariant())
                        );
                }
            }

            // Initialize dictionary with empty lists
            var notificationsByProduct = productCodeToTitleCaseMap.Values
                .Distinct()
                .ToDictionary(titleCaseCode => titleCaseCode, titleCaseCode => new List<ScannerNotification>());

            if (!hasActiveSubscription)
            {
                return notificationsByProduct;
            }

            var filter = Builders<PushNotification>.Filter.And(
                Builders<PushNotification>.Filter.Gte(p => p.CreatedOn, fromDateUtc),
                Builders<PushNotification>.Filter.Lte(p => p.CreatedOn, toDateUtc),
                Builders<PushNotification>.Filter.In(p => p.Topic, productCodeToTitleCaseMap.Keys)
            );

            var mongoNotifications = await _pushNotificationCollection
                .Find(filter)
                .SortByDescending(p => p.CreatedOn)
                .ToListAsync();

            // Step 2: Add notifications to their respective category lists
            foreach (var notification in mongoNotifications)
            {
                if (productCodeToTitleCaseMap.TryGetValue(notification.Topic, out var titleCaseProductCode))
                {
                    notificationsByProduct[titleCaseProductCode].Add(new ScannerNotification
                    {
                        IsRead = false,
                        ObjectId = notification.ObjectId,
                        TradingSymbol = notification.TradingSymbol,
                        CreatedOn = notification.CreatedOn.ToLocalTime().ToString("HH:mm, MMM dd"),
                        Title = notification.Title,
                        Message = notification.Message,
                        Price = (int)notification.Price,
                        TransactionType = notification.TransactionType,
                        Topic = notification.Topic,
                        ViewChart = ""
                    });
                }
            }

            return notificationsByProduct;


            //.Where(kv => kv.Value != null && kv.Value.Any())
            //.ToDictionary(kv => kv.Key, kv => kv.Value);
        }
        public async Task<bool> SavePushNotification(List<PushNotification> notification)
        {
            await _pushNotificationCollection.InsertManyAsync(notification);
            return true;
        }
        public async Task<string> SaveNotificationDataAsync(PushNotification notification, List<UserListForPushNotificationModel> userReceiverList)
        {
            try
            {
                await _pushNotificationCollection.InsertOneAsync(notification);

                if (userReceiverList?.Any() == true) // More concise check
                {
                    var receivers = userReceiverList.Select(item => new PushNotificationReceiver
                    {
                        ReceivedBy = item.PublicKey,
                        NotificationId = notification.ObjectId
                    }).ToList();

                    await SaveNotificationReceiverDataAsync(receivers);
                }

                return notification.ObjectId;
            }
            catch (Exception ex)
            {
                //await LogExceptions(ex, source: "FcmToken");
                return string.Empty; // Use string.Empty instead of ""
            }
        }
        public string SaveNotificationData(PushNotification notification)
        {
            try
            {
                _pushNotificationCollection.InsertOne(notification);
                return notification.ObjectId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return "";
            }
        }
        private async Task<bool> SaveNotificationReceiverDataAsync(List<PushNotificationReceiver> receivers)
        {
            try
            {
                //await _pushNotificationCollection.InsertOneAsync(notification);
                await _pushNotificationCollectionReceiver.InsertManyAsync(receivers);
                return true;
            }
            catch (Exception ex)
            {
                //await LogExceptions(ex, source: "FcmToken");
                return false;
            }
        }

        public bool SaveNotificationReceiverData(List<PushNotificationReceiver> receivers)
        {
            try
            {
                //await _pushNotificationCollection.InsertOneAsync(notification);
                _pushNotificationCollectionReceiver.InsertMany(receivers);
                return true;
            }
            catch (Exception ex)
            {
                // LogExceptions(ex);
                return false;
            }
        }

        public async Task<bool> SaveNotificationReceiverData(PushNotificationReceiver receivers)
        {
            try
            {
                //await _pushNotificationCollection.InsertOneAsync(notification);
                await _pushNotificationCollectionReceiver.InsertOneAsync(receivers);
                return true;
            }
            catch (Exception ex)
            {
                // LogExceptions(ex);
                return false;
            }
        }

        public async Task<ApiCommonResponseModel> GetMobileNotificationsAsync(QueryValues query)
        {
            int skip = (query.PageNumber - 1) * query.PageSize;

            var pipeline = new List<BsonDocument>
            {
                // Perform lookup to join with PushNotification collection
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "PushNotification" },
                    { "localField", "_id" },
                    { "foreignField", "_id" },
                    { "as", "notificationTempTable" }
                }),
                // Unwind the joined array
                new BsonDocument("$unwind", "$notificationTempTable")
            };

            // Apply filters
            var matchStage = BuildMatchStage(query);
            if (!matchStage.Equals(new BsonDocument()))
            {
                pipeline.Add(matchStage);
            }

            // Count total records before pagination
            var countPipeline = pipeline.ToList();
            countPipeline.Add(new BsonDocument("$count", "total"));
            var countResult = await _pushNotificationCollection.Aggregate<BsonDocument>(countPipeline).FirstOrDefaultAsync();
            int totalCount = countResult != null ? countResult["total"].AsInt32 : 0;

            // Sort by IsPinned (descending) and CreatedOn (descending)
            pipeline.Add(new BsonDocument("$sort", new BsonDocument
            {
                { "notificationTempTable.IsPinned", -1 },
                { "notificationTempTable.CreatedOn", -1 }
            }));

            // Set default values for nullable fields
            pipeline.Add(new BsonDocument("$addFields", new BsonDocument
            {
                { "notificationTempTable.IsPinned", new BsonDocument("$ifNull", new BsonArray { "$notificationTempTable.IsPinned", false }) },
                { "notificationTempTable.ImageUrl", new BsonDocument("$ifNull", new BsonArray { "$notificationTempTable.ImageUrl", "" }) }
            }));

            // Apply pagination
            pipeline.Add(new BsonDocument("$skip", skip));
            pipeline.Add(new BsonDocument("$limit", query.PageSize));

            // Project only the required fields
            pipeline.Add(new BsonDocument("$project", new BsonDocument
            {
                { "_id", 1 },
                { "Title", "$notificationTempTable.Title" },
                { "Message", "$notificationTempTable.Message" },
                { "CreatedOn", "$notificationTempTable.CreatedOn" },
                { "Topic", "$notificationTempTable.Topic" },
                { "IsPinned", "$notificationTempTable.IsPinned" },
                { "ScreenName", "$notificationTempTable.ScreenName" },
                { "ImageUrl", "$notificationTempTable.ImageUrl" }
            }));

            // Execute the aggregation pipeline
            var result = await _pushNotificationCollection
                .Aggregate<PushNotification>(pipeline)
                .ToListAsync();

            // Get unique topics
            var uniqueTopics = await _pushNotificationCollection
                .Distinct<string>("Topic", Builders<PushNotification>.Filter.Empty)
                .ToListAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Notifications retrieved successfully",
                Data = new
                {
                    Notifications = result,
                    Topics = uniqueTopics
                },
                Total = totalCount
            };
        }


        private BsonDocument BuildMatchStage(QueryValues query)
        {
            var matchConditions = new List<BsonDocument>();

            // Handle date filters if provided
            if (query.FromDate.HasValue || query.ToDate.HasValue)
            {
                var dateFilter = new BsonDocument();
                if (query.FromDate.HasValue)
                {
                    DateTime utcFromDate = DateTime.SpecifyKind(query.FromDate.Value.Date, DateTimeKind.Utc);
                    dateFilter.Add("$gte", utcFromDate);
                }
                if (query.ToDate.HasValue)
                {
                    DateTime utcToDate = DateTime.SpecifyKind(query.ToDate.Value.Date, DateTimeKind.Utc)
                        .AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999);
                    dateFilter.Add("$lte", utcToDate);
                }
                matchConditions.Add(new BsonDocument("notificationTempTable.CreatedOn", dateFilter));
            }

            // Handle IsDelete filter: false, null, or not existing
            matchConditions.Add(new BsonDocument("$or", new BsonArray
            {
                new BsonDocument("notificationTempTable.IsDelete", false),                  // IsDelete is false
                new BsonDocument("notificationTempTable.IsDelete", BsonNull.Value),        // IsDelete is null
                new BsonDocument("notificationTempTable.IsDelete", new BsonDocument("$exists", false)) // IsDelete does not exist
            }));

            // Search by text if provided
            if (!string.IsNullOrWhiteSpace(query.SearchText))
            {
                var searchRegex = new BsonRegularExpression(query.SearchText, "i"); // Case-insensitive search
                matchConditions.Add(new BsonDocument("$or", new BsonArray
                {
                    new BsonDocument("notificationTempTable.Title", searchRegex),
                    new BsonDocument("notificationTempTable.Message", searchRegex)
                }));
            }

            // Filter by PrimaryKey (Topic) if provided
            if (!string.IsNullOrWhiteSpace(query.PrimaryKey))
            {
                matchConditions.Add(new BsonDocument("notificationTempTable.Topic", query.PrimaryKey));
            }

            // If no conditions, return empty BsonDocument (matches all)
            if (matchConditions.Count == 0)
                return new BsonDocument();

            // Combine all conditions with $and
            return new BsonDocument("$match", new BsonDocument("$and", new BsonArray(matchConditions)));
        }
        //public async Task<List<PushNotification>> GetSpecificTopicNotification(string topic = "KALKIBAATAAJ")
        //{
        //    var filter = Builders<PushNotification>.Filter.Eq(p => p.Topic, topic);
        //    var result = await _pushNotificationCollection.Find(filter).ToListAsync();
        //    return result;
        //}

        public async Task<ApiCommonResponseModel> UpdatePinnedStatusAsync(string notificationId, bool isPinned)
        {
            if (string.IsNullOrEmpty(notificationId))
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Notification ID cannot be null or empty."
                };
            }

            var filter = Builders<PushNotification>.Filter.Eq(n => n.ObjectId, notificationId);

            var update = Builders<PushNotification>.Update.Set(n => n.IsPinned, !isPinned);

            var updateResult = await _pushNotificationCollection.UpdateOneAsync(filter, update);

            if (updateResult.ModifiedCount > 0)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Pinned status updated successfully."
                };
            }
            else if (updateResult.MatchedCount == 0)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Notification not found."
                };
            }
            else
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Failed to update pinned status."
                };
            }
        }

        public async Task<List<PushNotificationResponse>> GetPushNotification(GetNotificationRequestModel request)
        {
            var skip = (request.PageNumber - 1) * request.PageSize;
            var pipeline = new List<BsonDocument>();

            // 1. Base match
            var baseMatch = new BsonDocument
            {
                { "ReceivedBy", request.MobileUserKey.ToString().ToLower() },
                { "IsDelete", false }
            };

            // 2. Add IsRead filter if needed
            if (request.ShowUnread)
                baseMatch.Add("IsRead", false);

            pipeline.Add(new BsonDocument("$match", baseMatch));

            // 3. Lookup and unwind
            pipeline.AddRange(new[]
            {
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "PushNotification" },
                    { "localField", "NotificationId" },
                    { "foreignField", "_id" },
                    { "as", "notificationTempTable" }
                }),
                new BsonDocument("$unwind", "$notificationTempTable"),
                new BsonDocument("$match", new BsonDocument("notificationTempTable.Scanner", false)),
                new BsonDocument("$sort", new BsonDocument
                {
                    { "notificationTempTable.IsPinned", -1 },
                    { "notificationTempTable.CreatedOn", -1 }
                }),
                new BsonDocument("$skip", skip),
                new BsonDocument("$limit", request.PageSize),
                new BsonDocument("$project", new BsonDocument
                {
                    { "_id", 0 },
                    { "ObjectId", new BsonDocument("$toString", "$_id") },
                    { "NotificationId", new BsonDocument("$toString", "$NotificationId") },
                    { "ReceivedBy", 1 },
                    { "Message", "$notificationTempTable.Message" },
                    { "Title", "$notificationTempTable.Title" },
                    { "CreatedBy", "$notificationTempTable.CreatedBy" },
                    { "EnableTradingButton", "$notificationTempTable.EnableTradingButton" },
                    { "AppCode", "$notificationTempTable.AppCode" },
                    { "Exchange", "$notificationTempTable.Exchange" },
                    { "TradingSymbol", "$notificationTempTable.TradingSymbol" },
                    { "TransactionType", "$notificationTempTable.TransactionType" },
                    { "OrderType", "$notificationTempTable.OrderType" },
                    { "Price", new BsonDocument("$toInt", "$notificationTempTable.Price") },
                    { "ProductId", "$notificationTempTable.ProductId" },
                    { "ProductName", "$notificationTempTable.ProductName" },
                    { "Complexity", "$notificationTempTable.Complexity" },
                    { "CategoryId", "$notificationTempTable.CategoryId" },
                    { "IsRead", 1 },
                    { "IsDelete", "$notificationTempTable.IsDelete" },
                    { "ReadDate", 1 },
                    { "IsPinned", "$notificationTempTable.IsPinned" },
                    { "Topic", "$notificationTempTable.Topic" },
                    { "ScreenName", "$notificationTempTable.ScreenName" },
                    { "CreatedOn", "$notificationTempTable.CreatedOn" }
                })
            });

            var result = await _pushNotificationCollectionReceiver
                .Aggregate<PushNotificationResponse>(pipeline)
                .ToListAsync();

            return result;
        }


        ////public async Task<List<PushNotificationResponse>> GetPushNotification(GetNotificationRequestModel request)
        ////{
        ////    var skip = (request.PageNumber - 1) * request.PageSize;

        ////    var pipeline = new[]
        ////    {
        ////        new BsonDocument("$match", new BsonDocument
        ////        {
        ////            { "ReceivedBy", request.MobileUserKey.ToString().ToLower() },
        ////            { "IsDelete", false }
        ////        }),
        ////        new BsonDocument("$lookup", new BsonDocument
        ////        {
        ////            { "from", "PushNotification" },
        ////            { "localField", "NotificationId" },
        ////            { "foreignField", "_id" },
        ////            { "as", "notificationTempTable" }
        ////        }),
        ////        new BsonDocument("$unwind", "$notificationTempTable"),
        ////        new BsonDocument("$match", new BsonDocument("notificationTempTable.Scanner", false)),
        ////          new BsonDocument("$sort", new BsonDocument
        ////          {
        ////                { "notificationTempTable.IsPinned", -1 },
        ////                { "notificationTempTable.CreatedOn", -1 }
        ////          }),
        ////        new BsonDocument("$skip", skip),
        ////        new BsonDocument("$limit", request.PageSize),
        ////        new BsonDocument("$project", new BsonDocument
        ////        {
        ////            { "_id", 0 },
        ////            { "ObjectId", new BsonDocument("$toString", "$_id") },
        ////            { "NotificationId", new BsonDocument("$toString", "$NotificationId") },
        ////            { "ReceivedBy", 1 },
        ////            { "Message", "$notificationTempTable.Message" },
        ////            { "Title", "$notificationTempTable.Title" },
        ////            { "CreatedBy", "$notificationTempTable.CreatedBy" },
        ////            { "EnableTradingButton", "$notificationTempTable.EnableTradingButton" },
        ////            { "AppCode", "$notificationTempTable.AppCode" },
        ////            { "Exchange", "$notificationTempTable.Exchange" },
        ////            { "TradingSymbol", "$notificationTempTable.TradingSymbol" },
        ////            { "TransactionType", "$notificationTempTable.TransactionType" },
        ////            { "OrderType", "$notificationTempTable.OrderType" },
        ////            { "Price", new BsonDocument("$toInt", "$notificationTempTable.Price") },
        ////            { "ProductId", "$notificationTempTable.ProductId" },
        ////            { "ProductName", "$notificationTempTable.ProductName" },
        ////            { "Complexity", "$notificationTempTable.Complexity" },
        ////            { "CategoryId", "$notificationTempTable.CategoryId" },
        ////            { "IsRead", 1 },
        ////            { "IsDelete", "$notificationTempTable.IsDelete" },
        ////            { "ReadDate", 1 },
        ////            { "IsPinned", "$notificationTempTable.IsPinned" },
        ////            { "Topic", "$notificationTempTable.Topic" },
        ////            { "ScreenName", "$notificationTempTable.ScreenName" },
        ////            { "CreatedOn","$notificationTempTable.CreatedOn"}
        ////        })
        ////    };

        ////    var result = await _pushNotificationCollectionReceiver.Aggregate<PushNotificationResponse>(pipeline).ToListAsync();

        ////    return result;
        ////}

        public async Task<List<PushNotificationResponse>> GetPushNotificationOldV1(GetNotificationRequestModel request)
        {
            var skip = (request.PageNumber - 1) * request.PageSize;

            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument
                {
                    { "ReceivedBy", request.MobileUserKey.ToString().ToLower() },
                    { "IsDelete", false }
                }),
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "PushNotification" },
                    { "localField", "NotificationId" },
                    { "foreignField", "_id" },
                    { "as", "notificationTempTable" }
                }),
                new BsonDocument("$unwind", "$notificationTempTable"),
                new BsonDocument("$match", new BsonDocument("notificationTempTable.Scanner", false)),
                new BsonDocument("$addFields", new BsonDocument
                {
                    { "sortDate", new BsonDocument("$dateFromString", new BsonDocument
                        {
                            { "dateString", "$notificationTempTable.CreatedOn" },
                            { "onError", new BsonDateTime(DateTime.MinValue) }
                        })
                    }
                }),
                new BsonDocument("$sort", new BsonDocument
                {
                    { "notificationTempTable.IsPinned", -1 },
                    { "sortDate", -1 }
                }),
                new BsonDocument("$skip", skip),
                new BsonDocument("$limit", request.PageSize),
                new BsonDocument("$project", new BsonDocument
                {
                    { "_id", 0 },
                    { "ObjectId", new BsonDocument("$toString", "$_id") },
                    { "NotificationId", new BsonDocument("$toString", "$NotificationId") },
                    { "ReceivedBy", 1 },
                    { "Message", "$notificationTempTable.Message" },
                    { "Title", "$notificationTempTable.Title" },
                    { "CreatedBy", "$notificationTempTable.CreatedBy" },
                    { "EnableTradingButton", "$notificationTempTable.EnableTradingButton" },
                    { "AppCode", "$notificationTempTable.AppCode" },
                    { "Exchange", "$notificationTempTable.Exchange" },
                    { "TradingSymbol", "$notificationTempTable.TradingSymbol" },
                    { "TransactionType", "$notificationTempTable.TransactionType" },
                    { "OrderType", "$notificationTempTable.OrderType" },
                    { "Price", new BsonDocument("$toInt", "$notificationTempTable.Price") },
                    { "ProductId", "$notificationTempTable.ProductId" },
                    { "ProductName", "$notificationTempTable.ProductName" },
                    { "Complexity", "$notificationTempTable.Complexity" },
                    { "CategoryId", "$notificationTempTable.CategoryId" },
                    { "IsRead", 1 },
                    { "IsDelete", "$notificationTempTable.IsDelete" },
                    { "ReadDate", 1 },
                    { "IsPinned", "$notificationTempTable.IsPinned" },
                    { "Topic", "$notificationTempTable.Topic" },
                    { "ScreenName", "$notificationTempTable.ScreenName" },
                    { "CreatedOn", new BsonDocument("$dateToString", new BsonDocument
                        {
                            { "date", "$sortDate" },
                            { "format", "%H:%M, %b %d" }
                        })
                    }
                })
            };

            var result = await _pushNotificationCollectionReceiver.Aggregate<PushNotificationResponse>(pipeline).ToListAsync();

            return result;
        }

        public async Task<List<PushNotificationResponse>?> GetPushNotificationOld(GetNotificationRequestModel request)
        {

            var skip = (request.PageNumber - 1) * request.PageSize;
            var result = await _pushNotificationCollectionReceiver.Aggregate()
                .Match(new BsonDocument
                {
                        { "ReceivedBy", request.MobileUserKey.ToString().ToLower() },
                        { "IsDelete", false }
                })
                .Lookup("PushNotification", "NotificationId", "_id", "notificationTempTable")
                .Unwind("notificationTempTable")
                .Match(new BsonDocument
                {
                        {
                            "notificationTempTable.Scanner",
                            false
                        }
                })
                .AppendStage<BsonDocument>(
                    "{ $addFields: { sortDate: { $dateFromString: { dateString: '$notificationTempTable.CreatedOn', onError: new Date(0) } } } }")
                .Sort(new BsonDocument
                {
                        { "notificationTempTable.IsPinned", -1 },
                        { "sortDate", -1 }
                })
                .Skip(skip)
                .Limit(request.PageSize)
                .Project(new BsonDocument
                {
                        {
                            "_id",
                            0
                        },
                        {
                            "ObjectId",
                            new BsonDocument("$toString", "$_id")
                        },
                        {
                            "NotificationId",
                            new BsonDocument("$toString", "$NotificationId")
                        },
                        {
                            "ReceivedBy",
                            1
                        },
                        {
                            "Message",
                            "$notificationTempTable.Message"
                        },
                        {
                            "Title",
                            "$notificationTempTable.Title"
                        },
                        {
                            "CreatedBy",
                            "$notificationTempTable.CreatedBy"
                        },
                        {
                            "EnableTradingButton",
                            "$notificationTempTable.EnableTradingButton"
                        },
                        {
                            "AppCode",
                            "$notificationTempTable.AppCode"
                        },
                        {
                            "Exchange",
                            "$notificationTempTable.Exchange"
                        },
                        {
                            "TradingSymbol",
                            "$notificationTempTable.TradingSymbol"
                        },
                        {
                            "TransactionType",
                            "$notificationTempTable.TransactionType"
                        },
                        {
                            "OrderType",
                            "$notificationTempTable.OrderType"
                        },
                        {
                            "Price",
                            new BsonDocument("$toInt", "$notificationTempTable.Price")
                        },
                        {
                            "ProductName",
                            "$notificationTempTable.ProductName"
                        },
                        {
                            "Complexity",
                            "$notificationTempTable.Complexity"
                        },
                        {
                            "CategoryId",
                            "$notificationTempTable.CategoryId"
                        },
                        {
                            "IsRead",
                            1
                        },
                        {
                            "IsDelete",
                            "$notificationTempTable.IsDelete"
                        },
                        {
                            "ReadDate",
                            1
                        },
                        {
                            "IsPinned",
                            "$notificationTempTable.IsPinned"
                        },
                        {
                            "Topic",
                            "$notificationTempTable.Topic"
                        },
                        {
                            "ScreenName",
                            "$notificationTempTable.ScreenName"
                        },
                        {
                            "CreatedOn",
                            new BsonDocument("$dateToString",
                                new BsonDocument
                                {
                                    {
                                        "date",
                                        "$sortDate"
                                    },
                                    {
                                        "format",
                                        "%H:%M, %b %d"
                                    }
                                })
                        }
                })
                .ToListAsync();

            var aggregationJson =
                System.Text.Json.JsonSerializer.Deserialize<List<PushNotificationResponse>>(result.ToJson());

            return aggregationJson;

        }

        public async Task<bool> DeleteNotificationAsync(string notificationId)
        {
            var notificationObjectId = new ObjectId(notificationId);

            var updateReceiverResult = await _pushNotificationCollectionReceiver.UpdateOneAsync(
                Builders<PushNotificationReceiver>.Filter.And(
                    Builders<PushNotificationReceiver>.Filter.Eq("_id", notificationObjectId)
                ),
                Builders<PushNotificationReceiver>.Update.Set("IsDelete", true)
            );

            return updateReceiverResult.ModifiedCount > 0;
        }


        #endregion Notification Related Methods

        #region Users Related Methods

        public async Task<bool> SaveUserActivity(UserActivityEnum activityType, Guid mobileUserKey, int? productId)
        {
            var filter = Builders<UserActivity>.Filter.And(
                Builders<UserActivity>.Filter.Eq(ua => ua.CreatedBy, mobileUserKey),
                Builders<UserActivity>.Filter.Eq(ua => ua.ProductId, productId),
                Builders<UserActivity>.Filter.Eq(ua => ua.ActivityType, (int)activityType)
            );
            var existingActivity = await _userActivityCollection.Find(filter).FirstOrDefaultAsync();

            if (existingActivity == null)
            {
                var userActivity = new UserActivity
                {
                    ActivityType = (int)activityType,
                    CreatedBy = mobileUserKey,
                    ProductId = productId,
                };

                await _userActivityCollection.InsertOneAsync(userActivity);
                return true;
            }
            else
            {
                // Optional: Log or handle the case where a duplicate exists
                return false;
            }
        }

        public async Task<string?> AddUser(User user)
        {
            var filter = Builders<User>.Filter.Eq(b => b.PublicKey, user.PublicKey);

            var userExists = await _userCollection.Find(filter).FirstOrDefaultAsync();
            if (userExists == null)
            {
                await _userCollection.InsertOneAsync(user);
                return user.ObjectId;
            }
            else
            {
                var update = Builders<User>.Update.Set(u => u.FullName, user.FullName)
                    .Set(u => u.ModifiedOn, DateTime.UtcNow).Set(u => u.ProfileImage, user.ProfileImage)
                    .Set(u => u.CanCommunityPost, user.CanCommunityPost).Set(u => u.Gender, user.Gender);
                var updateResult = await _userCollection.UpdateOneAsync(filter, update);

                return updateResult.ModifiedCount > 0 ? userExists.ObjectId : null;
            }
        }

        public async Task<ApiCommonResponseModel> BlockUser(BlockUserRequestModel request)
        {
            var responseModel = new ApiCommonResponseModel();

            var user = await _userCollection.Find(x => x.PublicKey == request.UserKey).FirstOrDefaultAsync();
            var userId = user.ObjectId;

            if (userId == request.BlockedId)
            {
                responseModel.StatusCode = HttpStatusCode.Forbidden;
                responseModel.Message = "Cannot block themselves";
                return responseModel;
            }

            if (string.Equals(request.Type, "BLOCK", StringComparison.CurrentCultureIgnoreCase))
            {
                var existingBlock = _userBlockCollection.Find(x =>
                    x.BlockerId == ObjectId.Parse(userId) &&
                    x.BlockedId == ObjectId.Parse(request.BlockedId) &&
                    x.IsActive == true).FirstOrDefault();

                if (existingBlock != null)
                {
                    responseModel.StatusCode = HttpStatusCode.BadRequest;
                    responseModel.Message = "User is already blocked";
                    return responseModel;
                }

                var block = new UserBlock
                {
                    BlockerId = ObjectId.Parse(userId),
                    BlockedId = ObjectId.Parse(request.BlockedId),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _userBlockCollection.InsertOneAsync(block);
                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Message = "User blocked successfully";
            }
            else if (string.Equals(request.Type, "UNBLOCK", StringComparison.CurrentCultureIgnoreCase))
            {
                var filter = Builders<UserBlock>.Filter.And(
                    Builders<UserBlock>.Filter.Eq(x => x.BlockerId, ObjectId.Parse(userId)),
                    Builders<UserBlock>.Filter.Eq(x => x.BlockedId, ObjectId.Parse(request.BlockedId)),
                    Builders<UserBlock>.Filter.Eq(x => x.IsActive, true)
                );

                var update = Builders<UserBlock>.Update
                    .Set(x => x.IsActive, false);

                var result = await _userBlockCollection.UpdateOneAsync(filter, update);

                if (result.ModifiedCount > 0)
                {
                    responseModel.StatusCode = HttpStatusCode.OK;
                    responseModel.Message = "User unblocked successfully";
                }
                else
                {
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                    responseModel.Message = "No active block found for this user";
                }
            }
            else
            {
                responseModel.StatusCode = HttpStatusCode.BadRequest;
                responseModel.Message = "Invalid block type specified";
            }

            return responseModel;
        }

        public async Task<List<GetBlockedUser>?> GetBlockedUser(string mobileUserKey)
        {
            var user = await _userCollection.Find(x => x.PublicKey == Guid.Parse(mobileUserKey))
                .FirstOrDefaultAsync();

            if (user is null)
            {
                List<SqlParameter> sqlParameters = new()
            {
                new SqlParameter
                {
                    ParameterName = "MobileUserKey", Value = Guid.Parse(mobileUserKey),
                    SqlDbType = System.Data.SqlDbType.UniqueIdentifier
                },
            };

                GetMobileUserDetailsSpResponseModel result =
                    await _context.SqlQueryFirstOrDefaultAsync2<GetMobileUserDetailsSpResponseModel>(
                        ProcedureCommonSqlParametersText.GetMobileUserDetails, sqlParameters.ToArray());

                RM.Model.MongoDbCollection.User mongoUserDocument = new()
                {
                    FullName = result.Fullname,
                    PublicKey = result.PublicKey,
                    ProfileImage = result.ProfileImage,
                    Gender = result.Gender,
                    CanCommunityPost = result.HasActiveProduct
                };

                var insertedId = await AddUser(mongoUserDocument);

                user = await _userCollection.Find(x => x.PublicKey == mongoUserDocument.PublicKey)
               .FirstOrDefaultAsync();
            }

            PipelineDefinition<UserBlock, BsonDocument> pipeline = new BsonDocument[]
            {
                new BsonDocument("$match",
                    new BsonDocument
                    {
                        {
                            "BlockerId",
                            new ObjectId(user.ObjectId)
                        },
                        { "IsActive", true }
                    }),
                new BsonDocument("$lookup",
                    new BsonDocument
                    {
                        { "from", "User" },
                        { "localField", "BlockedId" },
                        { "foreignField", "_id" },
                        { "as", "user" }
                    }),
                new BsonDocument("$unwind",
                    new BsonDocument
                    {
                        { "path", "$user" },
                        { "preserveNullAndEmptyArrays", true }
                    }),
                new BsonDocument("$project",
                    new BsonDocument
                    {
                        { "_id", 0 },
                        {
                            "Id",
                            new BsonDocument("$toString", "$BlockedId")
                        },
                        { "fullName", "$user.FullName" },
                        //{ "profileImage", "$user.ProfileImage" },
                         {
                             "profileImage",
                             new BsonDocument("$concat", new BsonArray { _configuration["Azure:ImageUrlSuffix"], "$user.ProfileImage" })
                         },
                        { "gender", "$user.Gender" }
                    })
            };

            var aggregationResult = await _userBlockCollection.Aggregate(pipeline).ToListAsync();
            var aggregationJson = JsonConvert.DeserializeObject<List<GetBlockedUser>>(aggregationResult.ToJson());
            return aggregationJson;
        }

        public async Task<bool> RemoveProfileImage(Guid publickey)
        {
            var filter = Builders<User>.Filter.Eq(x => x.PublicKey, publickey);
            var update = Builders<User>.Update
                .Set(u => u.ProfileImage, null)
                .Set(u => u.ModifiedOn, DateTime.UtcNow);
            var updateResult = await _userCollection.UpdateOneAsync(filter, update);

            return updateResult.ModifiedCount > 0;
        }

        public async Task<bool> UpdateProfileImage(Guid publickey, string profileImage)
        {
            if (profileImage is not null && publickey != null)
            {
                var filter = Builders<User>.Filter.Eq(x => x.PublicKey, publickey);
                var update = Builders<User>.Update
                    .Set(u => u.ProfileImage, profileImage)
                    .Set(u => u.ModifiedOn, DateTime.UtcNow);
                var updateResult = await _userCollection.UpdateOneAsync(filter, update);

                return updateResult.ModifiedCount > 0;
            }
            else
            {
                return false;
            }
        }

        public async Task<ApiCommonResponseModel> AddSelfDeleteAccountRequestData(AccountDeleteRequestModel request, MobileUser mobileUser)
        {
            var objResponse = new ApiCommonResponseModel();
            var filter = Builders<MobileUserSelfDeleteData>.Filter.And(
                Builders<MobileUserSelfDeleteData>.Filter.Eq(b => b.MobileUserKey, request.MobileUserKey.ToString()));

            var userExists = await _mobileUserSelfDeleteDataCollection.Find(filter).AnyAsync();

            if (userExists)
            {
                objResponse.Message = "Already requested to delete the account.";
                objResponse.StatusCode = System.Net.HttpStatusCode.Ambiguous;
            }
            else
            {
                var mobileUserTemp = new MobileUserSelfDeleteData
                {
                    MobileUserKey = mobileUser.PublicKey.ToString(),
                    FullName = mobileUser.FullName,
                    Mobile = mobileUser.Mobile,
                    EmailId = mobileUser.EmailId,
                    RegistrationDate = mobileUser.RegistrationDate,
                    LeadKey = mobileUser.LeadKey.ToString(),
                    CreatedOn = mobileUser.CreatedOn,
                    LastLoginDate = mobileUser.LastLoginDate
                };

                await _mobileUserSelfDeleteDataCollection.InsertOneAsync(mobileUserTemp);
                objResponse.Message = "Successfull";
                objResponse.StatusCode = System.Net.HttpStatusCode.OK;
            }

            return objResponse;
        }

        public async Task<bool> DeleteSelfAccountRequestedData(string mobileNumber)
        {
            try
            {
                var filter = Builders<MobileUserSelfDeleteData>.Filter.And(
                    Builders<MobileUserSelfDeleteData>.Filter.Eq(b => b.Mobile, mobileNumber));
                await _mobileUserSelfDeleteDataCollection.DeleteOneAsync(filter);
                return true;
            }
            catch (Exception ex)
            {
                //await this.LogExceptions(ex, mobileNumber, source: "seflDelete");
                return false;
            }
        }

        #endregion Users Related Methods

        #region Get  Notification from topic

        public async Task<List<PushNotification>> GetNotificationFromTopic(string topic = "KALKIBAATAAJ")
        {
            var notifications = await _pushNotificationCollection.Find(x => x.Topic == topic).ToListAsync();
            return notifications;
        }

        #endregion Get  Notification from topic

        #region Remove notification data for topic

        public async Task<bool> DeleteNotificationForTopic(string topic)
        {
            try
            {
                await _pushNotificationCollection.DeleteManyAsync(x => x.Topic == topic);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion Remove notification data for topic

        public async Task ManageUserVersionReport(string message, string deviceType, string version, string mobileUserKey, long mobileUserId)
        {
            MobileUser? _mobileUser = null;

            // Check if mobileUserKey is a valid GUID and find user by PublicKey
            if (!string.IsNullOrWhiteSpace(mobileUserKey) && Guid.TryParse(mobileUserKey, out Guid parsedKey))
            {
                _mobileUser = await _context.MobileUsers.FirstOrDefaultAsync(item => item.PublicKey == parsedKey);
            }

            // If user is still null, check using mobileUserId
            if (_mobileUser == null && mobileUserId > 0)
            {
                _mobileUser = await _context.MobileUsers.FirstOrDefaultAsync(item => item.Id == mobileUserId);
            }

            // If user is found, update the device information if it has changed
            if (_mobileUser != null)
            {
                bool isDeviceUpdated = (_mobileUser.DeviceType != deviceType) || (_mobileUser.DeviceVersion != version);

                if (isDeviceUpdated)
                {
                    _mobileUser.DeviceType = deviceType;
                    _mobileUser.DeviceVersion = version;
                    await _context.SaveChangesAsync();
                }
            }
            else
            {

            }

        }


        //var existingRecord = await _userVersion.Find(x =>
        //    x.MobileUserKey == mobileUserKey &&
        //    x.Version == version &&
        //    x.DeviceType == deviceType
        //).FirstOrDefaultAsync();

        //if (existingRecord == null)
        //{
        //    await _userVersion.InsertOneAsync(new UserVersionReport()
        //    {
        //        CreatedOn = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
        //        Description = message,
        //        DeviceType = deviceType,
        //        Version = version,
        //        MobileUserId = mobileUserId,
        //        MobileUserKey = mobileUserKey
        //    });
        //}


        // Method to fetch all MobileUser, DeviceType, and Version from MongoDB whose device Version not matching to latest version
        // ToDo: Change the logic to fetch the IOs and Android Version.
        //public async Task<List<UserVersionReport>> GetAllUserVersionReport(string IosVersion, string AndroidVersion)
        //{
        //    try
        //    {
        //        // Fetch all documents from MongoDB

        //        var userVersions = await _userVersion.Find(x =>
        //        x.DeviceType == null || // Condition 1: DeviceType is null
        //        (x.DeviceType != null && x.DeviceType.Contains("Android") && x.Version != _configuration["AppSettings:Versions:Android"]!) || // Condition 2
        //        (x.DeviceType != null && x.DeviceType.Contains("ios") && x.Version != _configuration["AppSettings:Versions:iOS"]!) // Condition 3
        //        ).ToListAsync();

        //        // Return only the fields we need: MobileUserId, DeviceType, Version
        //        List<UserVersionReport> result = new();
        //        foreach (var userVersion in userVersions)
        //        {
        //            result.Add(new UserVersionReport
        //            {
        //                MobileUserId = userVersion.MobileUserId,
        //                DeviceType = userVersion.DeviceType,
        //                Version = userVersion.Version,
        //                MobileUserKey = userVersion.MobileUserKey
        //            });
        //        }

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error fetching data from MongoDB: {ex.Message}");
        //        return null;
        //    }
        //}
        //public async Task<List<string>> GetOutdatedUserKeysAsync()
        //{
        //    var currentVersions = await _context.Settings
        //        .Where(item => item.Code == "IosCurrentVersion" || item.Code == "AndroidCurrentVersion")
        //        .ToListAsync();

        //    var iosCurrentVersion = currentVersions.FirstOrDefault(s => s.Code == "IosCurrentVersion")?.Value;
        //    var androidCurrentVersion = currentVersions.FirstOrDefault(s => s.Code == "AndroidCurrentVersion")?.Value;

        //    if (iosCurrentVersion == null || androidCurrentVersion == null)
        //    {
        //        throw new Exception("Current versions not found in settings.");
        //    }

        //    var userReports = await _userVersion.Find(_ => true).ToListAsync();

        //    var latestUserReports = userReports
        //        .Where(ur => ur != null &&
        //                     !string.IsNullOrEmpty(ur.DeviceType) &&
        //                     !string.IsNullOrEmpty(ur.Version) &&
        //                     !string.IsNullOrEmpty(ur.MobileUserKey) &&
        //                     !string.IsNullOrEmpty(ur.CreatedOn))
        //        .GroupBy(ur => ur.MobileUserKey)
        //        .Select(group => group
        //            .OrderByDescending(ur => DateTime.TryParse(ur.CreatedOn, out var date) ? date : DateTime.MinValue)
        //            .First())
        //        .ToList();

        //    var outdatedUserKeys = latestUserReports
        //        .Where(ur => (ur.DeviceType.StartsWith("IOS") && ur.Version != iosCurrentVersion) ||
        //                     (ur.DeviceType.StartsWith("Android") && ur.Version != androidCurrentVersion))
        //        .Select(ur => ur.MobileUserKey)
        //        .Distinct()
        //        .ToList();

        //    return outdatedUserKeys;
        //}

        public async Task<bool> DeletePushNotification(string notificationId)
        {
            try
            {
                // Step 1: Update PushNotification collection (set IsDelete to true)
                var filter = Builders<PushNotification>.Filter.Eq(n => n.ObjectId, notificationId);
                var update = Builders<PushNotification>.Update.Set(n => n.IsDelete, true);

                var resultPushNotification = await _pushNotificationCollection.UpdateOneAsync(filter, update);

                // Step 2: Update PushNotificationReceiver collection for all related records
                var filterReceiver = Builders<PushNotificationReceiver>.Filter.Eq(r => r.NotificationId, notificationId);
                var updateReceiver = Builders<PushNotificationReceiver>.Update.Set(r => r.IsDelete, true);

                var resultReceiver = await _pushNotificationCollectionReceiver.UpdateManyAsync(filterReceiver, updateReceiver);

                // Return true if at least one document was modified in either collection
                return resultPushNotification.ModifiedCount > 0 || resultReceiver.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating push notification and receiver: {ex.Message}");
                return false;
            }


        }
        public async Task MarkOldAnnouncementsAsDeletedAsync()
        {
            // Calculate the date 7 days ago.
            DateTime sevenDaysAgo = DateTime.Now.AddDays(-7);

            // Build the filter to identify the records to update.
            var filter = Builders<PushNotification>.Filter.And(
                Builders<PushNotification>.Filter.Lt(x => x.CreatedOn, sevenDaysAgo),
                Builders<PushNotification>.Filter.Eq(x => x.Topic, "Announcement")
            );

            // Build the update definition to set IsDelete to true.
            var update = Builders<PushNotification>.Update.Set(x => x.IsDelete, true);

            try
            {
                // Perform the update operation.
                var result = await _pushNotificationCollection.UpdateManyAsync(filter, update);

                // Check the result (optional, but good practice).
                Console.WriteLine($"Modified Count: {result.ModifiedCount}");
                Console.WriteLine($"Matched Count: {result.MatchedCount}");

                // return result; // Return the update result for further processing if needed.
            }
            catch (Exception ex)
            {
                // Handle exceptions (log, rethrow, etc.).
                Console.WriteLine($"Error marking announcements as deleted: {ex.Message}");
                throw; // Re-throw the exception after logging.
            }

            // return oldAnnouncements;
        }


    }
}