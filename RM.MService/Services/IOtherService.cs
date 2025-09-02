using Azure;
using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.ResearchMantraContext;
using RM.Database.MongoDbContext;
using RM.Model;
using RM.Model.MongoDbCollection;
using RM.Model.RequestModel;
using RM.Model.ResponseModel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System.Data;
// using static iTextSharp.text.pdf.AcroFields;

namespace RM.MService.Services
{
    public interface IOtherService
    {
        Task<bool> CheckProductValidity(int productId, Guid mobileUserKey);
        Task<ApiCommonResponseModel> ClearUserData(string mobileNumber);
        Task SendWhatsappOrderConfirmationAsync(WhatsAppOrderConfirmationRequest requestModel);
        Task SendWhatsappOrderFailureAsync(WhatsAppOrderConfirmationRequest requestModel);
        Task SendWhatsappSubscriptionExpiryNotificationAsync();
        Task LmsWebhookWhatsAppAsync();
    }

    public class OtherService(ResearchMantraContext ResearchMantraContext, IConfiguration configuration, IMongoRepository<ExceptionLog> exception, IMongoRepository<Log> logger) : IOtherService
    {
        private readonly ApiCommonResponseModel responseModel = new();
        private readonly ResearchMantraContext _context = ResearchMantraContext;
        private readonly IMongoRepository<Log> _logger = logger;
        private readonly IMongoRepository<ExceptionLog> _exception = exception;

        public IConfiguration _config { get; } = configuration;

        public async Task<bool> CheckProductValidity(int productId, Guid mobileUserKey)
        {
            var dd = await _context.MyBucketM.Where(item => item.ProductId == productId && item.MobileUserKey == mobileUserKey && item.EndDate > DateTime.Now.Date).FirstOrDefaultAsync();
            return dd != null;
        }

        public async Task<ApiCommonResponseModel> ClearUserData(string mobileNumber)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>
            {
                new SqlParameter
                {
                    ParameterName = "MobileNumber", Value = mobileNumber,
                    SqlDbType = SqlDbType.VarChar, Size = 50
                },
            };

            var result = await _context.SqlQueryFirstOrDefaultAsync2<DeleteMobileUserSpResponseModel>(ProcedureCommonSqlParametersText.DeleteMobileUserData, sqlParameters.ToArray());
            responseModel.Message = result.Result;
            return responseModel;
        }

        public async Task SendWhatsappOrderConfirmationAsync(WhatsAppOrderConfirmationRequest requestModel)
        {
            try
            {
                var options = new RestClientOptions(_config["GupShup:Url"]!);
                var restClient = new RestClient(options);
                var request = new RestRequest("", Method.Post);

                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddHeader("accept", "application/json");
                request.AddHeader("apikey", _config["GupShup:ApiKey"]!);
                request.AddHeader("Cache-Control", "no-cache");

                request.AddParameter("channel", "whatsapp");
                request.AddParameter("source", _config["GupShup:GupShupMobile"]);
                request.AddParameter("src.name", _config["GupShup:Appname"]);
                request.AddParameter("destination", $"{requestModel.CountryCode}{requestModel.MobileNumber}");

                string templateId;
                string templateJson;
                if(!string.IsNullOrEmpty(requestModel.ProductCode) && requestModel.ProductCode == "BREAKFASTSTRATEGY")
                {
                    templateId = _config["GupShup:BreakfastOrderConfirmationTemplateId"]!;
                    templateJson = $"{{\"id\":\"{templateId}\",\"params\":[\"{requestModel.CustomerName}\",\"{requestModel.MobileNumber}\",\"{requestModel.StartDate}\",\"{requestModel.MobileNumber}\",\"{requestModel.StartDate}\"]}}";
                }
                else if (!string.IsNullOrEmpty(requestModel.ProductCode) && requestModel.ProductCode == "MIS")
                {
                    templateId = _config["GupShup:MISOrderConfirmationTemplateId"]!;
                    templateJson = $@"{{ ""id"": ""{templateId}"",""params"": [""{requestModel.CustomerName}"",""{requestModel.ProductName}"", ""{requestModel.ProductName}"",""{requestModel.ValidityInDays}"",""{requestModel.StartDate}"",""{requestModel.EndDate}"", ""{requestModel.ProductValue}"",""{requestModel.BonusProduct}"",""{requestModel.Community}""]}}";
                }
                else
                {
                    if (!string.IsNullOrEmpty(requestModel.BonusProduct) && !string.IsNullOrEmpty(requestModel.BonusProductValidity))
                    {
                        templateId = _config["GupShup:OrderConfirmationWithBonusTemplateId"]!;
                        templateJson = $"{{\"id\":\"{templateId}\",\"params\":[\"{requestModel.CustomerName}\",\"{requestModel.ProductName}\",\"{requestModel.Products}\",\"{requestModel.ValidityInDays}\",\"{requestModel.StartDate}\",\"{requestModel.EndDate}\",\"{requestModel.ProductValue}\",\"{requestModel.BonusProduct}\",\"{requestModel.BonusProductValidity}\"]}}";
                    }
                    else
                    {
                        templateId = _config["GupShup:OrderConfirmationTemplateId"]!;
                        templateJson = $"{{\"id\":\"{templateId}\",\"params\":[\"{requestModel.CustomerName}\",\"{requestModel.ProductName}\",\"{requestModel.Products}\",\"{requestModel.ValidityInDays}\",\"{requestModel.StartDate}\",\"{requestModel.EndDate}\",\"{requestModel.ProductValue}\"]}}";
                    }
                }

                request.AddOrUpdateParameter("template", templateJson);

                var response = await restClient.PostAsync(request);

                await _logger.AddAsync(new Log
                {
                    CreatedOn = DateTime.Now,
                    Message = response.IsSuccessful
                        ? $"Order confirmation message sent successfully. Status: {response.StatusCode}, Content: {response.Content}"
                        : $"Order confirmation message failed. Status: {response.StatusCode}, Content: {response.Content}",
                    Source = "GupShup",
                    Category = "WhatsappOrderConfirmation"
                });
            }
            catch (Exception ex)
            {

                await _exception.AddAsync(new ExceptionLog
                {
                    Source = "OTP",
                    RequestBody = JsonConvert.SerializeObject(requestModel),
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace,
                    CreatedOn = DateTime.Now
                });
            }
        }

