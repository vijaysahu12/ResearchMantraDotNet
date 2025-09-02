using FirebaseAdmin.Messaging;
using RM.BlobStorage;
using RM.CommonServices.Services;
using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.ResearchMantraContext;
using RM.Database.MongoDbContext;
using RM.Model;
using RM.Model.Common;
using RM.Model.MongoDbCollection;
using RM.Model.RequestModel;
using RM.Model.RequestModel.Notification;
using RM.Model.ResponseModel;
using RM.NotificationService;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Data;
using System.Net;

namespace RM.CommonServices
{
    public interface IMobileNotificationService
    {
        Task<ApiCommonResponseModel> FirebaseNotificationToToken(FirebaseNotificationRequestWithToken request);
        Task<ApiCommonResponseModel> GetUnreadNotificationCount(Guid userKey);
        Task<ApiCommonResponseModel> GetNotifications(GetNotificationRequestModel queryValues);
        Task<ApiCommonResponseModel> MarkNotificationAsRead(IdModel notificationObjectId);
        Task<ApiCommonResponseModel> SendSubscriptionExpiryCheckNotification();
        Task<ApiCommonResponseModel> DeleteNotification(string notificationObjectId);
        Task<ApiCommonResponseModel> SendNotificationToActiveToken(NotificationRequestModel messageBody);
        Task<ApiCommonResponseModel> SendNotificationToActiveToken(NotificationToMobileRequestModel messageBody);

        Task<ApiCommonResponseModel> SendNotificationToActiveTokenV1(SaveNotificationRequestModel messageBody);

        Task<ApiCommonResponseModel> SendNotificationToTokenToLogoutFromOldDevice(string? token);
        Task<ApiCommonResponseModel> SendAdvertismentNotification(AdModel request);
        Task<ApiCommonResponseModel> SavePushNotification(NotificationRequestModel request);
        Task<List<UserListForPushNotificationModel>> GetNotificationReceivers(string productCode, DateTime ToDate);
        Task<bool> SendAdvertismentNotification2(AdvertismentModel2 request);
        Task<object> SendNotificationToMobile(NotificationToMobileRequestModel param);
        Task<ApiCommonResponseModel> SendFreeNotification(SendFreeNotificationRequestModel param);
        Task<ApiCommonResponseModel> SendNotificationViaCrm(SendFreeNotificationRequestModel param);
        Task<ApiCommonResponseModel> SendFreeWhatsappNotification(SendFreeWhatsAppNotificationRequestModel param);
        Task<ApiCommonResponseModel> GetScannerNotifications(QueryValues request);
        Task<ApiCommonResponseModel> ManageProductNotification(bool allowNotify, Guid mobileUserKey, int productId);
        Task<bool> ValidateAllFCMTokens(NotificationToMobileRequestModel param);
        Task<ApiCommonResponseModel?> DeleteSevenDaysOldNotification();
        Task<List<UserListForPushNotificationModel>> GetUserWhichNeedToNotifyForAppUpdate();
        Task<ApiCommonResponseModel?> ManageScheduleNotification(ScheduledNotificationRequestModel notification);
        Task<ApiCommonResponseModel> SendNotificationToListOfUsers(NotificationRequestModel paramData, List<UserListForPushNotificationModel> listOfToken);
        Task<ApiCommonResponseModel> MarkAllNotificationAsRead(Guid mobileUserKey);
    }
    public class MobileNotificationService : IMobileNotificationService
    {
        private ApiCommonResponseModel responseModel = new();
        private readonly IConfiguration _configuration;
        private readonly ResearchMantraContext _dbContext;
        private readonly MongoDbService _mongoDbService;
        private readonly IMongoRepository<ExceptionLog> _exception;
        private readonly IMongoRepository<Log> _log;
        private readonly bool _enableLogging;
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        private readonly FirebaseNotification _firebaseNotification;

        public MobileNotificationService(IConfiguration configuration, ResearchMantraContext context,
            MongoDbService mongoDbService, FirebaseNotification firebaseNotification, IAzureBlobStorageService azureBlobStorageService,
            IMongoRepository<Log> mongoRepo, IMongoRepository<ExceptionLog> exception)
        {
            _configuration = configuration;
            _dbContext = context;
            _mongoDbService = mongoDbService;
            _enableLogging = _configuration["AppSettings:EnableLogging"] == "true";
            _firebaseNotification = firebaseNotification;
            _azureBlobStorageService = azureBlobStorageService;
            _log = mongoRepo;
            _exception = exception;
        }

        public async Task<List<UserListForPushNotificationModel>> GetNotificationReceivers(string productCode, DateTime ToDate)
        {
            try
            {
                List<SqlParameter> sqlParameters =
                [
                    new SqlParameter
            {
                ParameterName = "productCode",
                Value = string.IsNullOrEmpty(productCode) ? DBNull.Value : productCode,
                SqlDbType = SqlDbType.VarChar
            },
            new SqlParameter
            {
                ParameterName = "endDate",
                Value = ToDate,
                SqlDbType = SqlDbType.Date
            },
        ];

                var result = await _dbContext.SqlQueryToListAsync<UserListForPushNotificationModel>(
                    ProcedureCommonSqlParametersText.GetNotificationReceivers,
                    sqlParameters.ToArray());

                return result ?? new List<UserListForPushNotificationModel>();
            }
            catch (Exception ex)
            {
                await _log.AddAsync(new Log
                {
                    CreatedOn = DateTime.Now,
                    Message = $"Exception in GetNotificationReceivers: {ex.Message}",
                    Source = "GetNotificationReceivers",
                    Category = "notification"
                });

                return new List<UserListForPushNotificationModel>();
            }
        }

        public async Task<ApiCommonResponseModel> GetScannerNotifications(QueryValues request)
        {
            if (request != null && !string.IsNullOrWhiteSpace(request.PrimaryKey) && request.PrimaryKey.ToUpper() == "BREAKFAST")
            {   //Get Records from MSSQL DB 

                var result = await Task.Run(() => _dbContext.ScannerPerformanceM
                    .Where(s => s.Topic.ToLower() == "breakfast" &&
                                s.SentAt.Date >= request.FromDate &&
                                s.SentAt.Date <= request.ToDate)
                    .OrderByDescending(x => x.SentAt)
                    .Select(x => new
                    {
                        isRead = false,
                        objectid = "",
                        tradingSymbol = x.TradingSymbol,
                        createdOn = x.SentAt.ToString("HH:mm, MMM dd"),
                        title = "",
                        message = "",
                        price = x.Ltp,
                        transactionType = "",
                        topic = x.Topic,
                        viewChart = $"https://in.tradingview.com/chart/?symbol=NSE:{x.ViewChart}",
                        created = x.SentAt.ToString()
                    })
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList());

                return new ApiCommonResponseModel
                {
                    Data = result,
                    StatusCode = HttpStatusCode.OK,
                    Message = "Successful"
                };
            }
            else
            {

                //Get Records from MONGO DB 
                var mongoData = await _mongoDbService.GetScannerNotification(request);
                return new ApiCommonResponseModel
                {
                    Data = mongoData,
                    StatusCode = HttpStatusCode.OK,
                    Message = "Successful"
                };
            }
        }

