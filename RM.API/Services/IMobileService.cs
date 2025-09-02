using Azure.Storage.Blobs;
using ClosedXML.Excel;
using RM.API.Models;
using RM.BlobStorage;
using RM.CommonServices;
using RM.CommonServices.Services;
using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.ResearchMantraContext;
using RM.Database.MongoDbContext;
using RM.Model;
using RM.Model.Common;
using RM.Model.DB.Tables;
using RM.Model.Models;
using RM.Model.MongoDbCollection;
using RM.Model.RequestModel;
using RM.Model.RequestModel.Notification;
using RM.Model.ResponseModel;
using RM.NotificationService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using OfficeOpenXml;
using Quartz.Util;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static RM.Database.ResearchMantraContext.Subscriptions;
using static RM.Model.Models.PerformanceModel;
using Log = RM.Model.MongoDbCollection.Log;

namespace RM.API.Services
{
    public interface IMobileService
    {
        Task<ApiCommonResponseModel> GetMobileProducts(string searchText);
        Task<ApiCommonResponseModel> GetFilteredProductsAsync(ProductSearchRequestModel filter);
        Task<ApiCommonResponseModel> GetPromoImagesCrm(string? searchText);
        Task<ApiCommonResponseModel> DeletePrImage(int Id);
        //Task<string> SavePdfToAzure(IFormFile file);
        Task<ApiCommonResponseModel> AddPurchaseOrder( AddPurchaseOrderDetailsRequestModel model, Guid loggedInUser);
        Task<ApiCommonResponseModel> GetActiveAdsImagesCRM();


        Task<ApiCommonResponseModel> ManagePromotionAsync(PromotionRequestModel model, Guid loggedInUser);
        //Task<ApiCommonResponseModel> CreateProductCommunity(ProductCommunityMappingRequestModel request);
        Task<ApiCommonResponseModel> AddProductCommunity(ProductCommunityMappingRequestModel request);
        Task<ApiCommonResponseModel> EditProductCommunity(ProductCommunityMappingRequestModel request);
        Task<ApiCommonResponseModel> ToggleProductCommunityStatus(int id, int modifiedBy);
        Task<ApiCommonResponseModel> CreateBonusProductMapping(BonusProductMappingRequestModel request);
        Task<ApiCommonResponseModel> UpdateBonusProductMapping(BonusProductMappingRequestModel request);
        Task<ApiCommonResponseModel> ToggleBonusProductMappingStatus(int id, int modifiedBy);
        Task<ApiCommonResponseModel> GetProductCategories();
        Task<ApiCommonResponseModel> ManageProduct(MobileProductRequestModel request);
        Task<ApiCommonResponseModel> GetProductContent(int productId);
        Task<ApiCommonResponseModel> ManageProductContent(ManageProductContentRequestModel request);
        Task<ApiCommonResponseModel> GetAdImagesCrm(string type, string searchText);
        Task<ApiCommonResponseModel> EnableDisableImage(int id);
        Task<ApiCommonResponseModel> DisableProduct(int productId);
        Task<ApiCommonResponseModel> GetAdvertisementType();
        Task<ApiCommonResponseModel> DeleteProduct(int id);
        Task<ApiCommonResponseModel> DeleteAdImage(int imageId);
        Task<ApiCommonResponseModel> ManageArticle(ManageArticleModel request);
        Task<ApiCommonResponseModel> GetBaskets(string Status);
         Task<ApiCommonResponseModel> GetReasonsPurchaseAsync(int purchaseId);

        Task<ApiCommonResponseModel> ManageBasketsAsync(ManageBasketsRequestModel request);
        Task<ApiCommonResponseModel> UpdateBasketStatusAsync(UpdateBasketStatusRequestModel request);
        Task<ApiCommonResponseModel> UpdateUserMyBucketAsync(UpdateMyBucketResponseModel request);
        Task<ApiCommonResponseModel> GetCompanies(int basketId);
        Task<ApiCommonResponseModel> GetCompanyDetails(int companyId);
        Task<ApiCommonResponseModel> GetCompanyReportDetailsFromExcel(CompanyReportExcelImportModal param);
        Task<ApiCommonResponseModel> Coupon(QueryValues query);
        Task<ApiCommonResponseModel> ManageCoupon(ManageCouponModel request);
        Task<ApiCommonResponseModel> DeleteCoupon(int couponId, Guid userKey);
        Task<ApiCommonResponseModel> Ticket(GetTicketReqeustModel request);
        Task<ApiCommonResponseModel> ManageTicket(ManageTicketsRequestModel request);
        Task<ApiCommonResponseModel> GetPartnerNamesAsync();
        Task<ApiCommonResponseModel> GetPartnerDematAccounts(QueryValues query);
        Task<ApiCommonResponseModel> ManagePartnerDematAccount(PartnerDematAccountRequest accountRequest, Guid loggedUser);
        Task<ApiCommonResponseModel> GetFilteredPurchaseOrders(QueryValues queryValues);
        Task<ApiCommonResponseModel> SendWhatsappTemplateMessage(SendWhatsappMessageRequestModel param);
        Task<ApiCommonResponseModel> SaveChartImageForMobile(ImageUploadRequestModel request);
        Task<ApiCommonResponseModel> ManageAdvertisementImages(IFormFileCollection imageList, string type, string url, DateTime? expireOn, int? imageId = null, int? productId = null, string? productName = null);
        Task<ApiCommonResponseModel> GetFreeTrial(QueryValues queryValues);
        Task<ApiCommonResponseModel> GetPhonePe(QueryValues queryValues);
        Task<ApiCommonResponseModel> GetUserHistory(QueryValues queryValues);
        Task<ApiCommonResponseModel> GetSubscriptionDurationAsync();
        Task<ApiCommonResponseModel> GetSubscriptionPlanAsync();
        Task<ApiCommonResponseModel> GetSubscriptionDetailsAsync(QueryValues queryValues);
        Task<ApiCommonResponseModel> AddSubscriptionMappingAsync(SubscriptionModel.SubscriptionMappingRequestModel request);
        Task<ApiCommonResponseModel> UpdateSubscriptionMappingAsync(SubscriptionModel.SubscriptionMappingUpdateRequest request);
        Task<ApiCommonResponseModel> UpdateSubscriptionPlanAsync(int id, SubscriptionModel.SubscriptionPlanRequest request);
        Task<ApiCommonResponseModel> AddSubscriptionPlanAsync(SubscriptionModel.SubscriptionPlanRequest request);
        Task<ApiCommonResponseModel> ToggleSubscriptionDurationStatusAsync(int id, Guid loggedUser);
        Task<ApiCommonResponseModel> ToggleSubscriptionMappingStatusAsync(int id, Guid loggedUser);
        Task<ApiCommonResponseModel> ToggleSubscriptionPlanStatusAsync(int id, Guid loggedUser);
        Task<ApiCommonResponseModel> GetReasons(int basketId);
        Task<ApiCommonResponseModel> GetPhonePeChartDataAsync(QueryValues query);
        Task<ApiCommonResponseModel> GetPhonePePaymentReportChartAsync(PhonePePaymentReportChartResponceModel query);
        Task<ApiCommonResponseModel> SendWhatsappFromExcel();
        Task<ApiCommonResponseModel> UpdatePurchaseOrderAsync(int id, Guid loggedInUser);
        Task<ApiCommonResponseModel> ScheduleNotification(ScheduledNotificationRequestModel requestModel);
        Task<ApiCommonResponseModel> GetScheduleNotification(QueryValues query);
        Task<ApiCommonResponseModel> GetPerformance(GetPerformanceRequestModel requestModel);
        Task<ApiCommonResponseModel> DeletePerformance(int Id);
        Task<ApiCommonResponseModel> DeleteFreeTrail(int id, Guid loggedUser);
        Task<ApiCommonResponseModel> DeleteCompany(int id, Guid loggedUser);
        Task<ApiCommonResponseModel> DeleteCompanyDetails(int id, Guid loggedUser);
        Task<ApiCommonResponseModel> DeleteProductCommunity(int id, int loggedUser);
        Task<ApiCommonResponseModel> DeleteBonusProductMapping(int id, int loggedUser);
        Task<ApiCommonResponseModel> DeleteNotification(int id, int primarySid);
        Task<ApiCommonResponseModel> AddLeadFreeTrailAsync(LeadFreeTrialRequestModel request, Guid loggedInUser);
        Task<ApiCommonResponseModel> GetLeadFreeTrailsAsync(Guid LeadKey);
        Task<ApiCommonResponseModel> GetProductCommunityMappings(QueryValues queryValues);
        Task<ApiCommonResponseModel> GetBonusProductMappings(QueryValues queryValues);
        Task<ApiCommonResponseModel> GetLogsAsync(QueryValues query);
        Task<ApiCommonResponseModel> GetExceptionsAsync(QueryValues query);
        Task<ApiCommonResponseModel> DeleteLogByIdAsync(string id);
        Task<ApiCommonResponseModel> DeleteExceptionByIdAsync(string id);
        Task<ApiCommonResponseModel> DeleteLogsInBulk(QueryValues request);
        Task<ApiCommonResponseModel> DeleteExceptionsInBulk(QueryValues request);
        //Task<ApiCommonResponseModel> CreateChapterSubChapter(SubChapterRequestModel request);
        Task<ApiCommonResponseModel> AddSubChapter(SubChapterRequestModel request);
        Task<ApiCommonResponseModel> EditSubChapter(SubChapterRequestModel request);
        Task<ApiCommonResponseModel> EditChapter(SubChapterRequestModel request);
        Task<ApiCommonResponseModel> AddChapter(SubChapterRequestModel request);
        Task<ApiCommonResponseModel> DeleteChapter(int id, int loggedUser);
        Task<ApiCommonResponseModel> DeleteSubChapter(int id, int loggedUser);
        Task<ApiCommonResponseModel> UpdateSubChapterStatus(UpdateChapterStatusRequestModel request);
        Task<ApiCommonResponseModel> UpdateChapterStatus(UpdateChapterStatusRequestModel request);
        Task<ApiCommonResponseModel> UpdateQueryFormAsync(QueryFormRequestModel request);
        Task<ApiCommonResponseModel> GetQueryForms(QueryValues queryValues);
        Task<ApiCommonResponseModel> GetQueryFormRemarks(int queryFormId);
        Task<ApiCommonResponseModel> DeleteQueryForm(int id, int modifiedBy);
        Task<ApiCommonResponseModel> GetMobileUserBucketDetails(QueryValues queryValues);
        Task<ApiCommonResponseModel> TotalLogCount();
        Task<ApiCommonResponseModel> TotalExceptionCount();
    }

    public class MobileService : IMobileService
    {
        private readonly MongoDbService _mongoDbService;
        private readonly IMobileNotificationService _pushNotification;
        private readonly IMongoRepository<Log> _log;
        private readonly IMongoRepository<ExceptionLog> _exceptionLog;
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly FirebaseRealTimeDb _firebaseService;
        public MobileService(ResearchMantraContext context, IActivityService activityService,
            IConfiguration configuration, IMongoRepository<Log> mongoRepo, MongoDbService mongoDbService,
            IMobileNotificationService pushNotification, IMongoRepository<ExceptionLog> exceptionLog, IAzureBlobStorageService azureBlobStorageService, IHttpContextAccessor httpContextAccessor,
             FirebaseRealTimeDb firebaseService)
        {
            _dbContext = context;
            _activityService = activityService;
            _configuration = configuration;
            _mongoDbService = mongoDbService;
            _pushNotification = pushNotification;
            _log = mongoRepo;
            _exceptionLog = exceptionLog;
            _azureBlobStorageService = azureBlobStorageService;
            _httpContextAccessor = httpContextAccessor;
            _firebaseService = firebaseService;
        }

        private readonly ResearchMantraContext _dbContext;
        private readonly IActivityService _activityService;
        private readonly IConfiguration _configuration;
        private ApiCommonResponseModel responseModel = new();

        public async Task<ApiCommonResponseModel> ManageProduct(MobileProductRequestModel request)
        {
            try
            {
                //request.CreatedBy = Guid.Parse("4D8F904C-5AA1-EE11-812D-00155D23D79C"); //ToDo: Use Logged in user key instead of this
                request.CreatedBy = Guid.Parse(_configuration["AppSettings:DefaultAdmin"]); //ToDo: Use Logged in user key instead of this

                // Updating product
                if (request.Id != 0)
                {
                    var result = _dbContext.ProductsM.Where(item => item.Id == request.Id).FirstOrDefault();
                    if (result != null)
                    {
                        string landscapeImage = result.LandscapeImage;
                        string listImage = result.ListImage;

                        if (request.AttachmentType == "video")
                        {
                            landscapeImage = request.VideoUrl?.Trim();
                        }
                        else if (request.AttachmentType == "image" && request.LandscapeImage != null)
                        {
                            landscapeImage = await SaveProductLandscapeImage(request.LandscapeImage);
                        }

                        if (request.ListImage != null)
                        {
                            listImage = await SaveProductListImage(request.ListImage);
                        }
                        result.Name = request.Name;
                        result.Code = request.Code;
                        result.Description = request.Description;
                        result.DescriptionTitle = request.DescriptionTitle;
                        result.Price = request.Price;
                        result.SubscriptionId = request.SubscriptionId;
                        result.DiscountAmount = string.IsNullOrEmpty(request.DiscountAmount)
                            ? null
                            : decimal.Parse(request.DiscountAmount);
                        result.DiscountPercent = string.IsNullOrEmpty(request.DiscountPercent)
                            ? null
                            : int.Parse(request.DiscountPercent);
                        //result.IsActive = true;
                        result.CategoryID = request.Category == 0 ? result.CategoryID : request.Category;
                        result.ListImage = listImage;
                        result.LandscapeImage = landscapeImage;
                        result.ModifiedBy = request.CreatedBy;
                        result.ModifiedDate = DateTime.Now;
                        result.CanPost = request.CanPost;

                        responseModel.StatusCode = HttpStatusCode.OK;
                    }
                    else
                    {
                        responseModel.StatusCode = HttpStatusCode.NotFound;
                    }
                }
                // Creating product
                else
                {
                    string listImage = await SaveProductListImage(request.ListImage);
                    //string landscapeImage = await SaveProductLandscapeImage(request.LandscapeImage);
                    string landscapeImage = string.Empty;
                    if (request.AttachmentType == "video")
                    {
                        landscapeImage = request.VideoUrl?.Trim();
                    }
                    else if (request.AttachmentType == "image")
                    {
                        landscapeImage = await SaveProductLandscapeImage(request.LandscapeImage);
                    }
                    ProductsM newProduct = new()
                    {
                        Name = request.Name,
                        Code = request.Code,
                        Description = request.Description,
                        DescriptionTitle = request.DescriptionTitle,
                        CreatedBy = request.CreatedBy,
                        LandscapeImage = landscapeImage,
                        SubscriptionId = request.SubscriptionId,
                        ListImage = listImage,
                        CategoryID = request.Category,
                        DiscountAmount = request.DiscountAmount == null ? null : decimal.Parse(request.DiscountAmount),
                        DiscountPercent = request.DiscountPercent == null ? null : int.Parse(request.DiscountPercent),
                        Price = request.Price,
                        CreatedDate = DateTime.Now,
                        CanPost = request.CanPost
                    };

                    _ = await _dbContext.ProductsM.AddAsync(newProduct);

                    responseModel.StatusCode = HttpStatusCode.OK;
                }

                _ = await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var dd = ex;
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                responseModel.Message = ex.Message;
            }

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetPromoImagesCrm(string? searchText)
        {
            var query = from promo in _dbContext.PromotionM
                        where !promo.IsDelete &&
                              (string.IsNullOrWhiteSpace(searchText) || promo.Title.Contains(searchText))
                        join uCreated in _dbContext.Users on promo.CreatedBy equals uCreated.PublicKey into createdJoin
                        from createdBy in createdJoin.DefaultIfEmpty()

                        join uModified in _dbContext.Users on promo.ModifiedBy equals uModified.PublicKey into modifiedJoin
                        from modifiedBy in modifiedJoin.DefaultIfEmpty()

                        select new
                        {
                            Promo = promo,
                            CreatedByName = createdBy != null ? createdBy.FirstName : "Unknown",
                            ModifiedByName = modifiedBy != null ? modifiedBy.FirstName : "Unknown"
                        };

            var resultList = await query.ToListAsync();

            var responseList = resultList.Select(r => new GetPromotionResponse
            {
                Id = r.Promo.Id,
                mediaItems = !string.IsNullOrWhiteSpace(r.Promo.ButtonText)
                    ? JsonConvert.DeserializeObject<List<PromoMediaModel>>(r.Promo.ButtonText)
                    : new List<PromoMediaModel>(),

                StartDate = r.Promo.StartDate ?? DateTime.Now,
                EndDate = r.Promo.EndDate ?? DateTime.Now,
                Title = r.Promo.Title,
                Description = r.Promo.Description,
                MediaType = r.Promo.MediaType,
                ShouldDisplay = r.Promo.ShouldDisplay ?? false,
                MaxDisplayCount = r.Promo.MaxDisplayCount ?? 3,
                DisplayFrequency = r.Promo.DisplayFrequency ?? 5,
                LastShownAt = r.Promo.LastShownAt?.ToString("yyyy-MM-ddTHH:mm:ss") ?? "",
                IsNotification = r.Promo.IsNotification ?? false,
                ScheduleDate = r.Promo.ScheduleDate,
                GlobalButtonAction = r.Promo.GlobalButtonAction ?? false,
                Target = r.Promo.Target ?? "",
                ProductName = r.Promo.ProductName ?? "",
                ProductId = r.Promo.ProductId ?? 0,
                CreatedOn = r.Promo.CreatedOn ?? DateTime.Now,
                ModifiedOn = r.Promo.ModifiedOn ?? DateTime.Now,

                CreatedBy = r.CreatedByName ?? "Unknown",
                ModifiedBy = r.ModifiedByName ?? "Unknown",
                IsActive = r.Promo.IsActive ?? true
            }).ToList();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Success",
                Data = responseList,
                Total = responseList.Count
            };
        }


        public async Task<ApiCommonResponseModel> ManagePromotionAsync(PromotionRequestModel model, Guid loggedInUser)
        {
            long maxAllowedFileSize = long.Parse(_configuration["Azure:AdvertisementImageSizeCap"]!);
            List<string> allImageUrls = new();

            // Deserialize image URL list
            if (!string.IsNullOrEmpty(model.ImageUrlsRaw))
            {
                model.ImageUrls = JsonConvert.DeserializeObject<List<string>>(model.ImageUrlsRaw);
            }

            // Add existing image/video URLs once
            if (model.ImageUrls != null && model.ImageUrls.Count > 0)
                allImageUrls.AddRange(model.ImageUrls);

            if (model.MediaType == "video")
            {
                if (model.MediaFiles != null && model.MediaFiles.Count > 0)
                {
                    foreach (var file in model.MediaFiles)
                    {
                        var imageUrl = await SaveImageToAssetsFolder(file);
                        allImageUrls.Add(imageUrl);
                    }
                }

                // ❌ Don't re-add model.ImageUrls here
            }
            else if (model.MediaFiles != null && model.MediaFiles.Count > 0)
            {
                foreach (var file in model.MediaFiles)
                {
                    if (file.Length > maxAllowedFileSize)
                    {
                        return new ApiCommonResponseModel
                        {
                            Message = $"Each file must be less than {(double)maxAllowedFileSize / (1024 * 1024):F2} MB",
                            StatusCode = HttpStatusCode.Forbidden
                        };
                    }

                    var imageUrl = await SaveImageToAssetsFolder(file);
                    allImageUrls.Add(imageUrl);
                }
            }



            // 3. Create or update
            PromotionM promo;
            if (model.Id.HasValue)
            {
                promo = await _dbContext.PromotionM.FirstOrDefaultAsync(p => p.Id == model.Id.Value);
                if (promo == null)
                {
                    return new ApiCommonResponseModel
                    {
                        Message = "Promotion not found.",
                        StatusCode = HttpStatusCode.NotFound
                    };
                }
                promo.ModifiedBy = loggedInUser;
                promo.ModifiedOn = DateTime.Now;
            }
            else
            {

                promo = new PromotionM
                {
                    CreatedBy = loggedInUser,
                    CreatedOn = DateTime.Now
                };
                await _dbContext.PromotionM.AddAsync(promo);
            }


            // 4. Common Fields
            promo.Title = model.Title;
            promo.Description = model.Description;
            promo.MediaType = model.MediaType;
            promo.ScheduleDate = model.ScheduleDate;
            promo.IsNotification = model.IsNotification;
            promo.IsActive = model.IsActive;
            promo.ShouldDisplay = model.ShouldDisplay;
            promo.GlobalButtonAction = model.GlobalButtonAction;

            // Handle Nested Media + Buttons (per media file)
            // 5. Handle Nested Media + Buttons (per media file)
            var mediaWithButtons = new List<PromoMediaModel>();
            int pdfFileIndex = 0;

            for (int i = 0; i < allImageUrls.Count; i++)
            {
                var url = allImageUrls[i];
                var buttons = new List<PromoButtonModel>();
                List<MediaButtonMapping> mediaMappings = new();

                if (!string.IsNullOrEmpty(model.MediaButtonMappings))
                {
                    try
                    {
                        mediaMappings = JsonConvert.DeserializeObject<List<MediaButtonMapping>>(model.MediaButtonMappings);

                        var match = mediaMappings.FirstOrDefault(x => x.MediaIndex == i);
                        if (match != null && match.Buttons != null)
                        {
                            foreach (var btn in match.Buttons)
                            {
                                if (btn.Target == "pdf")
                                {
                                    if (string.IsNullOrWhiteSpace(btn.ActionUrl) || btn.ActionUrl == "file-placeholder")
                                    {
                                        if (model.PdfFiles != null && pdfFileIndex < model.PdfFiles.Count)
                                        {
                                            var pdfFile = model.PdfFiles[pdfFileIndex++];
                                            string pdfUrl = await SavePdfToAzure(pdfFile);
                                            btn.ActionUrl = pdfUrl;
                                        }
                                        else if (!string.IsNullOrWhiteSpace(btn.ActionUrl) && btn.ActionUrl != "file-placeholder")
                                        {
                                            // Use existing URL from frontend
                                            btn.ActionUrl = btn.ActionUrl;
                                        }
                                        else
                                        {
                                            return new ApiCommonResponseModel
                                            {
                                                Message = "Missing uploaded PDF file for one or more buttons.",
                                                StatusCode = HttpStatusCode.BadRequest
                                            };
                                        }
                                    }
                                }

                                else if (btn.Target == "url")
                                {
                                    // Already has the URL as ActionUrl
                                    btn.ActionUrl = btn.ActionUrl; // Keep it as is
                                }
                                else
                                {
                                    // Case: screen name target (e.g., subscriptonPlanScreen)
                                    // Store target screen name in ActionUrl
                                    btn.ActionUrl = btn.Target;
                                }

                                buttons.Add(new PromoButtonModel
                                {
                                    ButtonName = btn.ButtonName,
                                    Target = btn.Target,
                                    ActionUrl = btn.ActionUrl,
                                    ProductId = btn.ProductId,
                                    ProductName = btn.ProductName
                                });
                            }
                        }
                    }
                    catch
                    {
                        return new ApiCommonResponseModel
                        {
                            Message = "Invalid MediaButtonMappings JSON format",
                            StatusCode = HttpStatusCode.BadRequest
                        };
                    }
                }

                mediaWithButtons.Add(new PromoMediaModel
                {
                    mediaUrl = url,
                    Buttons = buttons
                });
            }

            // Finally, store the JSON into promo.ButtonText
            promo.ButtonText = JsonConvert.SerializeObject(mediaWithButtons);

            // 6. Handle global button case
            if (model.GlobalButtonAction == true)
            {
                if (!string.IsNullOrEmpty(model.Target) && model.Target.ToLower() == "url")
                {
                    promo.Target = model.ActionUrl;
                    promo.ActionUrl = null;
                }
                else
                {
                    promo.Target = model.Target;
                    promo.ProductId = model.ProductId;
                    promo.ProductName = model.ProductName;
                    promo.ActionUrl = null;
                }
            }
            else
            {
                promo.Target = null;
                promo.ProductId = null;
                promo.ProductName = null;
                promo.ActionUrl = null;
            }

            await _dbContext.SaveChangesAsync();
            // 7. Push to Firebase - Only if ShouldDisplay is true
            //if (promo.ShouldDisplay == true)
            //{
            //    var notificationData = new
            //    {
            //        lastChangedAt = (model.Id.HasValue ? promo.ModifiedOn : promo.CreatedOn)?.ToString("dd MMMM yyyy, hh:mm tt"),
            //        message = new
            //        {
            //            title = promo.Title,
            //            body = "New Promo Available!📢" // Or any short body like "New Promo Available "
            //        },
            //        promoIds = await _dbContext.PromotionM
            //                    .Where(p => p.ShouldDisplay == true && p.IsDelete == false && p.IsActive == true)
            //                    .OrderByDescending(p => p.ModifiedOn ?? p.CreatedOn)
            //                    .Select(p => p.Id)
            //                    .ToListAsync(),

            //        showLocalNotification = promo.Mandatory == true
            //    };

            //    await _firebaseService.UpdatePromotionDataAsync("PromotionData", notificationData);
            //}

            return new ApiCommonResponseModel
            {
                Message = model.Id.HasValue ? "Promotion updated successfully." : "Promotion created successfully.",
                StatusCode = HttpStatusCode.OK
            };
        }


        private async Task<string> SaveImageToAssetsFolder(IFormFile file)
        {

            string connectionString = _configuration["Azure:StorageConnection"];
            string containerName = _configuration["Azure:ContainerNameFirst"];
            string baseUrl = _configuration["Azure:ImageUrlSuffix"];

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync();
            await containerClient.SetAccessPolicyAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

            string fileName = Path.GetFileName(file.FileName);
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            return fileName;
        }
        private async Task<string> SavePdfToAzure(IFormFile file)
        {
            string connectionString = _configuration["Azure:StorageConnection"];
            string containerName = _configuration["Azure:ContainerNameFirst"];
            string baseUrl = _configuration["Azure:ImageUrlSuffix"];

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync();
            await containerClient.SetAccessPolicyAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

            string fileName = $"{DateTime.UtcNow.Ticks}-{file.FileName}";
            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            return $"{baseUrl}{fileName}";
        }

