using RM.BlobStorage;
using RM.CommonServices.Services;
using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.KingResearchContext;
using RM.Model;
using RM.Model.RequestModel;
using RM.Model.RequestModel.MobileApi;
using RM.Model.ResponseModel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Net;
using System.Text.Json;

namespace RM.MService.Services
{
    public interface IProductService
    {
        Task<ApiCommonResponseModel> GetProducts(string searchText, string userKey);

        Task<ApiCommonResponseModel> GetProductById(string id, string mobileUserKey);

        Task<ApiCommonResponseModel> LikeUnlikeProduct(LikeProductModel likedByRequest);

        Task<ApiCommonResponseModel> MyBucketContent(Guid userKey);

        Task<ApiCommonResponseModel> RateProduct(string userKey, int productId, float rating);

        Task<ApiCommonResponseModel> GetTopGainers(string type);

        Task<ApiCommonResponseModel> GetProductCategories();

        Task<ApiCommonResponseModel> GetProductContent(string userkey, int id);

        Task<ApiCommonResponseModel> GetProductContentV2(string userkey, int id);

        Task<ApiCommonResponseModel> GetLearningMaterial(Guid? MobileUserKey);

        Task<ApiCommonResponseModel> GetLearningContentList(GetLearningContentList request);

        Task<ApiCommonResponseModel> GetLearningContentById(int contentId, Guid mobileUserKey);

        Task<ApiCommonResponseModel> GetLearningContentExamples(int contentId);

        Task<ApiCommonResponseModel> LikeUnlikeLearningContent(LikeUnlikeLearningContentRequestModel request);

        Task<ApiCommonResponseModel> GetCoupons(Guid? userKey, int productId, int subscriptionDurationId);
        Task<ApiCommonResponseModel> ActiveProductCount();
        Task<ApiCommonResponseModel> ActiveProductStatus(Guid userKey);
        Task<ApiCommonResponseModel> GetPlayList(long mobileUserId, int productId);
        Task<ApiCommonResponseModel> AddQueryFormAsync(QueryFormRequestModel request);
    }

    public class ProductService : IProductService
    {
        private readonly KingResearchContext _context;
        private readonly IMemoryCache _cache;
        private readonly MongoDbService _mongoService;
        private ApiCommonResponseModel responseModel = new();
        private readonly IConfiguration _config;
        private readonly IAzureBlobStorageService _azureBlobStorageService;

        public ProductService(KingResearchContext context, IMemoryCache cache, MongoDbService mongoService,
            IConfiguration config, IAzureBlobStorageService azureBlobStorageService)
        {
            _context = context;
            _cache = cache;
            _mongoService = mongoService;
            _config = config;
            _azureBlobStorageService = azureBlobStorageService;
        }