        public async Task<ApiCommonResponseModel> GetUnreadNotificationCount(Guid userKey)
        {
            if (userKey == Guid.Empty)
            {
                responseModel.Message = "User Not Found.";
                responseModel.StatusCode = HttpStatusCode.NotFound;
                return responseModel;
            }
            var unReadCount = await _mongoDbService.GetUnreadNotificationCount(userKey);

            if (unReadCount < 0)
            {
                responseModel.Message = "User Not Found.";
                responseModel.StatusCode = HttpStatusCode.NotFound;
            }
            else
            {
                responseModel.Message = "Data Fetched Successfully.";
                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Data = unReadCount;
            }

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetNotifications(GetNotificationRequestModel queryValues)
        {
            var notifications = await _mongoDbService.GetPushNotification(queryValues);

            // Convert DateTime to string format inside the response
            var formattedNotifications = notifications.Select(n => new
            {
                n.NotificationId,
                n.ObjectId,
                n.ReceivedBy,
                n.Message,
                n.Title,
                n.EnableTradingButton,
                n.AppCode,
                n.Exchange,
                n.TradingSymbol,
                n.TransactionType,
                n.OrderType,
                n.Price,
                n.ProductId,
                n.ProductName,
                n.Complexity,
                n.CategoryId,
                n.IsRead,
                n.IsDelete,
                ReadDate = n.ReadDate,
                n.Topic,
                n.ScreenName,
                n.IsPinned,
                CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(n.CreatedOn, DateTimeKind.Utc),
                TimeZoneInfo.Local
            ).ToString("HH:mm, MMM dd")

            }).ToList();

            return new ApiCommonResponseModel
            {
                Message = "Data Fetched Successfully",
                StatusCode = HttpStatusCode.OK,
                Data = formattedNotifications
            };
        }


