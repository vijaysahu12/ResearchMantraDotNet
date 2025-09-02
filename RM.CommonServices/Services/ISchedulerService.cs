using JarvisAlgo.Partner.Service.Alice;
using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.ResearchMantraContext;
using RM.Database.MongoDbContext;
using RM.Model;
using RM.Model.MongoDbCollection;
using RM.Model.RequestModel.Notification;
using RM.Model.ResponseModel;
using RM.NotificationService;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace RM.CommonServices.Services
{
    public class SchedulerServiceMobile
    {
        private readonly ResearchMantraContext _dbContext;
        private readonly MongoDbService _mongoDbService;
        private readonly FirebaseRealTimeDb _firebaseService;
        private readonly FirebaseNotification _firebaseNotification;
        private readonly StockMarketContractsService _stockData;
        private readonly IMongoRepository<Log> _mongoRepo;
        private readonly IMongoRepository<ExceptionLog> _exception;
        private readonly IConfiguration _configuration;
        private readonly string _scheduleTheTaskProp = "ScheduleTheTask";
        DateTime now = DateTime.Now;
        public SchedulerServiceMobile(ResearchMantraContext context, IMongoRepository<Log> mongoRepo, MongoDbService mongoService,
            FirebaseRealTimeDb firebaseService, StockMarketContractsService stockData,
            FirebaseNotification firebaseNotification, IMongoRepository<ExceptionLog> exception, IConfiguration configuration)
        {
            _dbContext = context;
            _mongoDbService = mongoService; _firebaseService = firebaseService;
            _stockData = stockData;
            _firebaseNotification = firebaseNotification;
            _mongoRepo = mongoRepo;
            _exception = exception;
            _configuration = configuration;
        }

        /// <summary>
        /// Updat StockPrice -- Done 
        /// Update Notification IsSent = 0 for allow repeat = true  -- Done
        /// Update BreakfastScanner Data update  -- Done
        /// Change Status Of MyBucket For Expired Service
        /// ResearchReport Stock Price Update -- Done
        /// Mark Old Announcements notification As Deleted -- pending 
        /// API to delete old notification -- pending 
        /// Notification to MobileUsers that your product is about to expire -- pending 
        /// Leads - Free-Trial Service expired 
        /// MyBucket - IsExpired check the logic 
        /// </summary>
        public async Task ScheduleTheTaskOn5MinIntervalUsingWindowTaskScheduler()
        {

            var partner = await _dbContext.PartnerAccountsM.Where(item => item.PartnerId == "323377").FirstOrDefaultAsync();

            if (partner == null)
            {
                return;
            }
            AliceBlueV3 _ant = new()
            {
                _userId = partner.PartnerId,
                _sessionId = partner.GenerateToken
            };

            // Define market start and end times
            TimeSpan marketOpen = new(9, 40, 0);
            TimeSpan marketClose = new(15, 30, 0);
            if (now.TimeOfDay >= marketOpen && now.TimeOfDay <= marketClose)
            {
                // 🔹 Load both Breakfast and Scalping
                var camrillaStocks = await _firebaseService.Read("BreakfastScanner");
                var scalpingStocks = await _firebaseService.ReadScalping("BreakfastScanner");

                // === Breakfast Update ===
                if (camrillaStocks?.Breakfast != null)
                {
                    foreach (var item in camrillaStocks.Breakfast)
                    {
                        var symbolToken = await _stockData.FilterData(item.TradingSymbol, exchange: "NSE");
                        var symbolData = symbolToken.FirstOrDefault();

                        if (symbolData != null)
                        {
                            var tempPrice = await _ant.GetScripQuoteDetails("NSE", symbolData.Token);
                            if (tempPrice != null && Convert.ToDecimal(tempPrice.High) > Convert.ToDecimal(item.Close))
                            {
                                item.DayHigh = tempPrice.High;
                                item.Close = tempPrice.LTP;

                                double previousPrice = Convert.ToDouble(symbolData.Price);
                                double latestPrice = Convert.ToDouble(tempPrice.LTP);

                                item.PercentChange = string.Format("{0:0.00}", ((latestPrice - previousPrice) / previousPrice * 100));
                                item.NetChange = Math.Round(latestPrice - previousPrice, 2);
                            }
                        }
                    }

                    // 🔸 Update only the Breakfast node
                    await _firebaseService.UpdateBreakfastScanner("Breakfast", camrillaStocks.Breakfast);
                }

                // === Scalping Update ===
                if (scalpingStocks?.Stocks != null)
                {
                    foreach (var item in scalpingStocks.Stocks)
                    {
                        var symbolToken = await _stockData.FilterData(item.TradingSymbol, "NSE");
                        var symbolData = symbolToken.FirstOrDefault();

                        if (symbolData != null)
                        {
                            var tempPrice = await _ant.GetScripQuoteDetails("NSE", symbolData.Token);
                            if (tempPrice != null && Convert.ToDecimal(tempPrice.High) > Convert.ToDecimal(item.Close))
                            {
                                item.DayHigh = tempPrice.High;
                                item.Close = tempPrice.LTP;

                                double previousPrice = Convert.ToDouble(symbolData.Price);  // Ensure symbolData is not null
                                double latestPrice = Convert.ToDouble(tempPrice.LTP);    // Ensure tempPrice is not null

                                item.PercentChange = string.Format("{0:0.00}", ((latestPrice - previousPrice) / previousPrice * 100));
                                item.NetChange = Math.Round(latestPrice - previousPrice, 2);
                            }
                        }
                    }

                    // 🔸 Update only the Scalping node
                    await _firebaseService.UpdateBreakfastScanner("Scalping", scalpingStocks.Stocks);
                }
            }
            else if (now.Hour == 15 && now.Minute > 30 && now.Minute < 36) // between 4 pm to 4:10 PM
            {
                var companyDetails = await _dbContext.CompanyDetailM.Where(item => item.IsPublished && !item.IsDelete).ToListAsync();

                foreach (var item in companyDetails)
                {
                    try
                    {
                        if (item.Symbol.Contains(":"))
                        {
                            item.Symbol = Regex.Replace(item.Symbol, @"NSE|BSE|NFO|BFO|:", "", RegexOptions.IgnoreCase);
                        }

                        item.Symbol = item.Symbol.Trim();
                        var symbolToken = await _stockData.FilterData(item.Symbol, exchange: "NSE");
                        //var tempPrice = await _ant.GetScripQuoteDetails("NSE", symbolToken.FirstOrDefault()?.Token);
                        var tempPrice = 0.0;
                        if (symbolToken != null && symbolToken.Count > 0)
                        {
                            tempPrice = symbolToken.FirstOrDefault().Price;
                            item.YesterdayPrice = Convert.ToDecimal(tempPrice);
                        }
                    }
                    catch (Exception ex)
                    {
                        await _exception.AddAsync(new ExceptionLog
                        {
                            CreatedOn = now,
                            Message = ex.Message,
                            Source = "ResearchReport Stock Price Update",
                            StackTrace = ex.StackTrace,
                            InnerException = ex.InnerException?.Message,
                            RequestBody = JsonConvert.SerializeObject(item),
                        });
                        continue;
                    }
                }
                await _dbContext.SaveChangesAsync();
                await _mongoRepo.AddAsync(new Log { CreatedOn = now, Message = "ResearchReport Stock Price Update; triggered at " + now.ToString("dd-MMM-yyyy HH:mm:ss"), Source = "ResearchReportStockPriceUpdate", Category = _scheduleTheTaskProp });
                await this.NotifyToVijaySahu("Research Report Price Update Triggered", "Research Report Price Update Triggered for total Company = " + companyDetails.Count);
            }
            else if (now.Hour == 0 && now.Minute < 6)
            {
                //Reset Push Notification Logs
                await _mongoDbService.MarkOldAnnouncementsAsDeletedAsync();
                await UpdateLeadsFreeTrialService(now);

                //ToDo: Change the DateTime.Now to local variable
                await _mongoRepo.AddAsync(new Log
                {
                    CreatedOn = now,
                    Message = "Free Trial Validation Checked & Old Annoucement Deleted",
                    Source = "FreeTrial & Old Annoucement",
                    Category = _scheduleTheTaskProp
                });
            }
            else if (now.Hour == 3 && now.Minute < 10) // morning between 3 to 3:30 am 
            {
                var expiredNotification = await _dbContext.ScheduledNotificationM.Where(item => item.ScheduledTime <= now).ToListAsync();

                if (expiredNotification.Any())
                {
                    foreach (var item in expiredNotification)
                    {
                        //item.IsActive = false;
                        item.ModifiedOn = now;
                    }
                }

                var repeatingNotifications = await _dbContext.ScheduledNotificationM
                    .Where(n => n.AllowRepeat && now <= n.ScheduledEndTime) // Find notifications that should be sent at this time
                    .ToListAsync();

                if (repeatingNotifications.Any())
                {
                    foreach (var notification in repeatingNotifications)
                    {
                        notification.IsSent = false;
                    }
                }

                if (expiredNotification.Any() || repeatingNotifications.Any())
                {
                    _dbContext.UpdateRange(expiredNotification);// Update all at once
                    _dbContext.UpdateRange(repeatingNotifications);
                    await _dbContext.SaveChangesAsync();// Save changes once
                }

                await _mongoRepo.AddAsync(new Log { CreatedOn = now, Message = $" Total{expiredNotification.Count} expired notification.", Source = "ResetScheduledNotification", Category = _scheduleTheTaskProp });

            }
            else
            {
                await _mongoRepo.AddAsync(new Log
                {
                    CreatedOn = now,
                    Message = "5 min Interval Scheduler triggered at " + now.ToString("dd-MMM-yyyy HH:mm:ss"),
                    Source = "5MinInterval",
                    Category = _scheduleTheTaskProp
                });
            }
        }
        public async Task ChangeStatusOfMyBucketForExpiredService()
        {
            await _mongoRepo.AddAsync(new Log { CreatedOn = now, Message = "Update MyBucket IsExpired & Reminder for ExpiredService triggered at " + now.ToString("dd-MMM-yyyy HH:mm:ss"), Source = "MyBucketExpiry", Category = _scheduleTheTaskProp });
            var mybucketList = await _dbContext.SqlQueryToListAsync<MyBucketExpiredServiceModel>(ProcedureCommonSqlParametersText.GetExpiredServiceFromMyBucket, null);

            if (mybucketList != null)
            {
                foreach (var mybucket in mybucketList)
                {
                    var objNotify = new NotificationRequestModel
                    {
                        Topic = "Announcement",
                        Scanner = "false"
                    };

                    if (mybucket.Status == "Expired") // send notification on product expired 
                    {
                        var bucketIds = mybucket.MyBucketIds.Split(',').Select(id => int.Parse(id.Trim())).ToList();

                        foreach (var bucketId in bucketIds)
                        {
                            var mybucketEntity = new MyBucketM
                            {
                                Id = bucketId,
                                IsExpired = true,
                                Status = "0",
                                ModifiedDate = now
                            };

                            _dbContext.MyBucketM.Attach(mybucketEntity);
                            _dbContext.Entry(mybucketEntity).Property(x => x.IsExpired).IsModified = true;
                            _dbContext.Entry(mybucketEntity).Property(x => x.Status).IsModified = true;
                            _dbContext.Entry(mybucketEntity).Property(x => x.ModifiedDate).IsModified = true;
                        }
                    }
                    else // send notification on product about to expire
                    {
                        objNotify.Title = $"Reminder on subscription Expiring";
                        objNotify.Body = $"Your subscription for the following products: {mybucket.ProductNames} expires soon. Please renew to get timely updates.";
                        await SendNotificationToUsers(objNotify, mybucket);
                    }
                }
                await this.NotifyToVijaySahu("My Bucket Scheduler Triggered", "Update MyBucket IsExpired & Reminder for ExpiredService triggered now.");
                await _dbContext.SaveChangesAsync();
            }
        }
        public async Task SendNotificationToUsers(NotificationRequestModel paramData, MyBucketExpiredServiceModel fcmToken)
        {
            if (paramData != null && string.IsNullOrEmpty(paramData.Topic) && string.IsNullOrEmpty(paramData.Body))
            {
                return;
            }
            else
            {
                paramData.Topic = paramData.Topic.ToUpper();
                paramData.Scanner = paramData.Scanner.ToUpper();
                paramData.CreatedDate = now;
            }

            var dictionary = new Dictionary<string, string>
                    {
                        { "Scanner", paramData.Scanner }
                    };
            List<UserListForPushNotificationModel> fcmList =
            [
                new UserListForPushNotificationModel
                {
                    PublicKey = fcmToken.MobileUserKey,
                    FirebaseFcmToken = fcmToken.FirebaseFcmToken,
                    OldDevice = true
                },
            ];
            var responseFcm = await _firebaseNotification.SendFCMMessage(paramData.Title, paramData.Body, fcmList, paramData.Scanner == "TRUE" ? dictionary : null);

            if (responseFcm != null && responseFcm.StatusCode != HttpStatusCode.OK)
            {
                await _mongoRepo.AddAsync(new Log { CreatedOn = now, Message = responseFcm.Message, Source = "SendFCMMessage", Category = "FailedToSend" });
            }

            _ = Task.Run(async () =>
             {
                 DateTime utcDateTime = paramData.CreatedDate;
                 //string formattedDateTime = utcDateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");

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
                     Topic = paramData.Topic.ToUpper(),
                     ScreenName = "myBucketList"
                 };

                 _ = await _mongoDbService.SaveNotificationDataAsync(notification, fcmList);

             }).ConfigureAwait(false);
        }
        public async Task<ApiCommonResponseModel> RemoveFirebaseDataAndSaveIntoCallPerformanceTable()
        {
            DateTime now = DateTime.Now;

            await _mongoRepo.AddAsync(new Log
            {
                CreatedOn = now,
                Message = "SaveNotificationDataAndRemove Triggered On " + now.ToString("yyyy-MM-dd HH:mm"),
                Source = "Scheduler",
                Category = _scheduleTheTaskProp
            });

            var responseModel = new ApiCommonResponseModel();

            if (now.DayOfWeek != DayOfWeek.Saturday && now.DayOfWeek != DayOfWeek.Sunday)
            {
                #region Get Firebase: Breakfast and Scalping from firebase and then save it in sql and then remove it
                // Get breakfast and scalping notification and save it in sql and then delete it
                var breakfastItems = await _firebaseService.GetBreakFastNotification();
                var scalpingItems = await _firebaseService.GetScalpingNotification();

                List<ScannerPerformanceM> scannerPerformanceMs = new();

                // Handle BREAKFAST
                if (breakfastItems is not null && breakfastItems.Breakfast is not null)
                {
                    DateTime breakfastDate = DateTime.ParseExact(
                        breakfastItems.CurrentDate,
                        "dd MMMM yyyy HH:mm:ss",
                        CultureInfo.InvariantCulture
                    );

                    foreach (var item in breakfastItems.Breakfast)
                    {
                        if (item != null)
                        {
                            bool exists = await _dbContext.ScannerPerformanceM
                                .AnyAsync(x => x.SentAt.Date == now.Date && x.TradingSymbol == item.TradingSymbol && x.Topic == "BREAKFAST");

                            if (!exists)
                            {
                                scannerPerformanceMs.Add(new ScannerPerformanceM
                                {
                                    CreatedOn = now,
                                    Ltp = ClampDecimal(item.Ltp, 99999999.99m, 2),
                                    NetChange = ClampDecimal(item.NetChange.ToString(), 99999999.99m, 2),
                                    PercentChange = ClampDecimal(item.PercentChange, 999.99m, 2),
                                    SentAt = breakfastDate,
                                    TradingSymbol = item.TradingSymbol,
                                    ViewChart = item.ViewChart,
                                    Topic = "BREAKFAST"
                                });
                            }
                        }
                    }
                }

                // Handle SCALPING
                if (scalpingItems is not null)
                {
                    DateTime scalpingDate = DateTime.ParseExact(
                        breakfastItems.CurrentDate,
                        "dd MMMM yyyy HH:mm:ss",
                        CultureInfo.InvariantCulture
                    );

                    foreach (var item in scalpingItems)
                    {
                        if (item != null)
                        {
                            bool exists = await _dbContext.ScannerPerformanceM
                                .AnyAsync(x => x.SentAt.Date == now.Date && x.TradingSymbol == item.TradingSymbol && x.Topic == "SCALPING");

                            if (!exists)
                            {
                                scannerPerformanceMs.Add(new ScannerPerformanceM
                                {
                                    CreatedOn = now,
                                    Ltp = ClampDecimal(item.Ltp, 99999999.99m, 2),
                                    NetChange = ClampDecimal(item.NetChange.ToString(), 99999999.99m, 2),
                                    PercentChange = ClampDecimal(item.PercentChange, 999.99m, 2),
                                    SentAt = scalpingDate,
                                    TradingSymbol = item.TradingSymbol,
                                    ViewChart = item.ViewChart,
                                    Topic = "SCALPING"
                                });
                            }
                        }
                    }
                }

                // Save Firebase data if any
                if (scannerPerformanceMs.Any())
                {
                    await _dbContext.ScannerPerformanceM.AddRangeAsync(scannerPerformanceMs);
                    int insertedCount = await _dbContext.SaveChangesAsync();

                    if (insertedCount > 0)
                    {
                        await _firebaseService.ClearBreakfastData(); // Clears both Breakfast & Scalping
                    }
                }

                #endregion

                #region Get MongoDB: Other Topics

                string[] notificationList = { "KALKIBAATAAJ", "BREAKOUT", "MORNINGSHORT" };

                foreach (string notification in notificationList)
                {
                    var mongoItems = await _mongoDbService.GetNotificationFromTopic(notification);
                    List<ScannerPerformanceM> mongoScannerPerformanceMs = new();

                    foreach (var item in mongoItems)
                    {
                        mongoScannerPerformanceMs.Add(new ScannerPerformanceM
                        {
                            CreatedOn = now,
                            SentAt = Convert.ToDateTime(item.CreatedOn),
                            TradingSymbol = item.TradingSymbol,
                            Topic = item.Topic,
                            Message = item.Message,
                            Ltp = item.Price
                        });
                    }

                    if (mongoScannerPerformanceMs.Any())
                    {
                        await _dbContext.ScannerPerformanceM.AddRangeAsync(mongoScannerPerformanceMs);
                        int insertCount = await _dbContext.SaveChangesAsync();

                        if (insertCount > 0)
                        {
                            await _mongoDbService.DeleteNotificationForTopic(notification);
                        }
                    }
                }

                #endregion
            }

            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Successful";
            return responseModel;
        }

        public async Task UpdatePartnerAccountGenerateToken(string generateToken)
        {
            await _mongoRepo.AddAsync(new Log { CreatedOn = now, Message = $"Partner Account Update :  generateToken: {generateToken} " + now.ToString("dd-MMM-yyyy HH:mm:ss"), Source = "UpdatePartnerAccountGenerateToken", Category = _scheduleTheTaskProp });

            var partner = await _dbContext.PartnerAccountDetails.Where(item => item.PartnerCId == "323377").FirstOrDefaultAsync();

            if (partner != null && !string.IsNullOrEmpty(generateToken))
            {
                partner.GenerateToken = generateToken;
                partner.ModifiedOn = now;
                await _dbContext.SaveChangesAsync();
                await this.NotifyToVijaySahu("Auto Login Done For Alice Blue", "UpdatePartnerAccountGenerateToken for " + partner.PartnerCId + " executed successfully.");
            }
        }
        private async Task NotifyToVijaySahu(string title, string message)
        {

            List<SqlParameter> sqlParameters =
            [
                new SqlParameter
                    {
                        ParameterName = "productCode",
                        Value = "ANNOUNCEMENT",
                        SqlDbType = SqlDbType.VarChar
                    }
            ];
            var objMobileUser = await _dbContext.MobileUsers.Where(item => item.Mobile == "9411122233").FirstOrDefaultAsync();

            if (objMobileUser is not null)
            {
                var notificationReceivers = new List<UserListForPushNotificationModel>
                    {
                        new UserListForPushNotificationModel
                        {
                            FirebaseFcmToken = objMobileUser.FirebaseFcmToken,
                            FullName = objMobileUser.FullName,
                            Notification = null,
                            OldDevice = false,
                            PublicKey = objMobileUser.PublicKey
                        }
                    };

                var dictionary = new Dictionary<string, string> { { "Scanner", "false" }, { "Topic", "ANNOUNCEMENT" } };
                var responseFcm = await _firebaseNotification.SendFCMMessage(title, message, notificationReceivers, null);
            }
        }
        /// <summary>
        /// This method is used to send the notification which are schedule for future thorugh CRM notification scheduled screen, using window task scheduler
        /// </summary>
        public async Task<bool> SendScheduledNotification()
        {

            await _mongoRepo.AddAsync(new Log { CreatedOn = now, Message = "Scheduled Notification triggered at " + now.ToString("dd-MMM-yyyy HH:mm:ss"), Source = "SendScheduledNotification", Category = _scheduleTheTaskProp });



            //var dueNotifications = await _dbContext.ScheduledNotificationM.Where(n => (n.IsActive ?? true) && n.AllowRepeat && !n.IsSent)
            //                        .ToListAsync();

            ////dueNotifications = dueNotifications.Where(n =>
            ////         n.ScheduledTime.TimeOfDay <= now.TimeOfDay.Add(TimeSpan.FromMinutes(5)) && n.ScheduledTime.TimeOfDay >= now.TimeOfDay.Subtract(TimeSpan.FromMinutes(5)) || // Time check with buffer for repeating notifications
            ////          (now >= n.ScheduledTime && now <= n.ScheduledEndTime)) // Date and time check for non-repeating
            ////        .ToList();


            //dueNotifications = dueNotifications.Where(n =>
            //            (n.AllowRepeat &&
            //                n.ScheduledTime.TimeOfDay <= now.TimeOfDay.Add(TimeSpan.FromMinutes(5)) &&
            //                n.ScheduledTime.TimeOfDay >= now.TimeOfDay.Subtract(TimeSpan.FromMinutes(5)))
            //            ||
            //                (!n.AllowRepeat && now >= n.ScheduledTime && now <= n.ScheduledEndTime)).ToList();



            // Execute stored procedure
            var dueNotifications = await _dbContext.SqlQueryToListAsync<ScheduledNotificationSPResponse>(ProcedureCommonSqlParametersText.GetDueNotifications);

            if (dueNotifications == null || dueNotifications?.Count == 0)
            {
                return false;
            }

            foreach (var notification in dueNotifications)
            {
                try
                {
                    List<SqlParameter> sqlParameters =
                    new()
                    {
                        new SqlParameter { ParameterName = "@AudianceCategory", Value = notification.TargetAudience, SqlDbType = SqlDbType.VarChar },
                        new SqlParameter { ParameterName = "@topic", Value = notification.Topic, SqlDbType = SqlDbType.VarChar },
                        new SqlParameter { ParameterName = "@mobile", Value = string.IsNullOrEmpty(notification.MobileNumber) ? DBNull.Value : notification.MobileNumber, SqlDbType = SqlDbType.VarChar },
                        new SqlParameter { ParameterName = "@ProductId", Value = notification.ProductId > 0 ? notification.ProductId : DBNull.Value, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@FromDate", Value = DBNull.Value, SqlDbType = SqlDbType.DateTime },
                        new SqlParameter { ParameterName = "@EndDate", Value = DBNull.Value, SqlDbType = SqlDbType.DateTime }
                    };

                    // Execute stored procedure
                    var resultSp = await _dbContext.SqlQueryToListAsync<UserListForPushNotificationModel>(
                        ProcedureCommonSqlParametersText.GetTargetAudianceListForPushNotification, sqlParameters.ToArray());

                    if (resultSp != null && resultSp.Count != 0)
                    {
                        var listOfToken = resultSp
                            .Where(item => item.FirebaseFcmToken is not null && item.FirebaseFcmToken.Length > 50)
                            .ToList();

                        var product = _dbContext.ProductsM.FirstOrDefault(x => x.Id == notification.ProductId);

                        var data = new Dictionary<string, string>
                        {
                            { "Scanner", "False" },
                            { "ProductId", notification.ProductId.ToString() ?? string.Empty },
                            { "ProductName", product?.Name ?? string.Empty },
                            { "ScreenName", notification.LandingScreen },
                            { "NotificationImage", !string.IsNullOrEmpty(notification.Image) ? _configuration["Azure:ImageUrlSuffix"] + notification.Image : string.Empty }
                        };


                        var responseFcm = _ = await _firebaseNotification.SendFCMMessage(notification.Title, notification.Body, listOfToken, data, notification.Topic);

                        if (responseFcm != null && responseFcm.StatusCode != HttpStatusCode.OK)
                        {
                            await _mongoRepo.AddAsync(new Log { CreatedOn = now, Message = responseFcm.Message, Source = "SendFCMMessage", Category = "FailedToSend" });
                        }


                        #region Save notification to MongoDB
                        //var formattedDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                        var notificationPayload = new PushNotification
                        {
                            Title = notification.Title,
                            Message = notification.Body,
                            Scanner = false,
                            CreatedOn = now,
                            Topic = notification.Topic,
                            ProductId = notification.ProductId.ToString(),
                            ScreenName = notification.LandingScreen

                        };
                        _ = await _mongoDbService.SaveNotificationDataAsync(notificationPayload, listOfToken);

                        #endregion

                        var scheduledNotificationMResponse = await _dbContext.ScheduledNotificationM.Where(item => item.Id == notification.Id).FirstOrDefaultAsync();

                        if (scheduledNotificationMResponse != null)
                        {
                            scheduledNotificationMResponse.IsSent = true;
                            scheduledNotificationMResponse.ModifiedOn = now;
                            await _dbContext.SaveChangesAsync();
                        }
                        Console.WriteLine($"Notification sent successfully: {notification.Title}");
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine($"Error processing notification: {notification.Title}. Exception: {ex.Message}");
                    // Handle exceptions appropriately (e.g., retry, circuit breaker)
                }
            }
            return true;
        }
        #region private methods 

        /// <summary>
        /// Reset firebase data , means take all the breakfast or any other stocks and then update that into mssql db 
        /// </summary>
        /// <returns></returns>
        #endregion
        /// <summary>
        /// Requirement: Should be able to update the stock price for mobile app on daily basis
        /// Result: After this method my daily generateToken for AliceBlue will be updated on db so that through other method I can update the stock price  
        /// </summary>
public async Task<bool> ScheduleThePromotionForMobileAppPopUp()
 {
    var now = DateTime.Now;

            var fiveMinutesBefore = now.AddMinutes(-5);
            var fiveMinutesAfter = now.AddMinutes(5);

            var validPromoIds = await _dbContext.PromotionM
                .Where(p => p.ShouldDisplay == true &&
                            p.IsDelete == false &&
                            p.IsActive == true &&
                            p.ScheduleDate >= fiveMinutesBefore &&
                            p.ScheduleDate <= fiveMinutesAfter)
                .OrderByDescending(p => p.ModifiedOn ?? p.CreatedOn)
                .Select(p => p.Id)
                .ToListAsync();

            if (validPromoIds.Any())
    {
        var latestPromo = await _dbContext.PromotionM
            .Where(p => validPromoIds.Contains(p.Id))
            .OrderByDescending(p => p.ModifiedOn ?? p.CreatedOn)
            .FirstOrDefaultAsync();

        if (latestPromo != null)
        {
            var notificationData = new
            {
                lastChangedAt = (latestPromo.ModifiedOn ?? latestPromo.CreatedOn)?.ToString("dd MMMM yyyy, hh:mm tt") ?? DateTime.Now.ToString("dd MMMM yyyy, hh:mm tt"),
                message = new
                {
                    title = latestPromo.Title,
                    body = "New Promo Available! 📢",
                    isLocalNotification=true

                },
                promoIds = validPromoIds,
                showLocalNotification = latestPromo.IsNotification == true
            };

            await _firebaseService.UpdatePromotionDataAsync("PromotionData", notificationData);
            return true; // ✅ Successfully sent
        }
    }

    return false; // ✅ No promotion found or notification not sent
}

        public async Task UpdateLeadsFreeTrialService(DateTime now)
        {

            string timestamp = now.ToString("dd-MMM-yyyy HH:mm:ss");

            // Log scheduler trigger
            await _mongoRepo.AddAsync(new Log
            {
                CreatedOn = now,
                Message = $"UpdateLeadsFreeTrialService triggered at {timestamp}",
                Source = nameof(UpdateLeadsFreeTrialService),
                Category = _scheduleTheTaskProp
            });

            // Fetch expired trials
            var expiredTrials = await _dbContext.LeadFreeTrials
                .Where(trial => trial.EndDate.Date < now.Date && trial.IsActive)
                .ToListAsync();

            if (!expiredTrials.Any())
                return;

            foreach (var trial in expiredTrials)
            {
                trial.IsActive = false;
                trial.ModifiedOn = now;
            }

            _dbContext.UpdateRange(expiredTrials);
            await _dbContext.SaveChangesAsync();

            // Notify after deactivation
            await NotifyToVijaySahu(
                $"Total: {expiredTrials.Count} FreeTrial Deactivated",
                $"Total: {expiredTrials.Count} , Free trials have been marked inactive based on EndDate."
            );
        }


        private decimal? ClampDecimal(string value, decimal max, int scale)
        {
            if (decimal.TryParse(value, out var parsed))
            {
                if (parsed > max) return max;
                if (parsed < -max) return -max;
                return Math.Round(parsed, scale); // Round to DB column scale
            }
            return null;
        }
    }

    public class ScheduledNotificationSPResponse
    {
        public int Id { get; set; } // Assuming 'Id' is your primary key
        public bool? IsActive { get; set; } // Matches SQL BIT, nullable bool for ISNULL(IsActive, 1)
        public bool AllowRepeat { get; set; } // Matches SQL BIT
        public bool IsSent { get; set; }     // Matches SQL BIT
        public DateTime ScheduledTime { get; set; }
        public DateTime ScheduledEndTime { get; set; }

        // New columns added based on your stored procedure
        public string TargetAudience { get; set; } // Assuming string type
        public string MobileNumber { get; set; }   // Assuming string type for phone numbers
        public int ProductId { get; set; }       // Assuming int type for ProductId
        public string Topic { get; set; }
        public string LandingScreen { get; set; }
        public string? Image { get; set; }
        public string Body { get; set; }
        public string Title { get; set; }

        // Add any other properties here that correspond to columns you might add in the future.
    }
}