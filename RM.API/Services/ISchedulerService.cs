using RM.API.Hub;
using RM.API.Models;
using RM.API.Models.Mail;
using RM.CommonServices.Services;
using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.KingResearchContext;
using RM.Database.KingResearchContext.Tables;
using RM.Database.MongoDbContext;
using RM.Model.DB.Tables;
using RM.Model.MongoDbCollection;
using RM.Model.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using PushNotification = RM.Database.KingResearchContext.Tables.PushNotification;

namespace RM.API.Services
{
    public interface ISchedulerService
    {
        public Task NotifyOnExpiredServices(string ExpiredInDays);
        public Task UpdateExpiredService();
        public Task GetUntouchedLeads();
        public Task NotifyFollowUpReminder();
        public Task UpdateUntouchedLeadsToNull();
        public Task UpdateContracts();
    }

    [Authorize]
    public class SchedulerService : ISchedulerService
    {
        private readonly KingResearchContext _context;
        private readonly IMongoRepository<Log> _mongoRepo;
        private readonly IMailService _mailService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IConfiguration _config;
        private readonly IStocksService _stocksService;


        public SchedulerService(IStocksService stocksService, KingResearchContext context, IMongoRepository<Log> mongoRepo, IMailService mailService,
            IHubContext<NotificationHub> hubContext, IPushNotificationService pushNotificationService, IConfiguration config)
        {
            _config = config;
            _context = context;
            _mongoRepo = mongoRepo;
            _mailService = mailService;
            _hubContext = hubContext;
            _pushNotificationService = pushNotificationService;
            _stocksService = stocksService;

        }

        /// <summary>
        /// Notify the Sales Team and The Lead that their service going to expired soon.
        /// </summary>
        /// <param name="ExpiredInDays"></param>
        /// <returns></returns>
        public async Task NotifyOnExpiredServices(string ExpiredInDays)
        {
            #region Step 1 Get the Expired Service List Details from DB
            List<SqlParameter> sqlParameters = new()
            {
               new SqlParameter { ParameterName = "ExpiredInDays", Value =  ExpiredInDays == null ? DBNull.Value : ExpiredInDays ,SqlDbType = System.Data.SqlDbType.VarChar,Size = 100},
            };

            List<ExpiredServiceResponseModel> expiredListResult = await _context.SqlQueryToListAsync<ExpiredServiceResponseModel>(ProcedureCommonSqlParametersText.GetExpiredServices, sqlParameters.ToArray());

            #endregion

            #region Step 2 Send Mail To Customer and Push Notification to the assigned lead 

            if (expiredListResult != null && expiredListResult.Count > 0)
            {
                MailRequest dd = new();
                string AdminRole = _config.GetValue<string>("AppSettings:AdminRole");

                List<string> receiverList = new();

                foreach (ExpiredServiceResponseModel expired in expiredListResult)
                {
                    dd.ToEmail = expired.Receiver;
                    dd.Subject = expired.Subject;
                    dd.Body = expired.Body;
                    dd.CcEmail = expired.CC;

                    await _mailService.SendEmailAsync(dd);

                    receiverList.Add(expired.UserKey.ToString());
                    //_ = await _mongoDbService.InsertPushNotification(new CRMPushNotificationCollection
                    //{
                    //    CreatedBy = Guid.Parse(AdminRole),
                    //    CreatedDate = DateTime.Now,
                    //    Userkey = expired.UserKey,
                    //    IsActive = true,
                    //    IsRead = false,
                    //    Message = expired.FullName + " " + " subcription for " + expired.SubscriptionName + " about to expire in next " + expired.DaysToGo + " days.",
                    //    ModifiedBy = Guid.Parse(AdminRole),
                    //    ModifiedDate = DateTime.Now,
                    //    Source = "NotifyOnExpiredServices",
                    //    Destination = NotificationScreenEnum.Lead.ToString(),
                    //});
                    _ = await _pushNotificationService.PostPushNotification(new PushNotification
                    {
                        CreatedBy = Guid.Parse(AdminRole),
                        CreatedDate = DateTime.Now,
                        Userkey = expired.UserKey,
                        IsActive = true,
                        IsRead = false,
                        Message = expired.FullName + " " + " subcription for " + expired.SubscriptionName + " about to expire in next " + expired.DaysToGo + " days.",
                        ModifiedBy = Guid.Parse(AdminRole),
                        ModifiedDate = DateTime.Now,
                        Source = "NotifyOnExpiredServices",
                        Destination = $"{NotificationScreenEnum.SpecificLead}:{expired.MobileNumber}",
                    }, receiverList);
                    receiverList.Clear();
                }
            }
            #endregion

            Log log = new()
            {
                Message = "NotifyOnExpiredServices Scheduler Triggered",
                CreatedOn = DateTime.Now,
                Source = "NotifyOnExpiredServices"
            };

            _ = _mongoRepo.AddAsync(log);
            _ = await _context.SaveChangesAsync();
        }

