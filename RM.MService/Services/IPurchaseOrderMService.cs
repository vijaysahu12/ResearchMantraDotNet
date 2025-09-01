using Google.Apis.Auth.OAuth2;
// using iTextSharp.text;
using RM.CommonServices;
using RM.CommonServices.Services;
using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.KingResearchContext;
using RM.Database.MongoDbContext;
using RM.Model;
using RM.Model.Common;
using RM.Model.Models;
using RM.Model.RequestModel;
using RM.Model.RequestModel.MobileApi;
using RM.Model.ResponseModel;
using RM.NotificationService;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PusherServer;
using RestSharp;
using sib_api_v3_sdk.Model;
using System.Data;
using System.Data.SqlTypes;
using System.Globalization;
using System.Net;
using System.Text.Json;
using static RM.Database.KingResearchContext.Subscriptions;
using static RM.MService.Services.PurchaseorderMService;
using Task = System.Threading.Tasks.Task;

namespace RM.MService.Services
{
    public interface IPurchaseOrderMService
    {
        public Task<ApiCommonResponseModel> ManagePurchaseOrder(PurchaseOrderMRequestModel request);
        public Task<ApiCommonResponseModel> GenerateCoupon(GenerateCouponRequestModel request);
        //public Task<ApiCommonResponseModel> GenerateUniversalCoupon(GenerateCouponRequestModel request);
        public Task<ApiCommonResponseModel> ValidateCoupon(ValidateCouponRequestModel request);
        public Task<ApiCommonResponseModel> ByPassThePaymentGateway(ByPassPaymentGatewayRequestModel request);
        Task<ApiCommonResponseModel> GetPaymentGatewayDetails(string providerName);
        Task<ApiCommonResponseModel> GetPhonePeResponseStatus(string merchantTransactionId);
        Task<ApiCommonResponseModel> GetInstamojoResponseStatus(string paymentRequestId);
        Task<ApiCommonResponseModel> GetPayUResponseStatus(PaymentStatusRequest paymentStatusRequest);
        Task<ApiCommonResponseModel> VerifyPaymentIsSuccessfullOrNot(bool isPaymentSuccessfull, string paymentRequestId, ProductModel result);
        Task<PaymentRequestStatusM> AddIntoPaymentRequestStatus(PaymentDetailStatusRequestModel request, string status = "PENDING");
        Task<ApiCommonResponseModel> AddIntoPaymentRequestStatusV2(PaymentDetailStatusRequestModel request, string status = "PENDING");
        Task<ApiCommonResponseModel> AddIntoInstaMojoPaymentRequestStatusV2(PaymentDetailStatusRequestModel request);

        Task<ApiCommonResponseModel> InstaMojoWebhokDataProcessing(InstaMojoWebhookData webhookData);
        Task<ApiCommonResponseModel> PayUWebhookDataProcessing(PayUWebhookRequestModel webhookData);
        Task LogToMongo(string message, string category = "PG");
        Task PayULogToMongo(string message, string category);
        Task<ApiCommonResponseModel> GetInvoicesByMobileUserKeyAsync(Guid? mobileUserKey, int pageNumber, int pagiSize);
        Task SendPushNotificationToTheClientAfterPaymentConfirmations(string poResponseDataJson, MobileUser mobileUser, PurchaseOrderMRequestModel purchaseRequest);
    }

