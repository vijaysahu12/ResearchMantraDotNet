
using System.Net;
using RM.BlobStorage;
using RM.CommonServices;
using RM.Database.ResearchMantraContext;
using RM.Database.MongoDbContext;
using RM.Model;
using RM.Model.MongoDbCollection;
using RM.Model.RequestModel;
using RM.Model.RequestModel.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using User = RM.Model.MongoDbCollection.User;


namespace RM.MService.Services
{
    public interface ICommunityService
    {
        // public  Task<ApiCommonResponseModel> ManageCommunity(communityPostRequestModel blogPost);
        public Task<ApiCommonResponseModel> CreateCommunityPostAsync(CommunityPostRequestModel blogPost);

    }

    public class CommunityService : ICommunityService
    {
        private readonly ResearchMantraContext _context;

        private readonly IMongoRepository<Log> _log;
        private readonly IMongoRepository<CommunityPost> _communityPost;
        private readonly IMongoRepository<CommunityComments> _communityComment;
        private readonly IMobileNotificationService _mobileNotificationService;
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoDatabase _database;
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        private readonly IConfiguration _configuration;
        private readonly IMongoCollection<Blog> _blogCollection;






        public CommunityService(IOptions<MongoDBSettings> mongoDBSettings, ResearchMantraContext dbContext, IMongoRepository<Log> log, IMongoRepository<CommunityPost> communityPost,
             IConfiguration configuration, IMongoCollection<Blog> blogCollection,
            IMongoRepository<CommunityComments> communityComment, IMobileNotificationService mobileNotificationService, IMongoCollection<User> userCollection, IAzureBlobStorageService azureBlobStorageService)
        {
            MongoClient client = new(mongoDBSettings.Value.ConnectionURI);

            _database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);

            _log = log;
            _communityPost = communityPost;
            _communityComment = communityComment;
            _context = dbContext;
            _mobileNotificationService = mobileNotificationService;
            _userCollection = _database.GetCollection<User>("User");
            _configuration = configuration;
            _azureBlobStorageService = azureBlobStorageService;
            _blogCollection = _database.GetCollection<Blog>("Blog");

        }



        public void ManageCommunityComment()
        {

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
            string notificationImageUrl = null;
            var imageUrlSuffix = _configuration["Azure:ImageUrlSuffix"] ?? "";

            if (blogPost.Images != null && blogPost.Images.Any())
            {
                for (int i = 0; i < blogPost.Images.Count; i++)
                {
                    var item = blogPost.Images[i];
                    try
                    {
                        var name = await _azureBlobStorageService.UploadImage(item);

                        // Use the first uploaded image as notification image
                        if (i == 0)
                        {
                            notificationImageUrl = imageUrlSuffix + name;
                        }

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
            var mobileUsers = await _context.MobileUsers
                .Where(mu => mu.IsActive == true && mu.IsDelete != true && !string.IsNullOrEmpty(mu.FirebaseFcmToken))
                .Select(mu => mu.Mobile)
                .ToListAsync();

            if (mobileUsers != null && mobileUsers.Any())
            {
                var allMobilesUsers = string.Join(",", mobileUsers);

                var notificationModel = new NotificationToMobileRequestModel
                {
                    Title = "New Blog Posted!",
                    Body = $"{createdByName} posted:",
                    Mobile = allMobilesUsers,
                    Topic = "ANNOUNCEMENT",
                    ScreenName = "getAllBlogs",
                    NotificationImage = notificationImageUrl ?? "" // fallback empty string
                };

                try
                {
                    await _mobileNotificationService.SendNotificationToMobile(notificationModel);
                }
                catch (Exception ex)
                {
                    // Log full exception details including stack trace
                    Console.WriteLine($"Error sending notification: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                        Console.WriteLine(ex.InnerException.StackTrace);
                    }

                    throw;  // optionally rethrow to debug in debugger
                }
            }

                return responseModel;
            }

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

    }
}