        public async Task NotifyFollowUpReminder()
        {
            List<FollowUpReminderResponseModel> resultReminders = await _context.SqlQueryToListAsync<FollowUpReminderResponseModel>(ProcedureCommonSqlParametersText.GetFollowUpReminder);
            //var finallist = resultReminders.Select(item => item.Comments).ToList();

            if (resultReminders != null && resultReminders.Count > 0)
            {
                _ = _config.GetValue<string>("AppSettings:FinanceAdmin");
                List<string> receiverList = new();

                foreach (FollowUpReminderResponseModel item in resultReminders)
                {
                    receiverList.Add(item.AssignedTo.ToString());
                    //_ = await _mongoDbService.InsertPushNotification(new CRMPushNotificationCollection
                    //{
                    //    CreatedBy = item.AssignedTo,
                    //    CreatedDate = DateTime.Now,
                    //    IsActive = true,
                    //    IsImportant = false,
                    //    IsRead = false,
                    //    Message = item.Comments,
                    //    ModifiedBy = item.AssignedTo,
                    //    ModifiedDate = DateTime.Now,
                    //    ReadDate = DateTime.Now,
                    //    Userkey = item.AssignedTo
                    //});
                    _ = await _pushNotificationService.PostPushNotification(new PushNotification
                    {
                        CreatedBy = item.AssignedTo,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        IsImportant = false,
                        IsRead = false,
                        Message = item.Comments,
                        ModifiedBy = item.AssignedTo,
                        ModifiedDate = DateTime.Now,
                        ReadDate = DateTime.Now,
                        Userkey = item.AssignedTo
                    }, receiverList);
                    receiverList.Clear();
                }
            }
        }

        public async Task GetUntouchedLeads()
        {
            #region Step 1 Get the untouched Leads from DB
            List<UntouchedLeadsResponseModel> untouchedLeadsDetails = await _context.SqlQueryToListAsync<UntouchedLeadsResponseModel>(ProcedureCommonSqlParametersText.GetUntouchedLeads);
            #endregion

            #region Step 2 Send notification to sales person

            if (untouchedLeadsDetails != null && untouchedLeadsDetails.Count > 0)
            {
                List<string> receiverList = new();

                foreach (UntouchedLeadsResponseModel item in untouchedLeadsDetails)
                {
                    receiverList.Add(item.AssignedTo.ToString());
                    //_ = await _mongoDbService.InsertPushNotification(new CRMPushNotificationCollection
                    //{
                    //    CreatedBy = item.AdminRole,
                    //    CreatedDate = DateTime.Now,
                    //    IsActive = true,
                    //    IsImportant = false,
                    //    IsRead = false,
                    //    Message = item.Comments,
                    //    ModifiedBy = item.AdminRole,
                    //    ModifiedDate = DateTime.Now,
                    //    ReadDate = DateTime.Now,
                    //});
                    _ = await _pushNotificationService.PostPushNotification(new PushNotification
                    {
                        CreatedBy = item.AdminRole,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        IsImportant = false,
                        IsRead = false,
                        Message = item.Comments,
                        ModifiedBy = item.AdminRole,
                        ModifiedDate = DateTime.Now,
                        ReadDate = DateTime.Now,
                    }, receiverList);
                    receiverList.Clear();
                }
            }
            Log log = new()
            {
                Message = "GetUntouchedLeads Scheduler Triggered",
                CreatedOn = DateTime.Now,
                Source = "GetUntouchedLeads",
                Category = "Scheduler"
            };

            _ = _mongoRepo.AddAsync(log);
            _ = await _context.SaveChangesAsync();
            #endregion
        }
        public async Task UpdateUntouchedLeadsToNull()
        {
            List<UpdateUntouchedLeadsToNullResponseModel> leadsList = await _context.SqlQueryToListAsync<UpdateUntouchedLeadsToNullResponseModel>(ProcedureCommonSqlParametersText.UpdateUntouchedLeadsToNull);

            if (leadsList != null && leadsList.Count > 0)
            {
                foreach (UpdateUntouchedLeadsToNullResponseModel lead in leadsList)
                {
                    await _pushNotificationService.PushNotification(lead.AssignedTo, lead.Message);
                }
            }

            Log log = new()
            {
                Message = "GetUntouchedLeadsAccordingToDays Scheduler Triggered",
                CreatedOn = DateTime.Now,
                Source = "GetUntouchedLeadsAccordingToDays"
            };
            _ = _mongoRepo.AddAsync(log);

            //_ = await _context.SaveChangesAsync();

        }
        public async Task UpdateExpiredService()
        {
            _ = await _context.SqlQueryToListAsync<CommonResponseSp>(ProcedureCommonSqlParametersText.UpdateExpiredServices, null);

            Log log = new()
            {
                Message = "Service Expired Scheduler Triggered",
                CreatedOn = DateTime.Now,
                Source = "UpdateExpiredService"
            };

            _ = _mongoRepo.AddAsync(log);
            _ = await _context.SaveChangesAsync();
        }

        public async Task UpdateContracts()
        {
            HttpClientHandler handler = new()
            {
                CookieContainer = new CookieContainer(),
                UseCookies = true
            };
            using (HttpClient httpClient = new(handler))
            {
                List<string> contractsList = new()
                {
                    "NFO",
                    "NSE"
                };

                foreach (string exchange in contractsList)
                {

                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
                    // Define the URL you want to send the GET request to
                    string apiUrl = "https://v2api.aliceblueonline.com/restpy/contract_master?exch=" + exchange.ToUpper();
                    // Send the GET request and get the response
                    HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
                    // Check if the request was successful (status code 200)
                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();

                        if (apiUrl.Contains("NFO"))
                        {
                            NFOModel objNSO = JsonConvert.DeserializeObject<NFOModel>(responseData);
                            await _stocksService.ManageNFOContracts(objNSO);

                        }
                        else if (apiUrl.Contains("NSE"))
                        {
                            NSEModel objNSO = JsonConvert.DeserializeObject<NSEModel>(responseData);
                            await _stocksService.ManageNSEContracts(objNSO);
                        }
                    }
                }
            }
        }
    }
}