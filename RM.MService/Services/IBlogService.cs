using RM.BlobStorage;
using RM.CommonService;
using RM.CommonService;
using RM.CommonServices;
using RM.CommonServices.Services;
using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.KingResearchContext;
using RM.Database.MongoDbContext;
using RM.Database.MongoDbContext;
using RM.Model;
using RM.Model.Common;
using RM.Model.Common;
using RM.Model.MongoDbCollection;
using RM.Model.RequestModel;
using RM.Model.RequestModel.Notification;
using RM.Model.ResponseModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Net;
using ImageModel = RM.Model.MongoDbCollection.ImageModel;

namespace RM.MService.Services
{
    public interface IBlogService
    {
        Task<ApiCommonResponseModel> GetBlogsAsync(int pageNumber, int pageSize, Guid publicKey);
        Task<ApiCommonResponseModel> PostBlog(PostBlogRequestModel blogPost);
        Task<ApiCommonResponseModel> DeleteBlog(string blogId, string userObjectId);
        Task<ApiCommonResponseModel> GetComments(string blogId, int pageNumber, int pageSize);
        Task<ApiCommonResponseModel> AddComment(PostCommentRequestModel obj);
        Task<ApiCommonResponseModel> DeleteCommentOrReply(string commentId, string userObjectId, string type);
        Task<ApiCommonResponseModel> LikeBlog(string userObjectId, string blogId, bool isLiked);
        Task<ApiCommonResponseModel> CommentReply(CommentReplyRequestModel request);
        Task<ApiCommonResponseModel> GetReplies(string blogId);
        Task<ApiCommonResponseModel> EditBlog(EditBlogRequestModel request);
        Task<ApiCommonResponseModel> EditCommentOrReply(EditCommentOrReplyRequestModel request);
        Task<ApiCommonResponseModel> DisableBlogComment(DisableBlogCommentRequestModel request);
        Task<ApiCommonResponseModel> DisableCommunityPostForUser(Guid userKey);
        Task<ApiCommonResponseModel> ReportBlog(ReportBlogRequestModel request);
        Task<ApiCommonResponseModel> GetReportReason();
        Task<ApiCommonResponseModel> BlockUser(BlockUserRequestModel request);
        Task<ApiCommonResponseModel> GetBlockedUser(string? mobileUserKey);
        Task<ApiCommonResponseModel> CreateBlogForCrmPostAsync(CommunityPostRequestModel blogPost);
        Task<ApiCommonResponseModel> GetAllBlogs(QueryValues query);
        Task<ApiCommonResponseModel> ManageBlogStatusAsync(UpdateBlogStatusRequestModel request);
        Task<ApiCommonResponseModel> ManageUserPostPermissionAsync(RestrictUserRequestModel request);
        Task<ApiCommonResponseModel> DeleteBlogAsync(string id, Guid loggedInUser);
        Task<ApiCommonResponseModel> ManageBlogPinStatusAsync(UpdatePinnedStatusRequestModel request);
        Task<bool> AddComment(Comment blogComment);
        Task<ApiCommonResponseModel> AddBlogComment(PostCommentRequestModel obj, Guid loggedInUser);

    }