        public async Task SendWhatsappOrderFailureAsync(WhatsAppOrderConfirmationRequest requestModel)
        {
            var options = new RestClientOptions(_config["GupShup:Url"]!);
            var restClient = new RestClient(options);
            var request = new RestRequest("", Method.Post);

            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddHeader("accept", "application/json");
            request.AddHeader("apikey", _config["GupShup:ApiKey"]!);
            request.AddHeader("Cache-Control", "no-cache");

            request.AddParameter("channel", "whatsapp");
            request.AddParameter("source", _config["GupShup:GupShupMobile"]);
            request.AddParameter("src.name", _config["GupShup:Appname"]);
            request.AddParameter("destination", $"{requestModel.CountryCode}{requestModel.MobileNumber}");

            string templateId = _config["GupShup:OrderFailureTemplateId"]!;
            string templateJson = $"{{\"id\":\"{templateId}\",\"params\":[\"{requestModel.CustomerName}\",\"{requestModel.ProductName}\"]}}";

            request.AddOrUpdateParameter("template", templateJson);

            try
            {
                var response = await restClient.PostAsync(request);

                await _logger.AddAsync(new Log
                {
                    CreatedOn = DateTime.Now,
                    Message = response.IsSuccessful
                        ? $"Order failure message sent successfully. Status: {response.StatusCode}, Content: {response.Content}"
                        : $"Order failure message failed. Status: {response.StatusCode}, Content: {response.Content}",
                    Source = "GupShup",
                    Category = "WhatsappOrderFailure"
                });
            }
            catch (Exception ex)
            {
                await _exception.AddAsync(new ExceptionLog
                {
                    Source = "whatsapp",
                    RequestBody = JsonConvert.SerializeObject(requestModel),
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace,
                    CreatedOn = DateTime.Now
                });
            }
        }