        public async Task<ApiCommonResponseModel> DeletePrImage(int Id)
        {
            var result = _dbContext.PromotionM.FirstOrDefault(item => item.Id == Id);
            if (result == null)
            {
                responseModel.StatusCode = HttpStatusCode.NotFound;
                return responseModel;
            }

            result.IsDelete = true;
            await _dbContext.SaveChangesAsync();

            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }
        public async Task<ApiCommonResponseModel> GetMobileProducts(string searchText)
        {
            var sqlParameters = new SqlParameter[]
            {
                new SqlParameter
                {
                    ParameterName = "SearchText",
                    Value = String.IsNullOrEmpty(searchText) ? DBNull.Value : searchText.Trim(),
                    SqlDbType = SqlDbType.VarChar, Size = 100
                },
            };

            List<GetMobileProductsSpResponseModel> result =
                await _dbContext.SqlQueryToListAsync<GetMobileProductsSpResponseModel>(
                    ProcedureCommonSqlParametersText.GetMobileProducts, sqlParameters);

            responseModel.Data = result;
            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Successfull";

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetProductCommunityMappings(QueryValues queryValues)
        {
            var apiCommonResponse = new ApiCommonResponseModel();

            try
            {
                // Step 1: Prepare common SQL parameters
                List<SqlParameter> sqlParameters = new List<SqlParameter>
                {
                    new SqlParameter("@SearchText", string.IsNullOrEmpty(queryValues.SearchText) ? (object)DBNull.Value : queryValues.SearchText),
                    new SqlParameter("@PageNumber", queryValues.PageNumber),
                    new SqlParameter("@PageSize", queryValues.PageSize),
                    new SqlParameter("@FromDate", queryValues.FromDate ?? (object)DBNull.Value), // Handle null case
                    new SqlParameter("@ToDate", queryValues.ToDate ?? (object)DBNull.Value), // Handle null case
                    new SqlParameter("@Status", string.IsNullOrEmpty(queryValues.PrimaryKey) ? (object)DBNull.Value : queryValues.PrimaryKey)
                };

                // Step 2: Create the output parameter for TotalCount
                SqlParameter parameterOutValue = new SqlParameter
                {
                    ParameterName = "@TotalCount",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };

                // Step 3: Add the output parameter to the list
                sqlParameters.Add(parameterOutValue);

                // Step 4: Execute the stored procedure and fetch data
                List<ProductCommunityMappingResponseModel> productCommunityList = await _dbContext.SqlQueryToListAsync<ProductCommunityMappingResponseModel>(
                    "EXEC GetProductCommunityMappings @SearchText, @PageNumber, @PageSize, @FromDate, @ToDate, @Status, @TotalCount OUTPUT",
                    sqlParameters.ToArray()
                );

                // Step 5: Retrieve the output parameter value (TotalCount)
                int totalRecords = (parameterOutValue.Value == DBNull.Value) ? 0 : Convert.ToInt32(parameterOutValue.Value);

                // Step 6: Prepare the response
                apiCommonResponse.Data = productCommunityList;
                apiCommonResponse.StatusCode = HttpStatusCode.OK;
                apiCommonResponse.Total = totalRecords;
            }
            catch (Exception ex)
            {
                apiCommonResponse.StatusCode = HttpStatusCode.InternalServerError;
                apiCommonResponse.Message = $"An error occurred: {ex.Message}";

                // Log the exception for debugging (optional)
                Console.WriteLine(ex);

                return apiCommonResponse;
            }

            return apiCommonResponse;

        }

        public async Task<ApiCommonResponseModel> GetBonusProductMappings(QueryValues queryValues)
        {
            var apiCommonResponse = new ApiCommonResponseModel();

            try
            {
                // Step 1: Prepare common SQL parameters
                List<SqlParameter> sqlParameters = new List<SqlParameter>
                {
                    new SqlParameter("@SearchText", string.IsNullOrEmpty(queryValues.SearchText) ? (object)DBNull.Value : queryValues.SearchText),
                    new SqlParameter("@PageNumber", queryValues.PageNumber),
                    new SqlParameter("@PageSize", queryValues.PageSize),
                    new SqlParameter("@FromDate", queryValues.FromDate ?? (object)DBNull.Value), // Handle null case
                    new SqlParameter("@ToDate", queryValues.ToDate ?? (object)DBNull.Value), // Handle null case
                    new SqlParameter("@Status", string.IsNullOrEmpty(queryValues.PrimaryKey) ? (object)DBNull.Value : queryValues.PrimaryKey)
                };

                // Step 2: Create the output parameter for TotalCount
                SqlParameter parameterOutValue = new SqlParameter
                {
                    ParameterName = "@TotalCount",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };

                // Step 3: Add the output parameter to the list
                sqlParameters.Add(parameterOutValue);

                // Step 4: Execute the stored procedure and fetch data
                List<BonusProductMappingResponseModel> productCommunityList = await _dbContext.SqlQueryToListAsync<BonusProductMappingResponseModel>(
                    "EXEC GetBonusProductMappings @SearchText, @PageNumber, @PageSize, @FromDate, @ToDate, @Status, @TotalCount OUTPUT",
                    sqlParameters.ToArray()
                );

                // Step 5: Retrieve the output parameter value (TotalCount)
                int totalRecords = (parameterOutValue.Value == DBNull.Value) ? 0 : Convert.ToInt32(parameterOutValue.Value);

                // Step 6: Prepare the response
                apiCommonResponse.Data = productCommunityList;
                apiCommonResponse.StatusCode = HttpStatusCode.OK;
                apiCommonResponse.Total = totalRecords;
            }
            catch (Exception ex)
            {
                apiCommonResponse.StatusCode = HttpStatusCode.InternalServerError;
                apiCommonResponse.Message = $"An error occurred: {ex.Message}";

                // Log the exception for debugging (optional)
                Console.WriteLine(ex);

                return apiCommonResponse;
            }

            return apiCommonResponse;

        }


        public async Task<ApiCommonResponseModel> GetFilteredProductsAsync(ProductSearchRequestModel filter)
        {
            var sqlParameters = new SqlParameter[]
            {
                new SqlParameter
                {
                    ParameterName = "SearchText",
                    Value = string.IsNullOrEmpty(filter.SearchText) ? DBNull.Value : filter.SearchText.Trim(),
                    SqlDbType = SqlDbType.VarChar, Size = 100
                },
                new SqlParameter
                {
                    ParameterName = "CategoryId",
                    Value = filter.Category.HasValue ? filter.Category.Value : DBNull.Value,
                    SqlDbType = SqlDbType.Int
                },
                 new SqlParameter
                {
                    ParameterName = "Status",
                    Value = filter.Status.HasValue ? filter.Status.Value : DBNull.Value,
                    SqlDbType = SqlDbType.Int
                }
            };

            List<GetMobileProductsSpResponseModel> result =
                await _dbContext.SqlQueryToListAsync<GetMobileProductsSpResponseModel>(
                    ProcedureCommonSqlParametersText.GetFilteredMobileProducts, sqlParameters);

            responseModel.Data = result;
            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Successful";

            return responseModel;
        }

        private async Task<string> SaveProductListImage(IFormFile image)
        {
            if (image == null || image.ContentType == "application/pdf")
            {
                return null;
            }

            //save square image

            var mobileAssetsFolderPath = _configuration["Mobile:RootDirectory"];
            string imageDirectory = Path.Combine(mobileAssetsFolderPath, "Assets", "Products");

            if (!Directory.Exists(imageDirectory))
            {
                Directory.CreateDirectory(imageDirectory);
            }

            string fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}{Path.GetExtension(image.FileName)}";

            string filePath = Path.Combine(imageDirectory, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return fileName;
        }

        private async Task<string> SaveProductLandscapeImage(IFormFile image)
        {
            if (image == null)
            {
                return null;
            }

            // save landscape image
            var mobileAssetsFolderPath = _configuration["Mobile:RootDirectory"];

            string landscapeImageDirectory = Path.Combine(mobileAssetsFolderPath, "Assets", "Products");

            if (!Directory.Exists(landscapeImageDirectory))
            {
                Directory.CreateDirectory(landscapeImageDirectory);
            }

            string fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}{Path.GetExtension(image.FileName)}";

            string filePath = Path.Combine(landscapeImageDirectory, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return fileName;
        }

        public async Task<ApiCommonResponseModel> GetProductContent(int productId)
        {
            var contentList = _dbContext.ProductsContentM
                .Where(c => c.ProductId == productId)
                .OrderByDescending(c => c.IsDeleted == false ? 1 : 0)
                .ThenByDescending(c => c.IsActive)
                .ThenByDescending(c => c.ModifiedOn ?? c.CreatedOn)
                .ToList();

            responseModel.Data = contentList;
            responseModel.Message = "Success";
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> ManageProductContent(ManageProductContentRequestModel request)
        {
            try
            {
                //request.CreatedBy =
                //    Guid.Parse("4D8F904C-5AA1-EE11-812D-00155D23D79C"); ToDo use logged in userkey insead of this

                // for Adding product content
                if (request.Id == 0)
                {
                    var listImageName = string.Empty;
                    var thumbnailImageName = string.Empty;

                    if (request.ListImage is not null && request.ThumbnailImage is null)
                    {
                        listImageName = await SaveListImage(request.ListImage);
                    }
                    else if (request.ThumbnailImage is not null && request.ListImage is null)
                    {
                        thumbnailImageName = await _azureBlobStorageService.UploadImage(request.ThumbnailImage);
                    }

                    List<ImageModel> screenshotList = new();

                    if (request.Screenshots != null)
                    {
                        if (request.Screenshots.Count > 3)
                        {
                            responseModel.Message = "You can upload a maximum of 3 screenshots.";
                            responseModel.StatusCode = HttpStatusCode.BadRequest;
                            return responseModel;
                        }

                        if (request.Screenshots?.Any() == true)
                        {
                            for (int i = 0; i < request.Screenshots.Count; i++)
                            {
                                var image = request.Screenshots[i];
                                var aspectRatio = request.AspectRatios?.ElementAtOrDefault(i) ?? "auto";
                                var fileName = await _azureBlobStorageService.UploadImage(image);

                                screenshotList.Add(new ImageModel
                                {
                                    Name = fileName,
                                    AspectRatio = aspectRatio
                                });
                            }
                        }
                    }

                    string screenshotJson = screenshotList.Any()
                        ? JsonConvert.SerializeObject(screenshotList)
                        : null;

                    ProductsContentM productContent = new()
                    {
                        ProductId = request.ProductId,
                        Attachment = string.IsNullOrWhiteSpace(request.Attachment) ? string.Empty : request.Attachment,
                        ListImage = string.IsNullOrEmpty(listImageName) ? null : listImageName,
                        ThumbnailImage = string.IsNullOrEmpty(thumbnailImageName) ? null : thumbnailImageName,
                        Title = request.Title,
                        CreatedBy = request.CreatedBy,
                        AttachmentType = request.AttachmentType,
                        CreatedOn = DateTime.Now,
                        Description = request.Description,
                        ScreenshotJson = screenshotJson,
                        IsActive = false,
                        IsDeleted = false
                    };

                    //if (request.ThumbnailImage is not null)
                    //{
                    //    var imageUrl = await _azureBlobStorageService.UploadImage(request.ThumbnailImage);
                    //    productContent.ThumbnailImage = _configuration["Azure:ImageUrlSuffix"] + imageUrl;
                    //}

                    await _dbContext.ProductsContentM.AddAsync(productContent);
                    await _dbContext.SaveChangesAsync();

                    responseModel.StatusCode = HttpStatusCode.OK;
                    responseModel.Message = "Successfull";
                }
                // editing a product content
                else
                {
                    var item = _dbContext.ProductsContentM.FirstOrDefault(c => c.Id == request.Id);

                    if (item == null)
                    {
                        responseModel.StatusCode = HttpStatusCode.NotFound;
                        responseModel.Message = "Product content not found.";
                        return responseModel;
                    }

                    // 1. Update core fields
                    item.Title = request.Title;
                    item.Description = request.Description;
                    item.AttachmentType = request.AttachmentType;
                    item.ModifiedOn = DateTime.Now;
                    item.IsActive = request.IsActive;
                    item.IsDeleted = request.IsDeleted;

                    // Step 1: Deserialize old screenshot list from DB
                    var oldScreenshotList = !string.IsNullOrWhiteSpace(item.ScreenshotJson)
    ? JsonConvert.DeserializeObject<List<ScreenshotItem>>(item.ScreenshotJson)
    : new List<ScreenshotItem>();

                    // Step 2: Parse deleted image names from string
                    var deletedImages = !string.IsNullOrWhiteSpace(request.DeletedScreenshots)
                        ? JsonConvert.DeserializeObject<List<string>>(request.DeletedScreenshots)
                        : new List<string>();

                    // Step 3: Retain only those not deleted
                    var retainedScreenshots = oldScreenshotList
                        .Where(img => !deletedImages.Contains(img.Name))
                        .ToList();

                    // Step 4: Add new screenshots
                    var finalScreenshotList = new List<ScreenshotItem>();
                    finalScreenshotList.AddRange(retainedScreenshots);

                    if (request.Screenshots != null && request.Screenshots.Any())
                    {
                        for (int i = 0; i < request.Screenshots.Count; i++)
                        {
                            var file = request.Screenshots[i];
                            var uploadedName = await _azureBlobStorageService.UploadImage(file);
                            var aspectRatio = request.AspectRatios != null && request.AspectRatios.Count > i
                                ? request.AspectRatios[i]
                                : "auto";

                            finalScreenshotList.Add(new ScreenshotItem
                            {
                                Name = uploadedName,
                                AspectRatio = aspectRatio
                            });
                        }
                    }

                    // Step 5: Delete removed images
                    if (deletedImages != null)
                    {
                        foreach (var imgName in deletedImages)
                        {
                            if (!string.IsNullOrWhiteSpace(imgName))
                            {
                                await _azureBlobStorageService.DeleteImage(imgName);
                            }
                        }
                    }

                    // Step 6: Save ScreenshotJson
                    item.ScreenshotJson = JsonConvert.SerializeObject(finalScreenshotList);

                    // Step 7: Handle AttachmentType
                    switch (request.AttachmentType?.ToLower())
                    {
                        case "image":
                            if (request.ThumbnailImage != null)
                            {
                                item.ThumbnailImage = await _azureBlobStorageService.UploadImage(request.ThumbnailImage);
                            }
                            item.Attachment = string.Empty;
                            break;

                        case "video":
                        case "pdf":
                            item.Attachment = request.Attachment ?? string.Empty;
                            item.ThumbnailImage = string.Empty;
                            break;

                        default:
                            item.Attachment = request.Attachment ?? string.Empty;
                            break;
                    }

                    // Step 8: Handle ListImage
                    if (request.ListImage != null)
                    {
                        item.ListImage = await _azureBlobStorageService.UploadImage(request.ListImage);
                    }

                    // Step 9: Save DB changes
                    await _dbContext.SaveChangesAsync();

                    responseModel.StatusCode = HttpStatusCode.OK;
                    responseModel.Message = "Updated successfully";
                    return responseModel;
                }
            }
            catch (Exception ex)
            {
                var dd = ex;
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
            }

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetProductCategories()
        {
            var categoryList = await _dbContext.ProductCategoriesM.Where(c => c.IsActive == true && c.IsDelete == false)
                .Select(c => new { c.Id, c.Name, c.Code }).ToListAsync();

            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Success.";
            responseModel.Data = categoryList;
            return responseModel;
        }

        private async Task<string> SaveThumbnailImage(IFormFile image)
        {
            if (image == null)
            {
                return null;
            }

            var mobileAssetsFolderPath = _configuration["Mobile:RootDirectory"];

            string assetsDirectory =
                Path.Combine(mobileAssetsFolderPath, "Assets", "Products", "ProductContent", "Thumbnail");

            if (!Directory.Exists(assetsDirectory))
            {
                Directory.CreateDirectory(assetsDirectory);
            }

            string fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}{Path.GetExtension(image.FileName)}";

            string filePath = Path.Combine(assetsDirectory, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return fileName;
        }

        private async Task<string> SaveListImage(IFormFile image)
        {
            if (image == null)
            {
                return null;
            }

            var mobileAssetsFolderPath = _configuration["Mobile:RootDirectory"];

            string assetsDirectory = Path.Combine(mobileAssetsFolderPath, "Assets", "Products", "ProductContent");

            if (!Directory.Exists(assetsDirectory))
            {
                Directory.CreateDirectory(assetsDirectory);
            }

            string fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}{Path.GetExtension(image.FileName)}";

            string filePath = Path.Combine(assetsDirectory, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return fileName;
        }

        public async Task<ApiCommonResponseModel> GetAdImagesCrm(string type, string? searchText)
        {
            var sqlParameters = new SqlParameter[]
            {
                new SqlParameter
                {
                    ParameterName = "type", Value = String.IsNullOrEmpty(type) ? DBNull.Value : type.Trim(),
                    SqlDbType = System.Data.SqlDbType.VarChar, Size = 50
                },
                new SqlParameter
                {
                    ParameterName = "searchText",
                    Value = string.IsNullOrEmpty(searchText) ? DBNull.Value : searchText.Trim(),
                    SqlDbType = System.Data.SqlDbType.VarChar, Size = 100
                },
            };

            var imageList =
                await _dbContext.SqlQueryAsync<GetAdImagesResponseModel>(ProcedureCommonSqlParametersText.GetAdImagesM,
                    sqlParameters);

            responseModel.Data = imageList;
            responseModel.Message = "Successfull.";
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }


        public async Task<ApiCommonResponseModel> GetActiveAdsImagesCRM()
        {
            var result = await _dbContext.AdvertisementImageM
                .Where(x => x.Type == "DASHBOARD"
                            && x.IsDelete == false
                            && x.IsActive == true)
                .ToListAsync();

            responseModel.Data = result;
            responseModel.Message = "Successfull.";
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> EnableDisableImage(int id)
        {
            var image = _dbContext.AdvertisementImageM.Where(c => c.Id == id).FirstOrDefault();

            if (image is not null)
            {
                //image.IsDelete = !image.IsDelete;
                image.IsActive = !image.IsActive;
                image.ModifiedOn = DateTime.Now;
                await _dbContext.SaveChangesAsync();
            }

            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        //public async Task<ApiCommonResponseModel> PostAdvertisementImages(IFormFileCollection imageList, string type,
        //    string url, DateTime? ExpireOn)
        //{
        //    long maxAllowedAdvertisementFileSize = long.Parse(_configuration["Mobile:AdvertisementImageSizeCap"]!);

        //    if (imageList.Count == 0 || imageList == null)
        //    {
        //        return responseModel;
        //    }

        //    foreach (IFormFile item in imageList)
        //    {
        //        long fileSizeInBytes = item.Length;
        //        if (fileSizeInBytes > maxAllowedAdvertisementFileSize!)
        //        {
        //            responseModel.Message =
        //                $"Max Image Size should be less than {(double)maxAllowedAdvertisementFileSize / (1024 * 1024)} MB";
        //            responseModel.StatusCode = HttpStatusCode.Forbidden;
        //            return responseModel;
        //        }
        //    }

        //    foreach (IFormFile item in imageList)
        //    {
        //        //if (type == "PROFILESCREEN")
        //        //{
        //        //    string imageName = await SaveImageToAssetsFolderAsync(item);

        //        //    var profileScreenImage = await _context.AdvertisementImageM.Where(c => c.Type.ToUpper() == "PROFILESCREEN" && c.IsActive == true).FirstOrDefaultAsync();
        //        //    if (profileScreenImage is not null)
        //        //    {
        //        //        profileScreenImage.IsActive = false;
        //        //        profileScreenImage.IsDelete = true;

        //        //        AdvertisementImageM advertImage = new()
        //        //        {
        //        //            CreatedBy = Guid.Parse(_config.GetSection("AppSettings:DefaultAdmin").Value!),
        //        //            CreatedOn = DateTime.Now,
        //        //            Url = url,
        //        //            IsActive = true,
        //        //            IsDelete = false,
        //        //            Name = "PROFILESCREEN" + "." + imageName.Split(".")[1],
        //        //            Type = type
        //        //        };
        //        //        await _context.AdvertisementImageM.AddAsync(advertImage);
        //        //        await _context.SaveChangesAsync();

        //        //    }
        //        //    break;
        //        //}

        //        string fileName = await SaveImageToAssetsFolderAsync(item);
        //        AdvertisementImageM adImage = new()
        //        {
        //            CreatedBy = Guid.Parse(_configuration.GetSection("AppSettings:DefaultAdmin").Value!),
        //            CreatedOn = DateTime.Now,
        //            ModifiedOn = null,
        //            Url = url,
        //            IsActive = true,
        //            IsDelete = false,
        //            Name = fileName,
        //            Type = type.ToUpper(),
        //            ExpireOn = ExpireOn != DateTime.MinValue ? ExpireOn : null
        //        };
        //        _dbContext.AdvertisementImageM.Add(adImage);
        //    }

        //    await _dbContext.SaveChangesAsync();

        //    responseModel.StatusCode = HttpStatusCode.OK;
        //    responseModel.Message = "Successfull.";

        //    return responseModel;
        //}

        public async Task<ApiCommonResponseModel> ManageAdvertisementImages(IFormFileCollection imageList, string type, string url, DateTime? expireOn, int? imageId = null, int? productId = null, string? productName = null)
        {
            long maxAllowedAdvertisementFileSize = long.Parse(_configuration["Mobile:AdvertisementImageSizeCap"]!);

            if (imageList != null && imageList.Count > 0)
            {
                foreach (IFormFile item in imageList)
                {
                    long fileSizeInBytes = item.Length;
                    if (fileSizeInBytes > maxAllowedAdvertisementFileSize)
                    {
                        responseModel.Message = $"Max Image Size should be less than {(double)maxAllowedAdvertisementFileSize / (1024 * 1024)} MB";
                        responseModel.StatusCode = HttpStatusCode.Forbidden;
                        return responseModel;
                    }
                }
            }

            if (imageId.HasValue)
            {
                var existingAdImage = await _dbContext.AdvertisementImageM
                    .FirstOrDefaultAsync(ad => ad.Id == imageId.Value && !ad.IsDelete);

                if (existingAdImage == null)
                {
                    responseModel.Message = "Advertisement image not found.";
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                    return responseModel;
                }

                if (imageList != null && imageList.Count > 0)
                {
                    string fileName = await SaveImageToAssetsFolderAsync(imageList[0]);
                    existingAdImage.Name = fileName;
                }

                existingAdImage.ProductId = productId;
                existingAdImage.ProductName = productName;

                existingAdImage.Type = type.ToUpper();
                existingAdImage.Url = url;
                existingAdImage.ExpireOn = expireOn != DateTime.MinValue ? expireOn : null;
                existingAdImage.ModifiedOn = DateTime.Now;

                responseModel.Message = "Advertisement image updated successfully.";
            }
            else
            {
                if (imageList == null || imageList.Count == 0)
                {
                    responseModel.Message = "At least one image is required for a new advertisement.";
                    responseModel.StatusCode = HttpStatusCode.BadRequest;
                    return responseModel;
                }

                foreach (IFormFile item in imageList)
                {
                    string fileName = await SaveImageToAssetsFolderAsync(item);

                    AdvertisementImageM newAdImage = new()
                    {
                        CreatedBy = Guid.Parse(_configuration.GetSection("AppSettings:DefaultAdmin").Value!),
                        CreatedOn = DateTime.Now,
                        Url = url,
                        ProductName = productName,
                        ProductId = productId,
                        IsActive = true,
                        IsDelete = false,
                        Name = fileName,
                        Type = type.ToUpper(),
                        ExpireOn = expireOn != DateTime.MinValue ? expireOn : null
                    };

                    await _dbContext.AdvertisementImageM.AddAsync(newAdImage);
                }

                responseModel.Message = "Advertisement image added successfully.";
            }

            await _dbContext.SaveChangesAsync();

            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        private async Task<string> SaveImageToAssetsFolderAsync(IFormFile advertisementImage)
        {
            var mobileAssetsFolderPath = _configuration["Mobile:RootDirectory"];

            string assetsDirectory = Path.Combine(mobileAssetsFolderPath, "Assets", "Advertisement");

            if (!Directory.Exists(assetsDirectory))
            {
                Directory.CreateDirectory(assetsDirectory);
            }

            string fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}{Path.GetExtension(advertisementImage.FileName)}";

            string filePath = Path.Combine(assetsDirectory, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await advertisementImage.CopyToAsync(fileStream);
            }

            return fileName;
        }

        public async Task<ApiCommonResponseModel> DisableProduct(int productId)
        {
            try
            {
                var product = _dbContext.ProductsM.Where(c => c.Id == productId).FirstOrDefault();

                if (product == null)
                {
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                    return responseModel;
                }

                product.IsActive = !product.IsActive;
                product.ModifiedDate = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Message = "Successful.";
                return responseModel;
            }
            catch (Exception ex)
            {
                var dd = ex;
                return responseModel;
            }
        }

        //public async Task<ApiCommonResponseModel> GetSubscriptions()
        //{
        //    var subscription = _dbContext.Subscriptions.Where(c => c.IsActive == true && c.IsDeleted == false)
        //        .Select(c => new { c.Id, c.SubscriptionName }).ToList();
        //    responseModel.Data = subscription;
        //    responseModel.StatusCode = HttpStatusCode.OK;
        //    return responseModel;
        //}

        public async Task<ApiCommonResponseModel> GetAdvertisementType()
        {
            var imageTypes = _dbContext.AdvertisementImageM.Select(item => item.Type).Distinct();
            responseModel.Data = imageTypes;
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> DeleteProduct(int productId)
        {
            var currentDateTime = DateTime.Now;

            var result = _dbContext.MyBucketM
                .Where(bucket => bucket.ProductId == productId
                                 && bucket.IsActive
                                 && bucket.StartDate <= currentDateTime
                                 && (bucket.EndDate == null || bucket.EndDate >= currentDateTime))
                .ToList();

            if (result.Count > 0)
            {
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                responseModel.Message = "Active product cannot be deleted.";
                return responseModel;
            }

            var productToUpdate = _dbContext.ProductsM.FirstOrDefault(c => c.Id == productId);

            productToUpdate.IsDeleted = true;
            productToUpdate.IsActive = false;

            await _dbContext.SaveChangesAsync();

            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Product deleted successfully";

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> DeleteAdImage(int imageId)
        {
            var result = _dbContext.AdvertisementImageM.FirstOrDefault(item => item.Id == imageId);
            if (result == null)
            {
                responseModel.StatusCode = HttpStatusCode.NotFound;
                return responseModel;
            }

            result.IsDelete = true;
            await _dbContext.SaveChangesAsync();

            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> ManageArticle(ManageArticleModel request)
        {

            // add article details
            if (request.CompanyId == 0)
            {
                CompanyDetailM companyDetail = new()
                {
                    BasketId = request.BasketId,
                    IsPublished = request.IsPublished,
                    Symbol = request.Symbol,
                    IsFree = request.IsFree,
                    ChartImageUrl = request.ChartUrl,
                    Description = request.Description,
                    Name = request.Name,
                    WebsiteUrl = request.WebsiteUrl,
                    OtherImage = request.OtherUrl,
                    CreatedOn = DateTime.Now,
                    ShortSummary = request.ShortSummary,
                    CreatedBy = request.LoggedInUserKey,
                    IsActive = request.IsActive ?? false,
                    PublishDate = request.IsPublished ? DateTime.Now : null,
                    MarketCap = request.MarketCap,
                    PE = request.PE,
                    CurrentPrice = request.CurrentPrice,
                    YesterdayPrice = request.YesterdayPrice,
                    SharesInCrores = request.Shares,
                    TTMNetProfitInCrores = request.TtmNetProfit,
                    FaceValue = request.FaceValue,
                    NetWorth = request.NetWorth,
                    PromotersHolding = request.PromotersHolding,
                    ProfitGrowth = request.ProfitGrowth,
                };
                try
                {
                    _dbContext.CompanyDetailM.Add(companyDetail);
                    await _dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                int generatedCompanyId = companyDetail.Id;

                await _activityService.UserLog(request.LoggedInUserKey.ToString(), null,
                    request.IsPublished
                        ? Model.DB.Tables.ActivityTypeEnum.ReportPublished
                        : Model.DB.Tables.ActivityTypeEnum.ReportUnpublished, generatedCompanyId.ToString());
                CompanyTypeM companyType = new()
                {
                    FutureVisibility = request.Future_Visibility,
                    HNIInstitutionalPromotersBuy = request.HNI_Institutional_PromotersBuy,
                    FuturisticSector = request.Futuristic_Sector,
                    LTOPUptrend = request.LT_OP_Uptrend,
                    SpecialSituations = request.Special_Situations,
                    STOPOpUpTrend = request.ST_OP_UpTrend,
                    Symbol = request.Name,
                    CompanyId = generatedCompanyId
                };

                _dbContext.CompanyTypeM.Add(companyType);
                await _dbContext.SaveChangesAsync();

                var lastOneYearMonthlyPrices = request.LastYearMonthlyPrices
                    .Select(price => new LastOneYearMonthlyPriceM
                    {
                        Symbol = request.Name,
                        Month = price.Month,
                        Price = price.Price,
                        CompanyId = generatedCompanyId
                    })
                    .ToList();

                _dbContext.LastOneYearMonthlyPriceM.AddRange(lastOneYearMonthlyPrices);

                var lastTenYearSalesMList = new List<LastTenYearSalesM>();

                foreach (var yearValue in request.LastTenYearSales[0].Values)
                {
                    var lastTenYearSalesM = new LastTenYearSalesM
                    {
                        CompanyId = generatedCompanyId,
                        Year = yearValue.Year,
                        //Symbol = request.Name,
                        Sales = request.LastTenYearSales.FirstOrDefault(x => x.Metric == "sales")?.Values
                            .FirstOrDefault(v => v.Year == yearValue.Year)?.Value,
                        OpProfit = request.LastTenYearSales.FirstOrDefault(x => x.Metric == "opProfit")?.Values
                            .FirstOrDefault(v => v.Year == yearValue.Year)?.Value,
                        NetProfit = request.LastTenYearSales.FirstOrDefault(x => x.Metric == "netProfit")?.Values
                            .FirstOrDefault(v => v.Year == yearValue.Year)?.Value,
                        OTM = request.LastTenYearSales.FirstOrDefault(x => x.Metric == "otm")?.Values
                            .FirstOrDefault(v => v.Year == yearValue.Year)?.Value,
                        NPM = request.LastTenYearSales.FirstOrDefault(x => x.Metric == "npm")?.Values
                            .FirstOrDefault(v => v.Year == yearValue.Year)?.Value,
                        PromotersPercent = request.LastTenYearSales
                            .FirstOrDefault(x => x.Metric == "promotersPercent")?.Values
                            .FirstOrDefault(v => v.Year == yearValue.Year)?.Value,
                    };

                    lastTenYearSalesMList.Add(lastTenYearSalesM);
                }

                _dbContext.LastTenYearSalesM.AddRange(lastTenYearSalesMList);
                await _dbContext.SaveChangesAsync();
            }

            // edit company details
            else
            {
                var company = _dbContext.CompanyDetailM.Where(c => c.Id == request.CompanyId).FirstOrDefault();
                if (company == null)
                {
                    return null;
                }

                if (company.IsPublished != request.IsPublished)
                {
                    await _activityService.UserLog(request.LoggedInUserKey.ToString(), null,
                        request.IsPublished
                            ? Model.DB.Tables.ActivityTypeEnum.ReportPublished
                            : Model.DB.Tables.ActivityTypeEnum.ReportUnpublished, request.CompanyId.ToString());
                }

                company.Symbol = request.Symbol;
                company.IsFree = request.IsFree;
                company.IsPublished = request.IsPublished;
                company.ChartImageUrl = request.ChartUrl;
                company.Description = request.Description;
                company.ShortSummary = request.ShortSummary;
                company.ModifiedOn = DateTime.Now;
                company.Name = request.Name;
                company.WebsiteUrl = request.WebsiteUrl;
                company.OtherImage = request.OtherUrl;
                company.ModifiedBy = request.LoggedInUserKey;
                company.PublishDate = request.IsPublished
                    ? company.PublishDate ?? DateTime.Now
                    : null;
                company.IsActive = request.IsActive ?? false;
                company.MarketCap = request.MarketCap;
                company.PE = request.PE;
                company.CurrentPrice = request.CurrentPrice;
                company.YesterdayPrice = request.YesterdayPrice;
                company.SharesInCrores = request.Shares;
                company.TTMNetProfitInCrores = request.TtmNetProfit;
                company.FaceValue = request.FaceValue;
                company.NetWorth = request.NetWorth;
                company.PromotersHolding = request.PromotersHolding;
                company.ProfitGrowth = request.ProfitGrowth;

                await _dbContext.SaveChangesAsync();

                //update monthly price
                var existingMonthlyRecords = _dbContext.LastOneYearMonthlyPriceM
                    .Where(p => p.CompanyId == request.CompanyId).ToList();

                for (var i = 0; i < request.LastYearMonthlyPrices.Count; i++)
                {
                    if (request.LastYearMonthlyPrices[i].Id == 0)
                    {
                        LastOneYearMonthlyPriceM newRecord = new()
                        {
                            Price = request.LastYearMonthlyPrices[i].Price,
                            Month = request.LastYearMonthlyPrices[i].Month,
                            CompanyId = request.CompanyId,
                            Symbol = request.Symbol
                        };

                        _dbContext.LastOneYearMonthlyPriceM.Add(newRecord);
                    }
                    else
                    {
                        var recordToUpdate =
                            existingMonthlyRecords.FirstOrDefault(c => c.Id == request.LastYearMonthlyPrices[i].Id);

                        recordToUpdate.Price = request.LastYearMonthlyPrices[i].Price;
                        recordToUpdate.Month = request.LastYearMonthlyPrices[i].Month;
                    }
                }

                var recordsToDelete = existingMonthlyRecords.Select(r => r.Id)
                    .Except(request.LastYearMonthlyPrices.Select(r => r.Id)).ToList();

                if (recordsToDelete.Count > 0)
                {
                    var recordsToRemove = _dbContext.LastOneYearMonthlyPriceM
                        .Where(record => recordsToDelete.Contains(record.Id)).ToList();

                    _dbContext.LastOneYearMonthlyPriceM.RemoveRange(recordsToRemove);
                }

                await _dbContext.SaveChangesAsync();

                // update monthly price
                //var existingMonthlyRecords = _dbContext.LastOneYearMonthlyPriceM.Where(p => p.CompanyId == request.CompanyId).ToList();
                //if (existingMonthlyRecords.Count == 0)
                //{
                //    var lastOneYearMonthlyPrices = request.LastYearMonthlyPrices
                //        .Select(price => new LastOneYearMonthlyPriceM
                //        {
                //            Symbol = request.Name,
                //            Month = price.Month,
                //            Price = price.Price,
                //            CompanyId = request.CompanyId

                //        })
                //        .ToList();

                //    _dbContext.LastOneYearMonthlyPriceM.AddRange(lastOneYearMonthlyPrices);
                //}
                //else if (existingMonthlyRecords.Count < request.LastYearMonthlyPrices.Count)
                //{
                //    for (int i = 0; i < request.LastYearMonthlyPrices.Count; i++)
                //    {
                //        var updateRecord = _dbContext.LastOneYearMonthlyPriceM.Where(item => item.Month == request.LastYearMonthlyPrices[i].Month).FirstOrDefault();
                //        if(updateRecord is null)
                //        {
                //            LastOneYearMonthlyPriceM newRecord = new()
                //            {
                //                Month = request.LastYearMonthlyPrices[i].Month,
                //                Price = request.LastYearMonthlyPrices[i].Price,
                //                Symbol = request.Symbol,
                //                CompanyId = request.CompanyId
                //            };

                //            _dbContext.LastOneYearMonthlyPriceM.Add(newRecord);
                //        }
                //        else
                //        {
                //            updateRecord.Price = request.LastYearMonthlyPrices[i].Price;

                //        }

                //    }
                //}

                //else
                //{
                //    for (int i = 0; i < existingMonthlyRecords.Count; i++)
                //    {
                //        if (existingMonthlyRecords[i].Month == request.LastYearMonthlyPrices[i].Month)
                //        {
                //            existingMonthlyRecords[i].Price = request.LastYearMonthlyPrices[i].Price;
                //            existingMonthlyRecords[i].Month = request.LastYearMonthlyPrices[i].Month;
                //        }
                //        else if (!existingMonthlyRecords.Select(x => x.Month).Contains(existingMonthlyRecords[i].Month))
                //        {
                //        }
                //    }
                //}

                // update ten years price

                var existingRecords = _dbContext.LastTenYearSalesM
                    .Where(record => record.CompanyId == request.CompanyId).ToList();

                var convertedData = ConvertToLastTenYearSalesM(request.LastTenYearSales);

                for (int i = 0; i < existingRecords.Count; i++)
                {
                    if (existingRecords[i].Year == convertedData[i].Year)
                    {
                        existingRecords[i].Sales = convertedData[i].Sales;
                        existingRecords[i].OTM = convertedData[i].OTM;
                        existingRecords[i].NPM = convertedData[i].NPM;
                        existingRecords[i].OpProfit = convertedData[i].OpProfit;
                        existingRecords[i].NetProfit = convertedData[i].NetProfit;
                        existingRecords[i].PromotersPercent = convertedData[i].PromotersPercent;
                    }
                    else if (!existingRecords.Select(x => x.Year).Contains(convertedData[i].Year))
                    {
                        existingRecords.Remove(existingRecords[i]);
                    }
                }

                await _dbContext.SaveChangesAsync();

                //var records = _dbContext.LastTenYearSalesM.Where(record => record.CompanyId == request.CompanyId).ToList();

                //_dbContext.LastTenYearSalesM.RemoveRange(records);

                //var lastTenYearSalesMList = new List<LastTenYearSalesM>();

                //foreach (var yearValue in request.LastTenYearSales[0].Values)
                //{
                //    var lastTenYearSalesM = new LastTenYearSalesM
                //    {
                //        CompanyId = request.CompanyId,
                //        Year = yearValue.Year,
                //        Sales = request.LastTenYearSales.FirstOrDefault(x => x.Metric == "sales")?.Values.FirstOrDefault(v => v.Year == yearValue.Year)?.Value,
                //        OpProfit = request.LastTenYearSales.FirstOrDefault(x => x.Metric == "opProfit")?.Values.FirstOrDefault(v => v.Year == yearValue.Year)?.Value,
                //        NetProfit = request.LastTenYearSales.FirstOrDefault(x => x.Metric == "netProfit")?.Values.FirstOrDefault(v => v.Year == yearValue.Year)?.Value,
                //        OTM = request.LastTenYearSales.FirstOrDefault(x => x.Metric == "otm")?.Values.FirstOrDefault(v => v.Year == yearValue.Year)?.Value,
                //        NPM = request.LastTenYearSales.FirstOrDefault(x => x.Metric == "npm")?.Values.FirstOrDefault(v => v.Year == yearValue.Year)?.Value,
                //        PromotersPercent = request.LastTenYearSales.FirstOrDefault(x => x.Metric == "promotersPercent")?.Values.FirstOrDefault(v => v.Year == yearValue.Year)?.Value,
                //    };

                //    lastTenYearSalesMList.Add(lastTenYearSalesM);
                //}

                //_dbContext.LastTenYearSalesM.AddRange(lastTenYearSalesMList);
                //await _dbContext.SaveChangesAsync();

                var companyType = _dbContext.CompanyTypeM.Where(c => c.CompanyId == request.CompanyId).FirstOrDefault();

                companyType.FutureVisibility = request.Future_Visibility;
                companyType.HNIInstitutionalPromotersBuy = request.HNI_Institutional_PromotersBuy;
                companyType.FuturisticSector = request.Futuristic_Sector;
                companyType.LTOPUptrend = request.LT_OP_Uptrend;
                companyType.SpecialSituations = request.Special_Situations;
                companyType.STOPOpUpTrend = request.ST_OP_UpTrend;
                companyType.Symbol = request.Name;
                companyType.CompanyId = request.CompanyId;

                await _dbContext.SaveChangesAsync();
            }
            ;

            responseModel.StatusCode = HttpStatusCode.OK;

            return responseModel;
            //}
            //catch (Exception ex)
            //{
            //    responseModel.Message = ex.Message;
            //    responseModel.Exceptions = ex;
            //    return responseModel;
            //}
        }

        //public async Task<ApiCommonResponseModel> GetBaskets(bool? isActive)
        //{
        //    var query = _dbContext.BasketsM.Where(c => c.IsDelete == false);

        //    if (isActive.HasValue)
        //    {
        //        query = query.Where(c => c.IsActive == isActive.Value);
        //    }

        //    responseModel.Data = await query
        //     .OrderByDescending(c => c.SortOrder)
        //     .ToListAsync();
        //    responseModel.StatusCode = HttpStatusCode.OK;
        //    return responseModel;
        //}

        public async Task<ApiCommonResponseModel> GetBaskets(string status)
        {
            var query = _dbContext.BasketsM.AsQueryable();

            if (status == "active")
            {
                query = query.Where(c => c.IsDelete == false && c.IsActive == true);
            }
            else if (status == "inactive")
            {
                query = query.Where(c => c.IsDelete == false && c.IsActive == false);
            }
            //else if (status == "deleted")
            //{
            //    query = query.Where(c => c.IsDelete == true && c.IsActive == false);
            //}
            //else
            //{
            //    // Default: Return all non-deleted baskets if status is null/invalid
            //    query = query.Where(c => c.IsDelete == false);
            //}

            responseModel.Data = await query
                .OrderBy(c => c.IsDelete == false)
                    .ThenBy(c => c.SortOrder)
                .ToListAsync();

            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }


        public async Task<ApiCommonResponseModel> ManageBasketsAsync(ManageBasketsRequestModel request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                responseModel.StatusCode = HttpStatusCode.BadRequest;
            }

            Baskets basket;

            if (request.Id > 0)
            {
                basket = await _dbContext.BasketsM.Where(x => x.Id == request.Id)
                        .FirstOrDefaultAsync();

                if (basket == null)
                {
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                }

                basket.Title = request.Title;
                basket.Description = request.Description;
                basket.IsFree = request.IsFree;
                basket.SortOrder = request.SortOrder;

                await _dbContext.SaveChangesAsync();

                responseModel.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                var maxSortOrder = await _dbContext.BasketsM.MaxAsync(x => (int?)x.SortOrder) ?? 0;
                basket = new Baskets
                {
                    Title = request.Title,
                    Description = request.Description,
                    IsFree = request.IsFree,
                    IsActive = false,
                    IsDelete = false,
                    SortOrder = maxSortOrder + 1
                };

                _dbContext.BasketsM.Add(basket);
                await _dbContext.SaveChangesAsync();

                responseModel.StatusCode = HttpStatusCode.OK;
            }
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> UpdateBasketStatusAsync(UpdateBasketStatusRequestModel request)
        {
            var responseModel = new ApiCommonResponseModel();

            if (request.Id <= 0)
            {
                responseModel.StatusCode = HttpStatusCode.BadRequest;
                return responseModel;
            }

            var basket = await _dbContext.BasketsM.FirstOrDefaultAsync(x => x.Id == request.Id);

            if (basket == null)
            {
                responseModel.StatusCode = HttpStatusCode.NotFound;
                return responseModel;
            }

            basket.IsActive = request.IsActive;

            await _dbContext.SaveChangesAsync();

            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetCompanies(int basketId)
        {
            var responseModel = new ApiCommonResponseModel
            {
                Data = await _dbContext.CompanyDetailM
                    .Where(c => c.BasketId == basketId && c.IsActive == true)
                    .Select(c => new
                    {
                        c.Id,
                        c.BasketId,
                        c.Name,
                        c.Symbol,
                        c.Description,
                        c.ShortSummary,
                        c.MarketCap,
                        c.PE,
                        c.ChartImageUrl,
                        c.OtherImage,
                        c.WebsiteUrl,
                        c.CreatedOn,
                        c.CreatedBy,
                        c.ModifiedBy,
                        c.ModifiedOn,
                        c.IsActive,
                        c.IsDelete,
                        c.IsPublished,
                        c.TrialDescription,
                        c.IsFree,
                        c.PublishDate,
                        CommentCount = _dbContext.CompanyDetailMessageM
                            .Count(cm => cm.CompanyDetailId == c.Id && cm.IsActive)
                    })
                    .OrderByDescending(x => x.CreatedOn)
                    .ToListAsync(),

                StatusCode = HttpStatusCode.OK,
                Message = "Success"
            };

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetCompanyDetails(int companyId)
        {
            var company = _dbContext.CompanyDetailM
                .Where(c => c.Id == companyId)
                .Select(c => new
                {
                    CompanyId = c.Id,
                    c.Name,
                    c.Description,
                    c.ShortSummary,
                    c.MarketCap,
                    c.PE,
                    ChartUrl = c.ChartImageUrl,
                    OtherUrl = c.OtherImage,
                    c.WebsiteUrl,
                    c.Symbol,
                    c.IsPublished,
                    c.IsFree,
                    c.CurrentPrice,
                    c.SharesInCrores,
                    c.TTMNetProfitInCrores,
                    c.FaceValue,
                    c.NetWorth,
                    c.PromotersHolding,
                    c.ProfitGrowth,
                    c.PublishDate,
                    c.YesterdayPrice
                }).FirstOrDefault();

            if (company == null)
            {
                return null;
            }

            var companyComments = (from c in _dbContext.CompanyDetailMessageM
                                   join u in _dbContext.MobileUsers
                                       on c.ModifiedBy equals u.PublicKey into userJoin
                                   from user in userJoin.DefaultIfEmpty()
                                   where c.CompanyDetailId == companyId && c.IsActive == true
                                   select new CompanyCommentVm
                                   {
                                       Id = c.Id,
                                       CompanyDetailId = c.CompanyDetailId,
                                       Message = c.Message,
                                       CreatedByName = user.FullName,
                                       CreatedByPublicKey = user.PublicKey,
                                       ModifiedOn = c.ModifiedOn
                                   }).ToList();

            int totalCommentCount = companyComments.Count;



            var companyType = _dbContext.CompanyTypeM
                .Where(ct => ct.CompanyId == companyId)
                .Select(ct => new CompanyTypeM
                {
                    Id = ct.Id,
                    CompanyId = ct.CompanyId,
                    Symbol = ct.Symbol,
                    LTOPUptrend = ct.LTOPUptrend,
                    STOPOpUpTrend = ct.STOPOpUpTrend,
                    FuturisticSector = ct.FuturisticSector,
                    HNIInstitutionalPromotersBuy = ct.HNIInstitutionalPromotersBuy,
                    SpecialSituations = ct.SpecialSituations,
                    FutureVisibility = ct.FutureVisibility
                }).FirstOrDefault();

            var lastOneYearMonthlyPrices = _dbContext.LastOneYearMonthlyPriceM
                .Where(lm => lm.CompanyId == companyId)
                .Select(lm => new LastOneYearMonthlyPriceM
                {
                    Id = lm.Id,
                    CompanyId = lm.CompanyId,
                    Symbol = lm.Symbol,
                    Month = lm.Month,
                    Price = lm.Price
                }).ToList();

            var lastTenYearSales = _dbContext.LastTenYearSalesM
                .Where(ls => ls.CompanyId == companyId)
                .Select(ls => new LastTenYearSalesM
                {
                    Id = ls.Id,
                    CompanyId = ls.CompanyId,
                    Year = ls.Year,
                    //Symbol = ls.Symbol,
                    Sales = ls.Sales,
                    OpProfit = ls.OpProfit,
                    NetProfit = ls.NetProfit,
                    OTM = ls.OTM,
                    NPM = ls.NPM,
                    PromotersPercent = ls.PromotersPercent
                }).ToList();

            //return companyDetails;
            responseModel.Data = new CompanyDetailsVm
            {
                CompanyId = company.CompanyId,
                IsFree = company.IsFree,
                Name = company.Name,
                Symbol = company.Symbol,
                ShortSummary = company.ShortSummary,
                PE = company.PE,
                Description = company.Description,
                ChartUrl = company.ChartUrl,
                OtherUrl = company.OtherUrl,
                WebsiteUrl = company.WebsiteUrl,
                CompanyType = companyType,
                IsPublished = company.IsPublished,
                LastOneYearMonthlyPrices = lastOneYearMonthlyPrices,
                LastTenYearSales = lastTenYearSales,
                SharesInCrores = company.SharesInCrores,
                MarketCap = company.MarketCap,
                TTMNetProfitInCrores = company.TTMNetProfitInCrores,
                FaceValue = company.FaceValue,
                NetWorth = company.NetWorth,
                PromotersHolding = company.PromotersHolding,
                ProfitGrowth = company.ProfitGrowth,
                CurrentPrice = company.CurrentPrice,
                YesterdayPrice = company.YesterdayPrice,
                PublishDate = company.PublishDate,
                Comments = companyComments,
                TotalCommentCount = totalCommentCount
            };

            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetCompanyReportDetailsFromExcel(CompanyReportExcelImportModal param)
        {
            List<string> _exceptions = new();

            try
            {
                ManageArticleModel finalOutPut = new();
                using XLWorkbook package = new(param.excelFile.OpenReadStream());

                #region

                List<LastTenYearSales> lastTenYearSales = new();

                var companySheet = package.Worksheets.FirstOrDefault(item => item.Name.ToLower() == "company");

                if (companySheet != null)
                {
                    var rangeUsed = companySheet.RangeUsed();
                    ReadCompanyDetails(rangeUsed, finalOutPut, _exceptions);
                }

                var profitandlossSheet = package.Worksheets.FirstOrDefault(item => item.Name.ToLower() == "profitandloss");

                if (profitandlossSheet != null)
                {
                    var headers = new List<string>();
                    for (int row = 2; row <= 12; row++)
                    {
                        if (row == 2)
                        {
                            for (int col = 2; col <= 13; col++)
                            {
                                try
                                {
                                    var tempData = profitandlossSheet.Cell(row, col).GetString();
                                    if (!string.IsNullOrEmpty(tempData))
                                    {
                                        _ = DateTime.TryParse(tempData, out DateTime parsedDate);
                                        headers.Add(parsedDate.ToShortDateString());
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _exceptions.Add("WhileReading profitandlossSheet , Col: " + col + " Row: " + row);
                                    await _exceptionLog.AddAsync(new ExceptionLog
                                    {

                                        CreatedOn = DateTime.Now,
                                        InnerException = ex.InnerException?.ToString(),
                                        Message = ex.Message,
                                        RequestBody = Newtonsoft.Json.JsonConvert.SerializeObject(param),
                                        Source = "GetCompanyReportDetailsFromExcel",
                                        StackTrace = ex.StackTrace
                                    });
                                }
                            }
                        }
                        else
                        {
                            var tempFieldName = profitandlossSheet.Cell(row, 1).GetString();
                            if (!string.IsNullOrEmpty(tempFieldName))
                            {
                                tempFieldName = tempFieldName.ToLower().Trim();
                                tempFieldName = new string(tempFieldName.Where(c => char.IsLetterOrDigit(c))
                                    .ToArray());
                            }

                            if (string.IsNullOrEmpty(profitandlossSheet.Cell(1, 1).GetString()) &&
                                string.IsNullOrEmpty(profitandlossSheet.Cell(2, 2).GetString()))
                            {
                                break;
                            }
                            else if (tempFieldName != null && (
                                         tempFieldName.Contains("sales")
                                         || tempFieldName.Contains("expenses") ||
                                         tempFieldName.Contains("interest")
                                         || tempFieldName == ("netprofit") || tempFieldName == "dividendpayout")
                                     || tempFieldName.Contains("operating")
                                     || tempFieldName.Contains("npm")
                                     || tempFieldName.Contains("opm")
                                    )
                            {
                                var lastTenYearTemp = new LastTenYearSales
                                {
                                    Metric = (tempFieldName)
                                };
                                var tempYears = new List<YearValue>();
                                for (int i = 0; i < headers.Count; i++)
                                {
                                    var tempV = profitandlossSheet.Cell(row, i + 2).GetString();
                                    if (tempV.Contains("%"))
                                    {
                                        tempV = tempV.Replace("%", "");
                                    }

                                    var year = Convert.ToDateTime(headers[i]).Year;

                                    if (year >= 2015 && year <= 2024)
                                    {
                                        tempYears.Add(new YearValue
                                        {
                                            Year = Convert.ToDateTime(headers[i]),
                                            Value = (tempV)
                                        });
                                    }
                                }

                                lastTenYearTemp.Values = tempYears;
                                lastTenYearSales.Add(lastTenYearTemp);
                            }
                        }
                    }
                    finalOutPut.LastTenYearSales = lastTenYearSales;
                }

                var promotersSheet = package.Worksheets.FirstOrDefault(item => item.Name.ToLower() == "promoters");

                if (promotersSheet != null)
                {
                    finalOutPut.PromotersHoldingInPercent = new();
                    var lastFewYearsPromotersHolding = new LastTenYearSales();

                    var headers = new List<string>();

                    for (int row = 2; row <= 12; row++)
                    {
                        if (row == 2)
                        {
                            for (int col = 2; col <= 13; col++)
                            {
                                var tempData = promotersSheet.Cell(row, col).GetString();
                                if (!string.IsNullOrEmpty(tempData))
                                {
                                    var dateData =
                                        DateTime.Parse(tempData); //DateTime.FromOADate(double.Parse(cell.GetString()));
                                    headers.Add(dateData.ToShortDateString());
                                }
                            }
                        }
                        else
                        {
                            var tempFieldName = promotersSheet.Cell(row, 1).GetString();
                            if (!string.IsNullOrEmpty(tempFieldName))
                            {
                                tempFieldName = tempFieldName.ToLower().Trim();
                                tempFieldName = new string(tempFieldName.Where(c => char.IsLetterOrDigit(c))
                                    .ToArray());
                            }

                            if (string.IsNullOrEmpty(promotersSheet.Cell(1, 1).GetString()) &&
                                string.IsNullOrEmpty(promotersSheet.Cell(2, 2).GetString()))
                            {
                                break;
                            }
                            else if (tempFieldName != null && tempFieldName.Contains("promoters"))
                            {
                                var tempYears = new List<YearValue>();

                                // Loop through the years from 2015 to 2024
                                for (int year = 2015; year < 2025; year++)
                                {
                                    // Check if the current year is present in the headers
                                    var headerIndex =
                                        headers.FindIndex(h => Convert.ToDateTime(h).Year == year);

                                    if (headerIndex != -1)
                                    {
                                        // If the year exists in the headers, use the exact date from the header
                                        var actualDate =
                                            Convert.ToDateTime(
                                                headers[headerIndex]); // The full date from the header

                                        tempYears.Add(new YearValue
                                        {
                                            Year =
                                                actualDate, // Use the actual date (including the month from the header)
                                            Value = promotersSheet.Cell(row, headerIndex + 2)
                                                .GetString() // Get the value from the worksheet
                                        });
                                    }
                                    else
                                    {
                                        // Year missing in headers, add January 1st as the default date for the missing year

                                        if (year < 2024 && year > 2014)
                                        {
                                            tempYears.Add(new YearValue
                                            {
                                                Year = new DateTime(year, 1, 1), // Default to January 1st for missing years
                                                Value = "" // Empty value for missing years
                                            });
                                        }
                                    }
                                }

                                lastFewYearsPromotersHolding.Metric = tempFieldName;
                                lastFewYearsPromotersHolding.Values = tempYears;
                            }
                        }

                        finalOutPut.PromotersHoldingInPercent.Add(lastFewYearsPromotersHolding);
                    }
                }

                #endregion

                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Data = finalOutPut;
                responseModel.Exceptions = _exceptions;
                return responseModel;
            }
            catch (Exception ex)
            {
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                return responseModel;
            }
        }

        private class CompanyDetailsVm
        {
            public int CompanyId { get; set; }
            public string Name { get; set; }
            public string Symbol { get; set; }
            public string Description { get; set; }
            public string ShortSummary { get; set; }
            public decimal? MarketCap { get; set; }
            public decimal? PE { get; set; }
            public string ChartUrl { get; set; }
            public string OtherUrl { get; set; }
            public string WebsiteUrl { get; set; }
            public List<LastOneYearMonthlyPriceM> LastOneYearMonthlyPrices { get; set; }
            public List<LastTenYearSalesM> LastTenYearSales { get; set; }
            public CompanyTypeM CompanyType { get; set; }
            public bool IsPublished { get; set; }
            public decimal? CurrentPrice { get; set; }
            public decimal? YesterdayPrice { get; set; }
            public decimal? SharesInCrores { get; set; }
            public decimal? MarketCapInCrores { get; set; }
            public decimal? TTMNetProfitInCrores { get; set; }
            public decimal? PERatio { get; set; }
            public decimal? FaceValue { get; set; }
            public decimal? NetWorth { get; set; }
            public decimal? PromotersHolding { get; set; }
            public decimal? ProfitGrowth { get; set; }
            public bool IsFree { get; internal set; }
            public DateTime? PublishDate { get; set; }
            public List<CompanyCommentVm> Comments { get; set; }
            public int TotalCommentCount { get; set; }
        }

        private class CompanyCommentVm
        {
            public int Id { get; set; }
            public int CompanyDetailId { get; set; }
            public string Message { get; set; }
            public DateTime ModifiedOn { get; set; }
            public Guid ModifiedBy { get; set; }
            public Guid CreatedByPublicKey { get; set; }
            public string CreatedByName { get; set; }
        }

        private string companyName = "";
        private string CurrentPrice = "";
        private string NoOfShare = "";
        private string MarketCap = "";
        private string TTMNetProfit = "";
        private string PE = "";
        private string TTMOperatingProfitMargin = "";
        private string roae = "";
        private string roace = "";

        private async Task ReadCompanyDetails(IXLRange rangeUsed, ManageArticleModel data, List<string> _exceptions)
        {
            var rows = rangeUsed.RowsUsed();
            foreach (var row in rows.Skip(1))
            {
                try
                {
                    if (row.Cell(1).GetString() == "Company Name")
                    {
                        data.Name = row.Cell(2).GetString();
                    }

                    if (row.Cell(1).GetString() == "Symbol")
                    {
                        data.Symbol = row.Cell(2).GetString();
                    }

                    if (row.Cell(1).GetString() == "Current Price (INR)")
                    {
                        data.CurrentPrice = decimal.Parse(row.Cell(2).GetString());
                    }

                    if (row.Cell(1).GetString() == "No. of Shares (crores)")
                    {
                        NoOfShare = row.Cell(2).GetString();
                    }

                    if (row.Cell(1).GetString() == "Market Cap (crores)")
                    {
                        data.MarketCap = decimal.Parse(row.Cell(2).GetString());
                    }

                    if (row.Cell(1).GetString() == "TTM Net Profit (crores)")
                    {
                        TTMNetProfit = row.Cell(2).GetString();
                    }

                    if (row.Cell(1).GetString() == "TTM P/E ratio")
                    {
                        data.PE = decimal.Parse(row.Cell(2).GetString());
                    }

                    if (row.Cell(1).GetString() == "TTM Operating Profit Margin")
                    {
                        TTMOperatingProfitMargin = row.Cell(2).GetString();
                    }

                    if (row.Cell(1).GetString() == "Latest FY ROAE")
                    {
                        roae = row.Cell(2).GetString();
                    }

                    if (row.Cell(1).GetString() == "Latest FY ROACE")
                    {
                        roace = row.Cell(2).GetString();
                    }

                    if (row.Cell(1).GetString() == "Face Value")
                    {
                        data.FaceValue = decimal.Parse(row.Cell(2).GetString());
                    }

                    if (row.Cell(1).GetString() == "RSI")
                    {
                        data.RSI = row.Cell(2).GetString();
                    }

                    if (row.Cell(1).GetString() == "MACD")
                    {
                        data.MACD = row.Cell(2).GetString();
                    }

                    if (row.Cell(1).GetString() == "NetWorth")
                    {
                        data.NetWorth = decimal.Parse(row.Cell(2).GetString());
                    }

                    if (row.Cell(1).GetString() == "Promoters Holding")
                    {
                        data.PromotersHolding = decimal.Parse(row.Cell(2).GetString());
                    }

                    if (row.Cell(1).GetString() == "Profit Growth")
                    {
                        data.ProfitGrowth = decimal.Parse(row.Cell(2).GetString());
                    }
                }
                catch (Exception ex)
                {
                    _exceptions.Add("WhileReading ReadCompanyDetails: " + ex.Message);

                    await _exceptionLog.AddAsync(new ExceptionLog
                    {
                        CreatedOn = DateTime.Now,
                        InnerException = ex.InnerException?.ToString(),
                        Message = ex.Message,
                        RequestBody = "",
                        Source = "ReadCompanyDetails",
                        StackTrace = ex.StackTrace
                    });

                }
            }
        }

        private List<LastTenYearSalesM> ConvertToLastTenYearSalesM(List<LastTenYearSales> lastTenYearSales)
        {
            var result = new List<LastTenYearSalesM>();

            // Assuming each metric has values for all years
            // Get a unique set of years from the first metric's values
            var years = lastTenYearSales.FirstOrDefault()?.Values.Select(v => v.Year).Distinct();

            if (years == null)
            {
                return result;
            }

            // Loop through each year and create the LastTenYearSalesM object
            foreach (var year in years)
            {
                var salesM = new LastTenYearSalesM
                {
                    Year = year,
                    Sales = GetMetricValue(lastTenYearSales, year.ToString(), "sales"),
                    OpProfit = GetMetricValue(lastTenYearSales, year.ToString(), "opProfit"),
                    NetProfit = GetMetricValue(lastTenYearSales, year.ToString(), "netProfit"),
                    OTM = GetMetricValue(lastTenYearSales, year.ToString(), "otm"),
                    NPM = GetMetricValue(lastTenYearSales, year.ToString(), "npm"),
                    PromotersPercent = GetMetricValue(lastTenYearSales, year.ToString(), "promotersPercent")
                };

                result.Add(salesM);
            }

            return result;
        }

        private string GetMetricValue(List<LastTenYearSales> lastTenYearSales, string year, string metric)
        {
            var metricData =
                lastTenYearSales.FirstOrDefault(m => m.Metric.Equals(metric, StringComparison.OrdinalIgnoreCase));

            if (metricData != null)
            {
                var valueData = metricData.Values.FirstOrDefault(v => v.Year.Equals(DateTime.Parse(year)));

                if (valueData != null)
                {
                    return valueData.Value;
                }
            }

            return string.Empty; // Default value if not found
        }

        //public async Task<ApiCommonResponseModel> ManageCoupon(ManageCouponModel request)
        //{
        //    try
        //    {
        //        var sqlParameters = new SqlParameter[]
        //        {
        //            new() { ParameterName = "couponName", Value =  request.CouponName ,SqlDbType = SqlDbType.VarChar,Size = 100},
        //            new() { ParameterName = "couponKey", Value =   request.CouponKey == null || request.CouponKey == Guid.Empty ? DBNull.Value : request.CouponKey, SqlDbType = SqlDbType.UniqueIdentifier},
        //            new() { ParameterName = "description", Value = string.IsNullOrEmpty(request.Description) ? DBNull.Value : request.Description.Trim(),SqlDbType = SqlDbType.NVarChar,Size = 500},
        //            new() { ParameterName = "discountInPercentage", Value = request.DiscountInPercentage == null? DBNull.Value : request.DiscountInPercentage,SqlDbType = SqlDbType.VarChar,Size = 100},
        //            new() { ParameterName = "discountInPrice", Value = request.DiscountInPrice == null ? DBNull.Value : request.DiscountInPrice,SqlDbType = SqlDbType.VarChar,Size = 100},
        //            new() { ParameterName = "redeemLimit", Value = request.RedeemLimit == null ? DBNull.Value : request.RedeemLimit,SqlDbType = SqlDbType.VarChar,Size = 100},
        //            new() { ParameterName = "productValidityInDays", Value = request.ProductValidityInDays == null ? DBNull.Value :request.ProductValidityInDays,SqlDbType = SqlDbType.VarChar,Size = 100},
        //            new() { ParameterName = "createdBy", Value = request.CreatedBy  , SqlDbType = SqlDbType.UniqueIdentifier },
        //            new() { ParameterName = "productIds", Value = string.IsNullOrEmpty(request.ProductIds) ? DBNull.Value :request.ProductIds.Trim(), SqlDbType = SqlDbType.VarChar,Size = 100},
        //            new() { ParameterName = "mobileUserKeys", Value = string.IsNullOrEmpty(request.MobileUserKeys) ? DBNull.Value : request.MobileUserKeys.Trim(),SqlDbType = SqlDbType.VarChar,Size = 100},
        //        };

        //        responseModel.Data = await _dbContext.SqlQueryToListAsync<object>(ProcedureCommonSqlParametersText.ManageCouponM, sqlParameters);
        //        responseModel.StatusCode = HttpStatusCode.OK;
        //        return responseModel;
        //    }
        //    catch (Exception ex)
        //    {
        //        responseModel.StatusCode = HttpStatusCode.InternalServerError;
        //        responseModel.Message = ex.Message;
        //        return responseModel;
        //    }
        //}

        public async Task<ApiCommonResponseModel> Coupon(QueryValues query)
        {
            var responseModel = new ApiCommonResponseModel();

            SqlParameter parameterOutValue = new()
            {
                ParameterName = "@TotalCount",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output,
            };

            SqlParameter[] sqlParameters = new[]
            {
                    new SqlParameter
                    {
                        ParameterName = "@Status",
                        Value = query.PrimaryKey == "" ? 0 : Convert.ToInt32(query.PrimaryKey),
                        SqlDbType = System.Data.SqlDbType.Int,
                    },
                    new SqlParameter
                    {
                        ParameterName = "@PageSize",
                        Value = query.PageSize,
                        SqlDbType = System.Data.SqlDbType.Int,
                    },
                    new SqlParameter
                    {
                        ParameterName = "@PageNumber",
                        Value = query.PageNumber,
                        SqlDbType = System.Data.SqlDbType.Int,
                    },
                    new SqlParameter
                    {
                        ParameterName = "@FromDate",
                        Value = query.FromDate ?? Convert.DBNull,
                        SqlDbType = System.Data.SqlDbType.Date,
                    },
                    new SqlParameter
                    {
                        ParameterName = "@ToDate",
                        Value = query.ToDate ?? Convert.DBNull,
                        SqlDbType = System.Data.SqlDbType.Date,
                    },
                    new SqlParameter
                    {
                        ParameterName = "@SearchText",
                        Value = string.IsNullOrEmpty(query.SearchText) ? Convert.DBNull : query.SearchText,
                        SqlDbType = System.Data.SqlDbType.NVarChar,
                    },
                    parameterOutValue
            };

            var coupons = await _dbContext.SqlQueryToListAsync<CouponResponseModel>(
                "EXEC GetCoupons @PageSize, @PageNumber, @FromDate, @ToDate, @Status, @SearchText, @TotalCount OUTPUT",
                sqlParameters);

            responseModel.Data = coupons;
            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Total = Convert.ToInt32(parameterOutValue.Value);
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> ManageCoupon(ManageCouponModel request)
        {
            var sqlParameters = new SqlParameter[]
            {
                new()
                {
                    ParameterName = "couponName", Value = request.CouponName, SqlDbType = SqlDbType.VarChar, Size = 100
                },
                new()
                {
                    ParameterName = "couponKey",
                    Value = request.CouponKey == null || request.CouponKey == Guid.Empty
                        ? DBNull.Value
                        : request.CouponKey,
                    SqlDbType = SqlDbType.UniqueIdentifier
                },
                new()
                {
                    ParameterName = "description",
                    Value = string.IsNullOrEmpty(request.Description) ? DBNull.Value : request.Description.Trim(),
                    SqlDbType = SqlDbType.NVarChar, Size = 500
                },
                new()
                {
                    ParameterName = "discountInPercentage",
                    Value = request.DiscountInPercentage == null ? DBNull.Value : request.DiscountInPercentage,
                    SqlDbType = SqlDbType.VarChar, Size = 100
                },
                new()
                {
                    ParameterName = "discountInPrice",
                    Value = request.DiscountInPrice == null ? DBNull.Value : request.DiscountInPrice,
                    SqlDbType = SqlDbType.VarChar, Size = 100
                },
                new()
                {
                    ParameterName = "redeemLimit",
                    Value = request.RedeemLimit == null ? DBNull.Value : request.RedeemLimit,
                    SqlDbType = SqlDbType.VarChar, Size = 100
                },
                new()
                {
                    ParameterName = "productValidityInDays",
                    Value = request.ProductValidityInDays == null ? DBNull.Value : request.ProductValidityInDays,
                    SqlDbType = SqlDbType.VarChar, Size = 100
                },
                new()
                {
                    ParameterName = "createdBy", Value = request.CreatedBy, SqlDbType = SqlDbType.UniqueIdentifier
                },
                new()
                {
                    ParameterName = "productIds",
                    Value = string.IsNullOrEmpty(request.ProductIds) ? DBNull.Value : request.ProductIds.Trim(),
                    SqlDbType = SqlDbType.VarChar, Size = 100
                },
                new()
                {
                    ParameterName = "mobileNumbers",
                    Value = string.IsNullOrEmpty(request.MobileNumbers) ? DBNull.Value : request.MobileNumbers.Trim(),
                    SqlDbType = SqlDbType.VarChar, Size = -1
                },
                new()
                {
                    ParameterName = "action", Value = string.IsNullOrEmpty(request.Type) ? DBNull.Value : request.Type,
                    SqlDbType = SqlDbType.VarChar, Size = 50
                },
                new() { ParameterName = "override", Value = request.Override, SqlDbType = SqlDbType.Bit },
            };

            var result =
                await _dbContext.SqlQueryFirstOrDefaultAsync2<ManageCouponMSpResponseModel>(
                    ProcedureCommonSqlParametersText.ManageCouponM, sqlParameters);

            if (result is not null)
            {
                if (result.Result.ToUpper() == "CANNOTEDITNAME")
                {
                    responseModel.StatusCode = HttpStatusCode.Forbidden;
                    responseModel.Message = "Cannot edit name as the coupon is in use.";
                }

                if (result.Result.ToUpper() == "DUPLICATENAME")
                {
                    responseModel.StatusCode = HttpStatusCode.Forbidden;
                    responseModel.Message = "Coupon with same name already exists.";
                }

                if (result.Result.ToUpper() == "UPDATED")
                {
                    responseModel.StatusCode = HttpStatusCode.OK;
                    responseModel.Message = "Update successfull.";
                }

                if (result.Result.ToUpper() == "CREATESUCCESSFULL")
                {
                    responseModel.StatusCode = HttpStatusCode.OK;
                    responseModel.Message = "Regenerate successfull.";
                }
            }

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> DeleteCoupon(int couponId, Guid userKey)
        {
            var coupon = await _dbContext.CouponsM
                .FirstOrDefaultAsync(x => x.Id == couponId && !x.IsDelete);

            if (coupon == null)
            {
                responseModel.StatusCode = HttpStatusCode.NotFound;
                responseModel.Message = "Coupon not found";
                return responseModel;
            }
            var couponName = coupon.Name;
            coupon.IsDelete = true;
            coupon.IsActive = false;
            coupon.ModifiedBy = userKey;
            coupon.ModifiedOn = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = $"Coupon '{couponName}' deleted successfully";
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> Ticket(GetTicketReqeustModel request)
        {
            try
            {
                SqlParameter TotalCount = new()
                {
                    ParameterName = "@TotalCount",
                    Direction = System.Data.ParameterDirection.Output,
                    SqlDbType = System.Data.SqlDbType.Int,
                };
                SqlParameter TypeCount = new()
                {
                    ParameterName = "@TypeCount",
                    Direction = System.Data.ParameterDirection.Output,
                    SqlDbType = System.Data.SqlDbType.Int,
                };
                SqlParameter StatusCount = new()
                {
                    ParameterName = "@StatusCount",
                    Direction = System.Data.ParameterDirection.Output,
                    SqlDbType = System.Data.SqlDbType.Int,
                };
                SqlParameter PriorityCount = new()
                {
                    ParameterName = "@PriorityCount",
                    Direction = System.Data.ParameterDirection.Output,
                    SqlDbType = System.Data.SqlDbType.Int,
                };

                var sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@status", string.IsNullOrEmpty(request.Status) ? DBNull.Value : request.Status)
                        { SqlDbType = SqlDbType.VarChar, Size = 10 },
                    new SqlParameter("@ticketType",
                            string.IsNullOrEmpty(request.TicketType) ? DBNull.Value : request.TicketType)
                        { SqlDbType = SqlDbType.VarChar, Size = 100 },
                    new SqlParameter("@priority",
                            string.IsNullOrEmpty(request.Priority) ? DBNull.Value : request.Priority)
                        { SqlDbType = SqlDbType.VarChar, Size = 100 },
                    new SqlParameter("@startDate",
                            request.StartDate.HasValue ? request.StartDate.Value.Date : DBNull.Value)
                        { SqlDbType = SqlDbType.Date },
                    new SqlParameter("@endDate", request.EndDate.HasValue ? request.EndDate.Value.Date : DBNull.Value)
                        { SqlDbType = SqlDbType.Date },
                    new SqlParameter("@PageNumber", request.PageNumber) { SqlDbType = SqlDbType.Int },
                    new SqlParameter("@SearchText",string.IsNullOrEmpty(request.SearchText) ? Convert.DBNull : request.SearchText){ SqlDbType = SqlDbType.NVarChar},
                    TotalCount,
                    TypeCount,
                    StatusCount,
                    PriorityCount
                };

                var result = await _dbContext.SqlQueryToListAsync<GetTicketMResponseModel>(
                    ProcedureCommonSqlParametersText.GetTicketsM,
                    sqlParameters
                );

                responseModel.Data = new
                {
                    ticket = result,
                    totalCount = TotalCount.Value,
                    typeCount = TypeCount.Value,
                    statusCount = StatusCount.Value,
                    priorityCount = PriorityCount.Value
                };
                responseModel.StatusCode = HttpStatusCode.OK;
                return responseModel;
            }
            catch (Exception ex)
            {
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                responseModel.Message = ex.Message;
                return responseModel;
            }
        }

        public async Task<ApiCommonResponseModel> ManageTicket(ManageTicketsRequestModel request)
        {
            if (request.Action.Equals("EDIT", StringComparison.InvariantCultureIgnoreCase))
            {
                var ticket = await _dbContext.TicketM
                    .Where(x => x.Id == request.Id)
                    .FirstOrDefaultAsync();

                if (ticket is not null)
                {
                    ticket.Status = request.Status;
                    ticket.Comment = request.Comment;
                    ticket.ModifiedBy = request.ModifiedBy;
                    ticket.ModifiedOn = DateTime.Now;
                    if (!string.IsNullOrWhiteSpace(request.Comment))
                    {
                        var adminId = await _dbContext.Users
                            .Where(x => x.PublicKey == request.ModifiedBy)
                            .Select(x => x.Id)
                            .FirstOrDefaultAsync();

                        TicketCommentsM comment = new()
                        {
                            Comment = request.Comment,
                            TicketId = ticket.Id,
                            CommentByCrmId = adminId,
                            IsDelete = false,
                            CreatedOn = DateTime.Now
                        };
                        await _dbContext.TicketCommentsM.AddAsync(comment);
                    }
                    var user = await _dbContext.MobileUsers.Where(x => x.PublicKey == ticket.CreatedBy).FirstOrDefaultAsync();

                    await _dbContext.SaveChangesAsync();
                    await _pushNotification.SendNotificationToMobile(new NotificationToMobileRequestModel
                    {
                        Body = "Our team has responded to your ticket. Check now for details.",
                        Mobile = user.Mobile,
                        Title = "Ticket Update!",
                        Topic = "ANNOUNCEMENT",
                        ScreenName = "ticketsScreen"

                    });

                    responseModel.StatusCode = HttpStatusCode.OK;
                    responseModel.Message = "Update successful.";
                }
                else
                {
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                    responseModel.Message = "Ticket not found.";
                    return responseModel;
                }
            }
            else if (request.Action.Equals("DELETE", StringComparison.InvariantCultureIgnoreCase))
            {
                var ticket = await _dbContext.TicketM
                    .Where(x => x.Id == request.Id)
                    .FirstOrDefaultAsync();

                if (ticket is not null)
                {
                    ticket.IsActive = false;
                    ticket.IsDelete = true;
                    await _dbContext.SaveChangesAsync();
                    responseModel.StatusCode = HttpStatusCode.OK;
                    responseModel.Message = "Ticket Deleted.";
                }
                else
                {
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                    responseModel.Message = "Ticket Not Found.";
                }
            }

            return responseModel;
        }






        public async Task<ApiCommonResponseModel> GetPartnerNamesAsync()
        {
            var responseModel = new ApiCommonResponseModel();

            var partnerDetailsList = await _dbContext.PartnerNamesM
                .Where(item => item.IsActive && !item.IsDelete)
                .Select(item => new
                {
                    item.Name,
                    item.ReferralLink
                })
                .ToListAsync();

            responseModel.Data = partnerDetailsList;
            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Data Fetched Successfully.";

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetPartnerDematAccounts(QueryValues query)
        {
            var apiCommonResponse = new ApiCommonResponseModel();

            // Define the output parameter
            SqlParameter parameterOutValue = new()
            {
                ParameterName = "@TotalCount",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output,
            };

            // Define input parameters
            SqlParameter[] sqlParameters = new[]
            {
                    new SqlParameter
                    {
                        ParameterName = "@Status",
                        Value = string.IsNullOrEmpty(query.PrimaryKey) ? 0 : Convert.ToInt32(query.PrimaryKey),
                        SqlDbType = SqlDbType.Int,
                    },
                    new SqlParameter
                    {
                        ParameterName = "@PageSize",
                        Value = query.PageSize,
                        SqlDbType = SqlDbType.Int,
                    },
                    new SqlParameter
                    {
                        ParameterName = "@PageNumber",
                        Value = query.PageNumber,
                        SqlDbType = SqlDbType.Int,
                    },
                    new SqlParameter
                    {
                        ParameterName = "@FromDate",
                        Value = query.FromDate ?? Convert.DBNull,
                        SqlDbType = SqlDbType.Date,
                    },
                    new SqlParameter
                    {
                        ParameterName = "@ToDate",
                        Value = query.ToDate ?? Convert.DBNull,
                        SqlDbType = SqlDbType.Date,
                    },
                    new SqlParameter
                    {
                        ParameterName = "@SearchText",
                        Value = string.IsNullOrEmpty(query.SearchText) ? Convert.DBNull : query.SearchText,
                        SqlDbType = SqlDbType.NVarChar,
                    },
                       parameterOutValue
                };

            // Execute the query and get the result
            List<PartnerDematAccountResponse> partnerDematAccounts =
                await _dbContext.SqlQueryToListAsync<PartnerDematAccountResponse>(
                    "EXEC GetPartnerDematAccounts @PageSize, @PageNumber, @FromDate, @ToDate, @Status, @SearchText, @TotalCount OUTPUT",
                    sqlParameters
                );

            // Retrieve total count from output parameter
            apiCommonResponse.Total = Convert.ToInt32(parameterOutValue.Value);

            // Set the response data and status
            apiCommonResponse.Data = partnerDematAccounts;
            apiCommonResponse.StatusCode = HttpStatusCode.OK;

            return apiCommonResponse;
        }


        public async Task<ApiCommonResponseModel> ManagePartnerDematAccount(PartnerDematAccountRequest accountRequest, Guid logggedUser)
        {
            try
            {
                PartnerAccountsM? existingRecord =
                    await _dbContext.PartnerAccountsM.FirstOrDefaultAsync(p => p.Id == accountRequest.Id);

                if (existingRecord == null)
                {
                    return new ApiCommonResponseModel
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = "Partner account not found.",
                        Data = null
                    };
                }

                existingRecord.PartnerName = accountRequest.PartnerName;
                existingRecord.API = accountRequest.API;
                existingRecord.PartnerId = accountRequest.PartnerId;
                existingRecord.SecretKey = accountRequest.SecretKey;
                existingRecord.ModifiedOn = DateTime.Now;
                existingRecord.ModifiedBy = logggedUser;

                await _dbContext.SaveChangesAsync();

                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Partner account updated successfully.",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = ex.Message,
                    Data = null
                };
            }
        }

        public async Task<ApiCommonResponseModel> GetFilteredPurchaseOrders(QueryValues queryValues)
        {
            responseModel.Message = "Successfully";
            responseModel.StatusCode = HttpStatusCode.OK;

            List<SqlParameter> sqlParameters2 = ProcedureCommonSqlParameters.GetCommonSqlParameters(queryValues);

            SqlParameter parameterOutValue = new()
            {
                ParameterName = "TotalCount",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output,
            };

            SqlParameter parameterOutValue2 = new()
            {
                ParameterName = "TotalSales",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output,
            };

            sqlParameters2.AddRange(new SqlParameter[]
            {
                //service value
                new SqlParameter
                {
                    ParameterName = "PrimaryKey",
                    Value = string.IsNullOrEmpty(queryValues.PrimaryKey) ? DBNull.Value : queryValues.PrimaryKey,
                    SqlDbType = System.Data.SqlDbType.NVarChar
                },
                parameterOutValue,
                parameterOutValue2
            });

            List<PurchaseOrderResponseModel> result = await _dbContext.SqlQueryToListAsync<PurchaseOrderResponseModel>(
                "exec GetPurchaseOrdersM @IsPaging , @PageSize, @PageNumber , @SortExpression , @SortOrder , @RequestedBy,  @FromDate,@ToDate,@SearchText, @PrimaryKey, @TotalCount  OUTPUT, @TotalSales OUTPUT ",
                sqlParameters2.ToArray());

            decimal totalRecords = Convert.ToDecimal(parameterOutValue?.Value);
            decimal totalSales = 0;
            if (totalRecords > 0)
            {
                totalSales = Convert.ToDecimal(parameterOutValue2?.Value);
            }

            //// Use shared or configured invoice folder path
            //// Shared folder location for invoices
            //string invoiceDirectory = @"C:\SharedFiles\Invoices";

            //var invoiceFiles = Directory.Exists(invoiceDirectory)
            //    ? Directory.GetFiles(invoiceDirectory, "Invoice_*.pdf")
            //    : Array.Empty<string>();

            //// ✅ Base URL for serving invoices
            //var request = _httpContextAccessor.HttpContext?.Request;
            //string baseUrl = request != null ? $"{request.Scheme}://{request.Host}" : "https://localhost:7001";

            //foreach (var order in result)
            //{
            //    if (!string.IsNullOrEmpty(order.Mobile))
            //    {
            //        // Find matching invoice file that contains the mobile number
            //        string matchedFile = invoiceFiles
            //            .FirstOrDefault(file =>
            //                Path.GetFileName(file).Contains(order.Mobile, StringComparison.OrdinalIgnoreCase));

            //        if (!string.IsNullOrEmpty(matchedFile))
            //        {
            //            string fileNameOnly = Path.GetFileName(matchedFile);
            //            order.InvoiceFileName = fileNameOnly;
            //            order.InvoiceUrl = $"{baseUrl}/api/Invoice/DownloadInvoice?fileName={fileNameOnly}";
            //        }
            //    }
            //}

            dynamic runTimeObject = new ExpandoObject();
            runTimeObject.data = result;
            runTimeObject.totalRecords = totalRecords;
            runTimeObject.totalSales = totalSales;

            responseModel.Data = runTimeObject;

            return responseModel;
        }




        public async Task<ApiCommonResponseModel> SendWhatsappFromExcel()
        {
            var responseModel = new ApiCommonResponseModel();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Files", "CodelineTech.xlsx");
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets[0];

            var mobileNumbers = new List<string>();
            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                var cellValue = worksheet.Cells[row, 3].Text;
                if (!string.IsNullOrEmpty(cellValue))
                {
                    mobileNumbers.Add(cellValue.Trim());
                }
            }

            var options = new RestClientOptions(_configuration["Otp:Url"]!);
            var client = new RestClient(options);

            foreach (var mobileNumber in mobileNumbers)
            {
                var mobileUser = await _dbContext.MobileUsers
                    .Where(x => x.Mobile == mobileNumber)
                    .FirstOrDefaultAsync();

                if (mobileUser == null) continue;

                var templateValue = $"{{\"id\":\"98b1e6e9-e6e6-4eb4-97e6-3360b26d759e\",\"params\":[\"{mobileUser.FullName}\",\"{_configuration["Otp:TelegramLink"]}\"]}}";
                string messageJson = $"{{\"image\":{{\"id\":\"491337030653441\"}},\"type\":\"image\"}}";

                var request = new RestRequest("", Method.Post);
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddHeader("accept", "application/json");
                request.AddHeader("apikey", _configuration["Otp:ApiKey"]!);
                request.AddParameter("source", _configuration["Otp:GupShupMobile"]);
                request.AddParameter("src.name", _configuration["Otp:Appname"]);
                request.AddParameter("message", messageJson);
                request.AddOrUpdateParameter("template", templateValue);
                request.AddOrUpdateParameter("destination", $"91{mobileNumber}");

                var response = await client.PostAsync(request);

                if (response.IsSuccessful)
                {
                    //FileHelper.WriteToFile("WhatsappStatus", response.Content);
                    //await _log.AddAsync(new Log
                    //{
                    //    CreatedOn = DateTime.Now,
                    //    Message = $"WhatsappStatus sent successfully to {mobileNumber}. Response: {System.Text.Json.JsonSerializer.Serialize(response)}",
                    //    Source = "SendWhatsappStatus_Success",
                    //    Category = "Whatsapp"
                    //});
                }
                else
                {
                    await _log.AddAsync(new Log
                    {
                        CreatedOn = DateTime.Now,
                        Message = $"WhatsappStatus sent successfully to {mobileNumber}. Response: {System.Text.Json.JsonSerializer.Serialize(response)}",
                        Source = "SendWhatsappFromExcel_Failed",
                        Category = "Whatsapp"
                    });
                    //FileHelper.WriteToFile("WhatsappStatus", response.Content);
                }
            }

            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Messages sent successfully.";
            return responseModel;
        }



        public async Task<ApiCommonResponseModel> SendWhatsappTemplateMessage(SendWhatsappMessageRequestModel param)
        {
            List<string> mobileNumbers = param.MobileNumbers ?? new();
            List<string> serviceKeys = param.ServiceKeys ?? new();

            DateTime fromDate = DateTime.ParseExact(param.FromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var templateBodies = new List<CommonTemplateMessage>();

            List<WhatsappTemplateMessage> sp1Users = new();
            List<WhatsappLeadsTemplateMessage> sp2Users = new();

            //if(param.Category == "")
            //{
            //    responseModel.Message = "Please Select Category..";
            //    responseModel.StatusCode = HttpStatusCode.BadRequest;
            //    return responseModel;
            //}
            // 🔹 Run SP1 if Category is "Mobile" or "All"
            if (!string.IsNullOrWhiteSpace(param.TargetType))
            {
                var sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TemplateId", SqlDbType.VarChar, 50) { Value = param.TemplateId },
                    new SqlParameter("@TargetType", SqlDbType.VarChar) { Value = param.TargetType ?? (object)DBNull.Value },
                    new SqlParameter("@TemplateName", SqlDbType.VarChar, 50) { Value = param.TemplateName },
                    new SqlParameter("@MobileNumbers", SqlDbType.VarChar, 50) { Value = mobileNumbers.Count == 0 ? (object)DBNull.Value : string.Join(",", mobileNumbers)},
                };

                //foreach (var p in sqlParameters)
                //{
                //    Console.WriteLine($"{p.ParameterName} = {p.Value}");
                //}

                _dbContext.Database.SetCommandTimeout(120);

                sp1Users = await _dbContext.SqlQueryToListAsync<WhatsappTemplateMessage>(
                    "EXEC SendWhatsappTemplateMessages @TemplateId, @TargetType, @TemplateName, @MobileNumbers",
                    sqlParameters);
            }

            // 🔹 Run SP2 if Category is "Source" or "All"
            if (!string.IsNullOrWhiteSpace(param.Source))
            {
                // Validate required parameters
                if (string.IsNullOrWhiteSpace(param.TemplateId) || string.IsNullOrWhiteSpace(param.TemplateName))
                {
                    return new ApiCommonResponseModel{
                        Message = "TemplateId and TemplateName are required parameters",
                        StatusCode = HttpStatusCode.BadRequest
                    };
                }

                var sourceParameters = new SqlParameter[]
                {
                    new SqlParameter("@Source", SqlDbType.VarChar){Value = string.IsNullOrWhiteSpace(param.Source) ? (object)DBNull.Value : param.Source},
                    new SqlParameter("@ThirdKey", SqlDbType.VarChar){Value = string.IsNullOrWhiteSpace(param.LeadTypeKey) ? (object)DBNull.Value : param.LeadTypeKey},
                    new SqlParameter("@FourthKey", SqlDbType.VarChar){Value = string.IsNullOrWhiteSpace(param.LeadSourceKey) ? (object)DBNull.Value : param.LeadSourceKey},
                    new SqlParameter("@StatusType", SqlDbType.Int){ Value = param.StatusType ?? (object)DBNull.Value},
                    new SqlParameter("@FromDate", SqlDbType.Date){ Value = DateTime.TryParse(param.FromDate, out var fd) ? fd : (object)DBNull.Value},
                    new SqlParameter("@ToDate", SqlDbType.Date){ Value = DateTime.TryParse(param.ToDate, out var td) ? td : (object)DBNull.Value},
                    new SqlParameter
{
    ParameterName = "@PartnerWith",
    Value = string.IsNullOrEmpty(param.PartnerWith) ? DBNull.Value : param.PartnerWith,
    SqlDbType = System.Data.SqlDbType.VarChar
}

                ,
                    //new SqlParameter("@PartnerWith", SqlDbType.VarChar){Value = string.IsNullOrWhiteSpace(param.PartnerWith) ? (object)DBNull.Value : param.PartnerWith},
                    new SqlParameter("@SecondaryKey", SqlDbType.VarChar)
                    {
                        Value = serviceKeys != null && serviceKeys.Any(s => !string.IsNullOrWhiteSpace(s))
                        ? string.Join(",", serviceKeys.Where(s => !string.IsNullOrWhiteSpace(s))): (object)DBNull.Value
                    },
                    new SqlParameter("@Filter", SqlDbType.VarChar){Value = string.IsNullOrWhiteSpace(param.filter) ? (object)DBNull.Value : param.filter},
                    new SqlParameter("@FifthKey", SqlDbType.VarChar){Value = string.IsNullOrWhiteSpace(param.PoStatus) ? (object)DBNull.Value : param.PoStatus},
                    new SqlParameter("@TemplateId", SqlDbType.VarChar, 50){ Value = param.TemplateId},
                    new SqlParameter("@TemplateName", SqlDbType.VarChar){Value = param.TemplateName}
                };

                _dbContext.Database.SetCommandTimeout(120);

                try
                {
                    sp2Users = await _dbContext.SqlQueryToListAsync<WhatsappLeadsTemplateMessage>
                    (
                        "EXEC GetMobileNumbersBySourceType @Source, @ThirdKey, @FourthKey, @StatusType, @FromDate, @ToDate, @PartnerWith, @SecondaryKey, @Filter, @FifthKey, @TemplateId, @TemplateName",
                        sourceParameters
                    );
                }
                catch (Exception ex)
                {
                    // Log error and handle appropriately
                    throw new ApplicationException("Error executing stored procedure", ex);
                }
            }

            // 🔹 Merge and remove duplicates
            templateBodies = sp1Users
                .Select(x => new CommonTemplateMessage
                {
                    MobileNumber = x.MobileNumber,
                    TemplateBody = x.TemplateBody
                })
                .Concat(sp2Users.Select(x => new CommonTemplateMessage
                {
                    MobileNumber = x.MobileNumber,
                    TemplateBody = x.TemplateBody
                }))
                .GroupBy(x => x.MobileNumber)
                .Select(g => g.First())
                .ToList();

            // 🔹 Step 4: Return response
            if (templateBodies == null || templateBodies.Count == 0)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "No User Found with given Parameter or No template messages found for the given parameters."
                };
            }

            var options = new RestClientOptions(_configuration["Otp:Url"]!);
            var client = new RestClient(options);

            // Deserialize containerMeta
            dynamic container = null;
            if (!string.IsNullOrEmpty(param.ContainerMeta))
            {
                try
                {
                    container = JsonConvert.DeserializeObject<dynamic>(param.ContainerMeta);
                }
                catch (Exception ex)
                {
                    await _exceptionLog.AddAsync(new ExceptionLog
                    {
                        CreatedOn = DateTime.Now,
                        InnerException = ex.InnerException?.ToString(),
                        Message = ex.Message,
                        RequestBody = JsonConvert.SerializeObject(param),
                        Source = "SendWhatsappTemplateMessage",
                        StackTrace = ex.StackTrace
                    });
                }
            }

            foreach (var template in templateBodies)
            {
                var request = new RestRequest("", Method.Post);
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddHeader("accept", "application/json");
                request.AddHeader("apikey", _configuration["Otp:ApiKey"]!);
                request.AddParameter("source", _configuration["Otp:GupShupMobile"]);
                request.AddParameter("src.name", _configuration["Otp:Appname"]);

                if (container != null)
                {
                    if (container.mediaUrl != null)
                    {
                        var imageMessage = JsonConvert.SerializeObject(new
                        {
                            type = "image",
                            image = new { link = (string)container.mediaUrl }
                        });
                        request.AddParameter("message", imageMessage);
                    }

                    if (container.cards != null)
                    {
                        var cards = new List<object>();
                        foreach (var card in container.cards)
                        {
                            if (card.mediaUrl != null)
                            {

                                cards.Add(new { link = (string)card.mediaUrl });
                            }
                        }

                        if (cards.Any())
                        {
                            var carouselMessage = JsonConvert.SerializeObject(new
                            {
                                type = "carousel",
                                cardHeaderType = "IMAGE",
                                cards = cards
                            });
                            request.AddParameter("message", carouselMessage);
                        }
                    }
                }

                request.AddOrUpdateParameter("template", template.TemplateBody);
                request.AddOrUpdateParameter("destination", "91" + template.MobileNumber);

                var response = await client.PostAsync(request);

                if (!response.IsSuccessful)
                {
                    FileHelper.WriteToFile("WhatsappStatus", response.Content);
                }
                else
                {
                    await _log.AddAsync(new Log
                    {
                        CreatedOn = DateTime.Now,
                        Message = $"WhatsappStatus sent successfully to {template.MobileNumber}. Response : {response.Content}",
                        Source = "SendWhatsappStatus_Success",
                        Category = "Whatsapp"
                    });
                }
            }

            return new ApiCommonResponseModel { StatusCode = HttpStatusCode.OK, Message = "Message Sent" };
        }



        public async Task<ApiCommonResponseModel> SaveChartImageForMobile(ImageUploadRequestModel request)
        {
            if (request.Image == null || request.Image.Length == 0)
            {
                responseModel.StatusCode = HttpStatusCode.BadRequest;
                return responseModel;
            }

            var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            var fileExtension = Path.GetExtension(request.Image.FileName);

            var fullFileName = timeStamp + fileExtension;

            var filePath = Path.Combine(_configuration["Mobile:RootDirectory"], "Assets", "Products", fullFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.Image.CopyToAsync(stream);
            }

            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Data = fullFileName;

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetFreeTrial(QueryValues queryValues)
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
                 new SqlParameter
                 {
                        ParameterName = "Status",
                        Value = string.IsNullOrEmpty(queryValues.PrimaryKey) ? DBNull.Value : queryValues.PrimaryKey,
                        SqlDbType = SqlDbType.NVarChar
                 },
                parameterOutValue
            });

            List<FreeTrailResponseModel> freeTrail = await _dbContext.SqlQueryToListAsync<FreeTrailResponseModel>(
                "exec GetFreeTrial @IsPaging, @PageSize, @PageNumber, @SortExpression, @SortOrder, @RequestedBy, @FromDate, @ToDate, @SearchText, @Status, @TotalCount OUTPUT",
                sqlParameters.ToArray()
            );

            object totalRecords = parameterOutValue.Value;

            apiCommonResponse.Data = freeTrail;
            apiCommonResponse.StatusCode = HttpStatusCode.OK;
            apiCommonResponse.Total = Convert.ToInt32(totalRecords);

            return apiCommonResponse;
        }

        public async Task<ApiCommonResponseModel> GetPhonePe(QueryValues queryValues)
        {
            //var apiCommonResponse = new ApiCommonResponseModel();

            // Step 1: Prepare common SQL parameters
            List<SqlParameter> sqlParameters = ProcedureCommonSqlParameters.GetCommonSqlParameters(queryValues);

            // Step 2: Create the output parameter for TotalCount
            SqlParameter parameterOutValue = new SqlParameter
            {
                ParameterName = "@TotalCount", // Ensure the name matches exactly
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output,
            };

            SqlParameter parameterOutTotalPaidAmount = new SqlParameter
            {
                ParameterName = "@TotalPaidAmount", // Ensure the name matches exactly
                SqlDbType = SqlDbType.Decimal,
                Direction = ParameterDirection.Output,
            };

            // Add the output parameter to the list of SQL parameters
            sqlParameters.Add(parameterOutValue);
            sqlParameters.Add(parameterOutTotalPaidAmount);


            // Step 3: Add the @PrimaryKey parameter if it exists
            if (!string.IsNullOrEmpty(queryValues.PrimaryKey))
            {
                sqlParameters.Add(new SqlParameter("@PrimaryKey", SqlDbType.NVarChar)
                {
                    Value = queryValues.PrimaryKey
                });
            }
            else
            {
                // Add NULL to PrimaryKey if not specified
                sqlParameters.Add(new SqlParameter("@PrimaryKey", SqlDbType.NVarChar)
                {
                    Value = DBNull.Value
                });
            }

            if (!string.IsNullOrEmpty(queryValues.SecondaryKey))
            {
                sqlParameters.Add(new SqlParameter("@SecondaryKey", SqlDbType.NVarChar)
                {
                    Value = queryValues.SecondaryKey
                });
            }
            else
            {
                // Add NULL to PrimaryKey if not specified
                sqlParameters.Add(new SqlParameter("@SecondaryKey", SqlDbType.NVarChar)
                {
                    Value = DBNull.Value
                });
            }

            // Step 4: Execute the stored procedure and fetch the data
            List<PhonePeRequestModel> phonePeList = await _dbContext.SqlQueryToListAsync<PhonePeRequestModel>(
                "EXEC GetPhonePe @IsPaging, @PageSize, @PageNumber, @SortExpression, @SortOrder, @RequestedBy, @FromDate, @ToDate, @SearchText, @PrimaryKey, @SecondaryKey, @TotalCount OUTPUT, @TotalPaidAmount OUTPUT",
                sqlParameters.ToArray()
            );

            // Step 5: Retrieve the output parameter value (TotalCount)
            int totalRecords = parameterOutValue.Value != DBNull.Value ? Convert.ToInt32(parameterOutValue.Value) : 0;
            decimal totalPaidAmount = parameterOutTotalPaidAmount.Value != DBNull.Value ? Convert.ToDecimal(parameterOutTotalPaidAmount.Value) : 0;


            // Step 6: Prepare the response
            //apiCommonResponse.Data = phonePeList;
            //apiCommonResponse.StatusCode = HttpStatusCode.OK;
            //apiCommonResponse.Total = totalRecords;
            //apiCommonResponse.TotalPaidAmount = totalPaidAmount;

            dynamic runTimeObject = new ExpandoObject();
            runTimeObject.data = phonePeList;
            runTimeObject.Total = totalRecords;
            runTimeObject.TotalPaidAmount = totalPaidAmount;

            responseModel.Data = runTimeObject;
            responseModel.StatusCode = HttpStatusCode.OK;

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetPhonePeChartDataAsync(QueryValues query)

        {
            List<SqlParameter> sqlParameters = ProcedureCommonSqlParameters.GetCommonSqlParameters(query);


            var result = await _dbContext
                .SqlQueryToListAsync<PhonePeChartResponseModel>("EXEC [dbo].[GetPhonePeChartData] @FromDate, @ToDate", sqlParameters.ToArray());
            var reportTwo = await _dbContext
               .SqlQueryToListAsync<PhonePeReportPaymentDetails>("EXEC [dbo].[GetPhonePePaymentReportChart] @FromDate, @ToDate", sqlParameters.ToArray());

            decimal totalIncome = reportTwo.Sum(x => x.TotalPaidAmount);

            var resultNew = new
            {
                IncomeChart = result,
                DetailReport = reportTwo,
                IncomeChartReport = totalIncome,

            };

            ApiCommonResponseModel responseModel = new()
            {
                Data = resultNew,
                StatusCode = HttpStatusCode.OK
            };
            return responseModel;
        }
        public async Task<ApiCommonResponseModel> GetPhonePePaymentReportChartAsync(PhonePePaymentReportChartResponceModel query)
        {
            var sqlParameters = new[]
            {
        new SqlParameter("@FromDate", query.FromDate ?? (object)DBNull.Value),
        new SqlParameter("@ToDate", query.ToDate ?? (object)DBNull.Value),
        new SqlParameter("@ProductId", query.ProductId ?? (object)DBNull.Value),
        new SqlParameter("@DurationType", query.DurationType ?? (object)DBNull.Value),
    };

            var reportData = await _dbContext
                .SqlQueryToListAsync<PhonePeReportPaymentChartDetails>(
                    "EXEC [dbo].[GetPhonePePaymentReportBarChart] @FromDate, @ToDate, @ProductId, @DurationType",
                    sqlParameters
                );

            //var result = await _dbContext
            //    .SqlQueryToListAsync<PhonePeChartResponseModel>("EXEC [dbo].[GetPhonePeChartData] @FromDate, @ToDate", sqlParameters.ToArray());

            decimal totalIncome = reportData.Sum(x => x.TotalPaidAmount);
            decimal userCount = reportData.Sum(x => x.TotalUserCount);
            var duration = reportData.Select(x => x.Duration).Distinct().ToList();

            var resultNew = new
            {
                // IncomeChart = result,
                DetailReport = reportData,
                IncomeReport = totalIncome,
                TotalUsers = userCount,
                Duration = duration,

            };

            ApiCommonResponseModel responseModel = new()
            {
                Data = resultNew,
                StatusCode = HttpStatusCode.OK
            };

            return responseModel;
        }


        public async Task<ApiCommonResponseModel> UpdateUserMyBucketAsync(UpdateMyBucketResponseModel request)
        {
            var responseModel = new ApiCommonResponseModel();

            if (request.Id <= 0)
            {
                var existingProduct = await _dbContext.MyBucketM.FirstOrDefaultAsync(x =>
                x.ProductId == request.BucketProductId &&
                x.MobileUserKey == request.MobileUserPublicKey &&
                x.IsActive); // Optional: Check only active records

                if (existingProduct != null)
                {
                    responseModel.StatusCode = HttpStatusCode.InternalServerError; // Or an appropriate status code
                    responseModel.Message = "The product already exists in your bucket.";
                    return responseModel;
                }

                var myBucketAddProduct = new MyBucketM
                {
                    ProductId = request.BucketProductId,
                    ProductName = request.ProductName,
                    MobileUserKey = request.MobileUserPublicKey,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    IsActive = true,
                    IsExpired = false,
                    Notification = true,
                    Status = "1",
                    CreatedBy = request.PublicKey,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                };


                await _dbContext.MyBucketM.AddAsync(myBucketAddProduct);
                await _dbContext.SaveChangesAsync();

                var reasonChangeLog = new ReasonChangeLog
                {
                    MyBucketId = myBucketAddProduct.Id,
                    Reason = request.Reason,
                    CreatedBy = request.PublicKey,
                    CreatedDate = DateTime.Now,
                    ProductId = request.BucketProductId,
                    ProductName = request.ProductName,
                    BucketStartDate = request.StartDate,
                    BucketEndDate = request.EndDate
                };

                await _dbContext.ReasonChangeLogs.AddAsync(reasonChangeLog);
                await _dbContext.SaveChangesAsync();

                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Message = "Bucket Data Added Successfully.";
                return responseModel;
            }

            var basket = await _dbContext.MyBucketM.FirstOrDefaultAsync(x => x.Id == request.Id);

            if (basket == null)
            {
                responseModel.StatusCode = HttpStatusCode.NotFound;
                responseModel.Message = "Bucket not found.";
                return responseModel;
            }

            // Check for changes
            if (basket.StartDate.Value.Date != request.StartDate.Value.Date || (basket.EndDate.HasValue != request.EndDate.HasValue ||
                (basket.EndDate.HasValue && basket.EndDate.Value.Date != request.EndDate.Value.Date)) || basket.ProductName != request.ProductName)
            {
                if (basket.ProductId != request.BucketProductId)
                {
                    var existingProduct = await _dbContext.MyBucketM.FirstOrDefaultAsync(x =>
                    x.ProductId == request.BucketProductId &&
                    x.MobileUserKey == request.MobileUserPublicKey &&
                    x.IsActive); // Optional: Check only active records

                    // Check if the product already exists in the user's bucket
                    if (existingProduct != null)
                    {
                        responseModel.StatusCode = HttpStatusCode.InternalServerError; // Or an appropriate status code
                        responseModel.Message = "The product already exists in your bucket.";
                        return responseModel;
                    }
                }


                var reasonChangeLog = new ReasonChangeLog
                {
                    MyBucketId = request.Id,
                    Reason = request.Reason,
                    CreatedBy = request.PublicKey,
                    CreatedDate = DateTime.Now,
                    ProductId = request.BucketProductId,
                    ProductName = request.ProductName,
                    BucketStartDate = request.StartDate,
                    BucketEndDate = request.EndDate
                };

                // Update the Start and End Dates regardless of ProductName
                basket.StartDate = request.StartDate;
                basket.EndDate = request.EndDate;

                // Only update ProductName if it differs from the current value in the basket
                if (basket.ProductName != request.ProductName)
                {
                    basket.ProductName = request.ProductName;
                }

                // Update the ProductId
                basket.ProductId = request.BucketProductId;
                basket.ModifiedDate = DateTime.Now;
                // Set IsExpired based on EndDate
                if (basket.EndDate.Value.Date >= DateTime.Now.Date)
                {
                    basket.IsExpired = false;
                }

                // Insert the reason change log for this update
                await _dbContext.ReasonChangeLogs.AddAsync(reasonChangeLog);

                // Save all changes (update the basket and insert the reason log)
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                responseModel.StatusCode = HttpStatusCode.BadRequest;
                responseModel.Message = "No changes detected. Please modify values before updating.";
                return responseModel;
            }

            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Bucket Data Updated Successfully.";
            return responseModel;
        }



        public async Task<ApiCommonResponseModel> AddPurchaseOrder(AddPurchaseOrderDetailsRequestModel model, Guid loggedInUser)
        {


            

            var responseModel = new ApiCommonResponseModel();
            var lead = await _dbContext.Leads
                 .FirstOrDefaultAsync(l => l.MobileNumber == model.Mobile);

            //if (lead == null)
            //{
            //    responseModel.StatusCode = HttpStatusCode.BadRequest;
            //    responseModel.Message = "Lead not found for the given mobile number.";
            //    return responseModel;
            //}

            PurchaseOrderM myPurchaseOrder;

            if (model.Id > 0) // Update existing
            {
                myPurchaseOrder = await _dbContext.PurchaseOrdersM
                    .FirstOrDefaultAsync(p => p.Id == model.Id);

                if (myPurchaseOrder == null)
                {
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                    responseModel.Message = "Purchase Order not found.";
                    return responseModel;
                }

                // Update fields
                myPurchaseOrder.ClientName = model.ClientName;
                myPurchaseOrder.Mobile = model.Mobile;
                myPurchaseOrder.Email = model.Email;
                myPurchaseOrder.Dob = model.DOB;
                myPurchaseOrder.Remark = model.Remark;
                myPurchaseOrder.PaymentDate = model.PaymentDate;
                myPurchaseOrder.ModeOfPayment = model.ModeOfPayment;
                myPurchaseOrder.BankName = model.BankName;
                myPurchaseOrder.Pan = model.Pan;
                myPurchaseOrder.State = model.State;
                myPurchaseOrder.City = model.City;
               // myPurchaseOrder.TransasctionReference = model.TransasctionReference;
                myPurchaseOrder.ProductId = model.ProductId;
                myPurchaseOrder.Product = model.ProductName;
                myPurchaseOrder.NetAmount = model.NetAmount;
                myPurchaseOrder.PaidAmount = model.PaidAmount;
                myPurchaseOrder.Status = 1;
                myPurchaseOrder.ActionBy = model.PublicKey;
                myPurchaseOrder.PaymentStatusId = model.PaymentStatusId;
                myPurchaseOrder.PaymentActionDate = model.PaymentActionDate;
                myPurchaseOrder.StartDate = model.StartDate;
                myPurchaseOrder.EndDate = model.EndDate;
                myPurchaseOrder.KycApproved = model.KycApproved;
                myPurchaseOrder.KycApprovedDate = model.KycApprovedDate;
                myPurchaseOrder.SubscriptionMappingId = model.SubscriptionMappingId;
                myPurchaseOrder.TransactionId = model.TransactionId;
                myPurchaseOrder.Invoice = model.Invoice;
               // myPurchaseOrder.UpdatedOn = DateTime.UtcNow;
                //myPurchaseOrder.UpdatedBy = loggedInUser;
            }
            else // Add new
            {
                myPurchaseOrder = new PurchaseOrderM
                {
                    PublicKey = Guid.NewGuid(),
                    LeadId = lead?.Id ?? 0, // or null, depending on your schema
                    ClientName = model.ClientName,
                    Mobile = model.Mobile,
                    Email = model.Email,
                    Dob = model.DOB,
                    Remark = model.Remark,
                    PaymentDate = model.PaymentDate,
                    ModeOfPayment = model.ModeOfPayment,
                    BankName = model.BankName,
                    Pan = model.Pan,
                    State = model.State,
                    City = model.City,
                    //TransasctionReference = model.TransasctionReference,
                    ProductId = model.ProductId,
                    Product = model.ProductName,
                    NetAmount = model.NetAmount,
                    PaidAmount = model.PaidAmount,
                    Status = 1,
                    ActionBy = model.PublicKey,
                    PaymentStatusId = model.PaymentStatusId,
                    PaymentActionDate = model.PaymentActionDate,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = loggedInUser,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    IsActive = true,
                    KycApproved = model.KycApproved,
                    KycApprovedDate = model.KycApprovedDate,
                    SubscriptionMappingId = model.SubscriptionMappingId,
                    TransactionId = model.TransactionId,
                    Invoice = model.Invoice
                };

                _dbContext.PurchaseOrdersM.Add(myPurchaseOrder);
            }

            await _dbContext.SaveChangesAsync();

            // Always log reason change when updating
            var reasonChangePurchase = new ReasonChangePurchase
            {
                PurchaseOrderId = myPurchaseOrder.Id,
                Reason = model.Reason,
                CreatedBy = loggedInUser,
                CreatedDate = DateTime.Now,
                ProductId = model.ProductId,
                Product = model.ProductName,
                NetAmount = model.NetAmount,
                PaidAmount = model.PaidAmount,
                StartDate = model.StartDate,
                EndDate = model.EndDate
            };

            await _dbContext.ReasonChangePurchase.AddAsync(reasonChangePurchase);
            await _dbContext.SaveChangesAsync();

            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = model.Id > 0 ? "Purchase Order Updated Successfully." : "Purchase Order Added Successfully.";
            return responseModel;
        }
        public async Task<ApiCommonResponseModel> GetUserHistory(QueryValues queryValues)
        {
            var apiCommonResponse = new ApiCommonResponseModel();

            try
            {
                List<SqlParameter> sqlParameters = new List<SqlParameter>
                {
                    new SqlParameter("@PageNumber", queryValues.PageNumber > 0 ? queryValues.PageNumber : 1) { SqlDbType = SqlDbType.Int },
                    new SqlParameter("@PageSize", queryValues.PageSize > 0 ? queryValues.PageSize : 20) { SqlDbType = SqlDbType.Int },
                    new SqlParameter("@MobileNumber", string.IsNullOrEmpty(queryValues.PrimaryKey) ? DBNull.Value : queryValues.PrimaryKey) { SqlDbType = SqlDbType.NVarChar }
                };

                // Fetch data, ensure this matches the stored procedure and expected format
                List<UserHistoryResponseModel> userHistories = await _dbContext.SqlQueryToListAsync<UserHistoryResponseModel>(
                    "EXEC GetUserHistory @PageNumber, @PageSize, @MobileNumber",
                    sqlParameters.ToArray()
                );

                apiCommonResponse.Data = userHistories; // Ensure data is correctly populated
                apiCommonResponse.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                apiCommonResponse.Data = null;
                apiCommonResponse.StatusCode = HttpStatusCode.InternalServerError;
                apiCommonResponse.Message = $"An error occurred while fetching user history: {ex.Message}";
            }

            return apiCommonResponse;
        }



        public async Task<ApiCommonResponseModel> ToggleSubscriptionDurationStatusAsync(int id, Guid loggedUser)
        {
            var subscriptionDuration = await _dbContext.SubscriptionDurationM.FindAsync(id);

            if (subscriptionDuration == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Subscription duration not found.",
                    Data = null
                };
            }

            subscriptionDuration.IsActive = subscriptionDuration.IsActive.HasValue ? !subscriptionDuration.IsActive.Value : true;
            subscriptionDuration.ModifiedOn = DateTime.Now;

            await _dbContext.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Subscription duration status toggled successfully.",
                Data = subscriptionDuration
            };
        }

        public async Task<ApiCommonResponseModel> ToggleSubscriptionMappingStatusAsync(int id, Guid loggedUser)
        {
            var subscriptionMapping = await _dbContext.SubscriptionMappingM.FindAsync(id);

            if (subscriptionMapping == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Subscription mapping not found.",
                    Data = null
                };
            }

            subscriptionMapping.IsActive = subscriptionMapping.IsActive.HasValue ? !subscriptionMapping.IsActive.Value : true;
            subscriptionMapping.ModifiedOn = DateTime.Now;
            subscriptionMapping.ModifiedBy = loggedUser;

            await _dbContext.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Subscription mapping status toggled successfully.",
                Data = subscriptionMapping
            };
        }


        public async Task<ApiCommonResponseModel> ToggleSubscriptionPlanStatusAsync(int id, Guid loggedUser)
        {
            var subscriptionPlan = await _dbContext.SubscriptionPlanM.FindAsync(id);

            if (subscriptionPlan == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Subscription plan not found.",
                    Data = null
                };
            }

            subscriptionPlan.IsActive = subscriptionPlan.IsActive.HasValue ? !subscriptionPlan.IsActive.Value : true;
            subscriptionPlan.ModifiedOn = DateTime.Now;
            subscriptionPlan.ModifiedBy = loggedUser;

            await _dbContext.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Subscription plan status toggled successfully.",
                Data = subscriptionPlan
            };
        }

        public async Task<ApiCommonResponseModel> GetSubscriptionDurationAsync()
        {
            var durations = await _dbContext.SubscriptionDurationM.OrderByDescending(x => x.ModifiedOn).ToListAsync();
            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Successfully retrieved subscription durations.",
                Data = durations,
            };
        }




        public async Task<ApiCommonResponseModel> GetSubscriptionPlanAsync()
        {
            var plans = await _dbContext.SubscriptionPlanM
                .OrderByDescending(x => x.ModifiedOn)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Description,
                    x.IsActive,
                    x.CreatedOn,
                    x.ModifiedOn,
                    ModifiedBy = x.ModifiedBy != null
                        ? _dbContext.Users.Where(u => u.PublicKey == x.ModifiedBy)
                                          .Select(u => u.FirstName + " " + u.LastName)
                                          .FirstOrDefault()
                        : null
                })
                .ToListAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Successfully retrieved subscription plans.",
                Data = plans,
            };
        }



