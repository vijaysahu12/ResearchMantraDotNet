using RM.Database.Constants;
using RM.Database.ResearchMantraContext;
using RM.Model;
using RM.Model.RequestModel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net;
using Newtonsoft.Json;
namespace RM.MService.Services
{
    public interface IDashboardService
    {
        Task<ApiCommonResponseModel> GetAdvertisementImageList(string type);

        Task<ApiCommonResponseModel> DeleteAdvertisementImage(int id);

        Task<string> ProfileScreenAdvertisementImage();

        Task<ApiCommonResponseModel> ProfileScreenImage();

        Task<ApiCommonResponseModel> GetTop3Strategies(Guid mobileUserKey);

        Task<ApiCommonResponseModel> GetTop3Scanners(Guid mobileUserKey);

        Task<ApiCommonResponseModel> GetTop3Products(Guid mobileUserKey);
        Task<ApiCommonResponseModel> GetServiceAsync();
        Task<ApiCommonResponseModel> GetPromoPopUpAsync(Guid loggedInUser, RequestPopUpModel model);


    }

    public class DashboardService : IDashboardService
    {
        public DashboardService(ResearchMantraContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        private readonly ApiCommonResponseModel responseModel = new();
        private readonly ResearchMantraContext _context;
        private readonly IConfiguration _config;

        public async Task<ApiCommonResponseModel> GetAdvertisementImageList(string type)
        {
            var upperCaseString = type.ToUpper();
            //string format = "dd-MM-yyyy";

            if (upperCaseString == "ALL")
            {
                var imageNameList = await _context.AdvertisementImageM
               .Where(x => x.IsActive && x.IsDelete == false)
               .Select(x => new
               {
                   x.Id,
                   x.Name,
                   x.Url,
                   x.Type,
                   x.ProductId,
                   x.ProductName,
                   x.ExpireOn
               })
               .ToListAsync();
                imageNameList = imageNameList.AsEnumerable().Reverse().ToList();
                responseModel.Data = imageNameList;
                responseModel.Message = "Data Fetched Successfully";
                responseModel.StatusCode = HttpStatusCode.OK;
                return responseModel;
            }

            var validTypes = type.Split(',')
                            .Select(type => type.Trim())
                            .ToArray();

            var validTypesList = validTypes.ToList();

            var fileNameList = await _context.AdvertisementImageM
                .Where(x => x.IsActive && x.IsDelete == false && validTypesList.Contains(x.Type))
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Url,
                    x.Type,
                    x.ProductId,
                    x.ProductName,
                    x.ExpireOn,
                    ContentType = "other",
                }).ToListAsync();
            fileNameList = fileNameList.AsEnumerable().Reverse().ToList();
            responseModel.Data = fileNameList;
            responseModel.Message = "Data Fetched Successfully";
            responseModel.StatusCode = HttpStatusCode.OK;

            if (upperCaseString == "DASHBOARD")
            {


                var youtubeStreamItem = await GetPriorityYouTubeLiveStreamItem();
                if (youtubeStreamItem != null)
                {
                    var casted = fileNameList.Cast<object>().ToList();
                    casted.Insert(0, youtubeStreamItem); // insert at top
                    responseModel.Data = casted;
                }
            }
            return responseModel;
        }

        private async Task<object> GetPriorityYouTubeLiveStreamItem()
        {
            var apiKey = _config["Youtube:apiKey"];
            var channelId = _config["Youtube:channelId"];

            using var httpClient = new HttpClient();

            async Task<object> TryFetch(string eventType)
            {
                var url = $"https://www.googleapis.com/youtube/v3/search" +
                          $"?part=snippet&channelId={channelId}&eventType={eventType}" +
                          $"&type=video&order=date&maxResults=1&key={apiKey}";

                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;

                var content = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(content);

                if (result?.items?.Count == 0) return null;

                var item = result.items[0];
                string videoId = item.id.videoId;
                string title = item.snippet.title;
                string channelTitle = item.snippet.channelTitle;
                string publishedAt = item.snippet.publishedAt;

                return new
                {
                    Id = 0,
                    Name = title,
                    Url = $"https://www.youtube.com/watch?v={videoId}",
                    Type = "DASHBOARD",
                    ProductId = "",
                    ProductName = channelTitle,
                    ExpireOn = DateTime.TryParse((string)publishedAt, out var dt) ? dt : (DateTime?)null,
                    ContentType = "youtube"
                };
            }

            // 🔴 1. Check if currently live
            var live = await TryFetch("live");
            if (live != null) return live;

            // 📅 2. Check if scheduled upcoming
            var upcoming = await TryFetch("upcoming");
            if (upcoming != null) return upcoming;

            // 🎬 3. Fallback to last completed live stream
            var past = await TryFetch("completed");
            return past;
        }