        public async Task SendWhatsappSubscriptionExpiryNotificationAsync()
        {
            var today = DateTime.Today;
            var fiveDaysLater = today.AddDays(5);
            try
            {
                var data = from mobile in _context.MobileUsers
                           join bucket in _context.MyBucketM
                               on mobile.PublicKey equals bucket.MobileUserKey
                           where bucket.EndDate >= today && bucket.EndDate <= fiveDaysLater && bucket.IsActive == true
                           select new
                           {
                               CountryCode = mobile.CountryCode,
                               MobileNumber = mobile.Mobile,
                               CustomerName = mobile.FullName,
                               ServiceName = bucket.ProductName,
                               DaysLeft = EF.Functions.DateDiffDay(today, bucket.EndDate)
                           };
//#if DEBUG
//                data = data.Where(item=> item.MobileNumber == "6309373318");
//#endif
                foreach (var item in data)
                {

                    try
                    {
                        // await _context.MyBucketM.Where(item => item.ProductId == productId && item.MobileUserKey == mobileUserKey && item.EndDate > DateTime.Now.Date).FirstOrDefaultAsync();

                        WhatsAppSubscriptionExpiryRequest requestModel = new WhatsAppSubscriptionExpiryRequest
                        {
                            CountryCode = item.CountryCode,
                            MobileNumber = item.MobileNumber,
                            CustomerName = item.CustomerName,
                            DaysLeft = item.DaysLeft.ToString(),
                            ServiceName = item.ServiceName
                        };
                        var options = new RestClientOptions(_config["GupShup:Url"]!);
                        var restClient = new RestClient(options);
                        var request = new RestRequest("", Method.Post);

                        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                        request.AddHeader("accept", "application/json");
                        request.AddHeader("apikey", _config["GupShup:ApiKey"]!);
                        request.AddHeader("Cache-Control", "no-cache");

                        request.AddParameter("channel", "whatsapp");
                        request.AddParameter("source", _config["GupShup:GupShupMobile"]);
                        request.AddParameter("src.name", _config["GupShup:Appname"]);
                        request.AddParameter("destination", $"{requestModel.CountryCode}{requestModel.MobileNumber}");

                        var templateId = _config["GupShup:SubscriptionExpiryTemplateId"]!;
                        var templateJson = $"{{\"id\":\"{templateId}\",\"params\":[\"{requestModel.ServiceName}\",\"{requestModel.CustomerName}\",\"{requestModel.ServiceName}\",\"{requestModel.DaysLeft}\"]}}";


                        request.AddOrUpdateParameter("template", templateJson);

                        var response = await restClient.PostAsync(request);

                        await _logger.AddAsync(new Log
                        {
                            CreatedOn = DateTime.Now,
                            Message = response.IsSuccessful
                                ? $"Subscription expiry message sent successfully. Status: {response.StatusCode}, Content: {response.Content}"
                                : $"Subscription expiry message failed. Status: {response.StatusCode}, Content: {response.Content}",
                            Source = "GupShup",
                            Category = "WhatsappSubscriptionExpiry"
                        });
                    }
                    catch (Exception ex)
                    {
                        await _exception.AddAsync(new ExceptionLog
                        {
                            Source = "WhatsappSubscriptionExpiry",
                            RequestBody = JsonConvert.SerializeObject(item),
                            Message = ex.Message,
                            InnerException = ex.InnerException?.Message,
                            StackTrace = ex.StackTrace,
                            CreatedOn = DateTime.Now
                        });
                    }
                }
            }
            catch(Exception ex)             
            {
                await _logger.AddAsync(new Log
                {
                    CreatedOn = DateTime.Now,
                    Message = "Issues in data rerival from the database.",
                    Source = "Database",
                    Category = "WhatsappSubscriptionExpiry"
                });
            }
        }

        public async Task LmsWebhookWhatsAppAsync()
        {
            WhatsAppSubscriptionExpiryRequest requestModel = new WhatsAppSubscriptionExpiryRequest
            {
                CountryCode = "91",
                MobileNumber = "9542883533",
                CustomerName = "Nagaraju",
                DaysLeft = "5",
                ServiceName = "Demo"
            };
            var options = new RestClientOptions(_config["GupShup:Url"]!);
            var restClient = new RestClient(options);
            var request = new RestRequest("", Method.Post);

            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddHeader("accept", "application/json");
            request.AddHeader("apikey", _config["GupShup:ApiKey"]!);
            request.AddHeader("Cache-Control", "no-cache");

            request.AddParameter("channel", "whatsapp");
            request.AddParameter("source", _config["GupShup:GupShupMobile"]);
            request.AddParameter("src.name", _config["GupShup:Appname"]);
            request.AddParameter("destination", $"{requestModel.CountryCode}{requestModel.MobileNumber}");

            var templateId = _config["GupShup:SubscriptionExpiryTemplateId"]!;
            var templateJson = $"{{\"id\":\"{templateId}\",\"params\":[\"{requestModel.ServiceName}\",\"{requestModel.CustomerName}\",\"{requestModel.ServiceName}\",\"{requestModel.DaysLeft}\"]}}";


            request.AddOrUpdateParameter("template", templateJson);

            var response = await restClient.PostAsync(request);

            await _logger.AddAsync(new Log
            {
                CreatedOn = DateTime.Now,
                Message = response.IsSuccessful
                    ? $"Subscription expiry message sent successfully. Status: {response.StatusCode}, Content: {response.Content}"
                    : $"Subscription expiry message failed. Status: {response.StatusCode}, Content: {response.Content}",
                Source = "GupShup",
                Category = "WhatsappSubscriptionExpiry"
            });
        }
    }

}