        public async Task<ApiCommonResponseModel> GetSubscriptionDetailsAsync(QueryValues queryValues)


        {
            SqlParameter totalCountParam = new()
            {
                ParameterName = "TotalCount",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output,
            };

            List<SqlParameter> sqlParameters = new()
            {
                new SqlParameter { ParameterName = "@PageSize", Value = queryValues.PageSize, SqlDbType = SqlDbType.Int },
                new SqlParameter { ParameterName = "@PageNumber", Value = queryValues.PageNumber, SqlDbType = SqlDbType.Int },
                new SqlParameter { ParameterName = "@SearchText", Value = string.IsNullOrEmpty(queryValues.SearchText) ? DBNull.Value : queryValues.SearchText, SqlDbType = SqlDbType.NVarChar },
                new SqlParameter { ParameterName = "@SubscriptionPlanId", Value = queryValues.SubscriptionPlanId ?? (object)DBNull.Value, SqlDbType = SqlDbType.Int },
                new SqlParameter { ParameterName = "@SubscriptionDurationId", Value = queryValues.SubscriptionDurationId ?? (object)DBNull.Value, SqlDbType = SqlDbType.Int },
                totalCountParam
            };

            var spResult = await _dbContext.SqlQueryToListAsync<SubscriptionModel.SubscriptionDetailsResponseModel>(
                "EXEC GetSubscriptionDetails  @PageSize, @PageNumber, @SearchText,@SubscriptionPlanId,  @SubscriptionDurationId, @TotalCount OUTPUT",
                sqlParameters.ToArray()
            );
            //  @DurationName
            //   @PlanName
            var totalCount = (int)totalCountParam.Value;

            var responseModel = new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Data retrieved successfully",
                Data = new { data = spResult, totalCount }
            };

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> AddSubscriptionMappingAsync(SubscriptionModel.SubscriptionMappingRequestModel request)
        {
            var existingMapping = await _dbContext.Set<SubscriptionMappingM>()
                .FirstOrDefaultAsync(m =>
                    m.ProductId == request.ProductId &&
                    m.SubscriptionDurationId == request.SubscriptionDurationId &&
                    m.SubscriptionPlanId == request.SubscriptionPlanId);

            if (existingMapping != null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.Conflict,
                    Message = "Duplicate subscription mapping. A similar record already exists.",
                    Data = null,
                    Exceptions = null
                };
            }