    public class BlogService(IConfiguration configuration, KingResearchContext context, MongoDbService mongoService, IMobileNotificationService mobileNotificationService,

        IAzureBlobStorageService azureBlobStorageService, IMongoRepository<Model.MongoDbCollection.User> userCollection, IMongoRepository<BlogReport> blogReportCollection,
        IMongoRepository<Blog> blog, IMongoRepository<Comment> commnetCollection) : IBlogService
    {
        private readonly IMongoRepository<BlogReport> _blogReportCollection = blogReportCollection;

        private readonly IConfiguration _configuration = configuration;
        private readonly ApiCommonResponseModel responseModel = new();
        private readonly KingResearchContext _context = context;
        private readonly IMongoRepository<Model.MongoDbCollection.User> _userRepository = userCollection;
        private readonly IMongoRepository<Blog> _blog = blog;
        private readonly MongoDbService _mongoService = mongoService;
        private readonly IMobileNotificationService _mobileNotificationService = mobileNotificationService;
        private readonly IAzureBlobStorageService _azureBlobStorageService = azureBlobStorageService;
        private readonly IMongoRepository<Comment> _commentCollection = commnetCollection;


        #region Blogs Methods for mobile API 

        public async Task<ApiCommonResponseModel> DeleteBlog(string blogId, string userObjectId)
        {
            var saveToMongo = await _mongoService.DeleteBlog(blogId, userObjectId);
            if (saveToMongo)
            {
                responseModel.Message = "Successfull.";
                responseModel.StatusCode = HttpStatusCode.NoContent;
                return responseModel;
            }
            else
            {
                responseModel.Message = "Couldn't delete.";
                responseModel.StatusCode = HttpStatusCode.Forbidden;
                return responseModel;
            }
        }

        public async Task<ApiCommonResponseModel> PostBlog(PostBlogRequestModel blogPost)
        {
            var responseModel = new ApiCommonResponseModel();

            // Upload images and generate image models
            var imageList = new List<ImageModel>();
            if (blogPost.Images?.Any() == true)
            {
                for (int i = 0; i < blogPost.Images.Count; i++)
                {
                    var uploadedImageName = await _azureBlobStorageService.UploadImage(blogPost.Images[i]);
                    imageList.Add(new ImageModel
                    {
                        Name = uploadedImageName,
                        AspectRatio = i < blogPost.AspectRatios?.Count ? blogPost.AspectRatios[i] : string.Empty
                    });
                }
            }

            // Create blog document
            var blog = new Model.MongoDbCollection.Blog
            {
                Content = blogPost.Content,
                CreatedBy = blogPost.UserObjectId,
                Hashtag = blogPost.Hashtag,
                CreatedOn = DateTime.Now,
                IsDelete = false,
                IsActive = true,
                Image = imageList,
                EnableComments = true,
                Status = "Posted",
                IsPinned = false
            };

            // Save blog and prepare response
            var (success, message, blogId) = await _mongoService.SaveBlog(blog);

            responseModel.StatusCode = success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;
            responseModel.Message = success ? "Blog Created Successfully." : message;
            responseModel.Data = success ? blogId : null;

            return responseModel;
        }
        public async Task<ApiCommonResponseModel> GetBlogsAsync(int pageNumber, int pageSize, Guid publicKey)
        {
            const string userNotFoundMessage = "User Doesn't Exist.";
            const string fetchBlogsFailedMessage = "Failed to fetch blogs.";
            const string dataFetchedSuccessfullyMessage = "Data Fetched Successfully.";

            var sqlParameter = new SqlParameter("MobileUserKey", System.Data.SqlDbType.UniqueIdentifier)
            {
                Value = publicKey
            };

            var result = await _context.SqlQueryFirstOrDefaultAsync2<GetMobileUserDetailsSpResponseModel>(
                ProcedureCommonSqlParametersText.GetMobileUserDetails,
                [sqlParameter]
            );

            if (result is null)
            {
                return new ApiCommonResponseModel
                {
                    Message = userNotFoundMessage,
                    StatusCode = HttpStatusCode.NotFound
                };
            }

            var mongoUser = new RM.Model.MongoDbCollection.User
            {
                FullName = result.Fullname,
                PublicKey = result.PublicKey,
                ProfileImage = result.ProfileImage,
                Gender = result.Gender,
                CanCommunityPost = result.HasActiveProduct
            };

            var userObjectId = await _mongoService.AddUser(mongoUser);
            if (userObjectId is null)
            {
                return new ApiCommonResponseModel
                {
                    Message = fetchBlogsFailedMessage,
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }

            var blogs = await _mongoService.GetBlogs(pageNumber, pageSize, userObjectId);

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = dataFetchedSuccessfullyMessage,
                Data = new
                {
                    userObjectId,
                    CommunityPostEnabled = result.HasActiveProduct,
                    IsCommentEnabled = true,
                    blogs
                }
            };
        }

        public async Task<ApiCommonResponseModel> GetComments(string blogId, int pageNumber, int pageSize)
        {
            var data = await _mongoService.GetComments(blogId, pageNumber, pageSize);

            responseModel.Data = data;
            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Data Fetched Successfully.";

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> AddComment(PostCommentRequestModel obj)
        {
            ApiCommonResponseModel responseModel = new();

            // Create the comment object
            Comment blogComment = new()
            {
                ObjectId = ObjectId.GenerateNewId().ToString(),
                BlogId = obj.BlogId,
                Content = obj.Comment,
                Mention = obj.Mention,
                CreatedBy = obj.UserObjectId,
                CreatedOn = DateTime.Now,
                IsActive = true,
                IsDelete = false
            };

            // Call the service to save to Mongo
            var saveToMongo = await _mongoService.AddComment(blogComment);

            // Handle the result
            if (!saveToMongo)
            {
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                responseModel.Message = "Couldn't Add Comment.";
            }
            else
            {
                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Message = "Comment Added Successfully.";
            }

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> DeleteCommentOrReply(string objectId, string userObjectId, string type)
        {
            var saveToMongo = await _mongoService.DeleteCommentOrReply(objectId, userObjectId, type);
            if (saveToMongo)
            {
                responseModel.Message = "Delete Successfull.";
                responseModel.StatusCode = HttpStatusCode.NoContent;
                return responseModel;
            }
            else
            {
                responseModel.Message = "Couldn't delete.";
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                return responseModel;
            }
        }

        private async Task<string> SaveImageToAssetsFolderAsync(IFormFile profileImage, string fileName)
        {
            string assetsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Blog-images");
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

        public async Task<ApiCommonResponseModel> LikeBlog(string userObjectId, string blogId, bool isLiked)
        {
            Like blogLike = new()
            {
                CreatedBy = userObjectId,
                CreatedOn = DateTime.Now,
                BlogId = blogId
            };

            var saveToMongo = await _mongoService.LikeBlog(blogLike, isLiked);

            if (saveToMongo)
            {
                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Message = "Added Successfully.";
            }
            else
            {
                responseModel.Message = "An Error Occured.";
            }

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> CommentReply(CommentReplyRequestModel request)
        {
            Reply blogCommentCollectionItem = new()
            {
                ObjectId = ObjectId.GenerateNewId().ToString(),
                CommentId = request.CommentId,
                Content = request.Reply,
                Mention = request.Mention,
                CreatedBy = request.UserObjectId,
                CreatedOn = DateTime.Now,
                IsActive = true,
                IsDelete = false
            };

            var saveToMongo = await _mongoService.CommentReply(blogCommentCollectionItem);

            if (!saveToMongo)
            {
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                responseModel.Message = "An Error Occured.";
            }
            else
            {
                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Message = "Reply Added Successfully.";
            }

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetReplies(string commentId)
        {
            var data = await _mongoService.GetReplies(commentId);

            responseModel.Data = data;
            responseModel.StatusCode = HttpStatusCode.OK;

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> EditBlog(EditBlogRequestModel request)
        {
            var editedInMongo = await _mongoService.EditBlog(request.BlogId, request.UserObjectId, request.NewContent, request.NewHashtag);

            if (!editedInMongo)
            {
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                responseModel.Message = "An Error Occured.";
            }
            else
            {
                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Message = "Blog Edited Successfully.";
            }
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> EditCommentOrReply(EditCommentOrReplyRequestModel request)
        {
            var isEdited = await _mongoService.EditCommentOrReply(
                request.ObjectId,
                request.UserObjectId,
                request.NewContent,
                request.NewMention,
                request.Type
            );

            return new ApiCommonResponseModel
            {
                StatusCode = isEdited ? HttpStatusCode.OK : HttpStatusCode.InternalServerError,
                Message = isEdited
            ? $"{request.Type} edited successfully."
            : "An error occurred while editing.",
                Data = null
            };
        }

        public async Task<ApiCommonResponseModel> DisableBlogComment(DisableBlogCommentRequestModel request)
        {
            var isSuccess = await _mongoService.DisableBlogComment(request);

            return new ApiCommonResponseModel
            {
                StatusCode = isSuccess ? HttpStatusCode.OK : HttpStatusCode.InternalServerError,
                Message = isSuccess
            ? "Comment section disabled successfully."
            : "An error occurred while disabling comments.",
                Data = null
            };
        }

        public async Task<ApiCommonResponseModel> DisableCommunityPostForUser(Guid userKey)
        {
            var user = await _context.MobileUsers.FirstOrDefaultAsync(u => u.PublicKey == userKey);

            if (user is null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "User not found.",
                    Data = null
                };
            }

            user.CanCommunityPost = false;
            user.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Community post access disabled successfully.",
                Data = null
            };
        }

        public async Task<ApiCommonResponseModel> ReportBlog(ReportBlogRequestModel request)
        {
            var reportStatus = await _mongoService.ReportBlog(request);

            if (reportStatus.StatusCode == HttpStatusCode.OK)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Report Successful.",
                    Data = null
                };
            }

            if (reportStatus.StatusCode == HttpStatusCode.Accepted)
            {
                if (Guid.TryParse(reportStatus.Data?.ToString(), out var userPublicKey))
                {
                    await DisableCommunityPostForUser(userPublicKey);

                    var user = await _context.MobileUsers
                        .FirstOrDefaultAsync(u => u.PublicKey == userPublicKey);

                    if (user != null)
                    {
                        var notification = new SendFreeNotificationRequestModel
                        {
                            Title = "Community Access Restricted",
                            Body = "You have been restricted from using the community due to excessive reports.",
                            Mobile = user.Mobile,
                            Topic = "Announcement"
                        };

                        _ = await _mobileNotificationService.SendFreeNotification(notification);
                    }
                }

                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Blog Reported Successfully.",
                    Data = null
                };
            }

            // Default fallback
            return new ApiCommonResponseModel
            {
                StatusCode = reportStatus.StatusCode,
                Message = reportStatus.Message ?? "An error occurred while reporting the blog.",
                Data = null
            };

        }

        public async Task<ApiCommonResponseModel> GetReportReason()
        {
            var reasons = await _mongoService.GetReportReason();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Fetched successfully.",
                Data = reasons,
                Total = reasons?.Count ?? 0
            };
        }

        public async Task<ApiCommonResponseModel> BlockUser(BlockUserRequestModel request)
        {
            ApiCommonResponseModel blockStatus = await _mongoService.BlockUser(request);
            return blockStatus;
        }

        public async Task<ApiCommonResponseModel> GetBlockedUser(string mobileUserKey)
        {
            var blockedUserList = await _mongoService.GetBlockedUser(mobileUserKey);

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Fetched Blocked User Successfully.",
                Data = blockedUserList,
                Total = blockedUserList?.Count ?? 0
            };
        }
        #endregion



        #region Blogs Method For CRM API 

        public async Task<ApiCommonResponseModel> CreateBlogForCrmPostAsync(CommunityPostRequestModel blogPost)
        {
            // Step 1: Resolve public key by mobile number
            var publicKey = await _context.MobileUsers
                .Where(mu => mu.Mobile == blogPost.UserMobileNumber)
                .Select(mu => mu.PublicKey)
                .FirstOrDefaultAsync();

            if (publicKey == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "User with provided mobile number not found."
                };
            }

            // Step 2: Fetch MongoDB user
            var mongoUser = await _userRepository.FindOneAsync(u => u.PublicKey == publicKey);
            if (mongoUser == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "User not found in MongoDB."
                };
            }

