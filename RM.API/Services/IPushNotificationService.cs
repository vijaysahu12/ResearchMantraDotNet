using iTextSharp.text;
using iTextSharp.text.pdf;
using RM.API.Hub;
using RM.API.Models.PushNotification;
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
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PusherServer;
using Quartz.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using PushNotification = RM.Database.ResearchMantraContext.Tables.PushNotification;

namespace RM.API.Services
{
    public interface IPushNotificationService
    {
        Task<ApiCommonResponseModel> SendNotificationViaCrm(SendFreeNotificationRequestModel param);
        //void SendPushNotification(PushNotificationModel notificationModel);
        Task<ApiCommonResponseModel> GetPushNotification(QueryValues model);
        Task<ApiCommonResponseModel> PostPushNotification(PushNotification pushNotification, List<string> receiverList);
        Task<ApiCommonResponseModel?> ManageScheduleNotification(ScheduledNotificationRequestModel notification);
        Task PushNotification(string receiver, string message, string arg = "PR");
        Task InstaMojoEvent(string message);
        //Task<ApiCommonResponseModel> SendRemainderToUser(string phonenumber);
        Task<ApiCommonResponseModel> SendRemainderToUser(List<SendReminderToUserPushNotificationModel> listOfToken);

    }
    public class PushNotificationService : IPushNotificationService
    {
        private readonly ApiCommonResponseModel responseModel = new();

        private readonly ResearchMantraContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        private readonly FirebaseNotification _firebaseNotification;
        private readonly IConfiguration _configuration;
        private readonly MongoDbService _mongoDbService;
        private readonly IMongoRepository<Log> _mongoRepo;



        public PushNotificationService(IOptions<PushNotificationVM> pushnotification, ResearchMantraContext context,
            IHubContext<NotificationHub> hubContext, IAzureBlobStorageService azureBlobStorageService, FirebaseNotification firebaseNotification,
            IConfiguration configuration, MongoDbService mongoDbService, IMongoRepository<Log> mongoRepo)
        {
            _hubContext = hubContext;
            _context = context;
            _azureBlobStorageService = azureBlobStorageService;
            _firebaseNotification = firebaseNotification;
            _configuration = configuration;
            _mongoDbService = mongoDbService;
            _mongoRepo = mongoRepo;

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

            _context.Database.SetCommandTimeout(180);
            listOfToken = await _context.SqlQueryToListAsync<UserListForPushNotificationModel>(ProcedureCommonSqlParametersText.GetTargetAudianceListForPushNotification, sqlParameters.ToArray());
            if (listOfToken is not null && listOfToken.Count > 0)
            {
                // Filter out tokens that are null or too short
                listOfToken = listOfToken.Where(item => !string.IsNullOrWhiteSpace(item.FirebaseFcmToken) && item.FirebaseFcmToken.Length > 50).ToList();
            }
            //#if DEBUG
            //            listOfToken = listOfToken.Where(item => item.PublicKey == Guid.Parse("1194D99E-C419-EF11-B261-88B133B31C8F")).ToList();
            //#endif
            if (listOfToken is not null && listOfToken.Count == 0)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "FCM Token is Null or Mobile User not found."
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
                        { "ProductId", param.ProductId != 0 ? param.ProductId.ToString() : " " },
                        { "ProductName", param.ProductName != "0" ? param.ProductName : " "},
                        { "ScreenName", param.NotificationScreenName },
                        { "NotificationImage", !string.IsNullOrEmpty(NotificationImage) ? _configuration["Azure:ImageUrlSuffix"] + NotificationImage : "" }
                    };