    public class PurchaseorderMService : IPurchaseOrderMService
    {
        private readonly KingResearchContext _context;
        ApiCommonResponseModel responseModel = new();
        private readonly IConfiguration _config;
        private readonly ILogger<IPurchaseOrderMService> _logger;
        private readonly IMongoRepository<Model.MongoDbCollection.Log> _mongoRepo;
        private readonly MongoDbService _pushNotificationRepo;
        private readonly IOtherService _otherSerivce;
        private readonly IMobileNotificationService _pushNotification;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public PurchaseorderMService(KingResearchContext context, IConfiguration config, ILogger<IPurchaseOrderMService> logger, IMongoRepository<Model.MongoDbCollection.Log> mongoRepo,
            IOtherService otherSerivce, IMobileNotificationService pushNotification, MongoDbService mongoRepository, IEmailService emailService, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _config = config;
            _logger = logger;
            _mongoRepo = mongoRepo;
            _otherSerivce = otherSerivce;
            _pushNotification = pushNotification;
            _pushNotificationRepo = mongoRepository;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public async Task<ApiCommonResponseModel> ManagePurchaseOrder(PurchaseOrderMRequestModel request)
        {
            if (request.TransactionId == "#2377GTYTY")
            {
                throw new ArgumentException("Invalid Payment");
            }

            List<SqlParameter> sqlParameters = new()
                    {
                        new SqlParameter { ParameterName = "mobileUserKey",   Value = Guid.Parse(request.MobileUserKey)  ,SqlDbType = SqlDbType.UniqueIdentifier},
                        new SqlParameter { ParameterName = "productId",   Value = request.ProductId  ,SqlDbType = SqlDbType.Int},
                        new SqlParameter { ParameterName = "SubscriptionMappingId",   Value = request.SubscriptionMappingId == null ? DBNull.Value  : request.SubscriptionMappingId  ,SqlDbType = SqlDbType.Int},
                        new SqlParameter { ParameterName = "MerchantTransactionId",   Value = request.MerchantTransactionId  ,SqlDbType = SqlDbType.VarChar, Size = 100},
                        new SqlParameter { ParameterName = "TransactionId",   Value = request.TransactionId ,SqlDbType = SqlDbType.VarChar, Size = 100},
                        new SqlParameter { ParameterName = "paidAmount",   Value = request.PaidAmount  ,SqlDbType = SqlDbType.Decimal},
                        new SqlParameter { ParameterName = "couponCode",   Value = string.IsNullOrEmpty(request.CouponCode) ?  DBNull.Value : request.CouponCode   ,SqlDbType = SqlDbType.VarChar, Size = 20}
                    };

            var spResult = await _context.SqlQueryToListAsync<GetProcedureJsonResponse>(ProcedureCommonSqlParametersText.ManagePurchaseOrdersM, sqlParameters.ToArray());

            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Purchase Order Saved Successfully.";
            responseModel.Data = spResult[0].JsonData;

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> ValidateCoupon(ValidateCouponRequestModel request)
        {
            var spResult = await this.ValidateCouponSp(request);
            if (spResult is not null)
            {
                if (spResult.Result?.ToUpper() == "COUPONVALID")
                {
                    responseModel.Data = new { spResult.DeductedPrice };
                    responseModel.StatusCode = HttpStatusCode.OK;
                }
                if (spResult.Result?.ToUpper() == "COUPONINVALID")
                {
                    responseModel.StatusCode = HttpStatusCode.BadRequest;
                }
                if (spResult.Result?.ToUpper() == "COUPONEXPIRED")
                {
                    responseModel.StatusCode = HttpStatusCode.Gone;
                    responseModel.Data = new { spResult.DeductedPrice };
                }
                return responseModel;
            }
            else
            {
                return responseModel;
            }
        }

        private async Task<ValidateCouponSpResponse> ValidateCouponSp(ValidateCouponRequestModel request)
        {
            List<SqlParameter> sqlParameters = new()
                    {
                        new SqlParameter { ParameterName = "mobileUserKey",   Value = request?.MobileUserKey == null ? DBNull.Value : request.MobileUserKey  ,SqlDbType = SqlDbType.UniqueIdentifier},
                        new SqlParameter { ParameterName = "productId",   Value = request?.ProductId == null ? DBNull.Value : request.ProductId   ,SqlDbType = SqlDbType.Int},
                        new SqlParameter { ParameterName = "Coupon",   Value = request.CouponCode  ,SqlDbType = SqlDbType.VarChar,Size = 100},
                        new SqlParameter { ParameterName = "SubscriptionDurationId",   Value = request.SubscriptionDurationId  ,SqlDbType = SqlDbType.Int,Size = 100},
                    };

            ValidateCouponSpResponse spResult = await _context.SqlQueryFirstOrDefaultAsync2<ValidateCouponSpResponse>(ProcedureCommonSqlParametersText.ValidateCouponM, sqlParameters.ToArray());
            return spResult;
        }

        public async Task<ApiCommonResponseModel> GenerateCoupon(GenerateCouponRequestModel request)
        {
            List<SqlParameter> sqlParameters = new()
                    {
                        new SqlParameter { ParameterName = "mobileUserKey",   Value = request.MobileUserKey == null ? DBNull.Value: request.MobileUserKey ,SqlDbType = SqlDbType.UniqueIdentifier},
                        new SqlParameter { ParameterName = "productId",   Value = request.ProductId == null ? DBNull.Value: request.ProductId ,SqlDbType = SqlDbType.Int},
                        new SqlParameter { ParameterName = "couponType",   Value = request.CouponType.ToUpper()  ,SqlDbType = SqlDbType.VarChar,Size=10},
                        new SqlParameter { ParameterName = "discountInPrice",   Value = request.DiscountAmount == null ? DBNull.Value: request.DiscountAmount, SqlDbType = SqlDbType.Decimal},
                        new SqlParameter { ParameterName = "discountInPercentage",   Value = request.DiscountPercent == null ? DBNull.Value: request.DiscountPercent ,SqlDbType = SqlDbType.Decimal},
                        new SqlParameter { ParameterName = "validityInDays",   Value = request.ValidityInDays  ,SqlDbType = SqlDbType.Int},
                        new SqlParameter { ParameterName = "redeemLimit",   Value = request.RedeemLimit  ,SqlDbType = SqlDbType.Int},
                    };

            var spResult = await _context.SqlQueryFirstOrDefaultAsync2<GenerateCouponCodeSpResponse>(ProcedureCommonSqlParametersText.GenerateCouponCode, sqlParameters.ToArray());

            responseModel.Data = spResult?.CouponCode;

            // uncomment below to send sms with coupon

            //string text = _config["Plivo:CouponCodeMessage"]!;
            //text = text.Replace("{couponCode}", spResult.CouponCode.ToString());
            //var api = new PlivoApi(_config["Plivo:AuthId"], _config["Plivo:AuthToken"]);
            //var response = api.Message.Create(
            //    src: _config["Plivo:SenderId"],
            //    dst: "+" + spResult.CountryCode + spResult.Mobile,
            //     text: text
            //    );

            responseModel.Message = "Coupon Generated Successfully.";
            responseModel.StatusCode = HttpStatusCode.OK;

            return responseModel;
        }

        /// <summary>
        /// This method is only for ios device 
        /// Through this he can use the coupon to add the product in their my bucket. 
        public async Task<ApiCommonResponseModel?> ByPassThePaymentGateway(ByPassPaymentGatewayRequestModel request)
        {
            if (Guid.Empty != request.MobileUserKey && Guid.TryParse(request.MobileUserKey.ToString(), out Guid mobileUserKeyNew))
            {
                request.ActionType = request.ActionType.ToLower();
                responseModel.StatusCode = HttpStatusCode.OK;
                var configValue = _config.GetSection("AppSettings:EnableGetCouponForIosDevice").Value;
                switch (request.ActionType)
                {
                    case "checkstatus":
                        responseModel.Data = configValue is null or "true";
                        break;
                    case "apply":
                        {
                            ValidateCouponRequestModel validateCoupon = new()
                            {
                                CouponCode = request.CouponCode,
                                MobileUserKey = request.MobileUserKey,
                                ProductId = request.ProductId
                            };
                            var spResult = await this.ValidateCouponSp(validateCoupon);

                            if (spResult.Result?.ToUpper() == "COUPONVALID" && spResult.DeductedPrice <= 0)
                            {
                                var managePurchaseOrderResponse = await this.ManagePurchaseOrder(new PurchaseOrderMRequestModel
                                {
                                    CouponCode = request.CouponCode,
                                    MobileUserKey = request.MobileUserKey.ToString(),
                                    ProductId = request.ProductId,
                                    PaidAmount = (double)spResult.DeductedPrice,
                                    TransactionId = "WITHOUTPAYMENT",
                                    SubscriptionMappingId = null
                                });

                                responseModel.Data = managePurchaseOrderResponse.Data;
                                responseModel.StatusCode = managePurchaseOrderResponse.StatusCode;
                                responseModel.Message = "The product has been successfully added to your bucket";
                            }
                            else
                            {
                                responseModel.Message = "Invalid Coupon.";
                                responseModel.StatusCode = HttpStatusCode.BadRequest;
                            }

                            break;
                        }
                    case "get":
                        {
                            if (configValue is null or "true")
                            {
                                var user = await _context.MobileUsers.FirstOrDefaultAsync(x => x.PublicKey == request.MobileUserKey);
                                if (user != null) await this.SendLinkForPaymentUsingWhatsapp("91", user.Mobile);
                                else _logger.LogError("User not found for sending payment link with publicKey :{publicKey}", request.MobileUserKey);

                                responseModel.Data = true;
                                responseModel.Message = "Link sent to your registered WhatsApp number.";
                            }
                            else
                            {
                                responseModel.Data = false;
                                responseModel.Message = "";
                            }

                            break;
                        }
                    default:
                        responseModel.StatusCode = HttpStatusCode.BadRequest;
                        break;
                }
            }
            else
            {
                responseModel.StatusCode = HttpStatusCode.BadRequest;
                responseModel.Message = "Mobile user details not found to proceed.. Please relogin & fix the issue";
            }

            return responseModel;
        }

        private async Task<string> SendLinkForPaymentUsingWhatsapp(string countryCode, string mobileNumber)
        {
            try
            {
                var options = new RestClientOptions("https://api.gupshup.io/wa/api/v1/template/msg");
                var client = new RestClient(options);
                var request = new RestRequest("");
                request.AddHeader("accept", "application/json");
                request.AddHeader("apikey", _config["GupShup:ApiKey"]!);
                request.AddParameter("template",
                    $"{{\"id\":\"{_config["GupShup:PaymentLinkTemplateId"]}\",\"params\":[\"{_config["GupShup:PaymentLink"]}\"]}}");
                request.AddParameter("source", _config["GupShup:GupShupMobile"]);
                request.AddParameter("src.name", _config["GupShup:Appname"]);
                request.AddParameter("destination", countryCode + mobileNumber);
                request.AddParameter("channel", "whatsapp");

                var response = await client.PostAsync(request);
                return response.IsSuccessful ? "Message sent" : "Error";
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while sending the WhatsApp payment link: {Message}", e.Message);
                return "";
            }
        }

        public async Task<ApiCommonResponseModel> GetPaymentGatewayDetails(string providerName)
        {

            responseModel.Data = new GetPaymentGatewayDetailsModel();
            responseModel.StatusCode = HttpStatusCode.OK;

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetPhonePeResponseStatus(string merchantTransactionId)
        {
            var _response = new ApiCommonResponseModel();

            ProductModel result = new();
            var response = await _context.PurchaseOrdersM.FirstOrDefaultAsync(x => x.TransasctionReference == merchantTransactionId);
            if (response is null)
            {
                _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                _response.Message = "Transaction Not Found.";
                var requestStatus = await _context.PaymentRequestStatusM.FirstOrDefaultAsync(x => x.MerchantTransactionId == merchantTransactionId);
                if (requestStatus != null)
                {
                    var mobileUser = await _context.MobileUsers.FirstOrDefaultAsync(x => x.PublicKey == requestStatus.CreatedBy);

                    if (mobileUser != null)
                    {
                        var textInfo = CultureInfo.CurrentCulture.TextInfo;
                        var titleCaseFullName = textInfo.ToTitleCase(mobileUser.FullName.ToLower());
                        var product = await _context.ProductsM.FirstOrDefaultAsync(x => x.Id == requestStatus.ProductId);

                        var whatsappRequest = new WhatsAppOrderConfirmationRequest
                        {
                            MobileNumber = mobileUser.Mobile,
                            CustomerName = titleCaseFullName,
                            ProductName = product?.Name,
                            CountryCode = mobileUser.CountryCode
                        };

                        await _mongoRepo.AddAsync(new Model.MongoDbCollection.Log { CreatedOn = DateTime.Now, Message = "Sending WhatsApp Order Failure Notification: " + JsonConvert.SerializeObject(whatsappRequest), Source = "PhonePe", Category = "Failure" });

                        try
                        {
                            await _otherSerivce.SendWhatsappOrderFailureAsync(whatsappRequest);
                        }
                        catch (Exception ex)
                        {
                            await _mongoRepo.AddAsync(new Model.MongoDbCollection.Log { CreatedOn = DateTime.Now, Message = $"Failed to send WhatsApp notification: {ex.Message}", Source = "PhonePe", Category = "PaymentFailed Or Null Response" });


                        }

                    }
                }
                return (_response);
            }
            var myBucket = await _context.MyBucketM.Where(item => item.MobileUserKey == response.ActionBy && item.ProductId == response.ProductId && item.CreatedDate.Date == DateTime.Today).OrderByDescending(item => item.ModifiedDate)
            .FirstOrDefaultAsync();

            if (myBucket is not null)
            {
                result.Name = myBucket.ProductName;
            }

            response.StartDate ??= DateTime.Now;
            response.EndDate ??= DateTime.Now.AddDays(10);

            var ProductValidity = Math.Abs((Convert.ToDateTime(response.StartDate) - Convert.ToDateTime(response.EndDate)).Days);

            result.StartDate = Convert.ToDateTime(response.StartDate).ToString("dd-MMM-yyyy");
            result.EndDate = Convert.ToDateTime(response.EndDate).ToString("dd-MMM-yyyy");
            result.ProductValidity = ProductValidity;
            result.Code = "";

            _response.Data = result;
            _response.StatusCode = System.Net.HttpStatusCode.OK;
            _response.Message = PaymentResponseCodeMessage.GetPaymentResponseMessage("SUCCESS");

            return _response;
        }
        public async Task<ApiCommonResponseModel>  GetInstamojoResponseStatus(string paymentRequestId)
        {
            var _response = new ApiCommonResponseModel();
            ProductModel result = new();

            var requestData = await _context.PaymentRequestStatusM.Where(item => item.MerchantTransactionId == paymentRequestId).FirstOrDefaultAsync();

            if (requestData != null)
            {
                result.ProductId = requestData.ProductId;
                var paymentResponseData = await _context.PaymentResponseStatusM.Where(item => item.MerchantTransactionId == paymentRequestId).FirstOrDefaultAsync();

                var product = await _context.ProductsM
                    .FirstOrDefaultAsync(x => x.Id == requestData.ProductId);

                //if (paymentResponseData == null) // means there is no entry in this table because webhook not executed yet
                //{
                //    result.PaymentStatus = requestData.Status;
                //}

                if (paymentResponseData == null) // // means there is no entry in this table because webhook not executed yet
                {
                    result.PaymentStatus = requestData.Status;
                    await FillSubscriptionDatesAsync(requestData, result, product);
                }
                else
                {
                    bool isSuccess = Convert.ToBoolean(paymentResponseData.Success);
                    result.PaymentStatus = isSuccess ? "SUCCESS" : "FAILED";
                    result.CreatedOn = paymentResponseData.CreatedOn;

                    if (!isSuccess)
                    {
                        await FillSubscriptionDatesAsync(requestData, result, product);
                    }

                    _response = await this.VerifyPaymentIsSuccessfullOrNot(isSuccess, paymentResponseData.MerchantTransactionId, result);
                }
                //else
                //{
                //    result.PaymentStatus = Convert.ToBoolean(paymentResponseData.Success) == true ? "SUCCESS" : "FAILED";
                //    _response = await this.VerifyPaymentIsSuccessfullOrNot(Convert.ToBoolean(paymentResponseData.Success), paymentResponseData.MerchantTransactionId, result);
                //}
            }
            else
            {
                result.PaymentStatus = "NOT_FOUND";
            }
            _response.Data = result;
            _response.StatusCode = System.Net.HttpStatusCode.OK;
            _response.Message = PaymentResponseCodeMessage.GetPaymentResponseMessage(result.PaymentStatus);
            return (_response);
        }

        public async Task<ApiCommonResponseModel> GetPayUResponseStatus(PaymentStatusRequest paymentStatusRequest)
        {
            var _response = new ApiCommonResponseModel();
            ProductModel result = new();

            var requestData = await _context.PaymentRequestStatusM.Where(item => item.MerchantTransactionId == paymentStatusRequest.PaymentRequestId).FirstOrDefaultAsync();

            if(requestData == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.Message = "Data not found based on given PaymentRequestId in PaymentRequestStatusM table.";
                return _response;
            }

            var product = await _context.ProductsM
                                .FirstOrDefaultAsync(x => x.Id == requestData.ProductId);

            if (requestData != null)
            {
                result.ProductId = requestData.ProductId;
                var paymentResponseData = await _context.PaymentResponseStatusM.Where(item => item.MerchantTransactionId == paymentStatusRequest.PaymentRequestId).FirstOrDefaultAsync();

                if (paymentResponseData == null) // webhook not executed yet
                {
                    result.PaymentStatus = requestData.Status;
                   await FillSubscriptionDatesAsync(requestData, result, product);
                }
                else
                {
                    bool isSuccess = Convert.ToBoolean(paymentResponseData.Success);
                    result.PaymentStatus = isSuccess ? "SUCCESS" : "FAILED";
                    result.CreatedOn = paymentResponseData.CreatedOn;

                    if (!isSuccess)
                    {
                        await FillSubscriptionDatesAsync(requestData, result, product);
                    }

                    _response = await this.VerifyPaymentIsSuccessfullOrNot(isSuccess, paymentResponseData.MerchantTransactionId, result);
                }
            }
            else
            {
                result.PaymentStatus = "NOT_FOUND";
            }
            _response.Data = result;
            _response.StatusCode = System.Net.HttpStatusCode.OK;
            _response.Message = PaymentResponseCodeMessage.GetPaymentResponseMessage(result.PaymentStatus);
            return (_response);
        }

        private async Task FillSubscriptionDatesAsync(PaymentRequestStatusM requestData, ProductModel result, ProductsM products)
        {
            var subscriptionMapping = await _context.SubscriptionMappingM
                .FirstOrDefaultAsync(x => x.Id == requestData.SubscriptionMappingId);

            if (subscriptionMapping != null)
            {
                var duration = await _context.SubscriptionDurationM
                    .FirstOrDefaultAsync(x => x.Id == subscriptionMapping.SubscriptionDurationId);

                if (duration != null)
                {
                    var startDate = DateTime.Now;
                    var endDate = startDate.AddMonths(duration.Months);

                    result.StartDate = startDate.ToString("dd-MMM-yyyy");
                    result.EndDate = endDate.ToString("dd-MMM-yyyy");
                    result.ProductValidity = Math.Abs((endDate - startDate).Days);
                    result.Name = products.Name;
                }
            }
        }

        public async Task<ApiCommonResponseModel> VerifyPaymentIsSuccessfullOrNot(bool isPaymentSuccessfull, string paymentRequestId, ProductModel result)
        {
            var _response = new ApiCommonResponseModel();
            if (isPaymentSuccessfull)
            {
                var response = await _context.PurchaseOrdersM.Where(x => x.TransasctionReference.ToLower() == paymentRequestId).FirstOrDefaultAsync();
                if (response is null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.Message = "Transaction Not Found.";
                    var requestStatus = await _context.PaymentRequestStatusM.FirstOrDefaultAsync(x => x.MerchantTransactionId == paymentRequestId);
                    if (requestStatus != null)
                    {
                        var mobileUser = await _context.MobileUsers.FirstOrDefaultAsync(x => x.PublicKey == requestStatus.CreatedBy);

                        if (mobileUser != null)
                        {
                            var textInfo = CultureInfo.CurrentCulture.TextInfo;
                            var titleCaseFullName = textInfo.ToTitleCase(mobileUser.FullName.ToLower());
                            var product = await _context.ProductsM.FirstOrDefaultAsync(x => x.Id == requestStatus.ProductId);

                            var whatsappRequest = new WhatsAppOrderConfirmationRequest
                            {
                                MobileNumber = mobileUser.Mobile,
                                CustomerName = titleCaseFullName,
                                ProductName = product?.Name,
                                CountryCode = mobileUser.CountryCode
                            };

                            await _mongoRepo.AddAsync(new Model.MongoDbCollection.Log { CreatedOn = DateTime.Now, Message = "Sending WhatsApp Order Failure Notification: " + JsonConvert.SerializeObject(whatsappRequest), Source = "PhonePe", Category = "Failure" });

                            try
                            {
                                await _otherSerivce.SendWhatsappOrderFailureAsync(whatsappRequest);
                            }
                            catch (Exception ex)
                            {
                                await _mongoRepo.AddAsync(new Model.MongoDbCollection.Log { CreatedOn = DateTime.Now, Message = $"Failed to send WhatsApp notification: {ex.Message}", Source = "PhonePe", Category = "PaymentFailed Or Null Response" });
                            }
                        }
                    }
                    return (_response);
                }
                //var myBucket = await _context.MyBucketM.Where(item => item.MobileUserKey == response.ActionBy && item.ProductId == response.ProductId && item.CreatedDate.Date == DateTime.Today).OrderByDescending(item => item.ModifiedDate)
                //.FirstOrDefaultAsync();

                var myBucket = await _context.MyBucketM.Where(item => item.MobileUserKey == response.ActionBy && item.ProductId == response.ProductId).OrderByDescending(item => item.ModifiedDate)
                .FirstOrDefaultAsync();

                if (myBucket is not null)
                {
                    result.Name = myBucket.ProductName;
                }

                response.StartDate ??= DateTime.Now;
                response.EndDate ??= DateTime.Now.AddDays(10);

                var ProductValidity = Math.Abs((Convert.ToDateTime(response.StartDate) - Convert.ToDateTime(response.EndDate)).Days);

                result.StartDate = Convert.ToDateTime(response.StartDate).ToString("dd-MMM-yyyy");
                result.EndDate = Convert.ToDateTime(response.EndDate).ToString("dd-MMM-yyyy");
                result.ProductValidity = ProductValidity;
                result.Code = "";
            }

            return _response;
        }

        public async Task<PaymentRequestStatusM> AddIntoPaymentRequestStatus(PaymentDetailStatusRequestModel request, string status = "PENDING")
        {
            try
            {
                var _paymentRequest = await _context.PaymentRequestStatusM
                    .FirstOrDefaultAsync(item => item.MerchantTransactionId == request.MerchantTransactionID);

                if (_paymentRequest is null)
                {
                    PaymentRequestStatusM result = new()
                    {
                        Amount = request.Amount, // default I am getting in rupees so no need to change.
                        CreatedBy = request.MobileUserKey,
                        CreatedOn = DateTime.Now,
                        ProductId = request.ProductId,
                        SubcriptionModelId = request.SubcriptionModelId,
                        SubscriptionMappingId = request.SubscriptionMappingId,
                        MerchantTransactionId = request.MerchantTransactionID,
                        Status = status,
                        CouponCode = request.CouponCode,
                    };

                    _ = await _context.PaymentRequestStatusM.AddAsync(result);
                    _ = await _context.SaveChangesAsync();
                    return result;
                }
                else
                {
                    return _paymentRequest;
                }
            }
            catch (Exception ex)
            {
                // Optional: log the error
                _logger.LogError(ex, "Error while saving PaymentRequestStatusM for transaction {TransactionId}", request.MerchantTransactionID);

                // Optional: throw or return null/custom object
                return null; // or throw; or return new PaymentRequestStatusM { TransactionId = request.MerchantTransactionID, Status = "Error" };
            }
        }

        public async Task<ApiCommonResponseModel> AddIntoPaymentRequestStatusV2(PaymentDetailStatusRequestModel request, string status = "PENDING")
        {
            try
            {
                var _paymentRequest = await _context.PaymentRequestStatusM
                    .FirstOrDefaultAsync(item => item.MerchantTransactionId == request.MerchantTransactionID);

                if (_paymentRequest is null)
                {
                    PaymentRequestStatusM result = new()
                    {
                        Amount = request.Amount,
                        CreatedBy = request.MobileUserKey,
                        CreatedOn = DateTime.Now,
                        ProductId = request.ProductId,
                        SubcriptionModelId = request.SubcriptionModelId,
                        SubscriptionMappingId = request.SubscriptionMappingId,
                        MerchantTransactionId = request.MerchantTransactionID,
                        Status = status,
                        CouponCode = request.CouponCode,
                    };

                    await _context.PaymentRequestStatusM.AddAsync(result);
                    await _context.SaveChangesAsync();
                }

               var payuResponse = await CreatePayUPaymentLink(request);
               // var instamojoResponce = await CreateInstaMojoPaymentLink(request);

                return payuResponse;
               // return instamojoResponce;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving PaymentRequestStatusM for transaction {TransactionId}", request.MerchantTransactionID);
                throw; // Or return an error model
            }
        }



        public async Task<ApiCommonResponseModel> AddIntoInstaMojoPaymentRequestStatusV2(PaymentDetailStatusRequestModel request)
        {
            var instamojoResponce = await CreateInstaMojoPaymentLink(request);

            return instamojoResponce;
        }

        public async Task<ApiCommonResponseModel> CreatePayUPaymentLink(PaymentDetailStatusRequestModel requestModel)
        {
            try
            {
                var paymentRequest = await _context.PaymentRequestStatusM.Where(x => x.MerchantTransactionId == requestModel.MerchantTransactionID).FirstOrDefaultAsync();

                if (paymentRequest == null)
                {
                    // if MerchantTransactionID not found in PaymentRequestStatusM table safly return meassage.
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                    responseModel.Message = $"Error while fetching PaymentRequestStatusM table. MerchantTransactionID is not Found for MerchantTransactionID : {requestModel.MerchantTransactionID}";
                    return responseModel;
                }

                var product = await _context.ProductsM.Where(x => x.Id == paymentRequest.ProductId && x.IsActive == true).FirstOrDefaultAsync();

                if (product == null)
                {
                    // if ProductId not found in ProductsM table safly return meassage.
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                    responseModel.Message = $"Error while fetching ProductsM table. Product is not Found or Inactive for productId : {paymentRequest.ProductId}";
                    return responseModel;
                }

                var mobileUser = await _context.MobileUsers.Where(x => x.PublicKey == paymentRequest.CreatedBy && x.IsActive == true).FirstOrDefaultAsync();

                if (mobileUser == null)
                {
                    // if ProductId not found in ProductsM table safly return meassage.
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                    responseModel.Message = $"Error while fetching MobileUsers table. Mobile User is not Found or Inactive for MobileUserKey : {paymentRequest.CreatedBy}";
                    return responseModel;
                }

                var accessToken = await GetAccessTokenAsync();

                if (string.IsNullOrEmpty(accessToken))
                {
                    responseModel.Message = "Failed to retrieve access token from PayU";
                    responseModel.StatusCode = HttpStatusCode.InternalServerError;
                    return responseModel;
                }

                await PayULogToMongo(accessToken, "PayUAccessToken");

                #region Create Payment Link
                var client = new RestClient(_configuration["PayU:Url"]!);               // Production Host
                                                                                        //var clientUat = new RestClient(_configuration["PayU:UatUrl"]!);       // UAT Host

                var request = new RestRequest("/payment-links", Method.Post); // Correct endpoint

                request.AddHeader("Authorization", $"Bearer {accessToken}");
                request.AddHeader("accept", "application/json");
                request.AddHeader("content-type", "application/json");
                request.AddHeader("merchantId", _configuration["PayU:MerchantId"]!);            // PRODUCTION  MerchatId
                                                                                                //request.AddHeader("merchantId", _configuration["PayU:UatMerchantId"]!);       // UAT MerchantId

                string invoiceId = "TNX" + DateTime.UtcNow.ToString("yyMMddHHmmss");

                var paymentLinkBody = new
                {
                    invoiceNumber = invoiceId,
                    subAmount = paymentRequest.Amount,
                    transactionId = requestModel.MerchantTransactionID,
                    isAmountFilledByCustomer = false,
                    amount = paymentRequest.Amount,
                    currency = "INR",
                    customer = new
                    {
                        name = mobileUser.FullName,
                        email = mobileUser.EmailId,
                        phone = mobileUser.Mobile,
                    },
                    description = product.Name,   // Recommended for clarity
                    source = "API",               // Required by PayU
                    viaEmail = false,
                    viaSms = false,

                    // Angular page URLs
                    //successURL = "https://testmobileapi.kingresearch.co.in/api/Payment/payments/payu-success",
                    //failureURL = "https://testmobileapi.kingresearch.co.in/api/Payment/payments/payu-failure",

                    successURL = "https://mobileapi.kingresearch.co.in/api/Payment/payments/payment-success",
                    failureURL = "https://mobileapi.kingresearch.co.in/api/Payment/payments/payu-failure",
                };

                request.AddJsonBody(paymentLinkBody);

                var jsonsd = JsonConvert.SerializeObject(paymentLinkBody);

                await PayULogToMongo(jsonsd, "PayURequestPayment");

                //var response = await clientUat.ExecuteAsync(request);            // UAT Host

                var response = await client.ExecuteAsync(request);            // Production Host

                #endregion
                 
                if (response.IsSuccessful && !string.IsNullOrWhiteSpace(response.Content))
                {
                    var json = JsonDocument.Parse(response.Content);

                    await PayULogToMongo(json.RootElement.ToString(), "PayU PaymentLink");

                    var paymentLinkUrl = json.RootElement
                                        .GetProperty("result")
                                        .GetProperty("paymentLink")
                                        .GetString();

                    // Shape response for Angular
                    responseModel.StatusCode = HttpStatusCode.OK;
                    responseModel.Message = "Payment link created successfully";
                    responseModel.Data = new
                    {
                        url = paymentLinkUrl,
                        name = mobileUser.FullName,
                        email = mobileUser.EmailId,
                        amount = paymentLinkBody.amount,
                        product = product.Name,
                        merchantTransactionId = requestModel.MerchantTransactionID
                    };
                    return responseModel;
                }
                else
                {
                    await PayULogToMongo(
                        $"PayU request failed with status {response.StatusCode}: {response.Content}",
                        "PayU PaymentLink"
                    );

                    responseModel.StatusCode = HttpStatusCode.InternalServerError;
                    responseModel.Message = $"PayU request failed with status {response.StatusCode}: {response.Content}. Failed to create payment link. Please try again later.";
                    return responseModel;
                }

            }
            catch (Exception ex)
            {
                responseModel.Message = $"An error occurred while creating the payment link: {ex.Message}";
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                return responseModel;
            }
        }


        public async Task<ApiCommonResponseModel> CreateInstaMojoPaymentLink(PaymentDetailStatusRequestModel requestModel)
        {
            try
            {
                //var paymentRequest = await _context.PaymentRequestStatusM
                //    .Where(x => x.MerchantTransactionId == requestModel.MerchantTransactionID)
                //    .FirstOrDefaultAsync();

                //if (paymentRequest == null)
                //{
                //    responseModel.StatusCode = HttpStatusCode.NotFound;
                //    responseModel.Message = $"Error while fetching PaymentRequestStatusM table. MerchantTransactionID not found: {requestModel.MerchantTransactionID}";
                //    return responseModel;
                //}

                var product = await _context.ProductsM
                    .Where(x => x.Id == requestModel.ProductId && x.IsActive == true)
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                    responseModel.Message = $"Error while fetching ProductsM table. Product not found or inactive for productId: {requestModel.ProductId}";
                    return responseModel;
                }

                var mobileUser = await _context.MobileUsers
                    .Where(x => x.PublicKey == requestModel.MobileUserKey && x.IsActive == true)
                    .FirstOrDefaultAsync();

                if (mobileUser == null)
                {
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                    responseModel.Message = $"Error while fetching MobileUsers table. Mobile user not found or inactive for key: {requestModel.MobileUserKey}";
                    return responseModel;
                }

                //var accessToken = await GetInstaMojoAccessTokenAsync();

                var accessToken = await GetIntaMojoAccessTokenAsync(); // Use the same method for consistency

                if (string.IsNullOrEmpty(accessToken))
                {
                    responseModel.Message = "Failed to retrieve access token from Instamojo";
                    responseModel.StatusCode = HttpStatusCode.InternalServerError;
                    return responseModel;
                }

                await InstaMojoLogToMongo(accessToken, "InstaMojoAccessToken");

                #region Create Payment Link
                var client = new RestClient(_configuration["InstaMojo:PaymentUrl"]!);
                var request = new RestRequest("", Method.Post);

                request.AddHeader("Authorization", $"Bearer {accessToken}");
                request.AddHeader("accept", "application/json");
                request.AddHeader("content-type", "application/json");
               // request.AddHeader("content-type", "application/x-www-form-urlencoded");

                var paymentRequestBody = new
                {
                    //amount = paymentRequest.Amount,
                    amount = requestModel.Amount,
                    purpose = product.Name,
                    buyer_name = mobileUser.FullName,
                    email = mobileUser.EmailId,
                    phone = mobileUser.Mobile,
                    redirect_url = "https://mobileapi.kingresearch.co.in/api/Payment/payments/payment-success",
                    webhook = "https://mobileapi.kingresearch.co.in/api/Payment/InstaMojo",
                    allow_repeated_payments = false,
                    send_email = false,
                    send_sms = false
                };

                var jsonBody = JsonConvert.SerializeObject(paymentRequestBody);
                await InstaMojoLogToMongo(jsonBody, "InstaMojoRequestPayment");

                request.AddJsonBody(paymentRequestBody);

                var response = await client.ExecuteAsync(request);

                // Parse Instamojo JSON
                //var jsonDoc = JsonDocument.Parse(response.Content);
                //var root = jsonDoc.RootElement;

                // Parse Instamojo JSON safely
                using var jsonDoc = JsonDocument.Parse(response.Content);
                var root = jsonDoc.RootElement;

                // Check if Instamojo returned an error
                if (root.TryGetProperty("message", out var errorMessage))
                {
                    throw new Exception($"Instamojo error: {errorMessage.GetString()} | Raw Response: {response.Content}");
                }

                // Safely read fields
                string instaRequestId = null;
                decimal instaAmount = 0;

                if (root.TryGetProperty("id", out var idProp))
                    instaRequestId = idProp.GetString();

                if (root.TryGetProperty("amount", out var amountProp) &&
                    decimal.TryParse(amountProp.ToString(), out var amt))
                    instaAmount = amt;

                // Validate before saving
                if (string.IsNullOrEmpty(instaRequestId) || instaAmount <= 0)
                {
                    throw new Exception($"Invalid response from Instamojo. Content: {response.Content}");
                }
                #endregion

                if (response.IsSuccessful && !string.IsNullOrWhiteSpace(response.Content))
                {
                    // Save into DB using your API request model (PaymentDetailStatusRequestModel request)
                    PaymentRequestStatusM result = new()
                    {
                        Amount = instaAmount,
                        CreatedBy = requestModel.MobileUserKey,          // 👈 use your API input model
                        CreatedOn = DateTime.Now,
                        ProductId = requestModel.ProductId,
                        SubcriptionModelId = requestModel.SubcriptionModelId,
                        SubscriptionMappingId = requestModel.SubscriptionMappingId,
                        MerchantTransactionId = instaRequestId,          // 👈 Instamojo payment request id
                        CouponCode = requestModel.CouponCode,
                        Status = "PENDING"
                    };

                    _context.PaymentRequestStatusM.Add(result);
                    await _context.SaveChangesAsync();

                    var json = JsonDocument.Parse(response.Content);
                    await InstaMojoLogToMongo(json.RootElement.ToString(), "InstaMojo PaymentLink");

                    // Safely read "longurl"
                    string paymentLinkUrl = null;
                    if (json.RootElement.TryGetProperty("longurl", out var longUrlProp))
                    {
                        paymentLinkUrl = longUrlProp.GetString();
                    }


                    if (string.IsNullOrEmpty(paymentLinkUrl))
                    {
                        responseModel.StatusCode = HttpStatusCode.InternalServerError;
                        responseModel.Message = $"Payment link not returned by Instamojo. Response: {response.Content}";
                        return responseModel;
                    }

                    responseModel.StatusCode = HttpStatusCode.OK;
                    responseModel.Message = "Payment link created successfully";
                    responseModel.Data = new
                    {
                        url = paymentLinkUrl,
                        name = mobileUser.FullName,
                        email = mobileUser.EmailId,
                        amount = paymentRequestBody.amount,
                        product = product.Name,
                        merchantTransactionId = instaRequestId
                    };
                    return responseModel;
                }

                else
                {
                    await PayULogToMongo(
                        $"InstaMojo request failed with status {response.StatusCode}: {response.Content}",
                        "InstaMojo PaymentLink"
                    );

                    responseModel.StatusCode = HttpStatusCode.InternalServerError;
                    responseModel.Message = $"InstaMojo request failed with status {response.StatusCode}: {response.Content}. Failed to create payment link. Please try again later.";
                    return responseModel;
                }
            }
            catch (Exception ex)
            {
                responseModel.Message = $"An error occurred while creating the payment link: {ex.Message}";
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                return responseModel;
            }
        }

        private async Task<string?> GetInstaMojoAccessTokenAsync()
        {
            var clientId = "yGTKE01M0ZtzPj9cy7MJKsP7MRplo77BqRdDS1L8"; // PRODUCTION
            var clientSecret = "Af9j2V6sDOQM9QiXKXqcU7dPWDMS73Ke4g3PIbZoAozMLffKcXmQnU5lZbyZdWrVsXUbaVIIeI8k6pPSZl0lgROdH64bUs6P3R3w6MjAROpGJiL505nK8Mt3R9e9GNIS"; // PRODUCTION

            try
            {
                // Instamojo auth URL (same for prod & test, just use correct keys)
                var client = new RestClient("https://api.instamojo.com/oauth2/token/");

                var request = new RestRequest("", Method.Post);
                request.AddHeader("accept", "application/json");
                request.AddHeader("content-type", "application/x-www-form-urlencoded");

                // Required form data
                request.AddParameter("grant_type", "client_credentials");
                request.AddParameter("client_id", clientId);
                request.AddParameter("client_secret", clientSecret);

                var response = await client.ExecutePostAsync(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var content = JsonDocument.Parse(response.Content);
                    var token = content.RootElement.GetProperty("access_token").GetString();
                    return token!;
                }
                else
                {
                    await PayULogToMongo(
                        $"Failed to get Instamojo access token: {response.StatusCode}, Content: {response.Content}",
                        "Instamojo AccessToken"
                    );
                    return null;
                }
            }
            catch (Exception ex)
            {
                await PayULogToMongo($"Exception in GetInstaMojoAccessTokenAsync: {ex}", "Instamojo AccessToken");
                throw;
            }
        }

        private async Task<string?> GetIntaMojoAccessTokenAsync()
        {
            var options = new RestClientOptions(_configuration["InstaMojo:AuthUrl"]!);
            var client = new RestClient(options);

            var request = new RestRequest("", Method.Post);
            request.AddHeader("accept", "application/json");
            request.AddParameter("grant_type", "client_credentials");
            request.AddParameter("client_id", _configuration["InstaMojo:ClientId"]!);
            request.AddParameter("client_secret", _configuration["InstaMojo:ClientSecretId"]!);

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
                throw new System.Exception($"Failed to get token: {response.Content}");

            var jsonDoc = JsonDocument.Parse(response.Content!);
            return jsonDoc.RootElement.GetProperty("access_token").GetString();
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var clientId = _configuration["PayU:ClientId"];                     // PRODUCTION AUTH
            var clientSecret = _configuration["PayU:ClientSecretId"];           // PRODUCTION AUTH

            //var clientId = _configuration["PayU:UatClientId"];                     // UAT AUTH
            //var clientSecret = _configuration["PayU:UatClientSecretId"];           // UAT AUTH

            try
            {
                var client = new RestClient(_configuration["PayU:AuthUrl"]!); // PRODUCTION AUTH
                //var clientUat = new RestClient(_configuration["PayU:AuthUatUrl"]!); // UAT AUTH
                var request = new RestRequest("/oauth/token", Method.Post);

                request.AddHeader("accept", "application/json");
                request.AddHeader("content-type", "application/x-www-form-urlencoded");

                request.AddParameter("grant_type", "client_credentials");
                request.AddParameter("client_id", clientId);          //  Replace with your actual client ID
                request.AddParameter("client_secret", clientSecret);  //  Replace with your actual secret
                request.AddParameter("scope", "create_payment_links update_payment_links read_payment_links");

                //var response = await clientUat.ExecutePostAsync(request);
                var response = await client.ExecutePostAsync(request);

                if (response.IsSuccessful && response.Content != null)
                {
                    var content = JsonDocument.Parse(response.Content);
                    var token = content.RootElement.GetProperty("access_token").GetString();
                    return token!;
                }
                else
                {
                    // Log error if needed
                    await PayULogToMongo(
                        $"Failed to get access token: {response.StatusCode}, Content: {response.Content}",
                        "PayU AccessToken"
                    );

                    return null;
                }
            }
            catch (Exception ex)
            {
                throw; // rethrow to let your controller catch this if needed
            }
        }

        public async Task<ApiCommonResponseModel> InstaMojoWebhokDataProcessing(InstaMojoWebhookData webhookData)
        {

            var paymentResponseResult = await _context.PaymentResponseStatusM.Where(item => item.MerchantTransactionId == webhookData.payment_request_id).FirstOrDefaultAsync();
            var paymentResponseData = new PaymentResponseStatusM();

            if (paymentResponseResult is null)
            {
                paymentResponseData.Success = webhookData.status.Equals("Credit", StringComparison.OrdinalIgnoreCase);
                paymentResponseData.Code = webhookData.status;
                paymentResponseData.Message = $"Payment for {webhookData.purpose} by {webhookData.buyer_name}";
                paymentResponseData.MerchantId = "Instamojo";
                paymentResponseData.MerchantTransactionId = webhookData.payment_request_id;
                paymentResponseData.TransactionId = webhookData.payment_id;
                paymentResponseData.Amount = decimal.TryParse(webhookData.amount, out var amt) ? amt : null;
                paymentResponseData.State = webhookData.status;
                paymentResponseData.ResponseCode = webhookData.mac;
                paymentResponseData.PaymentInstrumentType = "instamojo";
                paymentResponseData.PaymentInstrumentUtr = webhookData.shorturl;
                paymentResponseData.FeesContextAmount = decimal.TryParse(webhookData.fees, out var fee2) ? fee2 : null;
                paymentResponseData.CreatedOn = DateTime.Now;
                await _context.PaymentResponseStatusM.AddAsync(paymentResponseData);
                await _context.SaveChangesAsync();
            }
            else
            {
                paymentResponseData = paymentResponseResult;
            }

            var paymentRequest = await _context.PaymentRequestStatusM
                .FirstOrDefaultAsync(x => x.MerchantTransactionId == webhookData.payment_request_id);

            if (paymentRequest == null)
            {
                await LogToMongo($"Payment Request Details Not Found in PaymentRequestStatusM table. {webhookData.payment_request_id}");

                return (new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Payment Request Details Not Found."
                });
            }
            // if the PaymentRequestStatusM table status is success which means we have to already verified and approved the product to the client.
            // In short , If payment already completed and status change the success
            //if (paymentRequest.Status.ToLower() == "success")
            //{
            //    return (new ApiCommonResponseModel
            //    {
            //        StatusCode = HttpStatusCode.OK,
            //        Message = "Payment already received and approved"
            //    });
            //}

            // if payment is successfull from the payment gateway (instamojo) then continue to check the next steps
            if (Convert.ToBoolean(paymentResponseData.Success))
            {
                paymentRequest.Status = "SUCCESS";
                await _context.SaveChangesAsync();


                var mobileUser = await _context.MobileUsers
                    .FirstOrDefaultAsync(x => x.PublicKey == paymentRequest.CreatedBy);

                var purchaseRequest = new PurchaseOrderMRequestModel
                {
                    MobileUserKey = paymentRequest.CreatedBy.ToString(),
                    PaidAmount = Convert.ToDouble(paymentResponseData.Amount),
                    TransactionId = paymentResponseData.TransactionId,
                    MerchantTransactionId = paymentRequest.MerchantTransactionId,
                    ProductId = paymentRequest.ProductId,
                    SubscriptionMappingId = paymentRequest.SubscriptionMappingId,
                    CouponCode = paymentRequest.CouponCode
                };

                var poResponse = await this.ManagePurchaseOrder(purchaseRequest);
                var poResponseDataJson = poResponse?.Data?.ToString();

                await this.SendPushNotificationToTheClientAfterPaymentConfirmations(poResponseDataJson, mobileUser, purchaseRequest);

                return new ApiCommonResponseModel
                {
                    Data = poResponseDataJson,
                    StatusCode = HttpStatusCode.OK,
                    Message = "Payment processed successfully."
                };
            }

            responseModel.Message = "Payment Failed.";
            responseModel.StatusCode = HttpStatusCode.BadRequest;
            return responseModel;
        }
        public async Task LogToMongo(string message, string category = "PG")
        {
            await _mongoRepo.AddAsync(new Model.MongoDbCollection.Log
            {
                CreatedOn = DateTime.Now,
                Message = message,
                Source = "InstaMojo",
                Category = category
            });
        }

        public async Task PayULogToMongo(string message, string category)
        {
            await _mongoRepo.AddAsync(new Model.MongoDbCollection.Log
            {
                CreatedOn = DateTime.Now,
                Message = message,
                Source = "PayU",
                Category = category
            });
        }
        public async Task InstaMojoLogToMongo(string message, string category)
        {
            await _mongoRepo.AddAsync(new Model.MongoDbCollection.Log
            {
                CreatedOn = DateTime.Now,
                Message = message,
                Source = "InstaMojo",
                Category = category
            });
        }
        public async Task SendPushNotificationToTheClientAfterPaymentConfirmations(string poResponseDataJson, MobileUser mobileUser, PurchaseOrderMRequestModel purchaseRequest)
        {
            if (!string.IsNullOrEmpty(poResponseDataJson))
            {
                try
                {
                    var purchaseDataList = JsonConvert.DeserializeObject<List<PurchaseOrderData>>(poResponseDataJson);
                    if (mobileUser == null || purchaseDataList == null || !purchaseDataList.Any())
                    {

                    }
                    else
                    {
                        var productItem = purchaseDataList.First();
                        var fullName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(mobileUser.FullName.ToLower());
                        var formattedDate = DateTime.Now.ToString("dd MMMM yyyy");

                        await _pushNotification.SendNotificationToMobile(new Model.RequestModel.Notification.NotificationToMobileRequestModel
                        {
                            Body = $"{productItem.Code} Payment has been received... Thanks for your love and support.",
                            Mobile = mobileUser.Mobile,
                            ScreenName = "myBucketList",
                            Title = $"{productItem.Code} Payment is successfull.",
                            Topic = "ANNOUNCEMENT"
                        });

                        await _pushNotification.SendNotificationToMobile(new Model.RequestModel.Notification.NotificationToMobileRequestModel
                        {
                            Body = $"{productItem.Code} Activation is successfull please check your my bucket to see the subscription",
                            Mobile = mobileUser.Mobile,
                            ScreenName = "myBucketList",
                            Title = $"{productItem.Code} Activation is successfull.",
                            Topic = "ANNOUNCEMENT"
                        });

                        var item = await _context.ProductsM
                                                         .Where(p => p.Code == productItem.Code)
                                                         .FirstOrDefaultAsync();

                        var notificationPayload = new Model.MongoDbCollection.PushNotification
                        {
                            Title = $"{productItem.Code}  Payment has been received",
                            Message = $"Your {productItem.Name} Payment has been received... Thanks for your love and support.",
                            Scanner = false,
                            CreatedOn = DateTime.Now,
                            Topic = "ANNOUNCEMENT",
                            ScreenName = "productDetailsScreenWidget",
                            ProductId = item.Id.ToString(),
                            ProductName = item.Name,
                            ImageUrl = null
                        };

                        // Prepare receiver list
                        var userReceiverList = new List<UserListForPushNotificationModel>
                        {
                            new UserListForPushNotificationModel
                            {
                                Notification = true,
                                FullName = mobileUser.FullName,
                                FirebaseFcmToken = mobileUser.FirebaseFcmToken,
                                PublicKey = mobileUser.PublicKey
                            }
                        };

                        // Send payment confirmation notification
                        await _pushNotificationRepo.SaveNotificationDataAsync(notificationPayload, userReceiverList);

                        // Create and send activation success notification
                        var notificationActivePayload = new Model.MongoDbCollection.PushNotification
                        {
                            Title = $"{productItem.Code} Activation is successful",
                            Message = $"Your {productItem.Name} activation is successful. Please check My Bucket to view your subscription.",
                            Scanner = false,
                            CreatedOn = DateTime.Now,
                            Topic = "ANNOUNCEMENT",
                            ScreenName = "productDetailsScreenWidget",
                            ProductId = item.Id.ToString(),
                            ProductName = item.Name,
                            ImageUrl = null
                        };

                        await _pushNotificationRepo.SaveNotificationDataAsync(notificationActivePayload, userReceiverList);

                        var whatsappRequest = new WhatsAppOrderConfirmationRequest
                        {
                            MobileNumber = mobileUser.Mobile,
                            CustomerName = fullName,
                            ProductCode = productItem.Code,
                            ProductName = productItem.Name,
                            CountryCode = mobileUser.CountryCode,
                            Products = productItem.Name,
                            ValidityInDays = productItem.ProductValidity.ToString(),
                            StartDate = formattedDate,
                            EndDate = productItem.EndDate?.ToString("dd MMMM yyyy") ?? "",
                            ProductValue = purchaseRequest.PaidAmount.ToString(),
                            BonusProduct = productItem.BonusProduct,
                            BonusProductValidity = productItem.BonusProductValidity.ToString(),
                            Community = productItem.Community,
                            ProductCategory = productItem.ProductCategory
                        };

                        await _otherSerivce.SendWhatsappOrderConfirmationAsync(whatsappRequest);
                        await _emailService.SendConfirmationActiveNotification(mobileUser.EmailId, mobileUser.FullName, mobileUser, productItem.Name, whatsappRequest.StartDate, whatsappRequest.ProductValue, whatsappRequest.ValidityInDays, purchaseRequest);
                    }
                }
                catch (Exception ex)
                {
                    await LogToMongo("Exception during notification: " + ex.Message, "ProcessingException");
                }
            }
        }

        public async Task<ApiCommonResponseModel> GetInvoicesByMobileUserKeyAsync(Guid? mobileUserKey, int pageNumber, int pageSize)
        {
            var responseModel = new ApiCommonResponseModel();

            var totalCountParam = new SqlParameter
            {
                ParameterName = "@TotalCount",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            var sqlParameters = new SqlParameter[]
            {
                new SqlParameter
                {
                    ParameterName = "@MobileUserKey",
                    Value = mobileUserKey,
                    SqlDbType = SqlDbType.UniqueIdentifier
                },
                new SqlParameter
                {
                    ParameterName = "@PageNumber",
                    Value = pageNumber,
                    SqlDbType = SqlDbType.Int
                },
                new SqlParameter
                {
                    ParameterName = "@PageSize",
                    Value = pageSize,
                    SqlDbType = SqlDbType.Int
                },
                    totalCountParam
            };

            try
            {
                var invoiceList = await _context.SqlQueryAsync<GetInvoiceResponseModel>(
                    "EXEC GetInvoicesByMobileUserKey @MobileUserKey, @PageNumber, @PageSize, @TotalCount OUTPUT",
                    sqlParameters);

                var request = _httpContextAccessor.HttpContext?.Request;
                string baseUrl = $"{request?.Scheme}://{request?.Host}";

                foreach (var invoice in invoiceList)
                {
                    if (!string.IsNullOrWhiteSpace(invoice.FileName))
                    {
                        invoice.DownloadUrl = $"{baseUrl}/api/Payment/DownloadInvoice?fileName={Uri.EscapeDataString(invoice.FileName)}";
                    }
                }

                responseModel.Data = invoiceList;
                responseModel.Message = "Successful.";
                responseModel.StatusCode = HttpStatusCode.OK;
            }
            catch (SqlNullValueException ex)
            {
                responseModel.Message = $"Column mapping failed. Check nullable types..{ex}";
                responseModel.StatusCode = HttpStatusCode.OK;
                throw new Exception("Column mapping failed. Check nullable types.", ex);
            }

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> PayUWebhookDataProcessing(PayUWebhookRequestModel webhookData)
        {
            var paymentResponseResult = await _context.PaymentResponseStatusM
                .FirstOrDefaultAsync(item => item.MerchantTransactionId == webhookData.txnid);

            var paymentResponseData = new PaymentResponseStatusM();

            try
            {
                if (paymentResponseResult is null)
                {
                    paymentResponseData.Success = webhookData.status.Equals("success", StringComparison.OrdinalIgnoreCase);
                    paymentResponseData.Code = webhookData.status;
                    paymentResponseData.Message = $"PayU payment for {webhookData.productinfo} by {webhookData.firstname}";
                    paymentResponseData.MerchantId = webhookData.pa_name ?? "PayU";
                    paymentResponseData.MerchantTransactionId = webhookData.txnid;
                    paymentResponseData.TransactionId = webhookData.mihpayid;
                    paymentResponseData.Amount = decimal.TryParse(webhookData.amount, out var amt) ? amt : null;
                    paymentResponseData.State = webhookData.field7;
                    paymentResponseData.ResponseCode = webhookData.status;
                    paymentResponseData.PaymentInstrumentType = webhookData.mode ?? "PayU";
                    paymentResponseData.PaymentInstrumentUtr = webhookData.field1;
                    paymentResponseData.FeesContextAmount = null; // PayU may not send fees in webhook
                    paymentResponseData.CreatedOn = DateTime.Now;

                    // Optional additional fields (uncomment if needed)
                    //paymentResponseData.Code = webhookData.bankcode;
                    //paymentResponseData.BankRefNum = webhookData.bank_ref_num;
                    //paymentResponseData.NameOnCard = webhookData.field3;
                    //paymentResponseData.MaskedCardNumber = webhookData.field2;

                    await _context.PaymentResponseStatusM.AddAsync(paymentResponseData);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    paymentResponseData = paymentResponseResult;
                }
            }
            catch (Exception ex)
            {
                // Log the exception to Mongo or file system
                await PayULogToMongo($"Exception in saving PayU webhook data: {ex.Message}", "PayU Webhook Error");

                // Optional: Re-throw or handle silently
                // throw;
            }


            var paymentRequest = await _context.PaymentRequestStatusM
                .FirstOrDefaultAsync(x => x.MerchantTransactionId == webhookData.txnid);

            if (paymentRequest == null)
            {
                await PayULogToMongo($"❌ Payment Request Not Found in PaymentRequestStatusM for txnid: {webhookData.txnid}", "Payment Request Not Found");

                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Payment Request Details Not Found."
                };
            }

            if (paymentRequest.Status.ToLower() == "success")
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Payment already received and approved"
                };
            }

            if (Convert.ToBoolean(paymentResponseData.Success))
            {
                paymentRequest.Status = "SUCCESS";
                await _context.SaveChangesAsync();

                var mobileUser = await _context.MobileUsers
               .FirstOrDefaultAsync(x => x.PublicKey == paymentRequest.CreatedBy);

                var purchaseRequest = new PurchaseOrderMRequestModel
                {
                    MobileUserKey = paymentRequest.CreatedBy.ToString(),
                    PaidAmount = Convert.ToDouble(paymentResponseData.Amount),
                    TransactionId = paymentResponseData.TransactionId,
                    MerchantTransactionId = paymentResponseData.MerchantTransactionId,
                    ProductId = paymentRequest.ProductId,
                    SubscriptionMappingId = paymentRequest.SubscriptionMappingId,
                    CouponCode = paymentRequest.CouponCode
                };

                var poResponse = await ManagePurchaseOrder(purchaseRequest);
                var poResponseDataJson = poResponse?.Data?.ToString();

                await SendPushNotificationToTheClientAfterPaymentConfirmations(poResponseDataJson, mobileUser, purchaseRequest);

                return new ApiCommonResponseModel
                {
                    Data = poResponseDataJson,
                    StatusCode = HttpStatusCode.OK,
                    Message = "Payment processed successfully."
                };
            }

            await PayULogToMongo($"Exception in saving PayU webhook data: {webhookData}", "PayU Webhook Error");

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Payment failed."
            };
        }

        public class PaymentStatusRequest
        {
            public string PaymentRequestId { get; set; }
            public string Status { get; set; }
        }

    }
}