            var createdBy = mongoUser.ObjectId;
            var createdByName = mongoUser.FullName;

            // Step 3: Upload blog images
            var (imageList, notificationImageUrl) = await UploadBlogImagesAsync(blogPost);

            // Step 4: Create blog document
            var blog = new Blog
            {
                Content = blogPost.Content,
                CreatedBy = createdBy,
                CreatedOn = DateTime.UtcNow,
                IsDelete = false,
                IsActive = true,
                Image = imageList,
                EnableComments = true,
                Status = "Posted",
                ModifiedBy = createdByName
            };

            await _blog.AddAsync(blog);

            if (string.IsNullOrEmpty(blog.ObjectId))
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Failed to create blog."
                };
            }

            // Step 5: Send push notifications to active users
            var activeMobiles = await _context.MobileUsers
                .Where(mu => mu.IsActive == true && mu.IsDelete != true && !string.IsNullOrEmpty(mu.FirebaseFcmToken))
                .Select(mu => mu.Mobile)
                .ToListAsync();

            var notification = new NotificationToMobileRequestModel
            {
                Title = "New Blog Posted!",
                Body = $"{createdByName} posted:",
                Topic = "ANNOUNCEMENT",
                ScreenName = "getAllBlogs",
                Mobile = string.Join(",", activeMobiles),
                NotificationImage = notificationImageUrl ?? string.Empty
            };

            await _mobileNotificationService.SendNotificationToMobile(notification);

            // Step 6: Return success
            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Blog Created Successfully.",
                Data = blog.ObjectId
            };
        }



        public async Task<ApiCommonResponseModel> GetAllBlogs(QueryValues query)
        {
            var adjustedToDate = query.ToDate.HasValue
                ? DateTime.SpecifyKind(query.ToDate.Value.AddDays(1).AddTicks(-1), DateTimeKind.Utc)
                : (DateTime?)null;

            var combinedFilter = await BuildCombinedFilter(query, adjustedToDate);

            // Get paginated blogs
            var (blogs, totalBlogsCount) = await _blog.GetPaginatedAsyncWithTotalCount(
                combinedFilter,
                b => b.CreatedOn,
                query.PageNumber,
                query.PageSize,
                isDescending: true
            );

            if (!blogs.Any())
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.OK,
                    Data = new List<object>()
                };
            }

            // Fetch only necessary users based on CreatedBy ObjectIds
            var createdByIds = blogs.Select(b => b.CreatedBy).Distinct().ToList();
            var userFilter = Builders<Model.MongoDbCollection.User>.Filter.In(u => u.ObjectId, createdByIds);
            var users = await _userRepository.FindAsync(userFilter);
            var userDict = users.ToDictionary(u => u.ObjectId);

            // Fetch only required mobile user public keys
            var mobileUserDict = await _context.MobileUsers
                .Where(mu => users.Select(u => u.PublicKey).Contains(mu.PublicKey))
                .Select(mu => new { mu.PublicKey, mu.Mobile })
                .ToDictionaryAsync(mu => mu.PublicKey, mu => mu.Mobile);

            var result = BuildResult(blogs, userDict, mobileUserDict);

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Data = result,
                Total = (int)totalBlogsCount
            };
        }


        private async Task<FilterDefinition<Blog>> BuildCombinedFilter(QueryValues query, DateTime? adjustedToDate)
        {
            var filterBuilder = Builders<Blog>.Filter;
            var baseFilter = GetStatusBasedFilter(query, adjustedToDate, filterBuilder);

            if (string.IsNullOrWhiteSpace(query.SearchText))
                return baseFilter;

            var regex = new BsonRegularExpression(query.SearchText, "i");

            var contentFilter = filterBuilder.Regex(b => b.Content, regex);

            // FullName Filter
            var fullNameFilter = Builders<Model.MongoDbCollection.User>.Filter.Regex(u => u.FullName, regex);
            var fullNameMatchingUsers = await _userRepository.FindAsync(fullNameFilter);
            var fullNameMatchingUserIds = fullNameMatchingUsers.Select(u => u.ObjectId).ToList();

            // Mobile Filter
            var matchingMobileUsers = await _context.MobileUsers
                .Where(mu => mu.Mobile.Contains(query.SearchText))
                .Select(mu => mu.PublicKey)
                .ToListAsync();

            var publicKeyFilter = Builders<Model.MongoDbCollection.User>.Filter.In(u => u.PublicKey, matchingMobileUsers);
            var mobileMatchedUsers = await _userRepository.FindAsync(publicKeyFilter);
            var mobileNumberMatchingUserIds = mobileMatchedUsers.Select(u => u.ObjectId).ToList();

            var allMatchingUserIds = fullNameMatchingUserIds
                .Union(mobileNumberMatchingUserIds)
                .Distinct();

            var creatorFilter = filterBuilder.In(b => b.CreatedBy, allMatchingUserIds);

            var combinedSearchFilter = filterBuilder.Or(contentFilter, creatorFilter);

            return filterBuilder.And(baseFilter, combinedSearchFilter);
        }

        private List<object> BuildResult(List<Blog> blogs,
           Dictionary<string, Model.MongoDbCollection.User> userDict,
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

        private FilterDefinition<Blog> BuildDefaultFilter(FilterDefinitionBuilder<Blog> filterBuilder, DateTime? from, DateTime? to)
        {
            var filters = new List<FilterDefinition<Blog>>
    {
        filterBuilder.Ne(b => b.IsDelete, true)
    };

            if (from.HasValue)
                filters.Add(filterBuilder.Gte(b => b.CreatedOn, from.Value));

            if (to.HasValue)
                filters.Add(filterBuilder.Lte(b => b.CreatedOn, to.Value));

            return filterBuilder.And(filters);
        }

        private async Task<(List<ImageModel>, string)> UploadBlogImagesAsync(CommunityPostRequestModel blogPost)
        {
            var imageList = new List<ImageModel>();
            string notificationImageUrl = null;
            var imageUrlSuffix = _configuration["Azure:ImageUrlSuffix"] ?? "";

            if (blogPost.Images == null || !blogPost.Images.Any())
                return (imageList, notificationImageUrl);

            for (int i = 0; i < blogPost.Images.Count; i++)
            {
                try
                {
                    var name = await _azureBlobStorageService.UploadImage(blogPost.Images[i]);

                    if (i == 0)
                        notificationImageUrl = imageUrlSuffix + name;

                    var aspectRatio = blogPost.AspectRatios?.ElementAtOrDefault(i) ?? "auto";

                    imageList.Add(new ImageModel
                    {
                        Name = name,
                        AspectRatio = aspectRatio
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Image Upload Failed (index {i}): {ex.Message}");
                    continue;
                }
            }

            return (imageList, notificationImageUrl);
        }

        #endregion

        public async Task<ApiCommonResponseModel> ManageBlogStatusAsync(UpdateBlogStatusRequestModel request)
        {
            var modifiedBy = await GetMobileUsersNameAsync(request.LoggedInUser);

            var filter = Builders<Blog>.Filter.Eq(b => b.ObjectId, request.BlogId);
            var blog = (await _blog.FindAsync(filter)).FirstOrDefault();

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

                var filters = Builders<BlogReport>.Filter.Eq(r => r.BlogId, request.BlogId) &
                 Builders<BlogReport>.Filter.Eq(r => r.Status, true);

                var relatedReports = await _blogReportCollection.FindAsync(filters);

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
            await _blog.ReplaceOneAsync(b => b.ObjectId == request.BlogId, blog);

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = request.BlogStatus ? "Blog blocked successfully" : "Blog unblocked successfully",
                Data = blog
            };
        }

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

        public async Task<ApiCommonResponseModel> ManageUserPostPermissionAsync(RestrictUserRequestModel request)
        {
            // Get blog by ObjectId
            var blogfilter = Builders<Blog>.Filter.Eq(b => b.ObjectId, request.BlogId);
            var blog = (await _blog.FindAsync(blogfilter)).FirstOrDefault();
            if (blog == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Blog not found",
                    Data = null
                };
            }

            // Get MongoDB User
            var filter = Builders<Model.MongoDbCollection.User>.Filter.Eq(u => u.ObjectId, request.CreatedBy);
            var user = (await _userRepository.FindAsync(filter)).FirstOrDefault();
            if (user == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "User not found in MongoDB",
                    Data = null
                };
            }

            // Get SQL MobileUser by PublicKey
            var mobileUser = await _context.MobileUsers.FirstOrDefaultAsync(u => u.PublicKey == user.PublicKey);
            if (mobileUser == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "User not found in SQL",
                    Data = null
                };
            }

            // Update permissions
            var now = DateTime.UtcNow;
            user.CanCommunityPost = request.CanCommunityPost;
            user.ModifiedOn = now;

            mobileUser.CanCommunityPost = request.CanCommunityPost;
            mobileUser.ModifiedOn = now;

            await _userRepository.ReplaceOneAsync(u => u.ObjectId == request.CreatedBy, user);

            // Update blog metadata
            blog.ModifiedBy = await GetMobileUsersNameAsync(request.LoggedInUser);
            blog.ModifiedOn = now;

            await _blog.ReplaceOneAsync(b => b.ObjectId == request.BlogId, blog);
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

        public async Task<ApiCommonResponseModel> DeleteBlogAsync(string id, Guid loggedInUser)
        {
            var deletedBy = await GetMobileUsersNameAsync(loggedInUser);
            var filter = Builders<Blog>.Filter.Where(b => b.ObjectId == id && !b.IsDelete);
            var blogs = await _blog.FindAsync(filter);
            var blog = blogs.FirstOrDefault();

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

            await _blog.ReplaceOneAsync(b => b.ObjectId == id, blog);

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Blog deleted successfully",
                Data = blog
            };
        }

        public async Task<ApiCommonResponseModel> ManageBlogPinStatusAsync(UpdatePinnedStatusRequestModel request)
        {
            var modifiedBy = await GetMobileUsersNameAsync(request.LoggedInUser);

            var blog = await _blog.FindOneAsync(b => b.ObjectId == request.Id);

            if (blog == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Blog not found."
                };
            }

            blog.IsPinned = !request.IsPinned; // Toggle pin status
            blog.ModifiedBy = modifiedBy;
            blog.ModifiedOn = DateTime.UtcNow;

            await _blog.ReplaceOneAsync(b => b.ObjectId == request.Id, blog);

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = blog.IsPinned ? "Blog pinned successfully." : "Blog unpinned successfully.",
                Data = blog
            };
        }

        public async Task<ApiCommonResponseModel> AddBlogComment(PostCommentRequestModel obj, Guid loggedInUser)
        {
            string commentBy = await GetMobileUserObjectIdAsync(loggedInUser);
            ApiCommonResponseModel responseModel = new();
            Comment blogCommentCollectionItem = new()
            {
                ObjectId = ObjectId.GenerateNewId().ToString(),
                BlogId = obj.BlogId,
                Content = obj.Comment,
                Mention = obj.Mention,
                CreatedBy = commentBy,
                CreatedOn = DateTime.Now,
                IsActive = true,
                IsDelete = false
            };

            var saveToMongo = await AddComment(blogCommentCollectionItem);

            if (!saveToMongo)
            {
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                responseModel.Message = "Couldn't Add Comment.";
            }
            else
            {
                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Message = "Comment Added Successfully.";
            }

            return responseModel;
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

            var filter = Builders<Model.MongoDbCollection.User>.Filter.Eq(x => x.PublicKey, loggedInUser);
            var users = await _userRepository.FindAsync(filter);
            var user = users.FirstOrDefault();


            return user?.ObjectId;
        }
        public async Task<bool> AddComment(Comment blogComment)
        {
            var filter = Builders<Blog>.Filter.And(
                Builders<Blog>.Filter.Eq(b => b.ObjectId, blogComment.BlogId),
                Builders<Blog>.Filter.Eq(b => b.IsActive, true),
                Builders<Blog>.Filter.Eq(b => b.IsDelete, false)
            );

            var blogList = await _blog.FindAsync(filter);
            var blogExists = blogList.Any();

            if (!blogExists)
            {
                return false;
            }

            await _blog.UpdateAsync(filter, Builders<Blog>.Update.Inc(b => b.CommentsCount, +1));

            await _commentCollection.InsertOneAsync(blogComment);

            return true;
        }
    }
}