                    var responseFcm = await _firebaseNotification.SendFCMMessage(param.Title, param.Body, notificationEnabledUsers, data);
                    if (responseFcm?.StatusCode != HttpStatusCode.OK)
                    {
                        await _mongoRepo.AddAsync(new Log()
                        {
                            CreatedOn = DateTime.Now,
                            Message = responseFcm.Message,
                            Source = "SendFCMMessage",
                            Category = "FailedToSend",
                        });
                        //await _mongoDbService.Log(responseFcm.Message, "SendFCMMessage", "FailedToSend", true);
                    }
                }
            }
            #endregion

            #region Save notification to MongoDB
            //var formattedDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var notificationPayload = new Model.MongoDbCollection.PushNotification
            {
                Title = param.Title,
                Message = param.Body,
                Scanner = false,
                CreatedOn = DateTime.Now,
                Topic = param.Topic,
                ScreenName = param.NotificationScreenName,
                ProductId = (param.ProductId == null || param.ProductId == 0) ? string.Empty : param.ProductId.ToString(),
                ProductName = string.IsNullOrWhiteSpace(param.ProductName) ? string.Empty : param.ProductName,
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
                    Topic = notification.Topic,
                    ScheduledEndTime = notification.ScheduledTime.AddDays(1),
                    ProductId = notification.ProductId,           // Nullable - will allow nulls
                    MobileNumber = notification.Mobile,  // Nullable - will allow nulls,
                    IsActive = true
                };

                // Upload image if provided
                if (notification.Image != null)
                {
                    var imageUrl = await _azureBlobStorageService.UploadImage(notification.Image);
                    scheduled.Image = imageUrl;
                }

                if (notification.Body?.Length > 400)
                {
                    return new ApiCommonResponseModel
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Message = $"Body length exceeds the allowed limit of 400 characters. Current length: {notification.Body.Length}."
                    };
                }


                try
                {
                    _context.ScheduledNotificationM.Add(scheduled);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    var innerMessage = ex.InnerException?.Message;
                    throw new Exception($"Database Save Error: {innerMessage}", ex);
                }

                return new ApiCommonResponseModel
                {
                    Message = "Scheduled Notification Added Successfuly.",
                    StatusCode = HttpStatusCode.OK
                };
            }

            var item = _context.ScheduledNotificationM.FirstOrDefault(n => n.Id == notification.Id);

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
            item.Title = notification.Title;
            item.Topic = notification.Topic;
            item.ScheduledEndTime = notification.ScheduledTime.AddDays(1);
            item.ProductId = notification.ProductId;
            item.MobileNumber = notification.Mobile;

            // Update image only if a new image is provided
            if (notification.Image is not null)
            {
                item.Image = await _azureBlobStorageService.UploadImage(notification.Image);
            }

            await _context.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                Message = "Scheduled Notification Updated Successfuly.",
                StatusCode = HttpStatusCode.OK
            };

        }


        public async Task<ApiCommonResponseModel> GetPushNotification(QueryValues model)
        {

            responseModel.Message = "Successfully";
            responseModel.StatusCode = HttpStatusCode.OK;

            SqlParameter parameterOutValue = new()
            {
                ParameterName = "TotalUnreadCount",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output,
            };

            List<SqlParameter> sqlParameters = new()
            {
                new SqlParameter { ParameterName = "PageSize",      Value = model.PageSize,SqlDbType = SqlDbType.Int},
                new SqlParameter { ParameterName = "PageNumber ",   Value = model.PageNumber ,SqlDbType = System.Data.SqlDbType.Int},
                new SqlParameter { ParameterName = "RequestedBy",   Value = string.IsNullOrEmpty(model.RequestedBy) ? DBNull.Value : model.RequestedBy,SqlDbType = SqlDbType.VarChar, Size = 100},
                parameterOutValue
            };

            List<GetProcedureJsonResponse> dd = await _context.SqlQueryToListAsync<GetProcedureJsonResponse>("exec GetPushNotification @PageSize = {0}, @PageNumber  = {1}, @RequestedBy = {2}, @TotalUnreadCount={3} OUTPUT", sqlParameters.ToArray());

            JsonResponseModel jsonTemp = new()
            {
                JsonData = dd.FirstOrDefault()?.JsonData,
                TotalCount = Convert.ToInt32(parameterOutValue.Value)
            };

            responseModel.Data = jsonTemp;

            return responseModel;
        }


        /// <summary>
        /// SignalR Push Notification to topic 
        /// </summary>
        /// <param name="pushNotification"></param>
        /// <param name="receiverList"></param>
        /// <returns></returns>
        public async Task<ApiCommonResponseModel> PostPushNotification(PushNotification pushNotification, List<string> receiverList)
        {
            try
            {
                responseModel.Message = "Added Successfully";

                if (pushNotification.IsImportant == true)
                {
                    pushNotification.Message += " * ";
                }
                _ = _context.PushNotifications.Add(pushNotification);
                _ = await _context.SaveChangesAsync();
                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Data = pushNotification;

                if (receiverList.Count > 0)
                {
                    receiverList.ForEach(async receiver =>
                    {
                        List<PushNotificationRequestModel> str = new()
                        {
                        new PushNotificationRequestModel
                            {
                            Message = pushNotification.Message,
                            Destination = pushNotification.Destination,
                            }
                        };

                        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "PR", receiver.Trim().ToUpper(), str);
                    });
                }

            }
            catch (Exception)
            {
                responseModel.Message = "Added Failed";
            }
            return responseModel;
        }

        public async Task PushNotification(string receiver, string message, string arg = "PR")
        {
            List<PushNotificationRequestModel> str = new()
                        {
                        new PushNotificationRequestModel
                            {
                            Message = message,
                            Destination = null,
                            }
                        };
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", arg, receiver.Trim().ToUpper(), str);
        }

        public async Task InstaMojoEvent(string message)
        {
            await _hubContext.Clients.All.SendAsync("InstaMojoEvent", message);
        }

        public async Task<ApiCommonResponseModel> SendRemainderToUser(List<SendReminderToUserPushNotificationModel> users)
        {
            if (users == null || users.Count == 0)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Invalid request parameters."
                };
            }

            var eligibleUsers = users.Where(u => u.Notification ?? true).ToList();

            if (eligibleUsers.Count == 0)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "No eligible mobile users found."
                };
            }

            foreach (var user in eligibleUsers)
            {
                var pushUser = new UserListForPushNotificationModel
                {
                    FirebaseFcmToken = user.FirebaseFcmToken,
                    PublicKey = user.PublicKey,
                    FullName = user.FullName,
                    OldDevice = user.OldDevice,
                    Notification = user.Notification
                };

                string body = $"The {user.ProductName} product will expire in just {user.DaysToGo} days.";
                string title = "⏳ About to Expire!";

                var data = new Dictionary<string, string>
                {
                    { "Scanner", "False" },
                    { "ProductId", user.ProductId.ToString() },
                    { "ProductName", user.ProductName },
                    { "ScreenName", "productDetailsScreenWidget" },
                    { "NotificationImage", null }
                };

                var response = await _firebaseNotification
                    .SendFCMMessage(title, body, new List<UserListForPushNotificationModel> { pushUser }, data)
                    .ConfigureAwait(false);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    await _mongoRepo.AddAsync(new Log
                    {
                        CreatedOn = DateTime.Now,
                        Message = response.Message,
                        Source = "SendFCMMessage",
                        Category = "FailedToSend",
                    }).ConfigureAwait(false);
                    continue;
                }

                var notificationPayload = new Model.MongoDbCollection.PushNotification
                {
                    Title = title,
                    Message = body,
                    Scanner = false,
                    CreatedOn = DateTime.Now,
                    Topic = user.ProductName,
                    ScreenName = "productDetailsScreenWidget",
                    ProductId = user.ProductId.ToString(),
                    ProductName = user.ProductName,
                    ImageUrl = null
                };

                await _mongoDbService
                    .SaveNotificationDataAsync(notificationPayload, new List<UserListForPushNotificationModel> { pushUser })
                    .ConfigureAwait(false);
            }

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Notifications sent successfully."
            };
        }

    }
}