        public async Task<ApiCommonResponseModel> GetPromoPopUpAsync(Guid loggedInUser, RequestPopUpModel model)
        {
            //var user = await _context.MobileUsers.FirstOrDefaultAsync(mu => mu.PublicKey == loggedInUser);

            //if (user != null)
            //{
            //    user.FirebaseFcmToken = model.fcmToken;
            //    user.DeviceType = model.deviceType;
            //    user.DeviceVersion = model.version;
            //    await _context.SaveChangesAsync();
            //}
            //else
            //{
            //    return new ApiCommonResponseModel
            //    {
            //        StatusCode = HttpStatusCode.NotFound,
            //        Message = "User Not Found."
            //    };
            //}

            if (!string.IsNullOrEmpty(model.fcmToken) || !string.IsNullOrEmpty(model.deviceType) || !string.IsNullOrEmpty(model.version))
            {
                Console.WriteLine($"[TEST] fcmToken: {model.fcmToken}, deviceType: {model.deviceType}, version: {model.version}");
            }

            var promotionsQuery = _context.PromotionM
       .Where(p => p.IsDelete == false && p.IsActive == true);

            if (model.PromoIds != null && model.PromoIds.Any())
            {
                promotionsQuery = promotionsQuery.Where(p => model.PromoIds.Contains(p.Id));
            }

            var promotions = await promotionsQuery.ToListAsync();


            var imageUrlSuffix = _config["Azure:ImageUrlSuffix"];

            var result = promotions.Select(p => new GetPromotionResponseModel
            {
                Id = p.Id,

                // ✅ Deserialize MediaWithButtonsJson (structured images + buttons)
                mediaItems = !string.IsNullOrWhiteSpace(p.ButtonText)
    ? JsonConvert.DeserializeObject<List<PromoMediaModel>>(p.ButtonText)!
        .Select(m => new PromoMediaModel
        {
            mediaUrl = (p.MediaType == "image" || p.MediaType == "gif")
                ? imageUrlSuffix + m.mediaUrl.Trim()
                : m.mediaUrl.Trim(), // for video, use direct URL
            Buttons = (m.Buttons ?? new List<PromoButtonModel>())
    .Select(b => new PromoButtonModel
    {
        ButtonName = b.ButtonName,
        ActionUrl = string.IsNullOrWhiteSpace(b.ActionUrl) || b.ActionUrl.ToLower() == "pdf"
            ? imageUrlSuffix + m.mediaUrl // or just m.mediaUrl if it's already full URL
            : b.ActionUrl,
        ProductId = b.ProductId,
        ProductName = b.ProductName,
        Target = b.Target
    }).ToList()
        }).ToList()
    : new List<PromoMediaModel>(),


                StartDate = p.StartDate ?? DateTime.Now,
                EndDate = p.EndDate ?? DateTime.Now,
                MediaType = p.MediaType,
                ShouldDisplay = p.ShouldDisplay ?? false,
                MaxDisplayCount = p.MaxDisplayCount ?? 3,
                DisplayFrequency = p.DisplayFrequency ?? 5,
                LastShownAt = p.LastShownAt?.ToString("yyyy-MM-ddTHH:mm:ss") ?? "",
                GlobalButtonAction = p.GlobalButtonAction ?? false,
                Target = p.Target ?? "",
                ProductName = p.ProductName ?? "",
                ProductId = p.ProductId ?? 0
            }).ToList();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Success",
                Data = result,
                Total = result.Count
            };
        }


        public async Task<ApiCommonResponseModel> DeleteAdvertisementImage(int id)
        {
            var image = await _context.AdvertisementImageM.FirstOrDefaultAsync(x => x.Id == id);

            if (image == null)
            {
                responseModel.StatusCode = HttpStatusCode.NotFound;
                responseModel.Message = "Image Not Found.";
                return responseModel;
            }
            image.IsDelete = true;
            image.IsActive = false;
            await _context.SaveChangesAsync();

            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Image Deleted Successfully.";
            return responseModel;
        }

        public async Task<string> ProfileScreenAdvertisementImage()
        {
            var imageName = await _context.AdvertisementImageM.Where(c => c.Type == "PROFILESCREEN" && c.IsActive == true && c.IsDelete == false).Select(c => c.Name).FirstOrDefaultAsync();
            return imageName;
        }

        public async Task<ApiCommonResponseModel> ProfileScreenImage()
        {
            var imageName = await _context.AdvertisementImageM.Where(c => c.Type == "PROFILESCREEN" && c.IsActive == true && c.IsDelete == false).Select(c => new { c.Url, c.Name }).FirstOrDefaultAsync();
            responseModel.Data = imageName;
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetTop3Strategies(Guid mobileUserKey)
        {
            int priorDaysInfo = _context.Settings
               .Where(s => s.Code == "PRIORDAYSINFO")
               .Select(s => int.Parse(s.Value))
               .FirstOrDefault();

            var query = from p in _context.ProductsM
                        join pm in _context.ProductCategoriesM on p.CategoryID equals pm.Id
                        join b in _context.MyBucketM.Where(b => b.MobileUserKey == mobileUserKey) on p.Id equals b.ProductId into bGroup
                        from b in bGroup.DefaultIfEmpty()
                        where pm.GroupName == "course"
                              && p.IsActive == true
                              && p.IsDeleted == false
                              && p.IsImportant == true
                        select new
                        {
                            p.Id,
                            p.Name,
                            p.ListImage,
                            p.Price,
                            Type = "Strategies",
                            BuyButtonText = b == null
                                ? "Buy"
                                : (b.EndDate.HasValue && DateTime.Compare(b.EndDate.Value.AddDays(-priorDaysInfo), DateTime.Now) <= 0
                                    ? "Renew"
                                    : "Purchased")
                        };

            var result = query.Take(3).ToList();

            var response = new ApiCommonResponseModel();

            response.Data = result;
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        public async Task<ApiCommonResponseModel> GetTop3Scanners(Guid mobileUserKey)
        {
            int priorDaysInfo = _context.Settings
                .Where(s => s.Code == "PRIORDAYSINFO")
                .Select(s => int.Parse(s.Value))
                .FirstOrDefault();

            var query = from p in _context.ProductsM
                        join pm in _context.ProductCategoriesM on p.CategoryID equals pm.Id
                        join b in _context.MyBucketM.Where(b => b.MobileUserKey == mobileUserKey) on p.Id equals b.ProductId into bGroup
                        from b in bGroup.DefaultIfEmpty()
                        where pm.GroupName == "strategy"
                              && p.IsActive == true
                              && p.IsDeleted == false
                              && p.IsImportant == true
                        select new
                        {
                            p.Id,
                            p.Name,
                            p.ListImage,
                            p.Price,
                            Type = "Scanner",
                            BuyButtonText = b == null
                                ? "Buy"
                                : (b.EndDate.HasValue && DateTime.Compare(b.EndDate.Value.AddDays(-priorDaysInfo), DateTime.Now) <= 0
                                    ? "Renew"
                                    : "Purchased")
                        };

            var result = query.Take(3).ToList();

            var response = new ApiCommonResponseModel();

            response.Data = result;
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        public async Task<ApiCommonResponseModel> GetTop3Products(Guid mobileUserKey)
        {
            List<object> result = new List<object>();

            var scannersResponse = await GetTop3Scanners(mobileUserKey);
            var strategiesResponse = await GetTop3Strategies(mobileUserKey);

            if (scannersResponse.Data != null)
            {
                result.AddRange((IEnumerable<object>)scannersResponse.Data);
            }

            if (strategiesResponse.Data != null)
            {
                result.AddRange((IEnumerable<object>)strategiesResponse.Data);
            }

            int priorDaysInfo = _context.Settings
                .Where(s => s.Code == "PRIORDAYSINFO")
                .Select(s => int.Parse(s.Value))
                .FirstOrDefault();

            var query = from p in _context.ProductsM
                        join pm in _context.ProductCategoriesM on p.CategoryID equals pm.Id
                        join b in _context.MyBucketM.Where(b => b.MobileUserKey == mobileUserKey) on p.Id equals b.ProductId into bGroup
                        from b in bGroup.DefaultIfEmpty()
                        where (pm.GroupName == "strategy" || pm.GroupName == "Scanner")
                              && p.IsActive == true
                              && p.IsDeleted == false
                              && p.IsImportant == true
                        select new
                        {
                            p.Id,
                            p.Name,
                            p.ListImage,
                            p.Price,
                            Type = pm.GroupName == "strategy" ? "Scanner" : "Strategy",
                            BuyButtonText = b == null
                                ? "Buy"
                                : (b.EndDate.HasValue && DateTime.Compare(b.EndDate.Value.AddDays(-priorDaysInfo), DateTime.Now) <= 0
                                    ? "Renew"
                                    : "Purchased")
                        };



            responseModel.Data = result;
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;

        }




        public async Task<ApiCommonResponseModel> GetServiceAsync()
        {
            var services = await _context.DashboardServiceM
                .Select(s => new
                {
                    s.Title,
                    s.Subtitle,
                    ImageUrl = _config["Azure:ImageUrlSuffix"] + s.ImageUrl,
                    s.Badge
                })
                .ToListAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "https://www.youtube.com/watch?v=k6eZLS4qPzc",
                Data = services,

            };
        }


    }
}