        public async Task<ApiCommonResponseModel> SendSubscriptionExpiryCheckNotification()
        {
            Message message = new()
            {
                Notification = new Notification
                {
                    Title = "Unlock New Possibilities",
                    Body =
                        "Unlock a world of possibilities! Renew your product subscriptions today to access new updates, features, and opportunities",
                },
                Topic = "FREE",
                Data = new Dictionary<string, string>
                {
                    { "Type", "REFRESHTOPIC" }
                }
            };

            var messaging = FirebaseMessaging.DefaultInstance;
            var result = await messaging.SendAsync(message);

            if (string.IsNullOrEmpty(result))
            {
                responseModel.Message = "Couldn't Send Notification";
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                return responseModel;
            }

            responseModel.Message = "Notification Sent Successfully.";
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> SendNotificationToTokenToLogoutFromOldDevice(string? token)
        {
            try
            {
                var message = new Message()
                {
                    Notification = new Notification()
                    {
                        Title = "New Device Login Detected",
                        Body =
                            "You will be logged out from this device as a new device login has been detected. You can countiue your trading journey in your new device.",
                    },
                    Data = new Dictionary<string, string>
                    {
                        { "Type", "NEWDEVICELOGIN" }
                    },
                    Token = token
                };

                var messaging = FirebaseMessaging.DefaultInstance;
                var result = await messaging.SendAsync(message);

                if (string.IsNullOrEmpty(result))
                {
                    responseModel.Message = "Couldn't Send Notification";
                    responseModel.StatusCode = HttpStatusCode.InternalServerError;
                    return responseModel;
                }

                responseModel.Message = "Notification Sent Successfully.";
                responseModel.StatusCode = HttpStatusCode.OK;
                return responseModel;
            }
            catch (Exception)
            {
                responseModel.Message = "Couldn't Send Notification";
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                return responseModel;
            }
        }

        public async Task<ApiCommonResponseModel> SendNotificationToActiveToken(NotificationRequestModel paramData)
        {
            try
            {
                List<UserListForPushNotificationModel> notificationReceivers;
                if (paramData != null && string.IsNullOrEmpty(paramData.Topic) && string.IsNullOrEmpty(paramData.Body))
                {

                    await _log.AddAsync(new Log
                    {
                        CreatedOn = DateTime.Now,
                        Message = "Step 1.0: Invalid Request Data",
                        Source = "SendNotificationToActiveToken",
                        Category = "notification"
                    });

                    responseModel.Message = "Invalid Request Data";
                    responseModel.StatusCode = HttpStatusCode.BadRequest;
                    return responseModel;
                }


                paramData.Topic = paramData.Topic.ToUpper();
                paramData.Scanner = paramData.Scanner.ToUpper();
                paramData.CreatedDate = DateTime.Now;

                List<Task> tasks = new();

                tasks.Add(Task.Run(async () =>
                {
                    notificationReceivers = await this.GetNotificationReceivers(paramData.Topic, DateTime.Now);

                    if (notificationReceivers is not null && notificationReceivers.Count != 0)
                    {
                        notificationReceivers = notificationReceivers.Where(item => item.FirebaseFcmToken is not null && item.FirebaseFcmToken.Length > 64).ToList();
#if DEBUG
                        notificationReceivers = notificationReceivers
                            .Where(item => item.FirebaseFcmToken.StartsWith("ewUhkWzhF055qYx18JskOx")).ToList();
#endif
                        if (paramData.TestNotification)
                        {
                            notificationReceivers = notificationReceivers.Where(item => item.PublicKey == Guid.Parse("1194D99E-C419-EF11-B261-88B133B31C8F")).ToList();//for testing purpose
                        }

                        var dictionary = new Dictionary<string, string> { { "Scanner", paramData.Scanner }, { "Topic", paramData.Topic } };
                        var responseFcm = await _firebaseNotification.SendFCMMessage(paramData.Title, paramData.Body, notificationReceivers, paramData.Scanner == "TRUE" ? dictionary : null);

                        if (responseFcm != null && responseFcm.StatusCode != HttpStatusCode.OK)
                        {
                            await _log.AddAsync(new Log { CreatedOn = DateTime.Now, Message = responseFcm.Message, Source = "SendFCMMessage", Category = "FailedToSend" });
                        }


                        //DateTime utcDateTime = paramData.CreatedDate;
                        //string formattedDateTime = utcDateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");

                        PushNotification notification = new()
                        {
                            Title = paramData.Title,
                            AppCode = paramData.AppCode,
                            Complexity = paramData.Complexity,
                            EnableTradingButton = paramData.EnableTradingButton,
                            Scanner = paramData.Scanner == "TRUE",
                            Exchange = paramData.Exchange,
                            Message = paramData.Body,
                            OrderType = paramData.CallType,
                            Price = paramData.Price,
                            ProductId = paramData.Product.ToString(),
                            ProductName = paramData.ProductName,
                            Quantity = paramData.Quantity,
                            Token = paramData.Token,
                            TradingSymbol = paramData.TradingSymbol,
                            TransactionType = paramData.TransactionType,
                            Validity = paramData.Validity,
                            CreatedOn = paramData.CreatedDate,
                            CategoryId = 9,
                            Topic = paramData.Topic.ToUpper()
                        };

                        _ = await _mongoDbService.SaveNotificationDataAsync(notification, notificationReceivers);
                    }
                    else
                    {
                        await _log.AddAsync(new Log
                        {
                            CreatedOn = DateTime.Now,
                            Message = "Step 1.1:" + "No receiver Found for Topic " + paramData.Topic,
                            Source = "SendNotificationToActiveToken",
                            Category = "notification"
                        });
                    }

                })); //task endblock

                await Task.WhenAll(tasks);
                responseModel.Message = "Message Sent Successfully.";
                responseModel.StatusCode = HttpStatusCode.OK;

            }
            catch (Exception ex)
            {
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                responseModel.Message = ex.ToString();
                await _log.AddAsync(new Log
                {
                    CreatedOn = DateTime.Now,
                    Message = "Exception Raised",
                    Source = "SendNotificationToActiveToken",
                    Category = "notification"
                });

                await _exception.AddAsync(new ExceptionLog
                {
                    CreatedOn = DateTime.Now,
                    Message = ex.Message,
                    Source = "SendNotificationToActiveToken",
                    StackTrace = ex.StackTrace,
                    InnerException = ex.InnerException?.Message,
                    RequestBody = JsonConvert.SerializeObject(paramData),
                });
                return responseModel;
            }

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> SendNotificationToActiveToken(NotificationToMobileRequestModel paramData)
        {
            try
            {
                List<UserListForPushNotificationModel> notificationReceivers;
                if (paramData != null && string.IsNullOrEmpty(paramData.Topic) && string.IsNullOrEmpty(paramData.Body))
                {

                    await _log.AddAsync(new Log
                    {
                        CreatedOn = DateTime.Now,
                        Message = "Step 1.0: Invalid Request Data",
                        Source = "SendNotificationToActiveToken",
                        Category = "notification"
                    });

                    responseModel.Message = "Invalid Request Data";
                    responseModel.StatusCode = HttpStatusCode.BadRequest;
                    return responseModel;
                }


                if (!string.IsNullOrWhiteSpace(paramData.Topic))
                {
                    paramData.Topic = paramData.Topic.ToUpper();
                }
                else
                {
                    responseModel.Message = "Invalid Topic";
                    responseModel.StatusCode = HttpStatusCode.BadRequest;
                    return responseModel;
                }

                List<Task> tasks = new();

                tasks.Add(Task.Run(async () =>
                {
                    notificationReceivers = await this.GetNotificationReceivers(paramData.Topic, DateTime.Now);

                    if (notificationReceivers is not null && notificationReceivers.Count != 0)
                    {
                        notificationReceivers = notificationReceivers.Where(item => item.FirebaseFcmToken is not null && item.FirebaseFcmToken.Length > 64).ToList();
#if DEBUG
                        //  notificationReceivers = notificationReceivers
                        //  .Where(item => item.FirebaseFcmToken.StartsWith("ewUhkWzhF055qYx18JskOx")).ToList();
#endif


                        var dictionary = new Dictionary<string, string> { { "Scanner", paramData.Scanner.ToString() }, { "Topic", paramData.Topic } };
                        var responseFcm = await _firebaseNotification.SendFCMMessage(paramData.Title, paramData.Body, notificationReceivers, paramData.Scanner == "TRUE" ? dictionary : null);

                        if (responseFcm != null && responseFcm.StatusCode != HttpStatusCode.OK)
                        {
                            await _log.AddAsync(new Log { CreatedOn = DateTime.Now, Message = responseFcm.Message, Source = "SendFCMMessage", Category = "FailedToSend" });
                        }

                    }
                    else
                    {
                        await _log.AddAsync(new Log
                        {
                            CreatedOn = DateTime.Now,
                            Message = "Step 1.1:" + "No receiver Found for Topic " + paramData.Topic,
                            Source = "SendNotificationToActiveToken",
                            Category = "notification"
                        });
                    }

                })); //task endblock

                await Task.WhenAll(tasks);
                responseModel.Message = "Message Sent Successfully.";
                responseModel.StatusCode = HttpStatusCode.OK;

            }
            catch (Exception ex)
            {
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                responseModel.Message = ex.ToString();
                await _log.AddAsync(new Log
                {
                    CreatedOn = DateTime.Now,
                    Message = "Exception Raised",
                    Source = "SendNotificationToActiveToken",
                    Category = "notification"
                });

                await _exception.AddAsync(new ExceptionLog
                {
                    CreatedOn = DateTime.Now,
                    Message = ex.Message,
                    Source = "SendNotificationToActiveToken",
                    StackTrace = ex.StackTrace,
                    InnerException = ex.InnerException?.Message,
                    RequestBody = JsonConvert.SerializeObject(paramData),
                });
                return responseModel;
            }

            return responseModel;
        }


        public async Task<ApiCommonResponseModel> SendNotificationToActiveTokenV1(SaveNotificationRequestModel paramData)
        {
            try
            {
                List<UserListForPushNotificationModel> notificationReceivers;
                if (paramData != null && string.IsNullOrEmpty(paramData.Topic) && string.IsNullOrEmpty(paramData.Body))
                {

                    await _log.AddAsync(new Log
                    {
                        CreatedOn = DateTime.Now,
                        Message = "Step 1.0: Invalid Request Data",
                        Source = "SendNotificationToActiveToken",
                        Category = "notification"
                    });

                    responseModel.Message = "Invalid Request Data";
                    responseModel.StatusCode = HttpStatusCode.BadRequest;
                    return responseModel;
                }


                paramData.Topic = paramData.Topic.ToUpper();
                paramData.Scanner = paramData.Scanner.ToUpper();
                paramData.CreatedDate = DateTime.Now;

                List<Task> tasks = new();

                tasks.Add(Task.Run(async () =>
                {
                    notificationReceivers = await this.GetNotificationReceivers(paramData.Topic, DateTime.Now);

                    if (notificationReceivers is not null && notificationReceivers.Count != 0)
                    {
                        notificationReceivers = notificationReceivers.Where(item => item.FirebaseFcmToken is not null && item.FirebaseFcmToken.Length > 64).ToList();
#if DEBUG
                        notificationReceivers = notificationReceivers
                            .Where(item => item.FirebaseFcmToken.StartsWith("ewUhkWzhF055qYx18JskOx")).ToList();
#endif
                        if (paramData.TestNotification)
                        {
                            notificationReceivers = notificationReceivers.Where(item => item.PublicKey == Guid.Parse("1194D99E-C419-EF11-B261-88B133B31C8F")).ToList();//for testing purpose
                        }

                        var dictionary = new Dictionary<string, string> { { "Scanner", paramData.Scanner }, { "Topic", paramData.Topic } };
                        var responseFcm = await _firebaseNotification.SendFCMMessage(paramData.Title, paramData.Body, notificationReceivers, paramData.Scanner == "TRUE" ? dictionary : null);

                        if (responseFcm != null && responseFcm.StatusCode != HttpStatusCode.OK)
                        {
                            await _log.AddAsync(new Log { CreatedOn = DateTime.Now, Message = responseFcm.Message, Source = "SendFCMMessage", Category = "FailedToSend" });
                        }


                        //DateTime utcDateTime = paramData.CreatedDate;
                        //string formattedDateTime = utcDateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");

                        PushNotification notification = new()
                        {
                            Title = paramData.Title,
                            AppCode = paramData.AppCode,
                            Complexity = paramData.Complexity,
                            EnableTradingButton = paramData.EnableTradingButton,
                            Scanner = paramData.Scanner == "TRUE",
                            Exchange = paramData.Exchange,
                            Message = paramData.Body,
                            OrderType = paramData.CallType,
                            Price = paramData.Price,
                            ProductId = paramData.Product,
                            ProductName = paramData.ProductName,
                            Quantity = paramData.Quantity,
                            Token = paramData.Token,
                            TradingSymbol = paramData.TradingSymbol,
                            TransactionType = paramData.TransactionType,
                            Validity = paramData.Validity,
                            CreatedOn = paramData.CreatedDate,
                            CategoryId = 9,
                            Topic = paramData.Topic.ToUpper()
                        };

                        _ = await _mongoDbService.SaveNotificationDataAsync(notification, notificationReceivers);
                    }
                    else
                    {
                        await _log.AddAsync(new Log
                        {
                            CreatedOn = DateTime.Now,
                            Message = "Step 1.1:" + "No receiver Found for Topic " + paramData.Topic,
                            Source = "SendNotificationToActiveToken",
                            Category = "notification"
                        });
                    }

                })); //task endblock

                await Task.WhenAll(tasks);
                responseModel.Message = "Message Sent Successfully.";
                responseModel.StatusCode = HttpStatusCode.OK;

            }
            catch (Exception ex)
            {
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                responseModel.Message = ex.ToString();
                await _log.AddAsync(new Log
                {
                    CreatedOn = DateTime.Now,
                    Message = "Exception Raised",
                    Source = "SendNotificationToActiveToken",
                    Category = "notification"
                });

                await _exception.AddAsync(new ExceptionLog
                {
                    CreatedOn = DateTime.Now,
                    Message = ex.Message,
                    Source = "SendNotificationToActiveToken",
                    StackTrace = ex.StackTrace,
                    InnerException = ex.InnerException?.Message,
                    RequestBody = JsonConvert.SerializeObject(paramData),
                });
                return responseModel;
            }

            return responseModel;
        }


        private static async Task<BatchResponse> SendFCMSilentNotification(List<string> listOfToken,
                 Dictionary<string, string> data)
        {
            try
            {
                var message = new MulticastMessage()
                {
                    Tokens = listOfToken,
                    //Notification = new Notification() // REMOVED THIS AS THIS IS A SILENT NOTIFICATION AND THIS PAYLOAD NEEDS TO BE REMOVED
                    //{
                    //    Title = title,
                    //    Body = body,
                    //},
                    Android = new AndroidConfig
                    {
                        Notification = new AndroidNotification
                        {
                            Sound = "notification_tone",
                            ChannelId = "kingResearchAcademy",
                            Priority = NotificationPriority.HIGH
                        },
                        Priority = Priority.High
                    },
                    Apns = new ApnsConfig
                    {
                        Aps = new Aps
                        {
                            Sound = "notification_tone.wav",
                            ContentAvailable = true,
                        }
                    }
                };

                if (data != null)
                {
                    message.Data = data;
                }

                return await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);
            }
            catch
            {
                //Console.WriteLine($"Error sending message: {ex.Message}");
                return null;
            }
        }

        public async Task<ApiCommonResponseModel> FirebaseNotificationToToken(FirebaseNotificationRequestWithToken request)
        {
            var data = new Dictionary<string, string>();
            var tokenList = new List<UserListForPushNotificationModel>();
            if (request.Type?.ToUpper() != "FREE")
            {
                var product = await _dbContext.ProductsM
                    .Where(item => item.Id == request.ProductId)
                    .Join(
                        _dbContext.ProductCategoriesM,
                        product => product.CategoryID,
                        category => category.Id,
                        (product, category) => new { CategoryId = category.Id }
                    )
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    responseModel.Message = "Requested Product Doesn't Exist.";
                    responseModel.StatusCode = HttpStatusCode.BadRequest;
                    return responseModel;
                }

                tokenList.Add(new UserListForPushNotificationModel { PublicKey = Guid.Empty, FirebaseFcmToken = request.DeviceToken, OldDevice = true });
                data.Add("Category", product.CategoryId.ToString());
            }

            responseModel = await _firebaseNotification.SendFCMMessage(request.Title, request.Body, tokenList, data);

            if (responseModel != null && responseModel.StatusCode != HttpStatusCode.OK)
            {
                await _log.AddAsync(new Log
                {
                    CreatedOn = DateTime.Now,
                    Message = responseModel.Message,
                    Source = "SendFCMMessage",
                    Category = "FailedToSend"
                });
            }

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> SendNotificationToListOfUsers(NotificationRequestModel paramData,
             List<UserListForPushNotificationModel> listOfToken)
        {
            if (paramData != null && string.IsNullOrEmpty(paramData.Topic) && string.IsNullOrEmpty(paramData.Body))
            {
                await _log.AddAsync(new Log
                {
                    CreatedOn = DateTime.Now,
                    Message = "Invalid Request Data",
                    Source = "SendNotificationToActiveToken",
                    Category = "notification"
                });
                responseModel.Message = "Invalid Request Data";
                responseModel.StatusCode = HttpStatusCode.BadRequest;
                return responseModel;
            }
            else
            {
                paramData.Topic = paramData.Topic.ToUpper();
                paramData.Scanner = paramData.Scanner.ToUpper();
                paramData.CreatedDate = DateTime.Now;
            }

            var dictionary = new Dictionary<string, string>
                    {
                        { "Scanner", paramData.Scanner }
                    };

            responseModel = await _firebaseNotification.SendFCMMessage(paramData.Title, paramData.Body, listOfToken, paramData.Scanner == "TRUE" ? dictionary : null);
            if (responseModel != null && responseModel.StatusCode != HttpStatusCode.OK)
            {

                await _log.AddAsync(new Log
                {
                    CreatedOn = DateTime.Now,
                    Message = responseModel.Message,
                    Source = "SendFCMMessage",
                    Category = "FailedToSend"
                });
            }


            var categoryId = await _dbContext.ProductsM
            .Where(item => item.Code == paramData.Topic)
            .Select(item => item.CategoryID)
            .FirstOrDefaultAsync();

            _ = Task.Run(() =>
            {
                DateTime utcDateTime = paramData.CreatedDate;
                string formattedDateTime = utcDateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");

                Model.MongoDbCollection.PushNotification notification = new()
                {
                    Title = paramData.Title,
                    AppCode = paramData.AppCode,
                    Complexity = paramData.Complexity,
                    EnableTradingButton = paramData.EnableTradingButton,
                    Scanner = paramData.Scanner == "TRUE",
                    Exchange = paramData.Exchange,
                    Message = paramData.Body,
                    OrderType = paramData.CallType,
                    Price = paramData.Price,
                    ProductId = paramData.Product.ToString(),
                    ProductName = paramData.ProductName,
                    Quantity = paramData.Quantity,
                    Token = paramData.Token,
                    TradingSymbol = paramData.TradingSymbol,
                    TransactionType = paramData.TransactionType,
                    Validity = paramData.Validity,
                    CreatedOn = paramData.CreatedDate,//formattedDateTime,
                    CategoryId = categoryId,
                    Topic = paramData.Topic.ToUpper()
                };

                var messageId = _mongoDbService.SaveNotificationData(notification);

                List<PushNotificationReceiver> notificationReceivers = new();
                listOfToken.ForEach(item =>
                {
                    notificationReceivers.Add(new PushNotificationReceiver
                    {
                        IsDelete = false,
                        IsRead = false,
                        NotificationId = messageId,
                        ReadDate = null,
                        ReceivedBy = item.PublicKey

                    });
                });
                _ = _mongoDbService.SaveNotificationReceiverData(notificationReceivers);

            }).ConfigureAwait(false);


            return responseModel;
        }

        public async Task<ApiCommonResponseModel> MarkNotificationAsRead(IdModel notificationObjectId)
        {
            var updateToMongo = await _mongoDbService.ReadPushNotification(notificationObjectId.Id.ToString());

            if (updateToMongo)
            {
                responseModel.Message = "Successfull.";
                responseModel.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                responseModel.Message = "Failed To Read Notification.";
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
            }

            return responseModel;
        }
        public async Task<ApiCommonResponseModel> SavePushNotification(NotificationRequestModel request)
        {
            var categoryId = await _dbContext.ProductsM
                .Where(item => item.Code == request.Topic)
                .Select(item => item.CategoryID)
                .FirstOrDefaultAsync();

            var currentDate = DateTime.Now.Date;

            var query = from mb in _dbContext.MyBucketM
                        join p in _dbContext.ProductsM on mb.ProductId equals p.Id into joinedProducts
                        from product in joinedProducts.DefaultIfEmpty()
                        where product.Code.ToUpper() == request.Topic.ToUpper() &&
                              mb.StartDate <= currentDate && (mb.EndDate ?? currentDate) >= currentDate
                        select new
                        {
                            mobileUserKey = mb.MobileUserKey
                        };
            var mobileUserKeyList = await query.ToListAsync();
            if (mobileUserKeyList.Count > 0)
            {
                List<Model.MongoDbCollection.PushNotification> notificationList = new();
                foreach (var item in mobileUserKeyList)
                {
                    Model.MongoDbCollection.PushNotification notification = new()
                    {
                        Title = request.Title,
                        AppCode = request.AppCode,
                        Complexity = request.Complexity,
                        EnableTradingButton = request.EnableTradingButton,
                        Exchange = request.Exchange,
                        Message = request.Body,
                        OrderType = request.CallType,
                        Price = request.Price,
                        ProductId = request.Product.ToString(),
                        ProductName = request.ProductName,
                        Quantity = request.Quantity,
                        Token = request.Token,
                        TradingSymbol = request.TradingSymbol,
                        TransactionType = request.TransactionType,
                        Validity = request.Validity,
                        CreatedOn = DateTime.Now,
                        CategoryId = categoryId,
                        Topic = request.Topic.ToUpper(),
                    };
                    notificationList.Add(notification);
                }

                await _mongoDbService.SavePushNotification(notificationList);
            }

            ;

            responseModel.Message = "Successfull";
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }


        public async Task<ApiCommonResponseModel> ManageProductNotification(bool allowNotify, Guid mobileUserKey,
            int productId)
        {
            var result = await _dbContext.MyBucketM.Where(item =>
                    item.MobileUserKey == mobileUserKey && item.ProductId == productId && item.IsActive &&
                    !item.IsExpired)
                .FirstOrDefaultAsync();
            if (result != null)
            {
                result.Notification = allowNotify;
                await _dbContext.SaveChangesAsync();
                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Message = "Updated Successfully";
                responseModel.Data = allowNotify;
            }
            else
            {
                responseModel.StatusCode = HttpStatusCode.NotFound;
                responseModel.Message = "Data Not Found.";
                responseModel.Data = allowNotify;
            }

            return responseModel;
        }
        public async Task<ApiCommonResponseModel> SendAdvertismentNotification(AdModel param)
        {
            Dictionary<string, string> dictionary = new();

            if (param.Data is not null)
            {
                dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(param.Data)!;
            }

            string fileName = "";
            if (param.CampaignImage is not null)
            {
                fileName = await SaveImageToAssetsFolderAsync(param.CampaignImage);
                AdvertisementImageM adImage = new()
                {
                    CreatedBy = Guid.Parse(_configuration.GetSection("AppSettings:DefaultAdmin").Value!),
                    CreatedOn = DateTime.Now,
                    Url = dictionary.TryGetValue("LandingPage", out string redirectUrl) ? redirectUrl : null,
                    IsActive = true,
                    IsDelete = false,
                    Name = fileName,
                    Type = "CAMPAIGN"
                };
                await _dbContext.AdvertisementImageM.AddAsync(adImage);
                await _dbContext.SaveChangesAsync();
            }

            if (dictionary.TryGetValue("ImageUrl", out string imageUrl))
            {
                dictionary.Add("ImageUrl", imageUrl);
            }
            else
            {
                dictionary.Add("ImageUrl", fileName);
            }

            List<SqlParameter> sqlParameters = new()
            {
                new SqlParameter
                {
                    ParameterName = "targetAudience",
                    Value = string.IsNullOrEmpty(param.TargetAudience) ? DBNull.Value : param.TargetAudience,
                    SqlDbType = SqlDbType.VarChar
                },
            };
            var spResult =
                await _dbContext.SqlQueryToListAsync<GetCampaignFcmTokenSpResponseModel>(
                    ProcedureCommonSqlParametersText.GetCampaignFcmTokenM, sqlParameters.ToArray());

            List<string> listOfToken = spResult.Select(c => c.FcmToken).ToList();

            if (listOfToken != null && listOfToken.Count > 0)
            {
                await SendFCMSilentNotification(listOfToken, dictionary);
                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Message = "Notification Sent.";
                return responseModel;
            }

            responseModel.StatusCode = HttpStatusCode.InternalServerError;
            return responseModel;
        }
        private async Task<string> SaveImageToAssetsFolderAsync(IFormFile advertisementImage)
        {
            string rootDirectory = Directory.GetCurrentDirectory();

            string assetsDirectory = Path.Combine(rootDirectory, "Assets", "Advertisement");

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
        public async Task<object> SendNotificationToMobile(NotificationToMobileRequestModel param)
        {
            try
            {
                // 1. Handle potential null or empty input:
                if (string.IsNullOrEmpty(param.Mobile))
                {
                    var receiverEmpty = new List<MobileUser>(); // Return empty list if input is null or empty
                    return receiverEmpty;
                }


                // 2. Split the input string by comma and trim whitespace:
                var mobileNumbers = param.Mobile.Split(',').Select(s => s.Trim('\'', ' ')).ToList(); // Trim both ' and spaces


                // 3. Query the database using Contains:
                var receiver = await _dbContext.MobileUsers
                    .Where(item => mobileNumbers.Contains(item.Mobile))
                    .Select(item => new UserListForPushNotificationModel
                    {
                        PublicKey = item.PublicKey,
                        FirebaseFcmToken = item.FirebaseFcmToken,
                        OldDevice = _firebaseNotification.IsOldDevice(item.DeviceVersion, item.DeviceType),
                        FullName = item.FullName
                    })
                    .ToListAsync();

                if (receiver is not null && receiver.Count > 0)
                {
                    var paramDict = new Dictionary<string, string>
                    {
                        { "Title", param.Title },
                        { "Body", param.Body },
                        { "Mobile", param.Mobile },
                        { "Topic", param.Topic },
                        { "ScreenName", param.ScreenName },
                    };
                    var responseFcm = await _firebaseNotification.SendFCMMessage(param.Title, param.Body, receiver, paramDict, withNotificationPayload: true);

                    if (responseFcm != null && responseFcm.StatusCode != HttpStatusCode.OK)
                    {
                        await _log.AddAsync(new Log { CreatedOn = DateTime.Now, Message = responseFcm.Message, Source = "SendFCMMessage", Category = "FailedToSend" });
                    }

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {

                await _exception.AddAsync(new ExceptionLog
                {
                    CreatedOn = DateTime.Now,
                    Message = ex.Message,
                    Source = "fcmToken",
                    StackTrace = ex.StackTrace,
                    InnerException = ex.InnerException?.Message,
                    RequestBody = JsonConvert.SerializeObject(param),
                });
                return false;
            }
        }

        public async Task<bool> SendAdvertismentNotification2(AdvertismentModel2 param)
        {
            List<UserListForPushNotificationModel> listOfToken = await _dbContext.MobileUsers.Where(item =>
            !string.IsNullOrEmpty(item.FirebaseFcmToken) && item.FirebaseFcmToken.Length > 60 &&
                    item.IsActive == true && item.IsDelete == false)
                .Select(item => new UserListForPushNotificationModel { PublicKey = item.PublicKey, FirebaseFcmToken = item.FirebaseFcmToken, OldDevice = _firebaseNotification.IsOldDevice(item.DeviceVersion, item.DeviceType) })
                .ToListAsync();

            if (listOfToken != null && listOfToken.Count > 0)
            {
                _ = await _firebaseNotification.SendFCMMessage(param.Title, param.Body, listOfToken.Where(item => item is not null).ToList(), param.Data);
                return true;
            }

            return false;
        }
        public async Task<ApiCommonResponseModel> SendFreeNotification(SendFreeNotificationRequestModel param)
        {
            if (!string.IsNullOrWhiteSpace(param.Topic))
            {
                var product = await _dbContext.ProductsM
                    .Where(p => p.Code == param.Topic)
                    .FirstOrDefaultAsync();
                if (product == null)
                {
                    return new ApiCommonResponseModel
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = "Topic not found."
                    };
                }
            }

            List<UserListForPushNotificationModel> listOfToken = new();
            if (param.TargetAudience == "UnMatchedDevice")
            {
                listOfToken = await GetUserWhichNeedToNotifyForAppUpdate();
            }
            else
            {
                // Prepare SQL parameters
                List<SqlParameter> sqlParameters =
                [
                    new SqlParameter { ParameterName = "@AudianceCategory", Value = param.TargetAudience, SqlDbType = SqlDbType.VarChar },
                    new SqlParameter { ParameterName = "@topic", Value = param.Topic, SqlDbType = SqlDbType.VarChar },
                    new SqlParameter { ParameterName = "@mobile", Value = string.IsNullOrEmpty(param.Mobile) ? DBNull.Value : param.Mobile, SqlDbType = SqlDbType.VarChar },
                    new SqlParameter { ParameterName = "@ProductId", Value = (object)DBNull.Value, SqlDbType = SqlDbType.Int }
                ];

                // Execute stored procedure
                listOfToken = await _dbContext.SqlQueryToListAsync<UserListForPushNotificationModel>(
                    ProcedureCommonSqlParametersText.GetTargetAudianceListForPushNotification, sqlParameters.ToArray());

                if (listOfToken == null || listOfToken.Count == 0)
                {
                    return new ApiCommonResponseModel
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = "Mobile User not found."
                    };
                }
                else
                {
                    listOfToken = listOfToken
                        .Where(item => item.FirebaseFcmToken is not null && item.FirebaseFcmToken.Length > 50)
                        //.Select(item => new MobileUserDto { MobileUserKey = item.PublicKey, FirebaseFcmToken = item.FirebaseFcmToken })
                        .ToList();
                }
            }

            // Send notifications if TurnOnNotification is true
            if (param.TurnOnNotification)
            {
                var data = new Dictionary<string, string>
                {
                    { "Scanner", "False" },
                };

                var responseFcm = await _firebaseNotification.SendFCMMessage(param.Title, param.Body, listOfToken, data);

                if (responseFcm != null && responseFcm.StatusCode != HttpStatusCode.OK)
                {
                    await _log.AddAsync(new Log { CreatedOn = DateTime.Now, Message = responseFcm.Message, Source = "SendFCMMessage", Category = "FailedToSend" });
                }
            }

            var formattedDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            // Save notification to MongoDB
            Model.MongoDbCollection.PushNotification notificationPayload = new()
            {
                Title = param.Title,
                Message = param.Body,
                Scanner = false,
                CreatedOn = DateTime.Now,
                Topic = param.Topic,
                ScreenName = param.NotificationScreenName
            };
            _ = await _mongoDbService.SaveNotificationDataAsync(notificationPayload, listOfToken);



            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Notification sent."
            };
        }

        public async Task<ApiCommonResponseModel> SendNotificationViaCrm(SendFreeNotificationRequestModel param)
        {
            #region Apply validation 
            if (param == null || string.IsNullOrWhiteSpace(param.Topic))
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Invalid request parameters."
                };
            }
            #endregion