            var subscriptionMapping = new SubscriptionMappingM
            {
                SubscriptionDurationId = request.SubscriptionDurationId,
                DiscountPercentage = request.DiscountPercentage,
                ProductId = request.ProductId,
                SubscriptionPlanId = request.SubscriptionPlanId,
                IsActive = request.IsActive,
                CreatedOn = DateTime.Now,
                ModifiedOn = DateTime.Now
            };

            _dbContext.Set<SubscriptionMappingM>().Add(subscriptionMapping);
            await _dbContext.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.Created,
                Message = "Subscription mapping added successfully.",
                Data = subscriptionMapping,
                Exceptions = null
            };
        }

        public async Task<ApiCommonResponseModel> AddSubscriptionPlanAsync(SubscriptionModel.SubscriptionPlanRequest request)
        {
            var newPlan = new SubscriptionPlanM
            {
                Name = request.Name,
                Description = request.Description,
                IsActive = request.IsActive,
                CreatedOn = DateTime.Now,
                ModifiedOn = DateTime.Now
            };

            await _dbContext.SubscriptionPlanM.AddAsync(newPlan);
            await _dbContext.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.Created,
                Message = "Subscription plan added successfully.",
                Data = newPlan
            };
        }
        public async Task<ApiCommonResponseModel> UpdateSubscriptionPlanAsync(int id, SubscriptionModel.SubscriptionPlanRequest request)
        {
            var existingPlan = await _dbContext.SubscriptionPlanM.FindAsync(id);

            if (existingPlan == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Subscription plan not found."
                };
            }

            existingPlan.Name = request.Name;
            existingPlan.Description = request.Description;
            existingPlan.IsActive = request.IsActive;
            existingPlan.ModifiedOn = DateTime.Now;
            existingPlan.ModifiedBy = request.ModifiedBy;
            _dbContext.SubscriptionPlanM.Update(existingPlan);
            await _dbContext.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Subscription plan updated successfully.",
                Data = existingPlan
            };
        }


        public async Task<ApiCommonResponseModel> UpdateSubscriptionMappingAsync(SubscriptionModel.SubscriptionMappingUpdateRequest request)
        {
            var existingRecord = await _dbContext.Set<SubscriptionMappingM>()
                .FirstOrDefaultAsync(m => m.Id == request.MappingId);

            if (existingRecord == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Subscription mapping not found.",
                    Data = null,
                    Exceptions = null
                };
            }

            var duplicateExists = await _dbContext.Set<SubscriptionMappingM>()
                .AnyAsync(m => m.ProductId == request.ProductId
                            && m.SubscriptionPlanId == request.SubscriptionPlanId
                            && m.SubscriptionDurationId == request.SubscriptionDurationId
                            && m.Id != request.MappingId);

            if (duplicateExists)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.Conflict,
                    Message = "A subscription mapping with the same product, plan, and duration already exists.",
                    Data = null,
                    Exceptions = null
                };
            }

            existingRecord.SubscriptionDurationId = request.SubscriptionDurationId;
            existingRecord.DiscountPercentage = request.DiscountPercentage;
            existingRecord.IsActive = request.IsActive;
            existingRecord.ModifiedOn = DateTime.Now;
            existingRecord.ModifiedBy = request.ModifiedBy;

            await _dbContext.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Subscription mapping updated successfully.",
                Data = existingRecord,
                Exceptions = null
            };
        }

        public async Task<ApiCommonResponseModel> GetReasons(int bucketId)
        {
            var responseModel = new ApiCommonResponseModel();
            var result = await (from rcl in _dbContext.ReasonChangeLogs
                                join mu in _dbContext.Users on rcl.CreatedBy equals mu.PublicKey
                                where rcl.MyBucketId == bucketId
                                select new
                                {
                                    LogId = rcl.LogId,
                                    BucketId = rcl.MyBucketId,
                                    Reason = rcl.Reason,
                                    CreatedBy = rcl.CreatedBy,
                                    CreatedDate = rcl.CreatedDate,
                                    ProductId = rcl.ProductId,
                                    ProductName = rcl.ProductName,
                                    BucketStartDate = rcl.BucketStartDate,
                                    BucketEndDate = rcl.BucketEndDate,
                                    UserName = mu.FirstName + " " + mu.LastName,
                                }).OrderByDescending(r => r.CreatedDate)
                                .ToListAsync();

            if (result.Count == 0)
            {
                Console.WriteLine("No records found");
            }

            responseModel.Data = result;
            return responseModel;
        }
        public async Task<ApiCommonResponseModel> GetReasonsPurchaseAsync(int purchaseId)
        {
            var responseModel = new ApiCommonResponseModel();
            var result = await (from rcl in _dbContext.ReasonChangePurchase
                                join mu in _dbContext.Users on rcl.CreatedBy equals mu.PublicKey
                                where rcl.PurchaseOrderId == purchaseId
                                select new
                                {
                                    Id = rcl.Id,
                                    PurchaseOrderId = rcl.PurchaseOrderId,
                                    Reason = rcl.Reason,
                                    CreatedBy = rcl.CreatedBy,
                                    CreatedDate = rcl.CreatedDate,
                                    ProductId = rcl.ProductId,
                                    Product = rcl.Product,
                                    StartDate = rcl.StartDate,
                                    EndDate = rcl.EndDate,
                                    UserName = mu.FirstName + " " + mu.LastName,
                                }).OrderByDescending(r => r.CreatedDate)
                                .ToListAsync();


            if (result.Count == 0)
            {
                Console.WriteLine("No records found");
            }

            responseModel.Data = result;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> UpdatePurchaseOrderAsync(int id, Guid loggedInUser)
        {
            var responseModel = new ApiCommonResponseModel();

            if (id < 0)
            {
                responseModel.StatusCode = HttpStatusCode.NotFound;
                return responseModel;
            }

            var purchaseOrder = await _dbContext.PurchaseOrdersM.FindAsync(id);

            if (purchaseOrder == null)
            {
                responseModel.StatusCode = HttpStatusCode.NotFound;
                responseModel.Message = "Purchase Order Not Found.";
                return responseModel;
            }

            purchaseOrder.IsActive = false;
            purchaseOrder.ModifiedOn = DateTime.Now;
            purchaseOrder.ModifiedBy = loggedInUser;

            await _dbContext.SaveChangesAsync();

            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Purchase Order Deleted SuccessFully.";
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> ScheduleNotification(ScheduledNotificationRequestModel notification)
        {
            try
            {
                ScheduledNotification scheduled = new()
                {
                    LandingScreen = notification.NotificationScreenName,
                    AllowRepeat = notification.AllowRepeat,
                    Body = notification.Body,
                    CreatedBy = notification.PublicKey,
                    CreatedOn = DateTime.Now,
                    IsSent = true,
                    TargetAudience = notification.TargetAudience,
                    ModifiedBy = notification.PublicKey,
                    ModifiedOn = DateTime.Now,
                    ScheduledTime = notification.ScheduledTime,
                    Title = notification.Title,
                    Topic = notification.Topic,
                    ScheduledEndTime = notification.ScheduledTime.AddDays(1).AddMinutes(10)
                };

                _dbContext.ScheduledNotificationM.Add(scheduled);
                await _dbContext.SaveChangesAsync();

                return new ApiCommonResponseModel
                {
                    Message = "Scheduled Notification Added Successfuly.",
                    StatusCode = HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                // Log the detailed exception for debugging
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");

                return new ApiCommonResponseModel
                {
                    Message = "An error occurred. Please try again.",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        public async Task<ApiCommonResponseModel> GetScheduleNotification(QueryValues queryValues)
        {
            var apiCommonResponse = new ApiCommonResponseModel();

            try
            {
                // Step 1: Prepare common SQL parameters
                List<SqlParameter> sqlParameters = ProcedureCommonSqlParameters.GetCommonSqlParameters(queryValues);

                // Step 2: Create the output parameter for TotalCount
                SqlParameter parameterOutValue = new SqlParameter
                {
                    ParameterName = "@TotalCount", // Ensure the name matches exactly
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output,
                };

                // Add the output parameter to the list of SQL parameters
                sqlParameters.Add(parameterOutValue);

                // Step 4: Execute the stored procedure and fetch the data
                List<ScheduledNotificationModel> notificationsList = await _dbContext.SqlQueryToListAsync<ScheduledNotificationModel>(
                    "EXEC GetScheduledNotifications @IsPaging, @PageSize, @PageNumber, @SortExpression, @SortOrder, @SearchText, @FromDate, @ToDate, @TotalCount OUTPUT",
                    sqlParameters.ToArray()
                );

                // Step 5: Retrieve the output parameter value (TotalCount)
                int totalRecords = parameterOutValue.Value != DBNull.Value ? Convert.ToInt32(parameterOutValue.Value) : 0;

                // Step 6: Prepare the response
                apiCommonResponse.Data = notificationsList;
                apiCommonResponse.StatusCode = HttpStatusCode.OK;
                apiCommonResponse.Total = totalRecords;

                return apiCommonResponse;
            }
            catch (Exception ex)
            {
                apiCommonResponse.StatusCode = HttpStatusCode.InternalServerError;
                apiCommonResponse.Message = $"An error occurred: {ex.Message}";
                return apiCommonResponse;
            }
        }

        public async Task<ApiCommonResponseModel> GetPerformance(GetPerformanceRequestModel requestModel)
        {
            var responseModel = new ApiCommonResponseModel();

            // Validate code and modify it if necessary
            if (!string.Equals(requestModel.Code, "ShortInvestment", StringComparison.OrdinalIgnoreCase) && !string.Equals(requestModel.Code, "Multibagger", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(requestModel.Code, "KALKIBAATAAJ", StringComparison.OrdinalIgnoreCase))
                {
                    requestModel.Code = "KALKIBAATAAJ";
                }
                else if (string.Equals(requestModel.Code, "BREAKFAST", StringComparison.OrdinalIgnoreCase))
                {
                    requestModel.Code = "BREAKFAST";
                }
                else
                {
                    // Return response model if no condition is matched
                    responseModel.StatusCode = HttpStatusCode.BadRequest;
                    responseModel.Message = "Invalid code provided";
                    return responseModel;  // Handle invalid scenario
                }

                int pageNumber = (int)(requestModel.pageNumber); // Default to 1 if null
                int pageSize = (int)(requestModel.pageSize); // Default to 10 if null

                // Step 1: Prepare SQL parameters
                List<SqlParameter> sqlParameters = new List<SqlParameter>
                {
                    new SqlParameter("@Topic", SqlDbType.NVarChar) { Value = requestModel.Code ?? (object)DBNull.Value },
                    new SqlParameter("@Symbol", SqlDbType.NVarChar) { Value = requestModel.Symbol ?? (object)DBNull.Value },
                    new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = requestModel.fromDate ?? (object)DBNull.Value },
                    new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = requestModel.toDate ?? (object)DBNull.Value },
                    new SqlParameter("@PageNumber", SqlDbType.Int) { Value = requestModel.pageNumber },
                    new SqlParameter("@PageSize", SqlDbType.Int) { Value = requestModel.pageSize }
                };

                // Step 2: Add output parameter for TotalCount
                SqlParameter parameterOutTotalCount = new SqlParameter
                {
                    ParameterName = "@TotalCount",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };

                // Add output parameter for TotalCount
                sqlParameters.Add(parameterOutTotalCount);

                // Step 3: Execute the stored procedure and fetch the data
                var notifications = await _dbContext.SqlQueryToListAsync<PerformanceResponseModel>(
                    "EXEC GetPerformance @Topic, @Symbol, @FromDate, @ToDate, @PageNumber, @PageSize, @TotalCount OUTPUT",
                    sqlParameters.ToArray()
                );

                // Step 4: Retrieve the output parameter value (TotalCount)
                int totalCount = parameterOutTotalCount.Value != DBNull.Value ? Convert.ToInt32(parameterOutTotalCount.Value) : 0;

                // Step 5: Format notifications (Group by Date and Symbol, calculate ROI, etc.)
                var formattedNotifications = notifications
                    .GroupBy(n => new { Date = n.SentAt.Date, Symbol = n.TradingSymbol })
                    .OrderByDescending(group => group.Key.Date)
                    .Select(group =>
                    {
                        var orderedNotifications = group.OrderBy(n => n.CreatedOn).ToList();
                        var firstNotification = orderedNotifications.First();
                        var lastNotification = orderedNotifications.Count > 1 ? orderedNotifications.Last() : null;

                        // Ensure Ltp is treated as decimal for arithmetic operations
                        decimal firstLtp = firstNotification.Ltp ?? 0m;  // Default to 0 if null
                        decimal lastLtp = lastNotification?.Ltp ?? 0m;  // Default to 0 if null

                        var investmentMessage = firstLtp > 0 && lastNotification != null
                            ? $"Captured {Math.Round(lastLtp - firstLtp)} {(Math.Round(lastLtp - firstLtp) > 0 ? "levels" : "level")}."
                            : "Not applicable for this.";

                        var roi = firstLtp != 0 && lastNotification != null
                            ? Math.Round(((lastLtp - firstLtp) / firstLtp) * 100, 2).ToString()
                            : "N/A";

                        return new TradeResponseModel
                        {
                            Id = firstNotification.Id,
                            Symbol = group.Key.Symbol,
                            Cmp = null,
                            Duration = "N/A",
                            InvestmentMessage = investmentMessage,
                            Roi = roi,
                            Status = "Open",
                            EntryPrice = CurrencyHelper.ConvertToINRFormat(firstLtp),
                            ExitPrice = lastNotification != null ? CurrencyHelper.ConvertToINRFormat(lastLtp) : null
                        };
                    }).ToList();

                // Step 6: Prepare the performance data with statistics
                var performance = new PerformanceData
                {
                    Balance = "N/A",
                    Statistics = new Statistics
                    {
                        TotalTrades = formattedNotifications.Count,
                        TotalProfitable = 0,  // Calculate as per your business logic
                        TotalLoss = 0,  // Calculate as per your business logic
                        TradeClosed = 0,  // Calculate as per your business logic
                        TradeOpen = 0   // Calculate as per your business logic
                    },
                    TradesResponse = formattedNotifications
                };

                // Step 7: Prepare the response
                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Data = performance;
                responseModel.Total = totalCount;  // Set the total count of matching records for pagination
                return responseModel;  // Return the performance data
            }
            else
            {
                // Handle the case when requestModel.Code is "si" or "mb"

                SqlParameter totalTrades = new()
                {
                    ParameterName = "TotalTrades",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output,
                };
                SqlParameter totalProfitable = new()
                {
                    ParameterName = "TotalProfitable",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output,
                };
                SqlParameter totalLoss = new()
                {
                    ParameterName = "TotalLoss",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output,
                };
                SqlParameter tradeClosed = new()
                {
                    ParameterName = "TradeClosed",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output,
                };
                SqlParameter tradeOpen = new()
                {
                    ParameterName = "TradeOpen",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output,
                };
                SqlParameter balance = new()
                {
                    ParameterName = "Balance",
                    SqlDbType = System.Data.SqlDbType.NVarChar,
                    Size = 100,
                    Direction = System.Data.ParameterDirection.Output,
                };

                List<SqlParameter> sqlParameters = new()
                {
                    new SqlParameter("@Code", SqlDbType.VarChar, 50) { Value = requestModel.Code ?? (object)DBNull.Value },
                    new SqlParameter("@Symbol", SqlDbType.VarChar, 50) { Value = (object)requestModel.Symbol ?? DBNull.Value }, // Fix: Ensuring null is passed properly
                    new SqlParameter("@PageNumber", SqlDbType.Int) { Value = requestModel.pageNumber },
                    new SqlParameter("@PageSize", SqlDbType.Int) { Value = requestModel.pageSize },
                    totalTrades, totalProfitable, totalLoss, tradeClosed, balance, tradeOpen
                };


                var spResult = await _dbContext.SqlQueryToListAsync<Trade>(ProcedureCommonSqlParametersText.GetPaginationCallPerformanceM, sqlParameters.ToArray());

                int totalTradesValue = totalTrades.Value is DBNull ? 0 : (int)totalTrades.Value;
                int totalProfitableValue = totalProfitable.Value is DBNull ? 0 : (int)totalProfitable.Value;
                int totalLossValue = totalLoss.Value is DBNull ? 0 : (int)totalLoss.Value;
                int tradeClosedValue = tradeClosed.Value is DBNull ? 0 : (int)tradeClosed.Value;
                int tradeOpenValue = tradeOpen.Value is DBNull ? 0 : (int)tradeOpen.Value;
                string balanceValue = balance.Value is DBNull ? string.Empty : balance.Value.ToString();


                var performanceData = new PerformanceData
                {
                    Balance = balanceValue,
                    Statistics = new Statistics
                    {
                        TotalTrades = totalTradesValue,
                        TotalProfitable = totalProfitableValue,
                        TotalLoss = totalLossValue,
                        TradeClosed = tradeClosedValue,
                        TradeOpen = tradeOpenValue
                    },
                    Trades = spResult
                };


                responseModel.Data = performanceData;
                responseModel.Total = performanceData.Statistics.TotalTrades;
                responseModel.StatusCode = System.Net.HttpStatusCode.OK;

                return responseModel;

            }
        }

        public async Task<ApiCommonResponseModel> AddLeadFreeTrailAsync(LeadFreeTrialRequestModel request, Guid loggedInUser)
        {

            var service = await _dbContext.Services
                .FirstOrDefaultAsync(s => s.PublicKey == request.ServiceKey);

            if (service == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Invalid ServiceKey provided.",
                    Data = null,
                    Exceptions = null
                };
            }

            if (request.Id == 0)
            {
                var existingLeadFreeTrial = await _dbContext.Set<LeadFreeTrial>()
                    .FirstOrDefaultAsync(l => l.IsActive && l.LeadKey == request.LeadKey);

                if (existingLeadFreeTrial != null)
                {
                    if (existingLeadFreeTrial.EndDate >= DateTime.Now)
                    {
                        return new ApiCommonResponseModel
                        {
                            StatusCode = HttpStatusCode.Conflict,
                            Message = "An active LeadFreeTrial already exists and has not expired.",
                            Data = null,
                            Exceptions = null
                        };
                    }
                }

                var leadFreeTrial = new LeadFreeTrial
                {
                    LeadKey = request.LeadKey,
                    ServiceKey = request.ServiceKey,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    IsActive = true,
                    CreatedBy = loggedInUser,
                    ModifiedBy = loggedInUser,
                    CreatedOn = DateTime.Now,
                    ModifiedOn = DateTime.Now
                };

                _dbContext.Add(leadFreeTrial);
                await _dbContext.SaveChangesAsync();

                var reasonLog = new LeadFreeTrailReasonLog
                {
                    LeadFreeTrialId = leadFreeTrial.Id,
                    LeadKey = request.LeadKey,
                    ServiceKey = request.ServiceKey,
                    Reason = request.Reason,
                    ServiceName = service.Name,
                    FreeTrailStartDate = request.StartDate,
                    FreeTrailEndDate = request.EndDate,
                    CreatedBy = loggedInUser,
                    CreatedDate = DateTime.Now
                };

                _dbContext.LeadFreeTrailReasonLog.Add(reasonLog);

                // adding into userActivity for updating lead
                await _activityService.UserLog(loggedInUser.ToString(), request.LeadKey, ActivityTypeEnum.FreeTrailActivated, "FreeTrail Activated");

                // adding into leadActivity for updating lead
                await _activityService.LeadLog(request.LeadKey.ToString(), loggedInUser.ToString(), ActivityTypeEnum.FreeTrailActivated, description: "FreeTrail Activated");
                await _dbContext.SaveChangesAsync();

                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.Created,
                    Message = "LeadFreeTrial added successfully.",
                    Data = leadFreeTrial,
                    Exceptions = null
                };
            }

            else
            {
                var item = await _dbContext.LeadFreeTrials
                    .FirstOrDefaultAsync(b => b.Id == request.Id);

                if (item == null)
                {
                    return new ApiCommonResponseModel
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = "LeadFreeTrial Not Found.",
                        Exceptions = null
                    };
                }

                if (item.StartDate.Date != request.StartDate.Date || item.EndDate.Date != request.EndDate.Date || item.IsActive != request.IsActive)
                {
                    item.StartDate = request.StartDate;
                    item.EndDate = request.EndDate;
                    item.IsActive = request.IsActive;
                    item.LeadKey = request.LeadKey;
                    item.ServiceKey = request.ServiceKey;
                    item.ModifiedBy = loggedInUser;
                    item.ModifiedOn = DateTime.Now;

                    await _dbContext.SaveChangesAsync();

                    var reasonLog = new LeadFreeTrailReasonLog
                    {
                        LeadFreeTrialId = request.Id,
                        LeadKey = request.LeadKey,
                        ServiceKey = request.ServiceKey,
                        Reason = request.Reason,
                        ServiceName = service.Name,
                        FreeTrailStartDate = request.StartDate,
                        FreeTrailEndDate = request.EndDate,
                        CreatedBy = loggedInUser,
                        CreatedDate = DateTime.Now
                    };


                    _dbContext.Add(reasonLog);


                    // adding into userActivity for updating lead
                    await _activityService.UserLog(loggedInUser.ToString(), request.LeadKey, ActivityTypeEnum.FreeTrailExtended, "FreeTrail Extended");

                    // adding into leadActivity for updating lead
                    await _activityService.LeadLog(request.LeadKey.ToString(), loggedInUser.ToString(), ActivityTypeEnum.FreeTrailExtended, description: "FreeTrail Extended");
                    await _dbContext.SaveChangesAsync();

                    return new ApiCommonResponseModel
                    {
                        StatusCode = HttpStatusCode.OK,
                        Message = "LeadFreeTrial Updated successfully.",
                        Exceptions = null
                    };
                }
                else
                {
                    return new ApiCommonResponseModel
                    {
                        StatusCode = HttpStatusCode.Conflict,
                        Message = "No Changes...",
                        Exceptions = null
                    };
                }
            }
        }


        public async Task<ApiCommonResponseModel> GetLeadFreeTrailsAsync(Guid LeadKey)
        {
            try
            {
                // Fetch all active lead free trials from the database
                var currentDate = DateTime.Now.Date;

                var leadFreeTrials = await (from free in _dbContext.LeadFreeTrials
                                            join Lead in _dbContext.Leads on free.LeadKey equals Lead.PublicKey
                                            join service in _dbContext.Services on free.ServiceKey equals service.PublicKey
                                            join user in _dbContext.Users on free.CreatedBy equals user.PublicKey
                                            join reasonlog in _dbContext.LeadFreeTrailReasonLog
                                                on free.Id equals reasonlog.LeadFreeTrialId into reasonLogs
                                            where free.LeadKey == LeadKey
                                            orderby free.ModifiedOn descending
                                            select new
                                            {
                                                Id = free.Id,
                                                LeadName = Lead.FullName,
                                                LeadKey = free.LeadKey,
                                                ServiceKey = free.ServiceKey,
                                                ServiceName = service.Name,
                                                StartDate = free.StartDate,
                                                EndDate = free.EndDate,
                                                Status = free.IsActive,
                                                CreatedOn = free.CreatedOn,
                                                CreatedBy = user.FirstName + " " + user.LastName,
                                                ModifiedBy = user.FirstName + " " + user.LastName,
                                                ModifiedOn = free.ModifiedOn,

                                                // ✅ Corrected Validity Calculation Based on Given Scenarios
                                                Validity = (currentDate < free.StartDate.Date)
                                                        ? (free.EndDate.Date - free.StartDate.Date).Days  // Future start date → full duration
                                                        : (free.EndDate.Date - currentDate).Days,
                                                ReasonLogCount = reasonLogs.Count(),
                                                StatusText = free.EndDate < currentDate
                                                                ? "Expired"
                                                                : free.IsActive ? "Active" : "Inactive"
                                            }).ToListAsync();


                // Prepare the response
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "LeadFreeTrials retrieved successfully.",
                    Data = leadFreeTrials,
                    Exceptions = null
                };
            }
            catch (Exception ex)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "An error occurred while retrieving LeadFreeTrials.",
                    Data = null,
                    Exceptions = ex.Message
                };
            }
        }

        public async Task<ApiCommonResponseModel> DeletePerformance(int Id)
        {
            // Validate the input object
            if (Id == 0)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "An error occurred while retrieving LeadFreeTrials.",
                    Data = null
                };
            }

            // Find the ScannerPerformanceM by ID
            var item = await _dbContext.ScannerPerformanceM.FirstOrDefaultAsync(s => s.ID == Id);

            if (item == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Performance not found.",
                    Data = null
                };
            }

            // Mark the item as deleted and inactive
            item.IsDelete = true;
            item.IsActive = false;
            await _dbContext.SaveChangesAsync();

            // Prepare the response
            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Performance Data retrieved successfully.",
                Exceptions = null
            };
        }

        public async Task<ApiCommonResponseModel> DeleteFreeTrail(int id, Guid loggedUser)
        {
            // Validate the input object
            if (id == 0)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "An error occurred while Deleting LeadFreeTrials.",
                    Data = null
                };
            }

            // Find the LeadFreeTrials by ID
            var item = await _dbContext.LeadFreeTrials.FirstOrDefaultAsync(b => b.Id == id);

            if (item == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Lead Free Trail not found.",
                    Data = null
                };
            }

            // Mark the item as deleted and inactive
            item.IsActive = false;
            item.IsDeleted = true;
            item.ModifiedOn = DateTime.Now;
            item.ModifiedBy = loggedUser;
            // adding into userActivity for updating lead
            await _activityService.UserLog(loggedUser.ToString(), item.LeadKey, ActivityTypeEnum.FreeTrailDeleted, "FreeTrail Deleted");

            // adding into leadActivity for updating lead
            await _activityService.LeadLog(item.LeadKey.ToString(), loggedUser.ToString(), ActivityTypeEnum.FreeTrailDeleted, description: "FreeTrail Deleted");
            await _dbContext.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "LeadFreeTrials Deleted Successfully.",
                Exceptions = null
            };
        }

        public async Task<ApiCommonResponseModel> DeleteNotification(int id, int primarySid)
        {
            // Validate the input object
            if (id == 0)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "An error occurred while retrieving LeadFreeTrials.",
                    Data = null
                };
            }

            // Find the ScheduledNotificationM by ID
            var item = await _dbContext.ScheduledNotificationM.FirstOrDefaultAsync(b => b.Id == id);

            if (item == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Notification Details not found.",
                    Data = null
                };
            }

            // Mark the item as deleted and inactive
            item.IsActive = false;
            item.ModifiedOn = DateTime.Now;
            item.ModifiedBy = primarySid;
            await _dbContext.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Notification Deleted successfully.",
                Exceptions = null
            };
        }

        public async Task<ApiCommonResponseModel> DeleteCompany(int id, Guid loggedInUser)
        {
            // Validate the input object
            if (id == 0)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Id Not Found In Company Database.",
                    Data = null
                };
            }

            // Find the CompanyDetailM by ID
            var item = await _dbContext.CompanyDetailM.FirstOrDefaultAsync(b => b.Id == id);

            // Check if the result is null
            if (item == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Company Details not found.",
                    Data = null
                };
            }

            // Mark the item as deleted and inactive
            item.IsActive = false;
            item.IsDelete = true;
            item.ModifiedOn = DateTime.Now;
            item.ModifiedBy = loggedInUser;
            await _dbContext.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Company Deleted successfully.",
                Exceptions = null
            };
        }

        public async Task<ApiCommonResponseModel> DeleteCompanyDetails(int id, Guid loggedInUser)
        {
            // Validate the input object
            if (id == 0)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Id Not Found In Company Database.",
                    Data = null
                };
            }

            // Find the CompanyDetailM by ID
            var item = await _dbContext.CompanyDetailMessageM.FirstOrDefaultAsync(b => b.Id == id);

            // Check if the result is null
            if (item == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Company Details not found.",
                    Data = null
                };
            }

            // Mark the item as deleted and inactive
            item.IsActive = false;
            item.ModifiedOn = DateTime.Now;
            item.ModifiedBy = loggedInUser;
            await _dbContext.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Company Deleted successfully.",
                Exceptions = null
            };
        }

        //public async Task<ApiCommonResponseModel> CreateProductCommunity(ProductCommunityMappingRequestModel request)
        //{
        //    if (request.Action == "Add")
        //    {
        //        var duplicateMapping = await _dbContext.Set<ProductCommunityMapping>()
        //         .AnyAsync(c =>
        //             c.IsDeleted == false &&
        //             (
        //                 // Prevent exact duplicate (same ProductId & CommunityId)
        //                 (c.CommunityId == request.CommunityId && c.ProductId == request.ProductId)
        //                 ||
        //                 // Prevent same Community linked to a different Product
        //                 (c.CommunityId == request.CommunityId && c.ProductId != request.ProductId)
        //                 ||
        //                 // Prevent same Product linked to a different Community
        //                 (c.ProductId == request.ProductId && c.CommunityId != request.CommunityId)
        //             )
        //         );

        //        if (duplicateMapping)
        //        {
        //            return new ApiCommonResponseModel
        //            {
        //                StatusCode = HttpStatusCode.Conflict,
        //                Message = "This Product-Community mapping already exists or is conflicting with another mapping..!!!",
        //                Data = null,
        //                Exceptions = null
        //            };
        //        }

        //        var productCommunity = new ProductCommunityMapping
        //        {
        //            CommunityId = request.CommunityId,
        //            CreatedBy = request.CreatedBy,
        //            CreatedDate = DateTime.Now,
        //            ModifiedBy = request.CreatedBy,
        //            ModifiedDate = DateTime.Now,
        //            IsActive = true,
        //            IsDeleted = false,
        //            ProductId = request.ProductId
        //        };

        //        _dbContext.Add(productCommunity);
        //        await _dbContext.SaveChangesAsync();

        //        // Fetch the product & community names for response
        //        var product = await _dbContext.ProductsM
        //            .Where(p => p.Id == request.ProductId)
        //            .Select(p => new { p.Name })
        //            .FirstOrDefaultAsync();

        //        var community = await _dbContext.ProductsM
        //            .Where(c => c.Id == request.CommunityId)
        //            .Select(c => new { c.Name })
        //            .FirstOrDefaultAsync();

        //        var user = await _dbContext.Users
        //            .Where(u => u.Id == request.CreatedBy)
        //            .Select(u => new { u.FirstName, u.LastName })
        //            .FirstOrDefaultAsync();

        //        // Construct response data
        //        var response = new
        //        {
        //            Id = productCommunity.Id,
        //            CommunityId = productCommunity.CommunityId,
        //            CreatedBy = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
        //            ModifiedBy = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
        //            CreatedDate = productCommunity.CreatedDate,
        //            ModifiedDate = productCommunity.ModifiedDate,
        //            ProductId = productCommunity.ProductId,
        //            ProductName = product?.Name ?? "Unknown Product",
        //            CommunityName = community?.Name ?? "Unknown Community",
        //            IsActive = productCommunity.IsActive,
        //            IsDeleted = productCommunity.IsDeleted
        //        };

        //        return new ApiCommonResponseModel
        //        {
        //            StatusCode = HttpStatusCode.OK,
        //            Message = "Created Product Community successfully.",
        //            Exceptions = null,
        //            Data = response
        //        };
        //    }

        //    else if (request.Action == "Edit")
        //    {
        //        // Fetch the existing record
        //        var existingRecord = await _dbContext.ProductCommunityMappingM
        //            .FirstOrDefaultAsync(c => c.Id == request.Id);

        //        if (existingRecord == null)
        //        {
        //            return new ApiCommonResponseModel
        //            {
        //                StatusCode = HttpStatusCode.InternalServerError,
        //                Message = "Product Community Mapping details not found.",
        //                Data = null
        //            };
        //        }

        //        //  If modifying ProductId, check if the new Product is already linked to ANY other Community
        //        if (existingRecord.ProductId != request.ProductId)
        //        {
        //            var isProductMapped = await _dbContext.Set<ProductCommunityMapping>()
        //                .AnyAsync(c =>
        //                    c.IsActive == true &&
        //                    c.IsDeleted == false &&
        //                    c.ProductId == request.ProductId
        //                );

        //            if (isProductMapped)
        //            {
        //                return new ApiCommonResponseModel
        //                {
        //                    StatusCode = HttpStatusCode.Conflict,
        //                    Message = "This Product is already linked to another Community.",
        //                    Data = null
        //                };
        //            }
        //        }

        //        //  If modifying CommunityId, check if the new Community is already linked to ANY other Product
        //        if (existingRecord.CommunityId != request.CommunityId)
        //        {
        //            var isCommunityMapped = await _dbContext.Set<ProductCommunityMapping>()
        //                .AnyAsync(c =>
        //                    c.IsActive == true &&
        //                    c.IsDeleted == false &&
        //                    c.CommunityId == request.CommunityId
        //                );

        //            if (isCommunityMapped)
        //            {
        //                return new ApiCommonResponseModel
        //                {
        //                    StatusCode = HttpStatusCode.Conflict,
        //                    Message = "This Community is already linked to another Product.",
        //                    Data = null
        //                };
        //            }
        //        }

        //        // Continue with Edit Logic
        //        var item = await _dbContext.ProductCommunityMappingM.FirstOrDefaultAsync(c => c.Id == request.Id);

        //        if (item == null)
        //        {
        //            return new ApiCommonResponseModel
        //            {
        //                StatusCode = HttpStatusCode.InternalServerError,
        //                Message = "Product Community Mapping details not found.",
        //                Data = null
        //            };
        //        }

        //        // Update existing record
        //        item.CommunityId = request.CommunityId;
        //        item.ModifiedBy = request.CreatedBy;
        //        item.ModifiedDate = DateTime.Now;
        //        item.ProductId = request.ProductId;

        //        _dbContext.Update(item);
        //        await _dbContext.SaveChangesAsync();

        //        // Fetch updated product & community names for response
        //        var product = await _dbContext.ProductsM
        //            .Where(p => p.Id == request.ProductId)
        //            .Select(p => new { p.Name })
        //            .FirstOrDefaultAsync();

        //        var community = await _dbContext.ProductsM
        //            .Where(c => c.Id == request.CommunityId)
        //            .Select(c => new { c.Name })
        //            .FirstOrDefaultAsync();

        //        var user = await _dbContext.Users
        //            .Where(u => u.Id == request.CreatedBy)
        //            .Select(u => new { u.FirstName, u.LastName })
        //            .FirstOrDefaultAsync();

        //        // Construct response data
        //        var response = new
        //        {
        //            Id = item.Id,
        //            CommunityId = item.CommunityId,
        //            CreatedBy = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
        //            ModifiedBy = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
        //            CreatedDate = item.CreatedDate,
        //            ModifiedDate = item.ModifiedDate,
        //            ProductId = item.ProductId,
        //            ProductName = product?.Name ?? "Unknown Product",
        //            CommunityName = community?.Name ?? "Unknown Community",
        //            IsActive = item.IsActive,
        //            IsDeleted = item.IsDeleted
        //        };

        //        return new ApiCommonResponseModel
        //        {
        //            StatusCode = HttpStatusCode.OK,
        //            Message = "Updated Product Community successfully.",
        //            Exceptions = null,
        //            Data = response
        //        };
        //    }

        //    else
        //    {
        //        var item = await _dbContext.ProductCommunityMappingM.FirstOrDefaultAsync(c => c.Id == request.Id);

        //        if (item == null)
        //        {
        //            return new ApiCommonResponseModel
        //            {
        //                StatusCode = HttpStatusCode.InternalServerError,
        //                Message = "Product Community Mapping details not found.",
        //                Data = null
        //            };
        //        }

        //        // Toggle the IsActive status
        //        item.IsActive = !item.IsActive;
        //        item.ModifiedBy = request.CreatedBy;
        //        item.ModifiedDate = DateTime.Now;

        //        _dbContext.Update(item);
        //        await _dbContext.SaveChangesAsync();

        //        return new ApiCommonResponseModel
        //        {
        //            StatusCode = HttpStatusCode.OK,
        //            Message = "Updated Product Community Satus successfully.",
        //            Exceptions = null
        //        };
        //    }
        //}

        public async Task<ApiCommonResponseModel> AddProductCommunity(ProductCommunityMappingRequestModel request)
        {
            var duplicate = await _dbContext.ProductCommunityMappingM.AnyAsync(c =>
                !c.IsDeleted &&
                ((c.CommunityId == request.CommunityId && c.ProductId == request.ProductId) ||
                 (c.CommunityId == request.CommunityId && c.ProductId != request.ProductId) ||
                 (c.ProductId == request.ProductId && c.CommunityId != request.CommunityId)));

            if (duplicate)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.Conflict,
                    Message = "Mapping already exists or is conflicting."
                };
            }

            var productCommunity = new ProductCommunityMapping
            {
                CommunityId = request.CommunityId,
                CreatedBy = request.CreatedBy,
                ModifiedBy = request.CreatedBy,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                IsActive = true,
                IsDeleted = false,
                ProductId = request.ProductId,
                DurationInDays = request.DurationInDays
            };

            _dbContext.ProductCommunityMappingM.Add(productCommunity);
            await _dbContext.SaveChangesAsync();

            // Fetch the product & community names for response
            var product = await _dbContext.ProductsM
                .Where(p => p.Id == request.ProductId)
                .Select(p => new { p.Name })
                .FirstOrDefaultAsync();

            var community = await _dbContext.ProductsM
                .Where(c => c.Id == request.CommunityId)
                .Select(c => new { c.Name })
                .FirstOrDefaultAsync();

            var user = await _dbContext.Users
                .Where(u => u.Id == request.CreatedBy)
                .Select(u => new { u.FirstName, u.LastName })
                .FirstOrDefaultAsync();

            // Construct response data
            var response = new
            {
                Id = productCommunity.Id,
                CommunityId = productCommunity.CommunityId,
                CreatedBy = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
                ModifiedBy = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
                CreatedDate = productCommunity.CreatedDate,
                ModifiedDate = productCommunity.ModifiedDate,
                ProductId = productCommunity.ProductId,
                ProductName = product?.Name ?? "Unknown Product",
                CommunityName = community?.Name ?? "Unknown Community",
                IsActive = productCommunity.IsActive,
                IsDeleted = productCommunity.IsDeleted,
                DurationInDays = productCommunity.DurationInDays
            };

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Created Product Community Mapping successfully.",
                Exceptions = null,
                Data = response
            };
        }

        public async Task<ApiCommonResponseModel> EditProductCommunity(ProductCommunityMappingRequestModel request)
        {
            var existing = await _dbContext.ProductCommunityMappingM.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (existing == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Mapping not found."
                };
            }

            if (existing.ProductId != request.ProductId)
            {
                var productMapped = await _dbContext.ProductCommunityMappingM.AnyAsync(c =>
                    c.ProductId == request.ProductId && !c.IsDeleted);

                if (productMapped)
                {
                    return new ApiCommonResponseModel
                    {
                        StatusCode = HttpStatusCode.Conflict,
                        Message = "This product is already linked to another community."
                    };
                }
            }

            if (existing.CommunityId != request.CommunityId)
            {
                var communityMapped = await _dbContext.ProductCommunityMappingM.AnyAsync(c =>
                    c.CommunityId == request.CommunityId && !c.IsDeleted);

                if (communityMapped)
                {
                    return new ApiCommonResponseModel
                    {
                        StatusCode = HttpStatusCode.Conflict,
                        Message = "This community is already linked to another product."
                    };
                }
            }

            existing.ProductId = request.ProductId;
            existing.CommunityId = request.CommunityId;
            existing.ModifiedBy = request.CreatedBy;
            existing.ModifiedDate = DateTime.Now;
            existing.DurationInDays = request.DurationInDays;

            _dbContext.Update(existing);
            await _dbContext.SaveChangesAsync();

            // Fetch updated product & community names for response
            var product = await _dbContext.ProductsM
                .Where(p => p.Id == request.ProductId)
                .Select(p => new { p.Name })
                .FirstOrDefaultAsync();

            var community = await _dbContext.ProductsM
                .Where(c => c.Id == request.CommunityId)
                .Select(c => new { c.Name })
                .FirstOrDefaultAsync();

            var user = await _dbContext.Users
                .Where(u => u.Id == request.CreatedBy)
                .Select(u => new { u.FirstName, u.LastName })
                .FirstOrDefaultAsync();

            // Construct response data
            var response = new
            {
                Id = existing.Id,
                CommunityId = existing.CommunityId,
                CreatedBy = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
                ModifiedBy = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
                CreatedDate = existing.CreatedDate,
                ModifiedDate = existing.ModifiedDate,
                ProductId = existing.ProductId,
                ProductName = product?.Name ?? "Unknown Product",
                CommunityName = community?.Name ?? "Unknown Community",
                IsActive = existing.IsActive,
                IsDeleted = existing.IsDeleted,
                DurationInDays = existing.DurationInDays
            };

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Updated Product Community successfully.",
                Exceptions = null,
                Data = response
            };
        }

        public async Task<ApiCommonResponseModel> ToggleProductCommunityStatus(int id, int modifiedBy)
        {
            var item = await _dbContext.ProductCommunityMappingM.FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Mapping not found."
                };
            }

            item.IsActive = !item.IsActive;
            item.ModifiedBy = modifiedBy;
            item.ModifiedDate = DateTime.Now;

            _dbContext.Update(item);
            await _dbContext.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Status updated successfully.",
                Data = new { item.Id, item.IsActive }
            };
        }

        public async Task<ApiCommonResponseModel> DeleteProductCommunity(int id, int modifiedBy)
        {
            // Validate the input object
            if (id == 0)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Id Not Found In Product Community MappingM Database Table.",
                    Data = null
                };
            }

            // Find the CompanyDetailM by ID
            var item = await _dbContext.ProductCommunityMappingM.Where(b => b.Id == id).FirstOrDefaultAsync();

            // Check if the result is null
            if (item == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Product Community MappingM Details not found.",
                    Data = null
                };
            }

            // Mark the item as deleted and inactive
            item.IsActive = false;
            item.IsDeleted = true;
            item.ModifiedDate = DateTime.Now;
            item.ModifiedBy = modifiedBy;
            await _dbContext.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Product Community MappingM Deleted successfully.",
                Exceptions = null
            };
        }

        //public async Task<ApiCommonResponseModel> CreateBonusProductMapping(BonusProductMappingRequestModel request)
        //{
        //    if (request.Action == "Add")
        //    {
        //        // Prevent mapping the same product as its own bonus
        //        if (request.ProductId == request.BonusProductId)
        //        {
        //            return new ApiCommonResponseModel
        //            {
        //                StatusCode = HttpStatusCode.Conflict,
        //                Message = "A product cannot be its own bonus product..!!!",
        //                Data = null,
        //                Exceptions = null
        //            };
        //        }

        //        var duplicateMapping = await _dbContext.Set<ProductBonusMappingM>()
        //                    .FirstOrDefaultAsync(c =>
        //                        c.IsDeleted == false &&
        //                        c.ProductId == request.ProductId
        //                    );

        //        if (duplicateMapping != null)
        //        {
        //            return new ApiCommonResponseModel
        //            {
        //                StatusCode = HttpStatusCode.Conflict,
        //                Message = "This Product is already mapped to another bonus product..!!!",
        //                Data = null,
        //                Exceptions = null
        //            };
        //        }

        //        // Check if the bonus product is already mapped to another product
        //        var bonusProductMapped = await _dbContext.Set<ProductBonusMappingM>()
        //            .FirstOrDefaultAsync(c =>
        //                c.IsActive == true &&
        //                c.IsDeleted == false &&
        //                c.BonusProductId == request.BonusProductId
        //            );

        //        if (bonusProductMapped != null)
        //        {
        //            return new ApiCommonResponseModel
        //            {
        //                StatusCode = HttpStatusCode.Conflict,
        //                Message = "This Bonus Product is already bound to another product..!!!",
        //                Data = null,
        //                Exceptions = null
        //            };
        //        }


        //        var existingSubscriptionMapping = _dbContext.SubscriptionMappingM
        //                    .Where(m => m.ProductId == request.ProductId)
        //                    .FirstOrDefault();

        //        if (existingSubscriptionMapping == null)
        //        {
        //            return new ApiCommonResponseModel
        //            {
        //                StatusCode = HttpStatusCode.NoContent,
        //                Message = "We Can't Create for this, Because this Product Don't have Subscription Mapping.",
        //                Data = null,
        //                Exceptions = null
        //            };
        //        }


        //        var productCommunity = new ProductBonusMappingM
        //        {
        //            BonusProductId = request.BonusProductId,
        //            CreatedBy = request.CreatedBy,
        //            CreatedOn = DateTime.Now,
        //            ModifiedBy = request.CreatedBy,
        //            ModifiedOn = DateTime.Now,
        //            IsActive = true,
        //            IsDeleted = false,
        //            ProductId = request.ProductId,
        //            DurationInDays = request.DurationInDays
        //        };

        //        _dbContext.Add(productCommunity);
        //        await _dbContext.SaveChangesAsync();

        //        // Fetch the product name using the ProductId
        //        var product = await _dbContext.ProductsM
        //            .Where(p => p.Id == request.ProductId)
        //            .Select(p => new { p.Name })
        //            .FirstOrDefaultAsync();

        //        var bonusProduct = await _dbContext.ProductsM
        //            .Where(p => p.Id == request.BonusProductId)
        //            .Select(p => new { p.Name })
        //            .FirstOrDefaultAsync();

        //        // Fetch the user details using CreatedBy
        //        var user = await _dbContext.Users
        //            .Where(u => u.Id == request.CreatedBy)
        //            .Select(u => new { u.FirstName, u.LastName })
        //            .FirstOrDefaultAsync();

        //        // Construct response data
        //        var response = new
        //        {
        //            Id = productCommunity.Id,
        //            BonusProductId = productCommunity.BonusProductId,
        //            CreatedBy = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
        //            ModifiedBy = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
        //            CreatedOn = productCommunity.CreatedOn,
        //            ModifiedOn = productCommunity.ModifiedOn,
        //            ProductId = productCommunity.ProductId,
        //            ProductName = product?.Name ?? "Unknown Product",
        //            BonusProductName = bonusProduct?.Name ?? "Unknown Bonus Product",
        //            IsActive = productCommunity.IsActive,
        //            IsDeleted = productCommunity.IsDeleted,
        //            DurationInDays = productCommunity.DurationInDays
        //        };

        //        return new ApiCommonResponseModel
        //        {
        //            StatusCode = HttpStatusCode.OK,
        //            Message = "Created Bonus Product MappingM successfully.",
        //            Exceptions = null,
        //            Data = response
        //        };
        //    }

        //    else if (request.Action == "Edit")
        //    {
        //        var existingRecord = await _dbContext.Set<ProductBonusMappingM>()
        //            .FirstOrDefaultAsync(c => c.Id == request.Id);

        //        if (existingRecord == null)
        //        {
        //            return new ApiCommonResponseModel
        //            {
        //                StatusCode = HttpStatusCode.NotFound,
        //                Message = "Record not found.",
        //                Data = null
        //            };
        //        }

        //        // If modifying ProductId, check if the new ProductId is already mapped to another BonusProduct
        //        if (existingRecord.ProductId != request.ProductId)
        //        {
        //            var isProductMapped = await _dbContext.Set<ProductBonusMappingM>()
        //                .AnyAsync(c =>
        //                    c.IsActive == true &&
        //                    c.IsDeleted == false &&
        //                    c.ProductId == request.ProductId &&
        //                    c.Id != request.Id
        //                );

        //            if (isProductMapped)
        //            {
        //                return new ApiCommonResponseModel
        //                {
        //                    StatusCode = HttpStatusCode.Conflict,
        //                    Message = "This Product is already mapped to another Bonus Product.",
        //                    Data = null
        //                };
        //            }
        //        }

        //        // If modifying BonusProductId, check if the new BonusProductId is already mapped to another Product
        //        if (existingRecord.BonusProductId != request.BonusProductId)
        //        {
        //            var isBonusMapped = await _dbContext.Set<ProductBonusMappingM>()
        //                .AnyAsync(c =>
        //                    c.IsActive == true &&
        //                    c.IsDeleted == false &&
        //                    c.BonusProductId == request.BonusProductId &&
        //                    c.Id != request.Id
        //                );

        //            if (isBonusMapped)
        //            {
        //                return new ApiCommonResponseModel
        //                {
        //                    StatusCode = HttpStatusCode.Conflict,
        //                    Message = "This Bonus Product is already bound to another Product.",
        //                    Data = null
        //                };
        //            }
        //        }

        //        // Update only if changes are detected
        //        existingRecord.ProductId = request.ProductId;
        //        existingRecord.BonusProductId = request.BonusProductId;
        //        existingRecord.DurationInDays = request.DurationInDays;
        //        existingRecord.ModifiedBy = request.CreatedBy;
        //        existingRecord.ModifiedOn = DateTime.Now;

        //        _dbContext.Update(existingRecord);
        //        await _dbContext.SaveChangesAsync();

        //        // Fetch the updated product details
        //        var product = await _dbContext.ProductsM
        //            .Where(p => p.Id == request.ProductId)
        //            .Select(p => new { p.Name })
        //            .FirstOrDefaultAsync();

        //        var bonusProduct = await _dbContext.ProductsM
        //            .Where(p => p.Id == request.BonusProductId)
        //            .Select(p => new { p.Name })
        //            .FirstOrDefaultAsync();

        //        // Fetch the user details using ModifiedBy
        //        var user = await _dbContext.Users
        //            .Where(u => u.Id == request.CreatedBy)
        //            .Select(u => new { u.FirstName, u.LastName })
        //            .FirstOrDefaultAsync();

        //        // Construct response data
        //        var response = new
        //        {
        //            Id = existingRecord.Id,
        //            BonusProductId = existingRecord.BonusProductId,
        //            ModifiedBy = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
        //            ModifiedOn = existingRecord.ModifiedOn,
        //            ProductId = existingRecord.ProductId,
        //            ProductName = product?.Name ?? "Unknown Product",
        //            BonusProductName = bonusProduct?.Name ?? "Unknown Bonus Product",
        //            IsActive = existingRecord.IsActive,
        //            IsDeleted = existingRecord.IsDeleted,
        //            DurationInDays = existingRecord.DurationInDays
        //        };

        //        return new ApiCommonResponseModel
        //        {
        //            StatusCode = HttpStatusCode.OK,
        //            Message = "Bonus Product Mapping updated successfully.",
        //            Exceptions = null,
        //            Data = response
        //        };
        //    }

        //    else
        //    {
        //        var item = await _dbContext.ProductBonusMappingM.FirstOrDefaultAsync(c => c.Id == request.Id);

        //        if (item == null)
        //        {
        //            return new ApiCommonResponseModel
        //            {
        //                StatusCode = HttpStatusCode.InternalServerError,
        //                Message = "Bonus Product MappingM details not found.",
        //                Data = null
        //            };
        //        }

        //        // Toggle the IsActive status
        //        item.IsActive = !item.IsActive;
        //        item.ModifiedBy = request.CreatedBy;
        //        item.ModifiedOn = DateTime.Now;

        //        _dbContext.Update(item);
        //        await _dbContext.SaveChangesAsync();

        //        return new ApiCommonResponseModel
        //        {
        //            StatusCode = HttpStatusCode.OK,
        //            Message = "Updated Bonus Product MappingM Satus successfully.",
        //            Exceptions = null
        //        };
        //    }
        //}

        public async Task<ApiCommonResponseModel> CreateBonusProductMapping(BonusProductMappingRequestModel request)
        {
            if (request.ProductId == request.BonusProductId)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.Conflict,
                    Message = "A product cannot be its own bonus product."
                };
            }

            var duplicateMapping = await _dbContext.ProductBonusMappingM
                .FirstOrDefaultAsync(c => !c.IsDeleted && c.ProductId == request.ProductId);

            if (duplicateMapping != null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.Conflict,
                    Message = "This Product is already mapped to another bonus product."
                };
            }

            var bonusProductMapped = await _dbContext.ProductBonusMappingM
                .FirstOrDefaultAsync(c => c.IsActive && !c.IsDeleted && c.BonusProductId == request.BonusProductId);

            if (bonusProductMapped != null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.Conflict,
                    Message = "This Bonus Product is already bound to another product."
                };
            }

            var existingSubscriptionMapping = await _dbContext.SubscriptionMappingM
                .FirstOrDefaultAsync(m => m.ProductId == request.ProductId);

            if (existingSubscriptionMapping == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotAcceptable,
                    Message = "This product does not have a subscription mapping."
                };
            }

            var productCommunity = new ProductBonusMappingM
            {
                ProductId = request.ProductId,
                BonusProductId = request.BonusProductId,
                DurationInDays = request.DurationInDays,
                CreatedBy = request.CreatedBy,
                ModifiedBy = request.CreatedBy,
                CreatedOn = DateTime.Now,
                ModifiedOn = DateTime.Now,
                IsActive = true,
                IsDeleted = false
            };

            _dbContext.Add(productCommunity);
            await _dbContext.SaveChangesAsync();

            //Fetch the product name using the ProductId
            var product = await _dbContext.ProductsM
                .Where(p => p.Id == request.ProductId)
                .Select(p => new { p.Name })
                .FirstOrDefaultAsync();

            var bonusProduct = await _dbContext.ProductsM
                .Where(p => p.Id == request.BonusProductId)
                .Select(p => new { p.Name })
                .FirstOrDefaultAsync();

            // Fetch the user details using CreatedBy
            var user = await _dbContext.Users
                .Where(u => u.Id == request.CreatedBy)
                .Select(u => new { u.FirstName, u.LastName })
                .FirstOrDefaultAsync();

            // Construct response data
            var response = new
            {
                Id = productCommunity.Id,
                BonusProductId = productCommunity.BonusProductId,
                CreatedBy = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
                ModifiedBy = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
                CreatedOn = productCommunity.CreatedOn,
                ModifiedOn = productCommunity.ModifiedOn,
                ProductId = productCommunity.ProductId,
                ProductName = product?.Name ?? "Unknown Product",
                BonusProductName = bonusProduct?.Name ?? "Unknown Bonus Product",
                IsActive = productCommunity.IsActive,
                IsDeleted = productCommunity.IsDeleted,
                DurationInDays = productCommunity.DurationInDays
            };

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Created Bonus Product MappingM successfully.",
                Exceptions = null,
                Data = response
            };
        }

        public async Task<ApiCommonResponseModel> UpdateBonusProductMapping(BonusProductMappingRequestModel request)
        {
            var existing = await _dbContext.ProductBonusMappingM.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (existing == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Bonus Product Mapping not found.",
                    Exceptions = null,
                };
            }

            if (existing.ProductId != request.ProductId)
            {
                var duplicate = await _dbContext.ProductBonusMappingM
                    .AnyAsync(x => x.ProductId == request.ProductId && x.Id != request.Id && x.IsActive && !x.IsDeleted);
                if (duplicate)
                {
                    return new ApiCommonResponseModel
                    {
                        StatusCode = HttpStatusCode.Conflict,
                        Message = "This Product is already mapped to another Bonus Product.",
                        Exceptions = null,
                    };
                }
            }

            if (existing.BonusProductId != request.BonusProductId)
            {
                var duplicate = await _dbContext.ProductBonusMappingM
                    .AnyAsync(x => x.BonusProductId == request.BonusProductId && x.Id != request.Id && x.IsActive && !x.IsDeleted);
                if (duplicate)
                {
                    return new ApiCommonResponseModel
                    {
                        StatusCode = HttpStatusCode.Conflict,
                        Message = "This Bonus Product is already bound to another Product.",
                        Exceptions = null,
                    };
                }
            }

            existing.ProductId = request.ProductId;
            existing.BonusProductId = request.BonusProductId;
            existing.DurationInDays = request.DurationInDays;
            existing.ModifiedBy = request.CreatedBy;
            existing.ModifiedOn = DateTime.Now;

            _dbContext.Update(existing);
            await _dbContext.SaveChangesAsync();

            // Fetch the updated product details
            var product = await _dbContext.ProductsM
                .Where(p => p.Id == request.ProductId)
                .Select(p => new { p.Name })
                .FirstOrDefaultAsync();

            var bonusProduct = await _dbContext.ProductsM
                .Where(p => p.Id == request.BonusProductId)
                .Select(p => new { p.Name })
                .FirstOrDefaultAsync();

            // Fetch the user details using ModifiedBy
            var user = await _dbContext.Users
                .Where(u => u.Id == request.CreatedBy)
                .Select(u => new { u.FirstName, u.LastName })
                .FirstOrDefaultAsync();

            // Construct response data
            var response = new
            {
                Id = existing.Id,
                BonusProductId = existing.BonusProductId,
                ModifiedBy = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
                ModifiedOn = existing.ModifiedOn,
                ProductId = existing.ProductId,
                ProductName = product?.Name ?? "Unknown Product",
                BonusProductName = bonusProduct?.Name ?? "Unknown Bonus Product",
                IsActive = existing.IsActive,
                IsDeleted = existing.IsDeleted,
                DurationInDays = existing.DurationInDays
            };

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Bonus Product Mapping updated successfully.",
                Exceptions = null,
                Data = response
            };
        }

        public async Task<ApiCommonResponseModel> ToggleBonusProductMappingStatus(int id, int modifiedBy)
        {
            var item = await _dbContext.ProductBonusMappingM.FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Bonus Product Mapping not found."
                };
            }

            item.IsActive = !item.IsActive;
            item.ModifiedBy = modifiedBy;
            item.ModifiedOn = DateTime.Now;

            _dbContext.Update(item);
            await _dbContext.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Bonus Product Mapping status Updated successfully.",
                Exceptions = null,
            };
        }

        public async Task<ApiCommonResponseModel> DeleteBonusProductMapping(int id, int modifiedBy)
        {
            // Validate the input object
            if (id == 0)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Id Not Found In Bonus Product MappingM Database Table.",
                    Data = null
                };
            }

            // Find the CompanyDetailM by ID
            var item = await _dbContext.ProductBonusMappingM.FirstOrDefaultAsync(b => b.Id == id);

            // Check if the result is null
            if (item == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Bonus Product MappingM Details not found.",
                    Data = null
                };
            }

            // Mark the item as deleted and inactive
            item.IsActive = false;
            item.IsDeleted = true;
            item.ModifiedOn = DateTime.Now;
            item.ModifiedBy = modifiedBy;
            await _dbContext.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Bonus Product MappingM Deleted successfully.",
                Exceptions = null
            };
        }

        public async Task<ApiCommonResponseModel> GetLogsAsync(QueryValues query)
        {
            var fromDate = query.FromDate.Value.Date; // Start of the day (00:00:00)
            var toDate = query.ToDate.Value.Date.AddDays(1).AddMilliseconds(-1); // End of the day (23:59:59.999)
            var filters = new List<FilterDefinition<Log>>();

            // Date Range Filter
            filters.Add(Builders<Log>.Filter.Gte(log => log.CreatedOn, fromDate));
            filters.Add(Builders<Log>.Filter.Lte(log => log.CreatedOn, toDate));

            if (!string.IsNullOrEmpty(query.SearchText))
            {
                filters.Add(Builders<Log>.Filter.Regex(log => log.Message, new BsonRegularExpression($".*{query.SearchText}.*", "i")));
            }

            if (query.PrimaryKey != null)
            {
                filters.Add(Builders<Log>.Filter.Regex(log => log.Source, query.PrimaryKey));
            }

            var finalFilter = Builders<Log>.Filter.And(filters);


            var (allLogs, totalCount) = await _log.GetPaginatedAsyncWithTotalCount(
                filter: finalFilter,  // Filter logs from the last 7 days
                sortBy: log => log.CreatedOn,              // Sort by CreatedOn field
                pageNumber: query.PageNumber,
                pageSize: query.PageSize,
                isDescending: true // Sort in descending order
            );

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Logs retrieved successfully",
                Data = allLogs,
                Total = (int)totalCount
            };
        }

        public async Task<ApiCommonResponseModel> GetExceptionsAsync(QueryValues query)
        {
            var fromDate = query.FromDate.Value.Date; // Start of the day (00:00:00)
            var toDate = query.ToDate.Value.Date.AddDays(1).AddMilliseconds(-1); // End of the day (23:59:59.999)

            var filters = new List<FilterDefinition<ExceptionLog>>
            {
                // Date Range Filter
                Builders<ExceptionLog>.Filter.Gte(log => log.CreatedOn, fromDate),
                Builders<ExceptionLog>.Filter.Lte(log => log.CreatedOn, toDate),
             };

            if (!string.IsNullOrEmpty(query.SearchText))
            {
                //filters.Add(Builders<ExceptionLog>.Filter.Regex(log => log.Message, new BsonRegularExpression(query.SearchText)));//working but searching complete word from the string like IDX14100 not idx
                filters.Add(Builders<ExceptionLog>.Filter.Regex(log => log.Message, new BsonRegularExpression($".*{query.SearchText}.*", "i")));
            }
            var finalFilter = Builders<ExceptionLog>.Filter.And(filters);


            var (allLogs, totalCount) = await _exceptionLog.GetPaginatedAsyncWithTotalCount(
                filter: finalFilter,  // Filter logs from the last 7 days
                sortBy: log => log.CreatedOn,              // Sort by CreatedOn field
                pageNumber: query.PageNumber,
                pageSize: query.PageSize,
                isDescending: true // Sort in descending order
            );


            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Logs retrieved successfully",
                Data = allLogs,
                Total = (int)totalCount
            };
        }
        public async Task<ApiCommonResponseModel> TotalLogCount()
        {
            var fromDate = new DateTime(2000, 1, 1);
            var toDate = DateTime.Today.AddDays(1).AddTicks(-1);
            var filters = new List<FilterDefinition<Log>>
            {
                // Date Range Filter
                Builders<Log>.Filter.Gte(log => log.CreatedOn, fromDate),
                Builders<Log>.Filter.Lte(log => log.CreatedOn, toDate),
             };
            var finalFilter = Builders<Log>.Filter.And(filters);
            var TotalCount = await _log.GetTotalCounts(finalFilter);
            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Total count of the logs retrieved",
                Total = (int)TotalCount
            };
        }

        public async Task<ApiCommonResponseModel> TotalExceptionCount()
        {
            var fromDate = new DateTime(2000, 1, 1);
            var toDate = DateTime.Today.AddDays(1).AddTicks(-1);
            var filters = new List<FilterDefinition<ExceptionLog>>
            {
                // Date Range Filter
                Builders<ExceptionLog>.Filter.Gte(log => log.CreatedOn, fromDate),
                Builders<ExceptionLog>.Filter.Lte(log => log.CreatedOn, toDate),
             };
            var finalFilter = Builders<ExceptionLog>.Filter.And(filters);
            var TotalCount = await _exceptionLog.GetTotalCounts(finalFilter);
            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Total count of the logs retrieved",
                Total = (int)TotalCount
            };
        }

        public async Task<ApiCommonResponseModel> DeleteLogByIdAsync(string id)
        {
            await _log.DeleteAsync(id);
            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Delete successfully",
            };
        }

        public async Task<ApiCommonResponseModel> DeleteExceptionByIdAsync(string id)
        {
            await _exceptionLog.DeleteAsync(id);
            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Delete successfully",
            };
        }

        public async Task<ApiCommonResponseModel> DeleteLogsInBulk(QueryValues query)
        {
            var fromDate = query.FromDate.Value.Date; // Start of the day (00:00:00)
            var toDate = query.ToDate.Value.Date.AddDays(1).AddMilliseconds(-1); // End of the day (23:59:59.999)

            await _log.DeleteInBulkAsync(log => log.CreatedOn >= fromDate
                                                && log.CreatedOn <= toDate
                                                    && (log.Source == query.PrimaryKey || query.PrimaryKey == ""));
            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Delete successfully",
            };
        }

        public async Task<ApiCommonResponseModel> DeleteExceptionsInBulk(QueryValues query)
        {
            if (!query.FromDate.HasValue || !query.ToDate.HasValue)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "FromDate and ToDate are required."
                };
            }

            var fromDate = query.FromDate.Value.Date;
            var toDate = query.ToDate.Value.Date.AddDays(1).AddMilliseconds(-1);

            await _exceptionLog.DeleteInBulkAsync(log => log.CreatedOn >= fromDate && log.CreatedOn <= toDate);

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Delete successfully",
            };
        }


        //public async Task<ApiCommonResponseModel> CreateChapterSubChapter(SubChapterRequestModel request)
        //{
        //    if (request.Action == "Add")
        //    {
        //        var addSubChapter = new SubChapter
        //        {
        //            ChapterId = request.ChapterId,
        //            Description = request.Description,
        //            IsVisible = true,
        //            Language = request.Language,
        //            Link = request.Link,
        //            VideoDuration = request.VideoDuration,
        //            Title = request.Title,
        //            CreatedBy = request.CreatedBy,
        //            CreatedOn = DateTime.Now,
        //            ModifiedBy = request.CreatedBy,
        //            ModifiedOn = DateTime.Now,
        //            IsActive = request.IsActive
        //        };

        //        _dbContext.Add(addSubChapter);
        //        await _dbContext.SaveChangesAsync();

        //        return new ApiCommonResponseModel
        //        {
        //            StatusCode = HttpStatusCode.OK,
        //            Message = "Sub Chapter Created successfully.",
        //            Exceptions = null,
        //            Data = addSubChapter
        //        };
        //    }

        //    else if (request.Action == "Edit")
        //    {
        //        var existingRecord = await _dbContext.Set<SubChapter>()
        //            .FirstOrDefaultAsync(c => c.Id == request.Id);

        //        if (existingRecord == null)
        //        {
        //            return new ApiCommonResponseModel
        //            {
        //                StatusCode = HttpStatusCode.NotFound,
        //                Message = "Record not found.",
        //                Data = null
        //            };
        //        }

        //        // Update only if changes are detected
        //        existingRecord.ChapterId = request.ChapterId;
        //        existingRecord.VideoDuration = request.VideoDuration;
        //        existingRecord.Description = request.Description;
        //        existingRecord.IsActive = request.IsActive;
        //        existingRecord.Link = request.Link;
        //        existingRecord.Language = request.Language;
        //        existingRecord.Title = request.Title;
        //        existingRecord.ModifiedBy = request.CreatedBy;
        //        existingRecord.ModifiedOn = DateTime.Now;

        //        _dbContext.Update(existingRecord);
        //        await _dbContext.SaveChangesAsync();

        //        return new ApiCommonResponseModel
        //        {
        //            StatusCode = HttpStatusCode.OK,
        //            Message = "Chapter updated successfully.",
        //            Exceptions = null,
        //            Data = existingRecord
        //        };
        //    }

        //    else if (request.Action == "EditC")
        //    {
        //        var existingRecord = await _dbContext.Set<Chapter>()
        //            .FirstOrDefaultAsync(c => c.Id == request.Id);

        //        if (existingRecord == null)
        //        {
        //            return new ApiCommonResponseModel
        //            {
        //                StatusCode = HttpStatusCode.NotFound,
        //                Message = "Record not found.",
        //                Data = null
        //            };
        //        }

        //        // Update only if changes are detected
        //        existingRecord.Description = request.Description;
        //        existingRecord.IsActive = request.IsActive;
        //        existingRecord.ChapterTitle = request.Title;
        //        existingRecord.ModifiedBy = request.CreatedBy;
        //        existingRecord.ModifiedOn = DateTime.Now;

        //        _dbContext.Update(existingRecord);
        //        await _dbContext.SaveChangesAsync();

        //        return new ApiCommonResponseModel
        //        {
        //            StatusCode = HttpStatusCode.OK,
        //            Message = "Sub Chapter updated successfully.",
        //            Exceptions = null,
        //            Data = existingRecord
        //        };
        //    }

        //    else
        //    {
        //        var addChapter = new Chapter
        //        {
        //            ProductId = request.ProductId,
        //            Description = request.Description,
        //            ChapterTitle = request.Title,
        //            CreatedBy = request.CreatedBy,
        //            CreatedOn = DateTime.Now,
        //            ModifiedBy = request.CreatedBy,
        //            ModifiedOn = DateTime.Now,
        //            IsActive = request.IsActive,
        //            IsDelete = false
        //        };

        //        _dbContext.Add(addChapter);
        //        await _dbContext.SaveChangesAsync();

        //        return new ApiCommonResponseModel
        //        {
        //            StatusCode = HttpStatusCode.OK,
        //            Message = "Sub Chapter Created successfully.",
        //            Exceptions = null,
        //            Data = addChapter
        //        };
        //    }
        //}

        public async Task<ApiCommonResponseModel> AddSubChapter(SubChapterRequestModel request)
        {
            var newSubChapter = new SubChapter
            {
                ChapterId = request.ChapterId,
                Description = request.Description,
                IsVisible = true,
                Language = request.Language,
                Link = request.Link,
                VideoDuration = request.VideoDuration,
                Title = request.Title,
                CreatedBy = request.CreatedBy,
                CreatedOn = DateTime.Now,
                ModifiedBy = request.CreatedBy,
                ModifiedOn = DateTime.Now,
                IsActive = request.IsActive
            };

            _dbContext.Add(newSubChapter);
            await _dbContext.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Sub Chapter Created successfully.",
                Data = newSubChapter
            };
        }

        public async Task<ApiCommonResponseModel> EditSubChapter(SubChapterRequestModel request)
        {
            var existingRecord = await _dbContext.Set<SubChapter>()
                .FirstOrDefaultAsync(c => c.Id == request.Id);

            if (existingRecord == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Sub Chapter not found.",
                    Data = null
                };
            }

            existingRecord.ChapterId = request.ChapterId;
            existingRecord.VideoDuration = request.VideoDuration;
            existingRecord.Description = request.Description;
            existingRecord.IsActive = request.IsActive;
            existingRecord.Link = request.Link;
            existingRecord.Language = request.Language;
            existingRecord.Title = request.Title;
            existingRecord.ModifiedBy = request.CreatedBy;
            existingRecord.ModifiedOn = DateTime.Now;

            _dbContext.Update(existingRecord);
            await _dbContext.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Sub Chapter updated successfully.",
                Data = existingRecord
            };
        }

        public async Task<ApiCommonResponseModel> EditChapter(SubChapterRequestModel request)
        {
            var existingRecord = await _dbContext.Set<Chapter>()
                .FirstOrDefaultAsync(c => c.Id == request.Id);

            if (existingRecord == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Chapter not found.",
                    Data = null
                };
            }

            existingRecord.Description = request.Description;
            existingRecord.IsActive = request.IsActive;
            existingRecord.ChapterTitle = request.Title;
            existingRecord.ModifiedBy = request.CreatedBy;
            existingRecord.ModifiedOn = DateTime.Now;

            _dbContext.Update(existingRecord);
            await _dbContext.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Chapter updated successfully.",
                Data = existingRecord
            };
        }

        public async Task<ApiCommonResponseModel> AddChapter(SubChapterRequestModel request)
        {
            var newChapter = new Chapter
            {
                ProductId = request.ProductId,
                Description = request.Description,
                ChapterTitle = request.Title,
                CreatedBy = request.CreatedBy,
                CreatedOn = DateTime.Now,
                ModifiedBy = request.CreatedBy,
                ModifiedOn = DateTime.Now,
                IsActive = request.IsActive,
                IsDelete = false
            };

            _dbContext.Add(newChapter);
            await _dbContext.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Chapter Created successfully.",
                Data = newChapter
            };
        }

        public async Task<ApiCommonResponseModel> DeleteChapter(int id, int modifiedBy)
        {
            // Validate the input object
            if (id == 0)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Invalid Chapter ID.",
                    Data = null
                };
            }

            // Find the Chapter by ID
            var item = await _dbContext.Chapter.FirstOrDefaultAsync(b => b.Id == id);

            // Check if the chapter exists
            if (item == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Chapter not found.",
                    Data = null
                };
            }

            // Mark the chapter as deleted and inactive
            item.IsActive = false;
            item.IsDelete = true;
            item.ModifiedOn = DateTime.Now;
            item.ModifiedBy = modifiedBy;

            await _dbContext.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Chapter deleted successfully.",
                Exceptions = null
            };
        }

        public async Task<ApiCommonResponseModel> DeleteSubChapter(int id, int modifiedBy)
        {
            // Validate the input object
            if (id == 0)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Id Not Found In Chapter Database Table.",
                    Data = null
                };
            }

            // Find the CompanyDetailM by ID
            var item = await _dbContext.SubChapter.FirstOrDefaultAsync(b => b.Id == id);

            // Check if the result is null
            if (item == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Product Community MappingM Details not found.",
                    Data = null
                };
            }

            // Mark the item as deleted and inactive
            item.IsActive = false;
            item.IsDelete = true;
            item.ModifiedOn = DateTime.Now;
            item.ModifiedBy = modifiedBy;
            await _dbContext.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Chapter Deleted successfully.",
                Exceptions = null
            };
        }

        public async Task<ApiCommonResponseModel> UpdateChapterStatus(UpdateChapterStatusRequestModel request)
        {
            var responseModel = new ApiCommonResponseModel();

            if (request.Id <= 0)
            {
                responseModel.StatusCode = HttpStatusCode.BadRequest;
                return responseModel;
            }

            var chapter = await _dbContext.Chapter.FirstOrDefaultAsync(x => x.Id == request.Id);

            if (chapter == null)
            {
                responseModel.StatusCode = HttpStatusCode.NotFound;
                return responseModel;
            }

            chapter.IsActive = request.IsActive;
            chapter.ModifiedOn = DateTime.Now;
            chapter.ModifiedBy = request.ModifiedBy;

            await _dbContext.SaveChangesAsync();

            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> UpdateSubChapterStatus(UpdateChapterStatusRequestModel request)
        {
            var responseModel = new ApiCommonResponseModel();

            if (request.Id <= 0)
            {
                responseModel.StatusCode = HttpStatusCode.BadRequest;
                return responseModel;
            }

            var subChapter = await _dbContext.SubChapter.FirstOrDefaultAsync(x => x.Id == request.Id);

            if (subChapter == null)
            {
                responseModel.StatusCode = HttpStatusCode.NotFound;
                return responseModel;
            }

            subChapter.IsActive = request.IsActive;
            subChapter.ModifiedOn = DateTime.Now;
            subChapter.ModifiedBy = request.ModifiedBy;

            await _dbContext.SaveChangesAsync();

            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetQueryForms(QueryValues queryValues)
        {
            var apiCommonResponse = new ApiCommonResponseModel();

            try
            {

                // Define SQL parameters for stored procedure
                List<SqlParameter> sqlParameters = new List<SqlParameter>
                {
                    new SqlParameter("@PageNumber", queryValues.PageNumber > 0 ? queryValues.PageNumber : 1) { SqlDbType = SqlDbType.Int },
                    new SqlParameter("@PageSize", queryValues.PageSize > 0 ? queryValues.PageSize : 20) { SqlDbType = SqlDbType.Int },
                    new SqlParameter("@PrimaryKey", string.IsNullOrEmpty(queryValues.PrimaryKey) ? DBNull.Value : queryValues.PrimaryKey) { SqlDbType = SqlDbType.NVarChar },
                    new SqlParameter("@FromDate", queryValues.FromDate.HasValue ? (object)queryValues.FromDate.Value.Date : DBNull.Value) { SqlDbType = SqlDbType.DateTime },
                    new SqlParameter("@ToDate", queryValues.ToDate.HasValue ? (object)queryValues.ToDate.Value.Date : DBNull.Value) { SqlDbType = SqlDbType.DateTime },
                    new SqlParameter("@SearchText", string.IsNullOrEmpty(queryValues.SearchText) ? DBNull.Value : queryValues.SearchText) { SqlDbType = SqlDbType.NVarChar }
                };

                SqlParameter parameterOutValue = new SqlParameter
                {
                    ParameterName = "@TotalCount",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };

                sqlParameters.Add(parameterOutValue);

                var categoryLookup = new Dictionary<int, string>
                {
                    { 1, "Entry Criteria" },
                    { 2, "Exit Strategy" },
                    { 3, "Risk Management" },
                    { 4, "Live Market" },
                    { 5, "Confusion" },
                    { 6, "Other Issues" }
                };

                // Execute stored procedure and get data as a list of QueryFormResponseModel
                List<QueryFormResponseModel> queryForms = await _dbContext.SqlQueryToListAsync<QueryFormResponseModel>(
                 "EXEC GetQueryForms  @PageSize, @PageNumber, @PrimaryKey, @FromDate, @ToDate, @SearchText, @TotalCount OUTPUT",
                 sqlParameters.ToArray()
                );

                // Using LINQ to map the category name without foreach
                var mappedQueryForms = queryForms.Select(item => new QueryFormResponseModel
                {
                    Id = item.Id,
                    MobileUserId = item.MobileUserId,
                    ProductId = item.ProductId,
                    Name = item.Name,
                    ProductName = item.ProductName,
                    Questions = item.Questions,
                    ScreenshotUrl = item.ScreenshotUrl,
                    QueryRelatedTo = item.QueryRelatedTo,
                    QueryRelatedToName = categoryLookup.TryGetValue(item.QueryRelatedTo, out var name) ? name : "Unknown", // Direct mapping here present. hardcode.
                    CreatedOn = item.CreatedOn,
                    CreatedBy = item.CreatedBy,
                    ModifiedOn = item.ModifiedOn,
                    ModifiedBy = item.ModifiedBy,
                    RemarksCount = item.RemarksCount,
                    Mobile = item.Mobile,
                    IsActive = item.IsActive
                }).ToList();

                int totalCount = parameterOutValue.Value != DBNull.Value ? Convert.ToInt32(parameterOutValue.Value) : 0;

                // Populate the response
                apiCommonResponse.Data = mappedQueryForms;
                apiCommonResponse.Total = totalCount;
                apiCommonResponse.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                apiCommonResponse.Data = null;
                apiCommonResponse.StatusCode = HttpStatusCode.InternalServerError;
                apiCommonResponse.Message = $"An error occurred while fetching query forms: {ex.Message}";
            }

            return apiCommonResponse;
        }


        public async Task<ApiCommonResponseModel> UpdateQueryFormAsync(QueryFormRequestModel request)
        {
            if (!int.TryParse(request.ProductId, out int productId))
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Invalid ProductId format.",
                    Data = null,
                    Exceptions = null
                };
            }

            var product = await _dbContext.ProductsM
                .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive == true);

            if (product == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Invalid ProductId provided or InActive Product Provider...",
                    Data = null,
                    Exceptions = null
                };
            }

            // Get existing remark to fetch QueryId
            var query = await _dbContext.QueryFormM.FirstOrDefaultAsync(q => q.Id == request.Id);

            if (query == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Query not found."
                };
            }

            // Update ModifiedBy and ModifiedOn for main query
            if (query != null)
            {
                query.IsActive = request.Status == 1; // bool to int
                query.ModifiedBy = request.CreatedBy;
                query.ModifiedOn = DateTime.Now;
            }

            // Add new remark if provided
            if (!string.IsNullOrWhiteSpace(request.Remarks))
            {
                await _dbContext.QueryFormRemarks.AddAsync(new QueryFormRemarks
                {
                    QueryId = request.Id,
                    Remarks = request.Remarks,
                    CreatedBy = request.CreatedBy,
                    CreatedOn = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false
                });
            }

            await _dbContext.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Query updated successfully.",
                Data = query
            };
        }

        public async Task<ApiCommonResponseModel> GetQueryFormRemarks(int queryFormId)
        {
            var responseModel = new ApiCommonResponseModel();

            // Fetch query form remarks along with user details
            var result = await (from qfr in _dbContext.QueryFormRemarks
                                join qf in _dbContext.QueryFormM on qfr.QueryId equals qf.Id
                                join mu in _dbContext.MobileUsers on qf.MobileUserId equals mu.PublicKey
                                join u in _dbContext.Users on qf.ModifiedBy equals u.Id
                                join p in _dbContext.ProductsM on qf.ProductId equals p.Id
                                where qfr.QueryId == queryFormId && qfr.IsDeleted == false
                                select new
                                {
                                    qfr.Id,
                                    qf.Questions,
                                    qf.QueryRelatedTo,
                                    qfr.QueryId,
                                    p.Name,
                                    mu.Mobile,
                                    qfr.Remarks,
                                    qfr.CreatedOn,
                                    UserName = mu.FullName,
                                    CreatedBy = u.FirstName + " " + u.LastName
                                }).ToListAsync();


            // Check if no remarks are found
            if (result.Count == 0)
            {
                Console.WriteLine("No remarks found for this query form.");
            }

            // Populate the response model with the results
            responseModel.Data = result;

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> DeleteQueryForm(int id, int modifiedBy)
        {
            // Validate the input object
            if (id == 0)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Id Not Found In Query Form Database Table.",
                    Data = null
                };
            }

            // Find the CompanyDetailM by ID
            var item = await _dbContext.QueryFormM.FirstOrDefaultAsync(b => b.Id == id);

            // Check if the result is null
            if (item == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Query Form Details not found.",
                    Data = null
                };
            }

            // Mark the item as deleted and inactive
            item.IsActive = false;
            item.IsDeleted = true;
            item.ModifiedOn = DateTime.Now;
            item.ModifiedBy = modifiedBy;
            await _dbContext.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Query Form Deleted successfully.",
                Exceptions = null
            };
        }

        public async Task<ApiCommonResponseModel> GetMobileUserBucketDetails(QueryValues queryValues)
        {
            var response = new ApiCommonResponseModel();

            var sqlParameters = ProcedureCommonSqlParameters.GetCommonSqlParameters(queryValues);

            var daysToGo = new SqlParameter
            {
                ParameterName = "DaysToGo",
                Value = queryValues.DaysToGo == null ? DBNull.Value : queryValues.DaysToGo,
                SqlDbType = SqlDbType.Int,
            };
            var productId = new SqlParameter
            {
                ParameterName = "ProductId",
                Value = queryValues.ProductId == null ? DBNull.Value : queryValues.ProductId,
                SqlDbType = SqlDbType.Int,
            };
            var outputParam = new SqlParameter
            {
                ParameterName = "@TotalCount",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };
            sqlParameters.Add(daysToGo);
            sqlParameters.Add(productId);
            sqlParameters.Add(outputParam);

            var data = await _dbContext.SqlQueryToListAsync<MobileUserBucketResponseModel>(
                "EXEC dbo.GetMobileUserBucketDetails @IsPaging, @PageSize, @PageNumber, @SearchText, @DaysToGo, @ProductId, @TotalCount OUTPUT",
                sqlParameters.ToArray()
            );

            response.Data = data;
            response.StatusCode = HttpStatusCode.OK;
            response.Total = Convert.ToInt32(outputParam.Value);

            return response;
        }

        public Task<string> GetImageAspectRatio(IFormFile imageFile)
        {
            using var stream = imageFile.OpenReadStream();
            using var image = Image.FromStream(stream); // using System.Drawing;
            decimal ratio = (decimal)image.Width / image.Height;
            return Task.FromResult(ratio.ToString("0.00"));
        }

    }
}