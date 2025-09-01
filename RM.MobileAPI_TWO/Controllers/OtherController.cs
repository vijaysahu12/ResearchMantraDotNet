using RM.CommonServices.Helpers;
using RM.CommonServices.Services;
using RM.Database.KingResearchContext;
using RM.Database.MongoDbContext;
using RM.Model;
using RM.Model.Models;
using RM.Model.MongoDbCollection;
using RM.Model.RequestModel;
using RM.Model.RequestModel.MobileApi;
using RM.MService.Services;
using RM.NotificationService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PhoneNumbers;
using RestSharp;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
namespace RM.MobileAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class OtherController : ControllerBase
    {
        private readonly MongoDbService _mongoDbService;
        private readonly IMongoRepository<Model.MongoDbCollection.Log> _mongoRepo;
        private readonly KingResearchContext _context;
        private readonly IPurchaseOrderMService _purchaseOrderService;
        private readonly ApiCommonResponseModel _response = new();
        private readonly IOtherService _otherSerivce;
        private readonly IEmailService _emailService;
        private static string _cachedAccessToken;
        private static DateTime _tokenExpiryTime = DateTime.MinValue;
        private static readonly object _tokenLock = new object();
        public IConfiguration _configuration { get; }

        public OtherController(IConfiguration configuration, MongoDbService mongoDbService, KingResearchContext kingResearchContext,
            IPurchaseOrderMService purchaseOrderService, IOtherService otherSerivce, IMongoRepository<Model.MongoDbCollection.Log> mongoRepo, 
            IEmailService emailService)
        {
            _mongoDbService = mongoDbService;
            _context = kingResearchContext;
            _purchaseOrderService = purchaseOrderService;
            _configuration = configuration;
            _otherSerivce = otherSerivce; _mongoRepo = mongoRepo;
            _phoneNumberUtil = PhoneNumberUtil.GetInstance();
            _emailService = emailService;
        }
        private readonly PhoneNumberUtil _phoneNumberUtil;
        //public OtherController(MongoDbService mongoDbService)
        //{
        //    _mongoDbService = mongoDbService;
        //    _phoneNumberUtil = PhoneNumberUtil.GetInstance();
        //}

        [HttpGet("UpdateBreakfastStocks")]
        [AllowAnonymous]
        public IActionResult UpdateBreakfastStocks()
        {
            var fileName = "BreakfastScanner_" + DateTime.Now.ToString("ddMMM");
            var filePath = "D:\\" + "SymbolData\\" + fileName;
            var breakfastJsonData = FileHelper.ReadFile(filePath);

            if (breakfastJsonData != null)
            {
                var breakfastList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CamrillaR4Model>>(breakfastJsonData);

                if (breakfastList != null)
                {
                    foreach (var param in breakfastList)
                    {
                        Model.MongoDbCollection.PushNotification notificationPayload = new()
                        {
                            Title = param.Ltp,
                            Message = "",
                            Scanner = true,
                            TradingSymbol = param.TradingSymbol,
                            TransactionType = "Buy",
                            CreatedOn = DateTime.Now,
                            Topic = "breakfast",
                        };
                        _ = _mongoDbService.SaveNotificationData(notificationPayload);
                    }
                }
                else
                {
                    return Ok("File exists but not records.");
                }
            }
            else
            {
                return Ok("No Records exists in file");
            }

            return Ok("Success");
        }

        [HttpPost]
        [Route("GupshupCallback")]
        [AllowAnonymous]
        public async Task<IActionResult> GupshupCallback()
        {
            using var reader = new StreamReader(HttpContext.Request.Body);
            var body = await reader.ReadToEndAsync();
            reader.Close();
            await _mongoRepo.AddAsync(new Model.MongoDbCollection.Log
            {
                CreatedOn = DateTime.Now,
                Message = "Body: " + body,
                Source = "gupsup_callback",
                Category = "null data"
            });
            return Ok("Succesfull");
        }

      

        [HttpPost]
        [Route("PhonePe")]
        [AllowAnonymous]
        //CallBack method which are getting call by phone api to update the payment status.
        public async Task<IActionResult> PhonePe()
        {
            using var reader = new StreamReader(HttpContext.Request.Body);
            var body = await reader.ReadToEndAsync();
            reader.Close();
            if (body == null)
            {

                await _mongoRepo.AddAsync(new Model.MongoDbCollection.Log
                {
                    CreatedOn = DateTime.Now,
                    Message = "0#BR JSON Body is null",
                    Source = "PhonePe",
                    Category = "null data"
                });
                return BadRequest();
            }

            PaymentWebhookResponseModel response = JsonConvert.DeserializeObject<PaymentWebhookResponseModel>(body);
            if (response == null || string.IsNullOrWhiteSpace(response.Response))
            {
                await _mongoRepo.AddAsync(new Model.MongoDbCollection.Log { CreatedOn = DateTime.Now, Message = "0#BR Invalid or missing 'Response' field", Source = "PhonePe", Category = "DeserializeObject is Null " });
                return BadRequest("Invalid or missing 'Response' field");
            }
            byte[] jsonBytes = Convert.FromBase64String(response.Response);
            string jsonString = Encoding.UTF8.GetString(jsonBytes);
            await _mongoRepo.AddAsync(new Model.MongoDbCollection.Log { CreatedOn = DateTime.Now, Message = "1#BR " + body.ToString(), Source = "PhonePe", Category = "PhonePe" });
            PhonePePaymentConvertedResponseStatusModel data = JsonConvert.DeserializeObject<PhonePePaymentConvertedResponseStatusModel>(jsonString);


#if DEBUG
            jsonString = "eyJzdWNjZXNzIjp0cnVlLCJjb2RlIjoiUEFZTUVOVF9TVUNDRVNTIiwibWVzc2FnZSI6IllvdXIgcGF5bWVudCBpcyBzdWNjZXNzZnVsLiIsImRhdGEiOnsibWVyY2hhbnRJZCI6IktJTkdST05MSU5FIiwibWVyY2hhbnRUcmFuc2FjdGlvbklkIjoiVFJBTlNBQ1RJT04yNDA0MjAyNTA5MTczNCIsInRyYW5zYWN0aW9uSWQiOiJPTU8yNTA0MjQwOTE3MzU0ODI4NDE4NzA2IiwiYW1vdW50IjoyMTAwLCJzdGF0ZSI6IkNPTVBMRVRFRCIsInJlc3BvbnNlQ29kZSI6IlNVQ0NFU1MiLCJwYXltZW50SW5zdHJ1bWVudCI6eyJ0eXBlIjoiVVBJIiwidXRyIjoiMTE1NjUxODMzNTgxIiwiaWZzYyI6IklERkIwMDQwMTAxIiwidXBpVHJhbnNhY3Rpb25JZCI6IllCTDNmOGVmMWU2ZGMxODQ2ZDhhYmRmMTkzNWU1ZjRkODg3IiwiY2FyZE5ldHdvcmsiOm51bGwsImFjY291bnRUeXBlIjoiU0FWSU5HUyJ9LCJmZWVzQ29udGV4dCI6eyJhbW91bnQiOjB9fX0=";
            string decodedJsonString = Encoding.UTF8.GetString(Convert.FromBase64String(jsonString));
            //Deserialize into the model
            data = JsonConvert.DeserializeObject<PhonePePaymentConvertedResponseStatusModel>(decodedJsonString);
#endif
            var result = await this.PhonepeDataProcessing(data);
            return result;
        }

        [NonAction]
        public async Task<IActionResult> PhonepeDataProcessing(PhonePePaymentConvertedResponseStatusModel data)
        {
            if (data != null && data.Data != null)
            {
                PaymentResponseStatusM paymentRespnseData = new();

                paymentRespnseData.Amount = data.Data.Amount / 100; // PhonePe sending INR into paisa tha'ts why we have to divide by 100
                paymentRespnseData.Code = data.Code ?? string.Empty;
                paymentRespnseData.FeesContextAmount = data.Data.FeesContext.Amount;
                paymentRespnseData.MerchantId = data.Data.MerchantId;
                paymentRespnseData.MerchantTransactionId = data.Data.MerchantTransactionId;
                paymentRespnseData.Message = data.Message;

                paymentRespnseData.ResponseCode = data.Data.ResponseCode ?? string.Empty;
                paymentRespnseData.State = data.Data.State ?? string.Empty;
                paymentRespnseData.Success = data.Success;
                paymentRespnseData.TransactionId = data.Data.TransactionId ?? string.Empty;


                if (data.Data.PaymentInstrument != null)
                {
                    paymentRespnseData.PaymentInstrumentType = data.Data.PaymentInstrument.Type ?? string.Empty;
                    paymentRespnseData.PaymentInstrumentUpiTransactionId = data.Data.PaymentInstrument.UpiTransactionId ?? string.Empty;
                    paymentRespnseData.PaymentInstrumentUtr = data.Data.PaymentInstrument.Utr ?? string.Empty;
                    paymentRespnseData.PaymentInstrumentAccountType = data.Data.PaymentInstrument.AccountType ?? string.Empty;
                }

                paymentRespnseData.PaymentInstrumentCardNetwork = data.Data.PaymentInstrument?.CardNetwork ?? string.Empty;
                _ = await _context.PaymentResponseStatusM.AddAsync(paymentRespnseData);

                if (data.Success)
                {
                    await _mongoRepo.AddAsync(new Model.MongoDbCollection.Log
                    {
                        CreatedOn = DateTime.Now,
                        Message = "PhonePe Payment = Success : " + JsonConvert.SerializeObject(data),
                        Source = "PhonePe",
                        Category = "Success"
                    });
                    var requestStatus = await _context.PaymentRequestStatusM.FirstOrDefaultAsync(x => x.MerchantTransactionId == data.Data.MerchantTransactionId);

                    if (requestStatus != null)
                    {
                        requestStatus.Status = data.Code;
                        _ = await _context.SaveChangesAsync();
                        PurchaseOrderMRequestModel request = new()
                        {
                            MobileUserKey = requestStatus.CreatedBy.ToString(),
                            PaidAmount = (double)data.Data.Amount / 100, // Convert Paisa to Rupees only for phonepe // REVERTED DIVISION BY 100 TO PREVENT IT FROM SAVING IN PAISA SAID BY VIJAY
                            TransactionId = data.Data.TransactionId, // @MerchantTransactionId
                            MerchantTransactionId = data.Data.MerchantTransactionId, // @MerchantTransactionId
                            ProductId = requestStatus.ProductId,
                            SubscriptionMappingId = requestStatus.SubscriptionMappingId,
                            CouponCode = requestStatus.CouponCode
                        };

                        var poResponse = await _purchaseOrderService.ManagePurchaseOrder(request);

                        var poResponseDataJson = poResponse.Data.ToString();
                        try
                        {
                            var poResponseDataList = JsonConvert.DeserializeObject<List<PurchaseOrderData>>(poResponseDataJson);

                            if (poResponseDataList != null && poResponseDataList.Count > 0)
                            {
                                var firstItem = poResponseDataList[0];
                                var mobileUser = _context.MobileUsers.FirstOrDefault(x => x.PublicKey == Guid.Parse(request.MobileUserKey));

                                if (mobileUser != null)
                                {
                                    var textInfo = CultureInfo.CurrentCulture.TextInfo;
                                    var titleCaseFullName = textInfo.ToTitleCase(mobileUser.FullName.ToLower());

                                    var logData = new
                                    {
                                        mobileUser.Mobile,
                                        FullName = titleCaseFullName,
                                        OrderName = firstItem.Name,
                                        firstItem.ProductValidity,
                                        EndDate = firstItem.EndDate?.ToString("dd MMMM yyyy") ?? "",
                                        PaidAmount = request.PaidAmount.ToString(),
                                        StartDate = DateTime.Now.ToString("dd MMMM yyyy"),
                                        mobileUser.CountryCode
                                    };


                                    await _mongoRepo.AddAsync(new Model.MongoDbCollection.Log { CreatedOn = DateTime.Now, Message = "Sending WhatsApp Order Confirmation: " + JsonConvert.SerializeObject(logData), Source = "PhonePe", Category = "Success" });


                                    WhatsAppOrderConfirmationRequest whatsAppOrderConfirmationRequest = new()
                                    {
                                        MobileNumber = mobileUser.Mobile,
                                        CustomerName = titleCaseFullName,
                                        ProductCode = firstItem.Code,
                                        ProductName = firstItem.Name,
                                        CountryCode = mobileUser.CountryCode,
                                        Products = firstItem.Name,
                                        ValidityInDays = firstItem.ProductValidity.ToString(),
                                        StartDate = DateTime.Now.ToString("dd MMMM yyyy"),
                                        EndDate = firstItem.EndDate?.ToString("dd MMMM yyyy") ?? "",
                                        ProductValue = request.PaidAmount.ToString(),
                                        BonusProduct = firstItem.BonusProduct,
                                        BonusProductValidity = firstItem.BonusProductValidity.ToString(),
                                        Community = firstItem.Community,
                                        ProductCategory = firstItem.ProductCategory
                                    };

                                    await _otherSerivce.SendWhatsappOrderConfirmationAsync(whatsAppOrderConfirmationRequest);
                                    await _emailService.SendPaymentSuccessEmailAsync(mobileUser.EmailId, titleCaseFullName, mobileUser.Mobile, DateTime.Now.ToString("dd MMMM yyyy"), firstItem.Code);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            //ToDo Add the logic to notify that the payment is getting failed because of some procedure issues.
                            throw;
                        }
                    }
                    else
                    {
                        await _mongoRepo.AddAsync(new Model.MongoDbCollection.Log { CreatedOn = DateTime.Now, Message = "PaymentRequestStatusM data not found so no call to ManagePurchaseOrder SP : " + JsonConvert.SerializeObject(data), Source = "PhonePe", Category = "requestStatus is null" });
                    }
                }
                else
                {
                    await _mongoRepo.AddAsync(new Model.MongoDbCollection.Log { CreatedOn = DateTime.Now, Message = "Failed : " + JsonConvert.SerializeObject(data), Source = "PhonePe", Category = "Failed" });
                }
                return Ok("Succesfull");
            } // else for data
            else
            {
                await _mongoRepo.AddAsync(new Model.MongoDbCollection.Log { CreatedOn = DateTime.Now, Message = "Data Is null", Source = "PhonePe", Category = "PaymentFailed Or Null Response" });
                return BadRequest("PhonePe data is null or Payment Failed");
            }
        }

        [HttpGet]
        [Route("GetPhonePeResponseStatus")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPhonePeResponseStatus([Required] string merchantTransactionId)
        {

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
                return Ok(_response);
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
            return Ok(_response);
        }

        /// <summary>
        /// Remove this method once all user moved to the latest version of the app
        /// PaymentDetailStatus renamed to PaymentRequestStatus
        /// </summary>
        [HttpPost("PaymentDetailStatus")]
        public async Task<IActionResult> PaymentDetailStatus([FromBody] PaymentDetailStatusRequestModel request)
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
                Status = "PENDING",
                CouponCode = request.CouponCode
            };

            _ = await _context.PaymentRequestStatusM.AddAsync(result);
            _ = await _context.SaveChangesAsync();

            _response.Data = true;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }


        //calling after the transaction is completed / preApiCall 
        [AllowAnonymous]
        [HttpPost("PaymentRequestStatus")]
        public async Task<IActionResult> PaymentRequestStatus([FromBody] PaymentDetailStatusRequestModel request)
        {
            var res = await this.AddIntoPaymentRequestStatus(request);
            _response.Data = res != null && res.Id > 0;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }

        [NonAction]
        public async Task<PaymentRequestStatusM> AddIntoPaymentRequestStatus(PaymentDetailStatusRequestModel request, string status = "PENDING")
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

        /// <summary>
        /// Note we have hardccoded productId for Breakfast only (production) change it to make it work for other strategies
        /// </summary>
        [HttpPost("CashfreeWebhook")]
        [AllowAnonymous]
        public async Task<IActionResult> CashfreeWebhook()
        {
            string requestBody = @"";
            int productId = await _context.ProductsM
                        .Where(x => x.Code == "BREAKFASTSTRATEGY")
                        .Select(x => x.Id)
                        .FirstOrDefaultAsync();
            try
            {
                // Read the raw body of the POST request
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    requestBody = await reader.ReadToEndAsync();

#if DEBUG
                    requestBody = @"
{
  ""data"": {
    ""order"": {
      ""order_id"": ""CFPay_breakfastStrategy_1ms3cvvii2"",
      ""order_amount"": 1.00,
      ""order_currency"": ""INR"",
      ""order_tags"": {
        ""cf_form_id"": ""77346605""
      }
    },
    ""payment"": {
      ""cf_payment_id"": ""3932031114"",
      ""payment_status"": ""SUCCESS"",
      ""payment_amount"": 4999.00,
      ""payment_currency"": ""INR"",
      ""payment_message"": ""00::Success"",
      ""payment_time"": ""2025-05-31T11:30:01+05:30"",
      ""bank_reference"": ""515108972271"",
      ""auth_id"": null,
      ""payment_method"": {
        ""upi"": {
          ""channel"": null,
          ""upi_id"": ""shahbajhossain0-1@oksbi""
        }
      },
      ""payment_group"": ""upi""
    },
    ""customer_details"": {
      ""customer_name"": ""Sk shahbaj hussain"",
      ""customer_id"": null,
      ""customer_email"": ""shahbajhossain0@gmail.com"",
      ""customer_phone"": ""+919163941573""
    },
    ""payment_gateway_details"": {
      ""gateway_name"": ""CASHFREE"",
      ""gateway_order_id"": ""3930805667"",
      ""gateway_payment_id"": ""3930805667"",
      ""gateway_status_code"": null,
      ""gateway_order_reference_id"": ""null"",
      ""gateway_settlement"": ""CASHFREE"",
      ""gateway_reference_name"": null
    },
    ""payment_offers"": null
  },
  ""event_time"": ""2025-05-11T11:30:18+05:30"",
  ""type"": ""PAYMENT_SUCCESS_WEBHOOK""
}";
#endif
                    await _mongoRepo.AddAsync(new Model.MongoDbCollection.Log
                    {
                        Category = "CashfreeWebhook",
                        CreatedOn = DateTime.Now,
                        Message = requestBody,
                        Source = "CashfreeWebhook"
                    });

                    var requestModel = JsonConvert.DeserializeObject<CashFreeWebhookRequestModel>(requestBody);

                    if (requestModel != null && requestModel.type == "PAYMENT_SUCCESS_WEBHOOK")
                    {
                        var gatewayPaymentId = requestModel?.data?.payment_gateway_details?.gateway_payment_id;

                        if (string.IsNullOrWhiteSpace(gatewayPaymentId))
                        {
                            await _mongoRepo.AddAsync(new Model.MongoDbCollection.Log
                            {
                                Category = "CashfreeWebhook",
                                CreatedOn = DateTime.Now,
                                Message = "Missing gateway_payment_id. Cannot process payment.",
                                Source = "CashfreeWebhook"
                            });

                            return BadRequest("Missing gateway_payment_id. Cannot process payment.");
                        }

                        var isDuplicate = await _context.PaymentResponseStatusM
                            .AnyAsync(x => x.TransactionId == gatewayPaymentId);

                        if (!isDuplicate)
                        {
                            isDuplicate = await _context.PurchaseOrdersM
                                .AnyAsync(x => x.TransactionId == gatewayPaymentId);
                        }

                        if (isDuplicate)
                        {
                            await _mongoRepo.AddAsync(new Model.MongoDbCollection.Log
                            {
                                Category = "CashfreeWebhook",
                                CreatedOn = DateTime.Now,
                                Message = $"Duplicate gateway_payment_id detected: {gatewayPaymentId}",
                                Source = "CashfreeWebhook"
                            });

                            return BadRequest("Duplicate TransactionId: Payment already processed.");
                        }
                        var currentTime = DateTime.Now;
                        var merchantTransactionId = "TRANSACTION" + currentTime.ToString("ddMMyyyyHHmmss");
                        var phoneNumber = "";
                        #region logic to add or update mobileUser

                        var fullPhone = requestModel.data.customer_details.customer_phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

                        var mobileUser = await _context.MobileUsers
                          .Where(item =>
                              item.CountryCode != null &&
                              item.Mobile != null &&
                              string.Concat("+", item.CountryCode, item.Mobile) == fullPhone).FirstOrDefaultAsync();

                        var mobilUserKey = Guid.Empty;

                        if (mobileUser == null)
                        {
                            phoneNumber = fullPhone;
                            string countryCode = "";

                            if (fullPhone.StartsWith("+"))
                            {
                                countryCode = fullPhone.Substring(1, 2);
                                phoneNumber = fullPhone.Substring(3);
                            }
                            else if (fullPhone.StartsWith("91") && fullPhone.Length > 10)
                            {
                                countryCode = "91";
                                phoneNumber = fullPhone.Substring(2);
                            }
                            else
                            {
                                countryCode = "91";
                            }

                            var newUser = new MobileUser
                            {
                                CreatedOn = DateTime.Now,
                                ModifiedOn = DateTime.Now,
                                Mobile = phoneNumber,
                                CountryCode = countryCode,
                                FullName = requestModel?.data?.customer_details?.customer_name,
                                EmailId = requestModel?.data?.customer_details?.customer_email,
                                Password = "123456",
                                DeviceType = "Android",
                                PublicKey = Guid.NewGuid(),
                                RegistrationDate = DateTime.Now,
                                MobileToken = "12121231",
                                Imei = "",
                                City = "",
                                About = "123",
                                StockNature = "",
                                AgreeToTerms = true,
                                SameForWhatsApp = true,
                                IsOtpVerified = false,
                                IsActive = true,
                                IsDelete = false,
                                DeviceVersion = _configuration["AppSettings:Versions:Android"]
                            };

                            await _context.MobileUsers.AddAsync(newUser);
                            await _context.SaveChangesAsync();

                            mobilUserKey = newUser.PublicKey;
                        }
                        else
                        {
                            mobilUserKey = mobileUser.PublicKey;
                            mobileUser.EmailId = requestModel?.data?.customer_details?.customer_email;
                            await _context.SaveChangesAsync();
                        }

                        #endregion

                        #region Logic to add the payment request from customer 

                        PaymentRequestStatusM result = new()
                        {
                            Amount = (decimal)requestModel.data.payment.payment_amount, // default I am getting in rupees so no need to change.
                            CreatedBy = mobilUserKey,
                            CreatedOn = DateTime.Now,
                            ProductId = productId,
                            SubcriptionModelId = 0,
                            MerchantTransactionId = merchantTransactionId, //TransactionId = MerchantTransactionId
                            Status = "PENDING",
                            CouponCode = "",
                        };
                        //ToDo Change productid 
                        var subscriptionmapping = await _context.SubscriptionMappingM.Where(item => item.ProductId == productId && item.IsActive == true && item.SubscriptionDurationId == 3).FirstOrDefaultAsync();
                        if (subscriptionmapping != null)
                        {
                            result.SubscriptionMappingId = subscriptionmapping.Id;
                        }

                        _ = await _context.PaymentRequestStatusM.AddAsync(result);
                        _ = await _context.SaveChangesAsync();
                        #endregion


                        var payment = requestModel?.data?.payment;
                        var gatewayDetails = requestModel?.data?.payment_gateway_details;
                        var upi = payment?.payment_method?.upi;

                        var data = new PhonePePaymentConvertedResponseStatusModel
                        {
                            Success = true,
                            Code = payment?.payment_status ?? "UNKNOWN",
                            Message = "Payment captured successfully",
                            Data = new PhonePeData
                            {
                                MerchantId = payment?.bank_reference ?? "N/A", // fallback to "N/A" if null
                                MerchantTransactionId = merchantTransactionId,
                                TransactionId = gatewayDetails?.gateway_payment_id ?? "UNKNOWN",
                                Amount = Convert.ToDecimal(payment?.payment_amount ?? 0) * 100, // convert the amount in Paisa 
                                State = payment?.payment_status ?? "UNKNOWN",
                                ResponseCode = payment?.payment_message?.Split("::")[0] ?? "00", // Extract "00" from "00::Success"

                                PaymentInstrument = new PaymentInstrument
                                {
                                    Type = payment?.payment_group ?? "UNKNOWN",
                                    Utr = payment?.bank_reference,
                                    UpiTransactionId = upi?.upi_id,
                                    CardNetwork = null, // still not available
                                    AccountType = null  // still not available
                                },

                                FeesContext = new FeesContext
                                {
                                    Amount = Convert.ToDecimal(requestModel?.data?.charges_details?.service_charge ?? 0)
                                }
                            }
                        };
                        _ = await this.PhonepeDataProcessing(data);
                    }
                }
            }
            catch (Exception ex)
            {
                requestBody = ex.InnerException?.Message ?? ex.Message;
                await this.Log("Exception : cashfree webhook : " + requestBody.ToString(), "cashfree");
            }
            return Ok();
        }

        [AllowAnonymous, HttpPost]
        public async Task<IActionResult> Log(string message, string category = "flutter")
        {
            await _mongoRepo.AddAsync(new Model.MongoDbCollection.Log { CreatedOn = DateTime.Now, Message = message, Source = "mobileApp", Category = "flutter" });
            return Ok();
        }

        /// <summary>
        /// helps to import all the old brekfast data into SQL ScannerPerformance table
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous, HttpGet("BreakfastOldJSonDataBinding")]
        public async Task BreakfastOldJSonDataBinding()
        {
            // Folder location to read files
            string _directoryPath = @"D:\\Breakfastjson";

            if (!Directory.Exists(_directoryPath))
            {
                Console.WriteLine($"Folder not found: {_directoryPath}");
                return;
            }

            // Get all files in the folder
            string[] files = Directory.GetFiles(_directoryPath);

            // Initialize a list to hold file data
            foreach (var file in files)
            {
                try
                {
                    var filepath = Path.Combine(_directoryPath, Path.GetFileName(file));

                    var fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
                    var breakfastList = System.Text.Json.JsonSerializer.DeserializeAsyncEnumerable<CamrillaR4Model>(fileStream);

                    string dateFormat = "ddMMM";

                    var DateTimeTemp = DateTime.Now;
                    await foreach (var stock in breakfastList)
                    {
                        var dateInput = Path.GetFileName(file).Replace("BreakfastScanner_", "");
                        if (DateTime.TryParseExact(dateInput, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                        {
                            DateTimeTemp = new DateTime(2025, parsedDate.Month, parsedDate.Day);
                        }

                        string formattedDateTime = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
                        ScannerPerformanceM item = new()
                        {
                            CreatedOn = DateTimeTemp,
                            Ltp = Convert.ToDecimal(stock.Ltp),
                            PercentChange = Convert.ToDecimal(stock.PercentChange),
                            NetChange = Convert.ToDecimal(stock.NetChange),
                            SentAt = Convert.ToDateTime(DateTimeTemp.ToString()),
                            Message = "",
                            ViewChart = stock.TradingSymbol,
                            TradingSymbol = stock.TradingSymbol,
                            Topic = "BREAKFAST"
                        };

                        var rr = await _context.ScannerPerformanceM
                            .Where(item => item.SentAt.Date == DateTime.Now.Date && item.TradingSymbol == item.TradingSymbol).FirstOrDefaultAsync();

                        if (rr == null)
                        {
                            _context.ScannerPerformanceM.Add(item);
                        }

                    }
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading file {file}: {ex.Message}");
                }
            }
        }

        [AllowAnonymous, HttpGet("GetAPIVersion")]
        public IActionResult GetAPIVersion(string deviceType, string version)
        {
            if (string.IsNullOrEmpty(deviceType) || string.IsNullOrEmpty(version))
            {
                return BadRequest(new ApiCommonResponseModel
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Message = "Device type and version are required"
                });
            }

            deviceType = deviceType.ToLower();

            string latestVersion;
            switch (deviceType)
            {
                case "ios":
                    latestVersion = _configuration["AppSettings:Versions:iOS"]!;
                    break;
                case "android":
                    latestVersion = _configuration["AppSettings:Versions:Android"]!;
                    break;
                default:
                    return BadRequest(new ApiCommonResponseModel
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        Message = "Invalid device type. Supported types are 'ios' and 'android'"
                    });
            }

            if (string.IsNullOrEmpty(latestVersion))
            {
                return StatusCode(500, new ApiCommonResponseModel
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Message = "Configuration for app version is missing"
                });
            }


            bool updateRequired = !version.Equals(latestVersion, StringComparison.OrdinalIgnoreCase);

            var apiResponse = new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Data = updateRequired,
                Message = updateRequired ? "Update available" : "App is up to date"
            };

            return Ok(apiResponse);
        }

        [AllowAnonymous, HttpGet("GetAPIVersionV2")]
        public async Task<IActionResult> GetAPIVersionV2(string deviceType, string version)
        {
            if (string.IsNullOrEmpty(deviceType) || string.IsNullOrEmpty(version))
            {
                return BadRequest(new ApiCommonResponseModel
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Message = "Device type and version are required"
                });
            }

            deviceType = deviceType.ToLower();

            string latestVersion;
            switch (deviceType)
            {
                case "ios":
                    latestVersion = _configuration["AppSettings:Versions:iOS"]!;
                    break;
                case "android":
                    latestVersion = _configuration["AppSettings:Versions:Android"]!;
                    break;
                default:
                    return BadRequest(new ApiCommonResponseModel
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        Message = "Invalid device type. Supported types are 'ios' and 'android'"
                    });
            }

            if (string.IsNullOrEmpty(latestVersion))
            {
                return StatusCode(500, new ApiCommonResponseModel
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Message = "Configuration for app version is missing"
                });
            }

            bool updateRequired = !version.Equals(latestVersion, StringComparison.OrdinalIgnoreCase);
            string updateMessage = await _context.Settings
                         .Where(x => x.Code == "newVersion")
                         .Select(x => x.Value)
                         .FirstOrDefaultAsync() ?? string.Empty;

            var apiResponse = new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Data = new
                {
                    updateRequired,
                    message = updateRequired ? updateMessage : "Not required"
                },
                Message = updateRequired ? "Update available" : "App is up to date"
            };

            return Ok(apiResponse);
        }

        [HttpPost("validate")]
        public IActionResult ValidatePhoneNumber([FromBody] PhoneNumberModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Return bad request if model state is invalid
            }

            try
            {
                // Parse the phone number based on the country code provided
                PhoneNumber phoneNumber = _phoneNumberUtil.Parse(model.PhoneNumber, model.CountryCode);

                // Check if the phone number is valid based on the country rules
                if (!_phoneNumberUtil.IsValidNumber(phoneNumber))
                {
                    return BadRequest("Invalid phone number.");
                }

                // Check if the parsed number's region (country) matches the given country code
                string parsedRegionCode = _phoneNumberUtil.GetRegionCodeForNumber(phoneNumber);
                if (parsedRegionCode != model.CountryCode.ToUpper())
                {
                    return BadRequest($"Phone number is not valid for the country code {model.CountryCode}.");
                }

                return Ok("Phone number is valid.");
            }
            catch (NumberParseException ex)
            {
                // Handle any exceptions (e.g., incorrect format or unsupported country)
                return BadRequest($"Error parsing phone number: {ex.Message}");
            }
        }

        [AllowAnonymous]
        [HttpPost("send-order-confirmation")]
        public async Task<IActionResult> SendOrderConfirmation([FromBody] WhatsAppOrderConfirmationRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request payload.");
            }

            await _otherSerivce.SendWhatsappOrderConfirmationAsync(request);
            return Ok(new { message = "Order confirmation message sent successfully." });
        }

        [AllowAnonymous]
        [HttpGet("ClearUserData/{mobileNumber}")]
        public async Task<IActionResult> ClearUserData(string mobileNumber)
        {
            return Ok(await _otherSerivce.ClearUserData(mobileNumber));
        }

        /// <summary>
        /// calling from jarvisAlgo to update the token for AliceBlue:323377
        /// </summary>
        [AllowAnonymous, HttpPost("AutoLogin")]
        public async Task<IActionResult> AutoLogin(PartnerAccountsM partnerAccountParam)
        {
            var partnerAccount = await _context.PartnerAccountsM.Where(item => item.PartnerId == partnerAccountParam.PartnerId).FirstOrDefaultAsync();

            if (partnerAccount != null)
            {
                partnerAccount.GenerateToken = partnerAccountParam.GenerateToken;
                partnerAccount.ModifiedOn = DateTime.Now;
                await _context.SaveChangesAsync();
                await _mongoRepo.AddAsync(new Model.MongoDbCollection.Log { CreatedOn = DateTime.Now, Message = "Auto Loin for " + partnerAccountParam.PartnerId, Source = "AutoLogin", Category = "Success" });
            }
            else
            {
                await _mongoRepo.AddAsync(new Model.MongoDbCollection.Log { CreatedOn = DateTime.Now, Message = "Auto Loin for " + partnerAccountParam.PartnerId, Source = "AutoLogin", Category = "Failed" });
            }
            return Ok();
        }

        [HttpGet("CheckProductValidity")]
        public async Task<IActionResult> CheckProductValidity(int productId)
        {
            Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out Guid mobileUserKey);// Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "MobileUserKey")?.Value);
            var response = await _otherSerivce.CheckProductValidity(productId, mobileUserKey);
            return Ok(new ApiCommonResponseModel
            {
                Data = response, // true means he has active product subscription , false means expired 
                StatusCode = HttpStatusCode.OK
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("SendWhatsappSubscriptionExpiryNotification")]
        public async Task<IActionResult> SendWhatsappSubscriptionExpiryNotification()
        {
            try
            {
                await _otherSerivce.SendWhatsappSubscriptionExpiryNotificationAsync();
                return Ok(new { message = "Renewal WhatsApp message sent successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error sending WhatsApp message: {ex.Message}");
            }
        }

        [AllowAnonymous]
        [HttpPost("LmsWebhook")]
        public async Task<IActionResult> LmsWebhookWhatsAppAsync([FromBody] WhatsAppSubscriptionExpiryRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request payload.");
            }

            await _otherSerivce.LmsWebhookWhatsAppAsync();
            _response.Data = HttpStatusCode.OK;
            _response.Message = "WhatsApp message sent successfully.";
            return Ok(_response);
        }

//        [HttpPost("PaymentSuccess")]
//        [AllowAnonymous]
//        public async Task<IActionResult> PaymentSuccess()
//        {

//            string body;

//            // Read raw body from request
//            using (var reader = new StreamReader(HttpContext.Request.Body))
//            {
//                body = await reader.ReadToEndAsync();
//            }

////#if DEBUG
////            body = "mihpayid=403993715534525542&mode=DC&status=success&key=uhyxSr&txnid=TRANX777777&amount=41.00&addedon=2025-08-11+11%3A34%3A30&productinfo=Break+Out+Test&firstname=Ajith&lastname=&address1=&address2=&city=&state=&country=&zipcode=&email=ajith.codeline%40gmail.com&phone=918309898368&udf1=&udf2=&udf3=&udf4=&udf5=&udf6=&udf7=&udf8=&udf9=&udf10=&card_token=&card_no=XXXXXXXXXXXX0003&field0=&field1=332821019644604800&field2=407137&field3=41.00&field4=&field5=00&field6=02&field7=AUTHPOSITIVE&field8=AUTHORIZED&field9=Transaction+is+Successful&payment_source=apiIntInvoice&PG_TYPE=DC-PG&error=E000&error_Message=No+Error&cardToken=&net_amount_debit=41&discount=0.00&offer_key=&offer_availed=&unmappedstatus=captured&hash=e0302b1dacb8077bef61482451ea26b8859ca994a72d38e25c11d3b6faadfc65e7c63cc04ea1fc7906812e242dd73a3e76183d5786614dc91e06c278b81f1fb4&bank_ref_no=332821019644604800&bank_ref_num=332821019644604800&bankcode=MAST&surl=https%3A%2F%2Fuattools.payu.in%2FpaymentLink%2FpostBackParam.do&curl=https%3A%2F%2Fuattools.payu.in%2FpaymentLink%2FpostBackParam.do&furl=https%3A%2F%2Fuattools.payu.in%2FpaymentLink%2FpostBackParam.do&card_hash=25d25456fd0216e927141c1cc5df282d166e1b71e99d8958196654cd7399aaf3&pa_name=PayU";
////#endif

//            var webhookData = ParsePayUWebhookDataToModel(body);
//            // Log initial payload
//            await _purchaseOrderService.PayULogToMongo(JsonConvert.SerializeObject(webhookData), "PayU");

//            return Ok(new
//            {
//                Message = "Webhook Call Successful"
//            });
//        }

        [HttpPost("PaymentSuccess")]
        [AllowAnonymous]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> PaymentSuccess()
        {
            var form = await Request.ReadFormAsync();

            var webhookData = new PayUWebhookRequestModel
            {
                mihpayid = form["mihpayid"],
                mode = form["mode"],
                status = form["status"],
                key = form["key"],
                txnid = form["txnid"],
                amount = form["amount"],
                addedon = form["addedon"],
                productinfo = form["productinfo"],
                firstname = form["firstname"],
                lastname = form["lastname"],
                address1 = form["address1"],
                address2 = form["address2"],
                city = form["city"],
                state = form["state"],
                country = form["country"],
                zipcode = form["zipcode"],
                email = form["email"],
                phone = form["phone"],
                udf1 = form["udf1"],
                udf2 = form["udf2"],
                udf3 = form["udf3"],
                udf4 = form["udf4"],
                udf5 = form["udf5"],
                udf6 = form["udf6"],
                udf7 = form["udf7"],
                udf8 = form["udf8"],
                udf9 = form["udf9"],
                udf10 = form["udf10"],
                hash = form["hash"],
                bank_ref_no = form["bank_ref_no"],
                bankcode = form["bankcode"],
                unmappedstatus = form["unmappedstatus"]
                // add any other fields here
            };

            await _purchaseOrderService.PayULogToMongo(JsonConvert.SerializeObject(webhookData), "PayU");

            return Ok(new
            {
                Message = "Webhook Call Successful"
            });
        }

        //[HttpPost("PayUWebhook")]
        //[AllowAnonymous]
        //[Consumes("application/x-www-form-urlencoded")]
        //public async Task<IActionResult> PayUWebhook([FromQuery] bool test = false)
        //{
        //    PayUWebhookRequestModel webhookData;

        //    if (!test)
        //    {
        //        // This is the REAL PayU callback case
        //        if (!Request.HasFormContentType)
        //            return BadRequest("Expected application/x-www-form-urlencoded content type");

        //        var form = await Request.ReadFormAsync();

        //        webhookData = new PayUWebhookRequestModel
        //        {
        //            mihpayid = form["mihpayid"],
        //            mode = form["mode"],
        //            status = form["status"],
        //            key = form["key"],
        //            txnid = form["txnid"],
        //            amount = form["amount"],
        //            addedon = form["addedon"],
        //            productinfo = form["productinfo"],
        //            firstname = form["firstname"],
        //            lastname = form["lastname"],
        //            address1 = form["address1"],
        //            address2 = form["address2"],
        //            city = form["city"],
        //            state = form["state"],
        //            country = form["country"],
        //            zipcode = form["zipcode"],
        //            email = form["email"],
        //            phone = form["phone"],
        //            udf1 = form["udf1"],
        //            udf2 = form["udf2"],
        //            udf3 = form["udf3"],
        //            udf4 = form["udf4"],
        //            udf5 = form["udf5"],
        //            udf6 = form["udf6"],
        //            udf7 = form["udf7"],
        //            udf8 = form["udf8"],
        //            udf9 = form["udf9"],
        //            udf10 = form["udf10"],
        //            card_token = form["card_token"],
        //            card_no = form["card_no"],
        //            payment_source = form["payment_source"],
        //            PG_TYPE = form["PG_TYPE"],
        //            error = form["error"],
        //            error_Message = form["error_Message"],
        //            net_amount_debit = form["net_amount_debit"],
        //            discount = form["discount"],
        //            offer_key = form["offer_key"],
        //            offer_availed = form["offer_availed"],
        //            unmappedstatus = form["unmappedstatus"],
        //            hash = form["hash"],
        //            bank_ref_no = form["bank_ref_no"],
        //            bank_ref_num = form["bank_ref_num"],
        //            bankcode = form["bankcode"],
        //            surl = form["surl"],
        //            curl = form["curl"],
        //            furl = form["furl"],
        //            pa_name = form["pa_name"]
        //        };
        //    }
        //    else
        //    {
        //        //// This is the TEST case
        //        //webhookData = new PayUWebhookRequestModel
        //        //{
        //        //    mihpayid = "24707268077",
        //        //    mode = "UPI",
        //        //    status = "success",
        //        //    key = "WyTcVR",
        //        //    txnid = "TRAN114082025093924708",
        //        //    amount = "1.00",
        //        //    addedon = "2025-08-14 09:39:34",
        //        //    productinfo = "Break out",
        //        //    firstname = "Mr Seshi",
        //        //    lastname = "",
        //        //    address1 = "",
        //        //    address2 = "",
        //        //    city = "",
        //        //    state = "",
        //        //    country = "",
        //        //    zipcode = "",
        //        //    email = "kkk@gmail.com",
        //        //    phone = "917730015908",
        //        //    udf1 = "",
        //        //    udf2 = "",
        //        //    udf3 = "",
        //        //    udf4 = "",
        //        //    udf5 = "",
        //        //    udf6 = "",
        //        //    udf7 = "",
        //        //    udf8 = "",
        //        //    udf9 = "",
        //        //    udf10 = "",
        //        //    card_token = "",
        //        //    card_no = "",
        //        //    payment_source = "apiIntInvoice",
        //        //    PG_TYPE = "UPI-PG",
        //        //    error = "E000",
        //        //    error_Message = "No Error",
        //        //    net_amount_debit = "1",
        //        //    discount = "0.00",
        //        //    offer_key = "",
        //        //    offer_availed = "",
        //        //    unmappedstatus = "captured",
        //        //    hash = "95f8d5be37fe3e312d1a4b22c89b13b91dd361a43f62443b5fe519318dabacfbeec949009023a6c83ff06741091b07f38c55c5855bdd138de602621b8ecfac24",
        //        //    bank_ref_no = "522609227933",
        //        //    bank_ref_num = "522609227933",
        //        //    bankcode = "UPI",
        //        //    surl = "https://tools.payu.in/paymentLink/postBackParam.do",
        //        //    curl = "https://mobileapi.kingresearch.co.in/api/Payment/payments/payu-success",
        //        //    furl = "https://mobileapi.kingresearch.co.in/api/Payment/payments/payu-failure",
        //        //    pa_name = ""
        //        //};

        //        // Hardcoded PayU webhook form body (same format PayU sends)
        //        string body = "mihpayid=403993715534525542&mode=DC&status=success&key=uhyxSr&txnid=TRANX777777&amount=41.00&addedon=2025-08-11+11%3A34%3A30&productinfo=Break+Out+Test&firstname=Ajith&lastname=&address1=&address2=&city=&state=&country=&zipcode=&email=ajith.codeline%40gmail.com&phone=918309898368&udf1=&udf2=&udf3=&udf4=&udf5=&udf6=&udf7=&udf8=&udf9=&udf10=&card_token=&card_no=XXXXXXXXXXXX0003&field0=&field1=332821019644604800&field2=407137&field3=41.00&field4=&field5=00&field6=02&field7=AUTHPOSITIVE&field8=AUTHORIZED&field9=Transaction+is+Successful&payment_source=apiIntInvoice&PG_TYPE=DC-PG&error=E000&error_Message=No+Error&cardToken=&net_amount_debit=41&discount=0.00&offer_key=&offer_availed=&unmappedstatus=captured&hash=e0302b1dacb8077bef61482451ea26b8859ca994a72d38e25c11d3b6faadfc65e7c63cc04ea1fc7906812e242dd73a3e76183d5786614dc91e06c278b81f1fb4&bank_ref_no=332821019644604800&bank_ref_num=332821019644604800&bankcode=MAST&surl=https%3A%2F%2Fuattools.payu.in%2FpaymentLink%2FpostBackParam.do&curl=https%3A%2F%2Fuattools.payu.in%2FpaymentLink%2FpostBackParam.do&furl=https%3A%2F%2Fuattools.payu.in%2FpaymentLink%2FpostBackParam.do&card_hash=25d25456fd0216e927141c1cc5df282d166e1b71e99d8958196654cd7399aaf3&pa_name=PayU";

        //        // Convert form-urlencoded to JSON
        //        var parsed = System.Web.HttpUtility.ParseQueryString(body);
        //        var dict = parsed.AllKeys.ToDictionary(k => k, k => parsed[k]);
        //        string json = System.Text.Json.JsonSerializer.Serialize(dict, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

        //        // json now contains proper JSON version of webhook data
        //        Console.WriteLine(json);
        //        await _purchaseOrderService.PayULogToMongo(JsonConvert.SerializeObject(json), test ? "PayU-Test" : "PayU");
        //    }

        //    // Save to Mongo
        //    await _purchaseOrderService.PayULogToMongo(JsonConvert.SerializeObject(web), test ? "PayU-Test" : "PayU");

        //    // Save to SQL
        //    var res = await _purchaseOrderService.PayUWebhookDataProcessing(webhookData);

        //    return Ok(res);
        //}

        [HttpPost("PayUWebhook")]
        [AllowAnonymous]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> PayUWebhook([FromQuery] bool test = false)
        {
            PayUWebhookRequestModel webhookData;

            if (!test)
            {
                // REAL PayU callback case
                if (!Request.HasFormContentType)
                    return BadRequest("Expected application/x-www-form-urlencoded content type");

                var form = await Request.ReadFormAsync();

                webhookData = new PayUWebhookRequestModel
                {
                    mihpayid = form["mihpayid"],
                    mode = form["mode"],
                    status = form["status"],
                    key = form["key"],
                    txnid = form["txnid"],
                    amount = form["amount"],
                    addedon = form["addedon"],
                    productinfo = form["productinfo"],
                    firstname = form["firstname"],
                    lastname = form["lastname"],
                    address1 = form["address1"],
                    address2 = form["address2"],
                    city = form["city"],
                    state = form["state"],
                    country = form["country"],
                    zipcode = form["zipcode"],
                    email = form["email"],
                    phone = form["phone"],
                    udf1 = form["udf1"],
                    udf2 = form["udf2"],
                    udf3 = form["udf3"],
                    udf4 = form["udf4"],
                    udf5 = form["udf5"],
                    udf6 = form["udf6"],
                    udf7 = form["udf7"],
                    udf8 = form["udf8"],
                    udf9 = form["udf9"],
                    udf10 = form["udf10"],
                    card_token = form["card_token"],
                    card_no = form["card_no"],
                    payment_source = form["payment_source"],
                    PG_TYPE = form["PG_TYPE"],
                    error = form["error"],
                    error_Message = form["error_Message"],
                    net_amount_debit = form["net_amount_debit"],
                    discount = form["discount"],
                    offer_key = form["offer_key"],
                    offer_availed = form["offer_availed"],
                    unmappedstatus = form["unmappedstatus"],
                    hash = form["hash"],
                    bank_ref_no = form["bank_ref_no"],
                    bank_ref_num = form["bank_ref_num"],
                    bankcode = form["bankcode"],
                    surl = form["surl"],
                    curl = form["curl"],
                    furl = form["furl"],
                    pa_name = form["pa_name"]
                };
            }
            else
            {
                // TEST case → simulate PayU webhook body
                string body =
                    "mihpayid=403993715534525542&mode=DC&status=success&key=uhyxSr&txnid=TRANX777777&amount=41.00&addedon=2025-08-11+11%3A34%3A30&productinfo=Break+Out+Test&firstname=Ajith&lastname=&address1=&address2=&city=&state=&country=&zipcode=&email=ajith.codeline%40gmail.com&phone=918309898368&udf1=&udf2=&udf3=&udf4=&udf5=&udf6=&udf7=&udf8=&udf9=&udf10=&card_token=&card_no=XXXXXXXXXXXX0003&field0=&field1=332821019644604800&field2=407137&field3=41.00&field4=&field5=00&field6=02&field7=AUTHPOSITIVE&field8=AUTHORIZED&field9=Transaction+is+Successful&payment_source=apiIntInvoice&PG_TYPE=DC-PG&error=E000&error_Message=No+Error&cardToken=&net_amount_debit=41&discount=0.00&offer_key=&offer_availed=&unmappedstatus=captured&hash=e0302b1dacb8077bef61482451ea26b8859ca994a72d38e25c11d3b6faadfc65e7c63cc04ea1fc7906812e242dd73a3e76183d5786614dc91e06c278b81f1fb4&bank_ref_no=332821019644604800&bank_ref_num=332821019644604800&bankcode=MAST&surl=https%3A%2F%2Fuattools.payu.in%2FpaymentLink%2FpostBackParam.do&curl=https%3A%2F%2Fuattools.payu.in%2FpaymentLink%2FpostBackParam.do&furl=https%3A%2F%2Fuattools.payu.in%2FpaymentLink%2FpostBackParam.do&card_hash=25d25456fd0216e927141c1cc5df282d166e1b71e99d8958196654cd7399aaf3&pa_name=PayU";

                var parsed = System.Web.HttpUtility.ParseQueryString(body);

                webhookData = new PayUWebhookRequestModel
                {
                    mihpayid = parsed["mihpayid"],
                    mode = parsed["mode"],
                    status = parsed["status"],
                    key = parsed["key"],
                    txnid = parsed["txnid"],
                    amount = parsed["amount"],
                    addedon = parsed["addedon"],
                    productinfo = parsed["productinfo"],
                    firstname = parsed["firstname"],
                    lastname = parsed["lastname"],
                    address1 = parsed["address1"],
                    address2 = parsed["address2"],
                    city = parsed["city"],
                    state = parsed["state"],
                    country = parsed["country"],
                    zipcode = parsed["zipcode"],
                    email = parsed["email"],
                    phone = parsed["phone"],
                    udf1 = parsed["udf1"],
                    udf2 = parsed["udf2"],
                    udf3 = parsed["udf3"],
                    udf4 = parsed["udf4"],
                    udf5 = parsed["udf5"],
                    udf6 = parsed["udf6"],
                    udf7 = parsed["udf7"],
                    udf8 = parsed["udf8"],
                    udf9 = parsed["udf9"],
                    udf10 = parsed["udf10"],
                    card_token = parsed["card_token"],
                    card_no = parsed["card_no"],
                    payment_source = parsed["payment_source"],
                    PG_TYPE = parsed["PG_TYPE"],
                    error = parsed["error"],
                    error_Message = parsed["error_Message"],
                    net_amount_debit = parsed["net_amount_debit"],
                    discount = parsed["discount"],
                    offer_key = parsed["offer_key"],
                    offer_availed = parsed["offer_availed"],
                    unmappedstatus = parsed["unmappedstatus"],
                    hash = parsed["hash"],
                    bank_ref_no = parsed["bank_ref_no"],
                    bank_ref_num = parsed["bank_ref_num"],
                    bankcode = parsed["bankcode"],
                    surl = parsed["surl"],
                    curl = parsed["curl"],
                    furl = parsed["furl"],
                    pa_name = parsed["pa_name"]
                };
            }

            // Save raw payload (for debugging/auditing)
            await _purchaseOrderService.PayULogToMongo(JsonConvert.SerializeObject(webhookData), test ? "PayU-Test" : "PayU");

            // Save processed data to SQL
            var res = await _purchaseOrderService.PayUWebhookDataProcessing(webhookData);

            return Ok(res);
        }

        [HttpPost("PaymentFailure")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymentFailure()
        {
            var form = await Request.ReadFormAsync();

            var txnId = form["txnid"];
            var status = form["status"];
            var amount = form["amount"];
            var postedHash = form["hash"];
            var error = form["error"];
            var formDataString = string.Join(", ", form.Select(x => $"{x.Key}={x.Value}"));
            // Log or update failed transaction in your system

            await _mongoRepo.AddAsync(new Log
            {
                Category = "PayU",
                CreatedOn = DateTime.Now,
                Message = $"Payment Failed. Data : {formDataString}",
                //Message = $"Payment Failed. Date : {DateTime.Now}, TransactionId : {txnId}, Status : {status}, Error : {error}. full form Data : {formDataString}",
                Source = "PayU"
            });

            return Ok(new
            {
                Message = "Payment Failed",
                TransactionId = txnId,
                Status = status,
                Error = error
            });
        }

        //[HttpPost("CreatePayuPayment")]
        //[AllowAnonymous]
        //public async Task<IActionResult> CreatePaymentLink([FromBody] PaymentDetailStatusRequestModel requestModel)
        //{
        //    var _response = await _purchaseOrderService.CreatePayUPaymentLink(requestModel);
        //    return Ok(_response);
        //}

        //private async Task<string> GetAccessTokenAsync()
        //{
        //    var clientId = _configuration["PayU:ClientId"];                     // PRODUCTION AUTH
        //    var clientSecret = _configuration["PayU:ClientSecretId"];           // PRODUCTION AUTH

        //    //var clientId = _configuration["PayU:UatClientId"];                     // UAT AUTH
        //    //var clientSecret = _configuration["PayU:UatClientSecretId"];           // UAT AUTH

        //    try
        //    {
        //         var client = new RestClient(_configuration["PayU:AthUrl"]!); // PRODUCTION AUTH
        //        //var clientUat = new RestClient(_configuration["PayU:AuthUatUrl"]!); // UAT AUTH
        //        var request = new RestRequest("/oauth/token", Method.Post);

        //        request.AddHeader("accept", "application/json");
        //        request.AddHeader("content-type", "application/x-www-form-urlencoded");

        //        request.AddParameter("grant_type", "client_credentials");
        //        request.AddParameter("client_id", clientId);          //  Replace with your actual client ID
        //        request.AddParameter("client_secret", clientSecret);  //  Replace with your actual secret
        //        request.AddParameter("scope", "create_payment_links update_payment_links read_payment_links");

        //        //var response = await clientUat.ExecutePostAsync(request);
        //         var response = await client.ExecutePostAsync(request);

        //        if (response.IsSuccessful && response.Content != null)
        //        {
        //            var content = JsonDocument.Parse(response.Content);
        //            var token = content.RootElement.GetProperty("access_token").GetString();
        //            return token!;
        //        }
        //        else
        //        {
        //            // Log error if needed
        //            await _purchaseOrderService.PayULogToMongo(
        //                $"Failed to get access token: {response.StatusCode}, Content: {response.Content}",
        //                "PayU AccessToken"
        //            );

        //            return null;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw; // rethrow to let your controller catch this if needed
        //    }
        //}

        //private async Task<string> GetAccessTokenAsync()
        //{
        //    // If we already have a token and it hasn't expired, return it
        //    lock (_tokenLock)
        //    {
        //        if (!string.IsNullOrEmpty(_cachedAccessToken) && DateTime.UtcNow < _tokenExpiryTime)
        //        {
        //            return _cachedAccessToken;
        //        }
        //    }

        //    try
        //    {
        //        var clientUat = new RestClient(_configuration["PayU:AuthUatUrl"]!);
        //        var request = new RestRequest("/oauth/token", Method.Post);

        //        request.AddHeader("accept", "application/json");
        //        request.AddHeader("content-type", "application/x-www-form-urlencoded");

        //        request.AddParameter("grant_type", "client_credentials");
        //        request.AddParameter("client_id", _configuration["PayU:ClientId"]);
        //        request.AddParameter("client_secret", _configuration["PayU:ClientSecretId"]);
        //        request.AddParameter("scope", "create_payment_links update_payment_links read_payment_links");

        //        var response = await clientUat.ExecutePostAsync(request);

        //        if (response.IsSuccessful && response.Content != null)
        //        {
        //            var content = JsonDocument.Parse(response.Content);
        //            var token = content.RootElement.GetProperty("access_token").GetString();
        //            var expiresIn = content.RootElement.GetProperty("expires_in").GetInt32(); // in seconds

        //            lock (_tokenLock)
        //            {
        //                _cachedAccessToken = token;
        //                _tokenExpiryTime = DateTime.UtcNow.AddSeconds(expiresIn - 60); // refresh 1 min before expiry
        //            }

        //            return token!;
        //        }
        //        else
        //        {
        //            await _purchaseOrderService.PayULogToMongo(
        //                $"Failed to get access token: {response.StatusCode}, Content: {response.Content}",
        //                "PayU AccessToken"
        //            );
        //            return null;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await _purchaseOrderService.PayULogToMongo(
        //            $"Exception while getting token: {ex.Message}",
        //            "PayU AccessToken"
        //        );
        //        throw;
        //    }
        //}
    }
}