            List<UserListForPushNotificationModel> listOfToken = [];
            string? NotificationImage = ""; //if there is no image i am sending empty string 

            var sqlParameters = new List<SqlParameter>{
            new("@AudianceCategory", param.TargetAudience) { SqlDbType = SqlDbType.VarChar },
            new("@topic", param.Topic) { SqlDbType = SqlDbType.VarChar },
            new("@mobile", string.IsNullOrEmpty(param.Mobile) ? DBNull.Value : param.Mobile) { SqlDbType = SqlDbType.VarChar },
            new("@ProductId", param.ProductId ?? (object)DBNull.Value) { SqlDbType = SqlDbType.Int },
            new("@FromDate", param.FromDate.HasValue ? (object)param.FromDate.Value.Date : DBNull.Value) { SqlDbType = SqlDbType.Date },
            new("@ToDate", param.ToDate.HasValue ? (object)param.ToDate.Value.Date : DBNull.Value) { SqlDbType = SqlDbType.Date }
            };

            listOfToken = await _dbContext.SqlQueryToListAsync<UserListForPushNotificationModel>(ProcedureCommonSqlParametersText.GetTargetAudianceListForPushNotification, sqlParameters.ToArray());
            if (listOfToken is not null && listOfToken.Count > 0)
            {
                // Filter out tokens that are null or too short
                listOfToken = listOfToken.Where(item => !string.IsNullOrWhiteSpace(item.FirebaseFcmToken) && item.FirebaseFcmToken.Length > 50).ToList();
            }
            if (listOfToken is not null && listOfToken.Count == 0)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Mobile User not found."
                };
            }
            if (param.Image is not null)
            {
                NotificationImage = await _azureBlobStorageService.UploadImage(param.Image);
            }

            var allUsers = listOfToken.ToList();

            #region send notification to mobile users
            if (param.TurnOnNotification)
            {
                // Filter out users who have notification = false
                var notificationEnabledUsers = listOfToken.Where(user => user.Notification == null || user.Notification == true).ToList();

                if (notificationEnabledUsers.Count > 0)
                {
                    var data = new Dictionary<string, string>
                    {
                        { "Scanner", "False" },
                        { "ProductId", param.ProductId.ToString()},
                        { "ProductName", param.ProductName},
                        { "ScreenName", param.NotificationScreenName },
                        { "NotificationImage", !string.IsNullOrEmpty(NotificationImage) ? _configuration["Azure:ImageUrlSuffix"] + NotificationImage : "" }
                    };

                    var responseFcm = await _firebaseNotification.SendFCMMessage(param.Title, param.Body, notificationEnabledUsers, data);
                    if (responseFcm?.StatusCode != HttpStatusCode.OK)
                    {
                        await _log.AddAsync(new Log { CreatedOn = DateTime.Now, Message = responseFcm.Message, Source = "SendFCMMessage", Category = "FailedToSend" });
                    }
                }
            }
            #endregion

            #region Save notification to MongoDB
            var formattedDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var notificationPayload = new Model.MongoDbCollection.PushNotification
            {
                Title = param.Title,
                Message = param.Body,
                Scanner = false,
                CreatedOn = DateTime.Now,
                Topic = param.Topic,
                ScreenName = param.NotificationScreenName,
                ProductId = param.ProductId.ToString(),
                ProductName = param.ProductName,
                ImageUrl = NotificationImage
            };
            _ = await _mongoDbService.SaveNotificationDataAsync(notificationPayload, allUsers);

            #endregion

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Notification sent."
            };
        }

        public async Task<ApiCommonResponseModel> SendFreeWhatsappNotification(SendFreeWhatsAppNotificationRequestModel param)
        {
            var response = new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK
            };

            List<SqlParameter> sqlParameters = new()
            {
                new SqlParameter
                {
                    ParameterName = "@AudianceCategory", Value = param.TargetAudience, SqlDbType = SqlDbType.VarChar
                },
                new SqlParameter { ParameterName = "@topic", Value = param.Topic, SqlDbType = SqlDbType.VarChar },
                new SqlParameter { ParameterName = "@mobile", Value = DBNull.Value, SqlDbType = SqlDbType.VarChar },
                new SqlParameter { ParameterName = "@ProductId", Value = (object)DBNull.Value, SqlDbType = SqlDbType.Int }

            };
            var targetAudienceList =
                await _dbContext.SqlQueryToListAsync<UserListForPushNotificationModel>(
                    ProcedureCommonSqlParametersText.GetTargetAudianceListForPushNotification, sqlParameters.ToArray());

            if (targetAudienceList is not null)
            {
                foreach (var item in targetAudienceList)
                {
                    //ToDo: Logic to send the what's app message to each user 1 by 1 , after replacing the name, links etc
                }
            }
            return response;
        }


        /// <summary>
        /// Send 1 notification at a time
        /// </summary>
        public async Task<bool> ValidateAllFCMTokens(NotificationToMobileRequestModel param)
        {
            var mobileUserTemp = await _dbContext.MobileUsers.Where(item =>
                  !string.IsNullOrEmpty(item.FirebaseFcmToken) && item.FirebaseFcmToken.Length > 60 &&
                  item.IsActive == true && item.IsDelete == false).ToListAsync();

            List<UserListForPushNotificationModel> listOfToken =
                             mobileUserTemp.Select(item => new UserListForPushNotificationModel
                             {
                                 FirebaseFcmToken = item.FirebaseFcmToken,
                                 PublicKey = item.PublicKey,
                                 OldDevice = _firebaseNotification.IsOldDevice(item.DeviceVersion, item.DeviceType)
                             }).ToList();

            if (listOfToken != null && listOfToken.Count > 0)
            {
                var responseFcm = await _firebaseNotification.SendFCMMessage(param.Title, param.Body, listOfToken, null, withNotificationPayload: true);

                if (responseFcm != null && responseFcm.StatusCode == HttpStatusCode.OK)
                {
                    var batchResponseList = (List<BatchResponse>)responseFcm.Data;

                    foreach (var batchResponse in batchResponseList)
                    {
                        if (batchResponse != null && batchResponse.FailureCount > 0)
                        {
                            var currentDate = DateTime.Now;
                            for (int i = 0; i < batchResponse.Responses.Count; i++)
                            {
                                if (!batchResponse.Responses[i].IsSuccess)
                                {
                                    string failedToken = listOfToken[i].FirebaseFcmToken; // Get the token that failed
                                    string errorMessage = batchResponse.Responses[i].Exception.Message; // Get error reason

                                    var itemUsers = mobileUserTemp.Where(item => item.FirebaseFcmToken == failedToken).ToList();

                                    foreach (var itt in itemUsers)
                                    {
                                        itt.FirebaseFcmToken = null;
                                        itt.ModifiedOn = currentDate;
                                    }

                                    Console.WriteLine($"Failed Token: {failedToken}, Error: {errorMessage}");
                                    await _log.AddAsync(new Log { CreatedOn = DateTime.Now, Message = errorMessage, Source = "SendFCMMessage", Category = "" });
                                }
                            }
                            await _dbContext.SaveChangesAsync();
                        }
                        else
                        {
                            await _log.AddAsync(new Log { CreatedOn = DateTime.Now, Message = "FCM Null Response", Source = "FCM Null Response", Category = "" });
                        }
                    }
                }
                else
                {
                    await _log.AddAsync(new Log { CreatedOn = DateTime.Now, Message = responseFcm.Message, Source = "SendFCMMessage", Category = "FailedToSend" });
                }
                return true;
            }
            return false;
        }
        public Task<ApiCommonResponseModel?> DeleteSevenDaysOldNotification()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method will fetch all the users whose mobile device version is not matching 
        /// , so that on result we can send push notification to update the app to latest
        /// </summary>
        public async Task<List<UserListForPushNotificationModel>> GetUserWhichNeedToNotifyForAppUpdate()
        {
            var listOfTokens = new List<UserListForPushNotificationModel>();

            try
            {
                // Fetch latest versions from config
                var androidVersion = _configuration["AppSettings:Versions:Android"]!;
                var iosVersion = _configuration["AppSettings:Versions:iOS"]!;

                // Fetch latest versions from the database if available
                var currentVersion = await _dbContext.Settings
                    .Where(item => item.Code == "IosCurrentVersion" || item.Code == "AndroidCurrentVersion")
                    .ToListAsync();

                if (currentVersion is not null)
                {
                    iosVersion = currentVersion.FirstOrDefault(item => item.Code == "IosCurrentVersion")?.Value ?? iosVersion;
                    androidVersion = currentVersion.FirstOrDefault(item => item.Code == "AndroidCurrentVersion")?.Value ?? androidVersion;
                }

                // Fetch all mobile users with a valid FCM token
                var mobileUsers = await _dbContext.MobileUsers
                    .Where(u => u.FirebaseFcmToken != null && u.FirebaseFcmToken.Length > 20)
                    .ToListAsync();

                foreach (var mobileUser in mobileUsers)
                {
                    // Check if the user is running an outdated version
                    if (!string.IsNullOrEmpty(mobileUser.DeviceVersion) && !string.IsNullOrEmpty(mobileUser.DeviceType))
                    {
                        bool isAndroid = mobileUser.DeviceType.StartsWith("Android:", StringComparison.OrdinalIgnoreCase);
                        bool isIos = mobileUser.DeviceType.StartsWith("IOS", StringComparison.OrdinalIgnoreCase) ||
                                     mobileUser.DeviceType.StartsWith("IosId:", StringComparison.OrdinalIgnoreCase);

                        bool needsUpdate = false;

                        if (isIos)
                        {
                            needsUpdate = mobileUser.DeviceVersion != iosVersion;
                        }
                        else if (isAndroid)
                        {
                            needsUpdate = mobileUser.DeviceVersion != androidVersion;
                        }

                        if (needsUpdate)
                        {
                            listOfTokens.Add(new UserListForPushNotificationModel
                            {
                                FirebaseFcmToken = mobileUser.FirebaseFcmToken,
                                PublicKey = mobileUser.PublicKey,

                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return listOfTokens;
        }


        public async Task<ApiCommonResponseModel?> ManageScheduleNotification(ScheduledNotificationRequestModel notification)
        {
            if (notification.Id == 0)
            {
                ScheduledNotification scheduled = new()
                {
                    LandingScreen = notification.NotificationScreenName,
                    AllowRepeat = notification.AllowRepeat,
                    Body = notification.Body,
                    CreatedBy = notification.PublicKey,
                    CreatedOn = DateTime.Now,
                    IsSent = false,
                    TargetAudience = notification.TargetAudience,
                    ModifiedBy = notification.PublicKey,
                    ModifiedOn = DateTime.Now,
                    ScheduledTime = notification.ScheduledTime,
                    Title = notification.Title,
                    IsActive = true,
                    Topic = notification.Topic,
                    ScheduledEndTime = notification.ScheduledTime.AddDays(1)
                };

                _dbContext.ScheduledNotificationM.Add(scheduled);
                await _dbContext.SaveChangesAsync();

                return new ApiCommonResponseModel
                {
                    Message = "Scheduled Notification Added Successfuly.",
                    StatusCode = HttpStatusCode.OK
                };
            }

            var item = await _dbContext.ScheduledNotificationM.FirstOrDefaultAsync(n => n.Id == notification.Id);

            if (item == null)
            {
                return new ApiCommonResponseModel
                {
                    Message = "Scheduled Notification Not Found...",
                    StatusCode = HttpStatusCode.NotFound
                };
            }

            item.LandingScreen = notification.NotificationScreenName;
            item.AllowRepeat = notification.AllowRepeat;
            item.Body = notification.Body;
            item.IsSent = false;
            item.TargetAudience = notification.TargetAudience;
            item.ModifiedBy = notification.PublicKey;
            item.ModifiedOn = DateTime.Now;
            item.ScheduledTime = notification.ScheduledTime;
            item.IsActive = true;
            item.Title = notification.Title;
            item.Topic = notification.Topic;
            item.ScheduledEndTime = notification.ScheduledTime.AddDays(1);

            await _dbContext.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                Message = "Scheduled Notification Updated Successfuly.",
                StatusCode = HttpStatusCode.OK
            };

        }
        public async Task<ApiCommonResponseModel> DeleteNotification(string notificationObjectId)
        {
            var deleteFromMongo = await _mongoDbService.DeleteNotificationAsync(notificationObjectId);

            if (deleteFromMongo)
            {
                responseModel.Message = "Notification deleted successfully.";
                responseModel.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                responseModel.Message = "Failed to delete notification.";
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
            }

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> MarkAllNotificationAsRead(Guid mobileUserKey)
        {
            var notifications = await _mongoDbService.MarkAllNotificationAsRead(mobileUserKey);
            return new ApiCommonResponseModel
            {
                Message = "Data Fetched Successfully",
                StatusCode = HttpStatusCode.OK,
                Data = notifications
            };
        }


        //public async Task<ApiCommonResponseModel> GetScheduleNotification(QueryValues queryValues)
        //{
        //    var apiCommonResponse = new ApiCommonResponseModel();

        //    try
        //    {
        //        // Step 1: Prepare common SQL parameters
        //        List<SqlParameter> sqlParameters = ProcedureCommonSqlParameters.GetCommonSqlParameters(queryValues);

        //        // Step 2: Create the output parameter for TotalCount
        //        SqlParameter parameterOutValue = new SqlParameter
        //        {
        //            ParameterName = "@TotalCount", // Ensure the name matches exactly
        //            SqlDbType = SqlDbType.Int,
        //            Direction = ParameterDirection.Output,
        //        };

        //        // Add the output parameter to the list of SQL parameters
        //        sqlParameters.Add(parameterOutValue);

        //        // Step 4: Execute the stored procedure and fetch the data
        //        List<ScheduledNotificationModel> notificationsList = await _dbContext.SqlQueryToListAsync<ScheduledNotificationModel>(
        //            "EXEC GetScheduledNotifications @IsPaging, @PageSize, @PageNumber, @SortExpression, @SortOrder, @SearchText, @FromDate, @ToDate, @TotalCount OUTPUT",
        //            sqlParameters.ToArray()
        //        );

        //        // Step 5: Retrieve the output parameter value (TotalCount)
        //        int totalRecords = parameterOutValue.Value != DBNull.Value ? Convert.ToInt32(parameterOutValue.Value) : 0;

        //        // Step 6: Prepare the response
        //        apiCommonResponse.Data = notificationsList;
        //        apiCommonResponse.StatusCode = HttpStatusCode.OK;
        //        apiCommonResponse.Total = totalRecords;

        //        return apiCommonResponse;
        //    }
        //    catch (Exception ex)
        //    {
        //        apiCommonResponse.StatusCode = HttpStatusCode.InternalServerError;
        //        apiCommonResponse.Message = $"An error occurred: {ex.Message}";
        //        return apiCommonResponse;
        //    }
        //}
    }
}