        public async Task<ApiCommonResponseModel> GetLearningContentList(GetLearningContentList request)
        {
            responseModel.Data = await _context.LearningContentM
                .Where(x => x.MaterialId == request.Id && x.IsActive)
                .Select(lc => new
                {
                    lc.Id,
                    Name = lc.Title,
                    ImageUrl = lc.ListImageUrl,
                    LikeCount = _context.LearningContentLikesM.Where(x => x.IsLiked)
                        .Count(l => l.LearningContentId == lc.Id),
                    IsLiked = _context.LearningContentLikesM
                        .Any(x => x.LearningContentId == lc.Id && x.UserKey == request.UserKey && x.IsLiked)
                })
                .ToListAsync();

            responseModel.StatusCode = HttpStatusCode.OK;

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetLearningContentById(int contentId, Guid mobileUserKey)
        {
            bool isIndicator = await _context.LearningMaterialM
                .Join(_context.LearningContentM,
                    lm => lm.Id,
                    lc => lc.MaterialId,
                    (lm, lc) => new { LearningMaterial = lm, LearningContent = lc })
                .AnyAsync(x => x.LearningMaterial.Id == 3 && x.LearningContent.Id == contentId);

            // _ = await _mongoService.SaveUserActivity(Model.MongoDbCollection.UserActivityEnum.ContentClicked, mobileUserKey, contentId);

            if (isIndicator)
            {
                responseModel.Data = await _context.LearningContentM
                    .Where(lc => lc.IsActive == true && lc.IsDelete == false && lc.Id == contentId)
                    .Select(lc => new
                    {
                        heading = lc.Title,
                        bindedData = lc.Description,
                        attachmemt = lc.Attachment,
                        attachmentDescription = lc.AttachmentDescription,
                        attachmentTitle = lc.AttachmentTitle,
                        data = _context.LearningContentExampleM.Where(lce =>
                                lce.ContentId == contentId && lce.IsActive == true && lce.IsDelete == false)
                            .Select(lce => new
                            {
                                id = lce.Id,
                                name = lce.Title,
                                imageUrl = lce.ImageUrl,
                            })
                            .ToList(),
                    }).FirstOrDefaultAsync();
            }
            else
            {
                responseModel.Data = await _context.LearningContentM
                    .Where(lc => lc.IsActive && lc.IsDelete == false && lc.Id == contentId)
                    .Select(lc => new
                    {
                        lc.Id,
                        lc.Title,
                        lc.Description,
                        lc.ThumbnailImageUrl,
                        lc.AttachmentDescription,
                        lc.AttachmentTitle,
                        lc.AttachmentType,
                        lc.Attachment
                    }).FirstOrDefaultAsync();
            }

            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Data Fetched Successfully.";
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetProductById(string id, string mobileUserkey)
        {
            try
            {
                //int ProductId = primaryKey,
                //Guid MobileUserKey = secondaryKey
                if (id.Length > 0 && mobileUserkey.Length > 0)
                {
                    int productId = int.Parse(id);
                    Guid mobileUserKey = Guid.Parse(mobileUserkey);

                    List<SqlParameter> sqlParameters = new()
                    {
                        new SqlParameter
                        {
                            ParameterName = "ProductId", Value = productId, SqlDbType = SqlDbType.Int
                        },
                        new SqlParameter
                        {
                            ParameterName = "MobileUserKey", Value = mobileUserKey,
                            SqlDbType = SqlDbType.UniqueIdentifier
                        }
                    };

                    var spResult =
                        await _context.SqlQueryFirstOrDefaultAsync2<ProductsByIdMResponseModel>(
                            ProcedureCommonSqlParametersText.GetProductById, sqlParameters.ToArray());
                    _ = await _mongoService.SaveUserActivity(Model.MongoDbCollection.UserActivityEnum.ProductClicked,
                        mobileUserKey, productId);

                    responseModel.Data = spResult;
                    responseModel.Message = "Data Fetched Succesfully";
                    responseModel.StatusCode = HttpStatusCode.OK;
                    return responseModel;
                }
                else
                {
                    responseModel.StatusCode = HttpStatusCode.BadRequest;
                    responseModel.Message = "Invalid Payload";
                    return responseModel;
                }
            }
            catch (Exception ex)
            {
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                responseModel.Message = ex.Message;
                return responseModel;
            }
        }

        public async Task<ApiCommonResponseModel> GetProductCategories()
        {
            responseModel.Data = await _context.ProductCategoriesM.Where(item => item.IsActive == true)
                .Select(item => new ProductCategoriesModel { Id = item.Id, Name = item.Name }).ToListAsync();
            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Data Fetched Successfully.";
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetProductContent(string userkey, int id)
        {
            SqlParameter IsInMyBucket = new()
            {
                ParameterName = "IsInMyBucket",
                SqlDbType = SqlDbType.Bit,
                Direction = ParameterDirection.Output,
            };

            SqlParameter IsInValidity = new()
            {
                ParameterName = "IsInValidity",
                SqlDbType = SqlDbType.Bit,
                Direction = ParameterDirection.Output,
            };

            List<SqlParameter> sqlParameters = new List<SqlParameter>()
            {
                new SqlParameter
                {
                    ParameterName = "Userkey",
                    Value = Guid.Parse(userkey),
                    SqlDbType = SqlDbType.UniqueIdentifier
                },
                new SqlParameter
                {
                    ParameterName = "ProductId",
                    Value = id,
                    SqlDbType = SqlDbType.Int,
                    Size = 100,
                },
                IsInMyBucket,
                IsInValidity
            };

            var spResult =
                await _context.SqlQueryToListAsync<GetProductContentMSpResponseModel>(
                    ProcedureCommonSqlParametersText.GetProductContentM, sqlParameters.ToArray());

            responseModel.Data = spResult;
            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Data Fetched Successfully.";
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetProductContentV2(string userkey, int id)
        {
            SqlParameter IsInMyBucket = new()
            {
                ParameterName = "IsInMyBucket",
                SqlDbType = SqlDbType.Bit,
                Direction = ParameterDirection.Output,
            };

            SqlParameter IsInValidity = new()
            {
                ParameterName = "IsInValidity",
                SqlDbType = SqlDbType.Bit,
                Direction = ParameterDirection.Output,
            };

            List<SqlParameter> sqlParameters = new List<SqlParameter>()
            {
                new SqlParameter
                {
                    ParameterName = "Userkey",
                    Value = Guid.Parse(userkey),
                    SqlDbType = SqlDbType.UniqueIdentifier
                },
                new SqlParameter
                {
                    ParameterName = "ProductId",
                    Value = id,
                    SqlDbType = SqlDbType.Int,
                    Size = 100,
                },
                IsInMyBucket,
                IsInValidity
            };

            var spResult = (await _context.SqlQueryToListAsync<GetProductContentMV2SpResponseModel>(
                 ProcedureCommonSqlParametersText.GetProductContentMV2, sqlParameters.ToArray()))
                 .Select(item =>
                 {
                     // Attach image URL prefix
                     if (!string.IsNullOrWhiteSpace(item.ThumbnailImage))
                         item.ThumbnailImage = _config["Azure:ImageUrlSuffix"] + item.ThumbnailImage.Trim();

                     // Deserialize screenshot list
                     if (!string.IsNullOrWhiteSpace(item.ScreenshotJson))
                     {
                         try
                         {
                             item.ScreenshotList = JsonSerializer.Deserialize<List<ScreenshotItem>>(item.ScreenshotJson) ?? new();
                             item.ScreenshotList.ForEach(ss =>
                             {
                                 if (!string.IsNullOrWhiteSpace(ss.Name))
                                     ss.Name = _config["Azure:ImageUrlSuffix"] + ss.Name.Trim();
                             });
                         }
                         catch
                         {
                             item.ScreenshotList = new(); // fallback on error
                         }
                     }

                     return item;
                 })
                 .ToList();

            responseModel.Data = spResult;
            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Data Fetched Successfully.";
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetProducts(string SearchText, string userKey)
        {
            ApiCommonResponseModel apiCommonResponse = new();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                SearchText = null; // or handle it according to your database logic
            }

            List<SqlParameter> sqlParameters = new List<SqlParameter>
            {
                new SqlParameter
                {
                    ParameterName = "SearchText", Value = SearchText == null ? DBNull.Value : SearchText,
                    SqlDbType = SqlDbType.VarChar, Size = 50
                },
                new SqlParameter
                {
                    ParameterName = "UserKey", Value = userKey == null ? DBNull.Value : Guid.Parse(userKey),
                    SqlDbType = SqlDbType.UniqueIdentifier,
                },
                new SqlParameter
                {
                    ParameterName = "IsValidUser",
                    SqlDbType = SqlDbType.Bit,
                    Direction = ParameterDirection.Output
                }
            };

            List<ProductsMResponseModel> result =
                await _context.SqlQueryToListAsync<ProductsMResponseModel>(
                    ProcedureCommonSqlParametersText.GetProductsM, sqlParameters.ToArray());

            int isValidUser = Convert.ToInt32(sqlParameters[2].Value);

            if (isValidUser == 1)
            {
                result.ForEach(x => x.IsIosCouponEnabled = _config["AppSettings:EnableGetCouponForIosDevice"]);

                apiCommonResponse.Data = result is null ? null : result;
                apiCommonResponse.StatusCode = HttpStatusCode.OK;
                apiCommonResponse.Message = "Successfull.";
            }
            else if (isValidUser == -1)
            {
                // Handle the case where the user is not valid
                apiCommonResponse.StatusCode =
                    HttpStatusCode.Unauthorized; // You can use a different status code as needed
                apiCommonResponse.Message = "Invalid user";
            }
            else
            {
                // Handle other cases if needed
                apiCommonResponse.StatusCode = HttpStatusCode.InternalServerError;
                apiCommonResponse.Message = "Unexpected error";
            }

            return apiCommonResponse;
        }

        public async Task<ApiCommonResponseModel> GetTopGainers(string type)
        {
            List<SqlParameter> sqlParameters = new()
            {
                new SqlParameter
                {
                    ParameterName = "Type", Value = type == null ? DBNull.Value : type, SqlDbType = SqlDbType.VarChar,
                    Size = 10
                }
            };

            var spResult =
                await _context.SqlQueryToListAsync<TopGainerLoserSpResponseModel>(
                    ProcedureCommonSqlParametersText.GetTopGainerLoser, sqlParameters.ToArray());

            responseModel.Data = spResult;
            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Data Fetched Successfully.";
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> LikeUnlikeProduct(LikeProductModel likedByRequest)
        {
            List<SqlParameter> sqlParameters =
            [
                new SqlParameter
                {
                    ParameterName = "ProductId",
                    Value = likedByRequest.ProductId,
                    SqlDbType = SqlDbType.Int,
                    Size = 50
                },
                new SqlParameter
                {
                    ParameterName = "CreatedBy",
                    Value = Guid.Parse(likedByRequest.CreatedBy),
                    SqlDbType = SqlDbType.UniqueIdentifier
                },
                new SqlParameter
                {
                    ParameterName = "LikeId",
                    Value = likedByRequest.LikeId,
                    SqlDbType = SqlDbType.Int,
                },
                new SqlParameter
                {
                    ParameterName = "Action",
                    Value = likedByRequest.Action == null ? DBNull.Value : likedByRequest.Action.Trim().ToUpper(),
                    SqlDbType = SqlDbType.VarChar,
                    Size = 10,
                }
            ];

            List<LikeProductResponse> spResult =
                await _context.SqlQueryToListAsync<LikeProductResponse>(ProcedureCommonSqlParametersText.LikeProductM,
                    sqlParameters.ToArray());

            if (spResult.Count > 0 && spResult[0].Message == "SUCCESSFULL")
            {
                responseModel.Message = "Like added successfully.";
                responseModel.StatusCode = HttpStatusCode.OK;
                return responseModel;
            }
            else if (spResult.Count > 0 && spResult[0].Message == "ALREADYLIKED")
            {
                responseModel.Message = "Already Liked.";
                responseModel.StatusCode = HttpStatusCode.OK;
                return responseModel;
            }
            else if (spResult.Count > 0 && spResult[0].Message == "UNLIKED")
            {
                responseModel.Message = "Like Removed Successfully.";
                responseModel.StatusCode = HttpStatusCode.OK;
                return responseModel;
            }
            else if (spResult.Count > 0 && spResult[0].Message == "INVALIDACTION")
            {
                responseModel.Message = "Invalid Action Type.";
                responseModel.StatusCode = HttpStatusCode.BadRequest;
                return responseModel;
            }
            else if (spResult.Count > 0 && spResult[0].Message == "NOTLIKED")
            {
                responseModel.Message = "Not Liked.";
                responseModel.StatusCode = HttpStatusCode.OK;
                return responseModel;
            }

            responseModel.Message = "UserKey or ProductID not found.";
            responseModel.StatusCode = HttpStatusCode.NotFound;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> MyBucketContent(Guid userKey)
        {
            List<SqlParameter> sqlParameters = new()
            {
                new SqlParameter
                {
                    ParameterName = "mobileUserKey", Value = userKey, SqlDbType = SqlDbType.UniqueIdentifier,
                }
            };

            var spResult =
                await _context.SqlQueryToListAsync<MyBucketContentResponseModel>(
                    ProcedureCommonSqlParametersText.GetMyBucketContent, sqlParameters.ToArray());

            if (spResult.Count == 0)
            {
                responseModel.Message = "No Data For Provided User";
                responseModel.StatusCode = HttpStatusCode.OK;
                return responseModel;
            }

            responseModel.Message = "Data Fetched Successfully";
            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Data = spResult;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> RateProduct(string userKey, int productId, float rating)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>
            {
                new()
                {
                    ParameterName = "ProductId", Value = productId, SqlDbType = SqlDbType.Int,
                },
                new()
                {
                    ParameterName = "UserKey", Value = Guid.Parse(userKey), SqlDbType = SqlDbType.UniqueIdentifier,
                },
                new()
                {
                    ParameterName = "Rating", Value = rating, SqlDbType = SqlDbType.Float,
                }
            };

            var spResult =
                await _context.SqlQueryToListAsync<ProcedureCommonResponseModel>(
                    ProcedureCommonSqlParametersText.RateProductM, sqlParameters.ToArray());

            if (spResult[0].Result == 1)
            {
                responseModel.Message = "Successfull.";
                responseModel.StatusCode = HttpStatusCode.OK;
                return responseModel;
            }

            if (spResult[0].Result == 0)
            {
                responseModel.Message = "Bad Request.";
                responseModel.StatusCode = HttpStatusCode.BadRequest;
                return responseModel;
            }

            responseModel.Message = "An Error Occured.";
            responseModel.StatusCode = HttpStatusCode.InternalServerError;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetLearningMaterial(Guid? mobileUserKey)
        {
            responseModel.Data = _context.LearningMaterialM.Where(c => c.IsActive == true && c.IsDelete == false)
                .Select(lp => new
                {
                    lp.Id,
                    Name = lp.Title,
                    lp.ImageUrl,
                    lp.Description
                }).ToList();

            // _ = await _mongoService.SaveUserActivity(Model.MongoDbCollection.UserActivityEnum.GetLearningMaterial, mobileUserKey, null);

            responseModel.StatusCode = HttpStatusCode.OK;

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetLearningContentExamples(int contentId)
        {
            responseModel.Data = await _context.LearningContentExampleM
                .Where(c => c.IsActive == true && c.IsDelete == false && c.ContentId == contentId).Select(lp => new
                {
                    lp.Id,
                    Name = lp.Title,
                    ImageUrl = lp.ImageUrl
                }).ToListAsync();

            responseModel.StatusCode = HttpStatusCode.OK;

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> LikeUnlikeLearningContent(
            LikeUnlikeLearningContentRequestModel request)
        {
            var result = await _context.LearningContentLikesM.FirstOrDefaultAsync(x =>
                x.LearningContentId == request.Id && x.UserKey == request.UserKey);
            if (result == null)
            {
                LearningContentLikesM likeData = new LearningContentLikesM()
                {
                    CreatedOn = DateTime.Now,
                    IsLiked = request.IsLiked,
                    LearningContentId = request.Id,
                    UserKey = request.UserKey,
                };
                _ = await _context.LearningContentLikesM.AddAsync(likeData);
            }
            else
            {
                result.IsLiked = request.IsLiked;
            }

            _ = await _context.SaveChangesAsync();

            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetCoupons(Guid? userKey, int productId, int subscriptionDurationId)
        {
            List<SqlParameter> sqlParameters =
            [
                new(){ParameterName = "UserKey", Value = userKey == Guid.Empty  ? DBNull.Value : userKey , SqlDbType = SqlDbType.UniqueIdentifier},
                new(){ParameterName = "ProductId", Value = productId, SqlDbType = SqlDbType.Int},
                new(){ParameterName = "SubscriptionDurationId", Value = subscriptionDurationId, SqlDbType = SqlDbType.Int}
            ];

            var spResult = await _context.SqlQueryToListAsync<GetCouponsSpResponse>(ProcedureCommonSqlParametersText.GetCouponsM, sqlParameters.ToArray());

            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Data = spResult;

            return responseModel;
        }

        //public async Task<> GetImage(string imageName)
        //{
        //    var image = System.IO.File.OpenRead($"Assets\\Images\\{imageName}.jpeg");
        //    responseModel.Data = image;
        //    return responseModel;
        //}

        public async Task<ApiCommonResponseModel> ActiveProductCount()
        {
            int result = await _context.ProductsM
                .Where(x => x.IsActive == true
                    && x.IsDeleted == false
                    && x.CategoryID != 12)
                // && x.CategoryID != 10)
                .CountAsync();
            responseModel.Data = new { TotalActiveProductCount = result };
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        /// <summary>
        /// Checks a user's active product subscriptions and determines the following flags:
        /// - `scanner`: true if the user has purchased all active products (excluding "research").
        /// - `research`: true if the user has purchased any product with the code "research".
        /// - `breakfast`: true if the user has purchased any product with the code "breakfast".
        /// </summary>
        /// <param name="userKey">User's unique public key.</param>
        /// <returns>
        /// ApiCommonResponseModel with flags: { scanner, research, breakfast }
        /// </returns>

        public async Task<ApiCommonResponseModel> ActiveProductStatus(Guid userKey)
        {
            // Use List<string> instead of array
            var excludedGroupNames = new List<string> { "research", "community" };

            var excludedCategoryIds = await _context.ProductCategoriesM
                .Where(x => excludedGroupNames.Contains(x.GroupName) && x.IsActive == true && x.IsDelete == false)
                .Select(x => x.Id)
                .ToListAsync();


            var activeProducts = await _context.ProductsM
                 .Where(x => x.IsActive && !x.IsDeleted && !excludedCategoryIds.Contains(x.CategoryID))
                 .Select(x => new { x.Id, x.Code })
                 .ToListAsync();

            var activeProductIds = activeProducts.Select(x => x.Id).ToList();

            var userPurchasedProducts = await (
                                    from bucket in _context.MyBucketM
                                    join product in _context.ProductsM on bucket.ProductId equals product.Id
                                    join category in _context.ProductCategoriesM on product.CategoryID equals category.Id
                                    where bucket.MobileUserKey == userKey
                                        && bucket.EndDate != null
                                        && bucket.EndDate.Value.Date >= DateTime.Now.Date
                                        && (
                                            activeProductIds.Contains(bucket.ProductId)
                                            || category.GroupName.ToLower() == "research"
                                        )
                                    select new
                                    {
                                        product.Id,
                                        product.Code
                                    }).ToListAsync();


            var filteredUserPurchasedProducts = userPurchasedProducts
                      .Where(x => !x.Code.Equals("research", StringComparison.OrdinalIgnoreCase))
                      .ToList();

            bool scanner = filteredUserPurchasedProducts.Count == activeProducts.Count;
            bool research = userPurchasedProducts.Any(x => x.Code.ToLower() == "research");
            bool breakfast = userPurchasedProducts.Any(x => x.Code.ToLower() == "breakfast");

            responseModel.Data = new
            {
                scanner,
                research,
                breakfast
            };
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }




        public async Task<ApiCommonResponseModel> ActiveProductList()
        {
            var result = await _context.ProductsM
                .Where(x => x.IsActive == true
                    && x.IsDeleted == false
                    && x.CategoryID != 12)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Code,
                })
                // && x.CategoryID != 10)
                .ToListAsync();  // This will fetch the actual product data

            responseModel.Data = result;
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }


        public async Task<ApiCommonResponseModel> GetPlayList(long mobileUserId, int productId)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>()
            {
                new SqlParameter
                {
                    ParameterName = "productId",
                    Value = productId,
                    SqlDbType = SqlDbType.Int,
                },
                new SqlParameter
                {
                    ParameterName = "mobileUserId",
                    Value = mobileUserId,
                    SqlDbType = SqlDbType.BigInt
                }
            };

            var playlistResults =
                await _context.SqlQueryToListAsync<GetPlayListSpModel>(
                    ProcedureCommonSqlParametersText.GetPlayList, sqlParameters.ToArray());

            if (playlistResults.Count == 0)
            {
                return new ApiCommonResponseModel
                {
                    Data = new List<Chapter>(),
                    StatusCode = HttpStatusCode.OK,
                    Message = "No data found for the provided product."
                };
            }

            // Group data by ChapterId
            var chapters = playlistResults
                .GroupBy(p => new { p.ChapterId, p.ChapterTitle, p.ChapterDescription })
                .Select(g => new ChapterResponseModel
                {
                    Id = g.Key.ChapterId,
                    ChapterTitle = g.Key.ChapterTitle,
                    Description = g.Key.ChapterDescription,
                    SubChapters = g.Where(sc => sc.SubChapterId.HasValue) // Only add subchapters if they exist
                                   .Select(sc => new SubChapterResponseModel
                                   {
                                       Id = sc.SubChapterId.Value,
                                       Title = sc.SubChapterTitle,
                                       Link = sc.SubChapterLink,
                                       Description = sc.SubChapterDescription,
                                       Language = sc.SubChapterLanguage,
                                       VideoDuration = sc.VideoDuration ?? 0,
                                       IsVisible = sc.IsVisible
                                   }).ToList()
                }).ToList();

            var responseModel = new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Data Fetched Successfully.",
                Data = new
                {
                    Playlist = chapters // Wrap the chapters inside a Playlist array
                }
            };

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> AddQueryFormAsync(QueryFormRequestModel request)
        {
            if (request == null || request.Id != 0)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Invalid request. Only new queries can be added.",
                    Data = null,
                    Exceptions = null
                };
            }

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

            var product = await _context.ProductsM
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

            var queryForm = new QueryFormM
            {
                MobileUserId = request.MobileUserId,
                ProductId = productId,
                Questions = request.QueryDescription,
                QueryRelatedTo = request.QueryCategory,
                CreatedBy = request.CreatedBy,
                CreatedOn = DateTime.Now,
                IsActive = true,
                IsDeleted = false
            };

            if (request.ScreenshotUrl is not null)
            {
                queryForm.ScreenshotUrl = await _azureBlobStorageService.UploadImage(request.ScreenshotUrl);
            }

            _context.QueryFormM.Add(queryForm);
            await _context.SaveChangesAsync();

            //if (!string.IsNullOrWhiteSpace(request.Remarks))
            //{
            //    var remark = new QueryFormRemarks
            //    {
            //        QueryId = queryForm.Id,
            //        Remarks = request.Remarks,
            //        CreatedBy = request.CreatedBy,
            //        CreatedOn = DateTime.Now,
            //        IsActive = true,
            //        IsDeleted = false
            //    };

            //    _context.QueryFormRemarks.Add(remark);
            //    await _context.SaveChangesAsync();
            //}

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Query added successfully.",
                Exceptions = null
            };
        }
    }
}