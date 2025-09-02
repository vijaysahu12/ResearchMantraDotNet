using Azure.Core;
using RM.CommonServices;
using RM.CommonServices.Services;
using RM.Database.ResearchMantraContext;
using RM.Database.MongoDbContext;
using RM.Model;
using RM.Model.Common;
using RM.Model.Models;
using RM.Model.RequestModel;
using RM.Model.RequestModel.MobileApi;
using RM.MService.Helpers;
using RM.MService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net;
using System.Text.Json;
using static RM.MService.Services.PurchaseorderMService;

namespace RM.MobileAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        readonly IPurchaseOrderMService _purchaseOrderService;
        private readonly ResearchMantraContext _context;
        private readonly IMongoRepository<Model.MongoDbCollection.Log> _mongoRepo;
        private readonly IOtherService _otherSerivce;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public PaymentController(IPurchaseOrderMService purchaseOrderService, ResearchMantraContext context,
            IMongoRepository<Model.MongoDbCollection.Log> mongoRepo, IOtherService otherSerivce, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _purchaseOrderService = purchaseOrderService;
            _context = context;
            _mongoRepo = mongoRepo;
            _otherSerivce = otherSerivce;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        [HttpPost("GetPaymentGatewayDetails")]
        public async Task<IActionResult> GetPaymentGatewayDetails(string providerName)
        {
            return Ok(await _purchaseOrderService.GetPaymentGatewayDetails(providerName));
        }

        
        [HttpPost("ManagePurchaseOrder")]
        public async Task<IActionResult> ManagePurchaseOrder(PurchaseOrderMRequestModel request)
        {
            return Ok(await _purchaseOrderService.ManagePurchaseOrder(request));
        }

        //[HttpPost("GenerateProductSpecificCoupon")]
        //public async Task<IActionResult> GenerateProductSpecificCoupon(GenerateCouponRequestModel request)
        //{
        //    return Ok(await _purchaseOrderService.GenerateProductSpecificCoupon(request));
        //}

        [HttpPost("ValidateCoupon")]
        public async Task<IActionResult> ValidateCoupon(ValidateCouponRequestModel request)
        {
            return Ok(await _purchaseOrderService.ValidateCoupon(request));
        }

        [HttpPost("GenerateCoupon")]
        public async Task<IActionResult> GenerateCoupon(GenerateCouponRequestModel request)
        {
            return Ok(await _purchaseOrderService.GenerateCoupon(request));
        }

        [HttpPost("ByPassThePaymentGateway")]
        public async Task<IActionResult> ByPassThePaymentGateway(ByPassPaymentGatewayRequestModel request)
        {
            return Ok(await _purchaseOrderService.ByPassThePaymentGateway(request));
        }


        /// <summary>
        /// THis method is only to create 1 entry in PaymentRequestStatus Table so that on webhook we can verify the productid,clientId and subscriptionModel
        /// </summary>
        [HttpPost("AddPaymentRequest")]
        public async Task<IActionResult> PaymentRequestStatusInstamojo([FromBody] PaymentDetailStatusRequestModel request)
        {
            var res = await _purchaseOrderService.AddIntoPaymentRequestStatus(request);
            return Ok(new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = res != null && res.Id > 0 ? "Payment Request Status Added Successfully" : "Failed to Add Payment Request Status",
                Data = res != null && res.Id > 0
            });
        }

        [HttpPost("AddPaymentRequestV2")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymentRequestStatusV2([FromBody] PaymentDetailStatusRequestModel request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.MerchantTransactionID))
            {
                return BadRequest("Invalid request data.");
            }

            // Get the active payment gateway setting
            var response = await _context.Settings
                .Where(x => x.Code == "InstaMojoGateWay" || x.Code == "payUGateWay" && x.IsActive == true)
                .FirstOrDefaultAsync();

            if (response == null)
            {
                return BadRequest("No active payment gateway found in settings.");
            }

            object res;

            if (response.Value == "InstaMojo")
            {
                res = await _purchaseOrderService.AddIntoInstaMojoPaymentRequestStatusV2(request);
            }
            else
            {
                res = await _purchaseOrderService.AddIntoPaymentRequestStatusV2(request);
            }

            return Ok(res);
        }

       

        [HttpPost]
        [Route("InstaMojo")]
        [AllowAnonymous] //Kept due to web hook call -- mojo team will be calling this API
        public async Task<IActionResult> InstaMojo()
        {
            string body;
            using (var reader = new StreamReader(HttpContext.Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }

#if DEBUG
            body = "payment_id=MOJO5818L05Q77630988&status=Credit&shorturl=https%3A%2F%2Fimjo.in%2F4T2zYJ&longurl=https%3A%2F%2Fwww.instamojo.com%2F%40TheKingResearch%2F7c29ca0a2e624903a0f6a79f179b63fa&purpose=Momentum+Investment+Strategy&amount=10.00&fees=0.10&currency=INR&buyer=pranathi%40gmail.com&buyer_name=pranathi&buyer_phone=%2B918423657055&payment_request_id=7c29ca0a2e624903a0f6a79f179b63fa&mac=834a336ae226b818543e24fec27d7c882a4828bc ";
#endif

            FileHelper.WriteToFile("InstaMojo_PG", body);

            if (string.IsNullOrWhiteSpace(body))
            {
                await _purchaseOrderService.LogToMongo("0#BR JSON Body is null");
                return BadRequest();
            }

            var webhookData = ParseInstaMojoWebhookDataToModel(body);
            await _purchaseOrderService.LogToMongo(body);

            if (webhookData == null)
            {
                await _purchaseOrderService.LogToMongo("0#BR Invalid or missing 'Response' field", "DeserializeObject is Null");
                return BadRequest("Invalid or missing 'Response' field");
            }
            var res = await _purchaseOrderService.InstaMojoWebhokDataProcessing(webhookData);

            return Ok(res);
        }

        [HttpGet]
        [Route("GetPaymentStatus")]
        public async Task<IActionResult> GetInstamojoResponseStatus([Required] string paymentRequestId, string paymentGatewayName)
        {
            if (paymentGatewayName == "INSTAMOJO")
            {
                var _response = await _purchaseOrderService.GetInstamojoResponseStatus(paymentRequestId);
                return Ok(_response);
            }
            else if (paymentGatewayName == "PHONEPE")
            {
                var _response = await _purchaseOrderService.GetPhonePeResponseStatus(paymentRequestId);
                return Ok();
            }
            return Ok();
        }

        /// <summary>
        /// PayU ResponseStatusV2 for checking with only paymnetRequestId. 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetPaymentStatusV2")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPaymentStatusV2([FromQuery] string paymentRequestId)
        {
            if (string.IsNullOrEmpty(paymentRequestId))
                return BadRequest("PaymentRequestId is required.");

            // ✅ Check which gateway is active
            var activeGateway = await _context.Settings
                .Where(x => x.IsActive == true &&
                            (x.Code == "InstaMojoGateWay" || x.Code == "PayUGateWay"))
                .Select(x => x.Code)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(activeGateway))
                return BadRequest("No active payment gateway configured.");

            // ✅ Log which gateway is used
            await _purchaseOrderService.PayULogToMongo(paymentRequestId, $"Active Gateway: {activeGateway}");

            object response;

            if (activeGateway == "PayUGateWay")
            {
                var request = new PaymentStatusRequest
                {
                    PaymentRequestId = paymentRequestId,
                };

                await _purchaseOrderService.PayULogToMongo(paymentRequestId, "PayU Raw");
                response = await _purchaseOrderService.GetPayUResponseStatus(request);
            }
            else if (activeGateway == "InstaMojoGateWay")
            {
                await _purchaseOrderService.PayULogToMongo(paymentRequestId, "Instamojo Raw");
                response = await _purchaseOrderService.GetInstamojoResponseStatus(paymentRequestId);
            }
            else
            {
                await _purchaseOrderService.PayULogToMongo(paymentRequestId, "Instamojo Raw");
                return BadRequest("Unsupported payment gateway.");
            }

            return Ok(response);
        }


        //[HttpPost("payments/payu-success")]
        //[AllowAnonymous]
        //public  IActionResult PayUSuccess([FromForm] PayUCallbackModel model)
        //{
        //    _purchaseOrderService.PayULogToMongo(model.txnid, "PayUAccessToken");
        //    if (model == null || string.IsNullOrWhiteSpace(model.txnid))
        //    {
        //         _purchaseOrderService.PayULogToMongo(model.txnid, "PayUAccessToken");
        //        return BadRequest("Invalid transaction ID.");
        //    }
        //    // Verify payment, update DB, etc.
        //    return Redirect($"https://crm.kingresearch.co.in/#/paymentgateway?mtid={model.txnid}&status=success");
        //}

        //[HttpPost("payments/payu-failure")]
        //[AllowAnonymous]
        //public IActionResult PayUfailure([FromForm] PayUCallbackModel model)
        //{
        //    // Verify payment, update DB, etc.
        //    return Redirect($"https://crm.kingresearch.co.in/#/paymentgateway?mtid={model.txnid}&status=failure");
        //}

        //[HttpGet("payments/payment-success")]
        //[HttpPost("payments/payment-success")]
        //[AllowAnonymous]
        //public async Task<IActionResult> PayUSuccess()
        //{
        //    // 🔹 STEP 1: Check active payment gateway setting
        //    var activeGateway = await _context.Settings
        //        .Where(x => x.IsActive == true &&
        //                    (x.Code == "InstaMojoGateWay" || x.Code == "PayUGateWay"))
        //        .Select(x => x.Code)
        //        .FirstOrDefaultAsync();

        //    if (string.IsNullOrEmpty(activeGateway))
        //    {
        //        return BadRequest("No active payment gateway found in settings.");
        //    }

        //    // 🔹 STEP 2: Log headers for debugging
        //    foreach (var header in Request.Headers)
        //    {
        //        await _purchaseOrderService.PayULogToMongo($"{header.Key}: {header.Value}", $"{activeGateway} Headers");
        //    }

        //    // 🔹 STEP 3: Read raw body
        //    string rawBody;
        //    using (var reader = new StreamReader(Request.Body))
        //    {
        //        rawBody = await reader.ReadToEndAsync();
        //    }
        //    await _purchaseOrderService.PayULogToMongo(rawBody, $"{activeGateway} Raw Callback");

        //    object model = null;

        //    // 🔹 STEP 4: Parse callback differently for PayU vs InstaMojo
        //    if (activeGateway == "PayUGateWay")
        //    {
        //        if (Request.ContentType?.Contains("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) == true)
        //        {
        //            var form = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(rawBody);
        //            model = new PayUCallbackModel
        //            {
        //                txnid = form.TryGetValue("txnid", out var txn) ? txn.ToString() : null,
        //                status = form.TryGetValue("status", out var Status) ? Status.ToString() : null,
        //                mihpayid = form.TryGetValue("mihpayid", out var mihpayid) ? mihpayid.ToString() : null,
        //                amount = form.TryGetValue("amount", out var amt) ? amt.ToString() : null
        //            };
        //        }
        //        else if (Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
        //        {
        //            model = System.Text.Json.JsonSerializer.Deserialize<PayUCallbackModel>(rawBody);
        //        }
        //    }
        //    else if (activeGateway == "InstaMojoGateWay")
        //    {
        //        model = System.Text.Json.JsonSerializer.Deserialize<InstaMojoCallbackModel>(rawBody);
        //    }

        //    // 🔹 STEP 5: Validate
        //    if (model == null)
        //    {
        //        await _purchaseOrderService.PayULogToMongo("Invalid model", $"{activeGateway} Error");
        //        return BadRequest("Invalid payment callback data.");
        //    }

        //    // 🔹 STEP 6: Redirect with common format
        //    string txnid = activeGateway == "PayUGateWay"
        //        ? ((PayUCallbackModel)model).txnid
        //        : ((InstaMojoCallbackModel)model).PaymentId;

        //    string status = activeGateway == "PayUGateWay"
        //        ? ((PayUCallbackModel)model).status
        //        : ((InstaMojoCallbackModel)model).Status;

        //    return Redirect($"https://crm.kingresearch.co.in/#/paymentgateway?mtid={txnid}&status={status}");
        //}

        [HttpGet("payments/payment-success")]
        [HttpPost("payments/payment-success")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymentSuccess()
        {
            // ✅ Convert Query to JSON for logging

            var queryDict = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
            await _purchaseOrderService.PayULogToMongo($"Payment Redirect Query JSON: {queryDict}", "PaymentRedirect");

            var queryJson = System.Text.Json.JsonSerializer.Serialize(queryDict);

            await _purchaseOrderService.PayULogToMongo($"Payment Redirect Query JSON: {queryJson}", "PaymentRedirect");

            // ✅ STEP 1: Get active gateway from DB
            var activeGateway = await _context.Settings
                .Where(x => x.IsActive == true &&
                            (x.Code == "InstaMojoGateWay" || x.Code == "PayUGateWay"))
                .Select(x => x.Code)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(activeGateway))
            {
                return BadRequest("No active payment gateway found in settings.");
            }

            // ✅ STEP 2: Log headers for debugging
            foreach (var header in Request.Headers)
            {
                await _purchaseOrderService.PayULogToMongo($"{header.Key}: {header.Value}", $"{activeGateway} Headers");
            }

            // ✅ STEP 3: Handle PayU
            if (activeGateway == "PayUGateWay")
            {
                var payuModel = new PayUCallbackModel
                {
                    txnid = queryDict.ContainsKey("txnid") ? queryDict["txnid"] : null,
                    mihpayid = queryDict.ContainsKey("mihpayid") ? queryDict["mihpayid"] : null,
                    status = queryDict.ContainsKey("status") ? queryDict["status"] : null,
                    amount = queryDict.ContainsKey("amount") ? queryDict["amount"] : null,
                    productinfo = queryDict.ContainsKey("productinfo") ? queryDict["productinfo"] : null,
                    firstname = queryDict.ContainsKey("firstname") ? queryDict["firstname"] : null,
                    email = queryDict.ContainsKey("email") ? queryDict["email"] : null
                };

                return Redirect($"https://crm.kingresearch.co.in/#/paymentgateway?mtid={payuModel.txnid}&status={payuModel.status}");
            }

            // ✅ STEP 4: Handle Instamojo
            if (activeGateway == "InstaMojoGateWay")
            {
                var instamojoModel = new InstaMojoCallbackModel
                {
                    PaymentId = queryDict.ContainsKey("payment_id") ? queryDict["payment_id"] : null,
                    Status = queryDict.ContainsKey("payment_status") ? queryDict["payment_status"] : null,
                    PaymentRequestId = queryDict.ContainsKey("payment_request_id") ? queryDict["payment_request_id"] : null
                };

                return Redirect($"https://crm.kingresearch.co.in/#/paymentgateway?mtid={instamojoModel.PaymentRequestId}&status={instamojoModel.Status}");
            }

            return BadRequest("Unsupported payment gateway");
        }


        [HttpPost("payments/payu-failure")]
        [AllowAnonymous]
        public async Task<IActionResult> PayUfailure()
        {
            string rawBody;
            using (var reader = new StreamReader(Request.Body))
            {
                rawBody = await reader.ReadToEndAsync();
            }
            await _purchaseOrderService.PayULogToMongo(rawBody, "PayU Failure Raw");

            // You can parse and log the form data here like in PayUSuccess()

            var form = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(rawBody);
            var txnid = form.ContainsKey("txnid") ? form["txnid"].ToString() : null;

            return Redirect($"https://crm.kingresearch.co.in/#/paymentgateway?mtid={txnid}&status=failure");
        }


        public class PayUCallbackModel
        {
            public string mihpayid { get; set; }
            public string txnid { get; set; }
            public string status { get; set; }
            public string amount { get; set; }
            public string productinfo { get; set; }
            public string firstname { get; set; }
            public string email { get; set; }
        }

        public class InstaMojoCallbackModel
        {
            public string PaymentId { get; set; }          // payment_id
            public string PaymentRequestId { get; set; }   // payment_request_id
            public string Status { get; set; }             // status (Credit, Failed, etc.)
            public string BuyerName { get; set; }          // buyer_name
            public string BuyerEmail { get; set; }         // buyer (email)
            public string BuyerPhone { get; set; }         // buyer_phone
            public string Currency { get; set; }           // currency
            public string Amount { get; set; }             // amount
            public string Fees { get; set; }               // fees
            public string Purpose { get; set; }            // purpose (your product name)
            public string Mac { get; set; }                // mac (HMAC signature for verification)
        }


        private InstaMojoWebhookData ParseInstaMojoWebhookDataToModel(string formBody)
        {
            var dict = ParseFormBody(formBody);
            var model = new InstaMojoWebhookData();

            foreach (var prop in typeof(InstaMojoWebhookData).GetProperties())
            {
                if (dict.TryGetValue(prop.Name, out var value))
                {
                    prop.SetValue(model, value);
                }
            }
            return model;
        }

        private Dictionary<string, string> ParseFormBody(string formBody)
        {
            return formBody.Split('&')
                           .Select(part => part.Split('='))
                           .Where(part => part.Length == 2)
                           .ToDictionary(
                               part => Uri.UnescapeDataString(part[0]),
                               part => Uri.UnescapeDataString(part[1]));
        }

        /// <summary>
        /// Gets all generated invoice PDFs from the Invoices folder.
        /// </summary>
        /// <returns>List of invoice file names and download URLs.</returns>
        [HttpGet("GetInvoices")]
        public IActionResult GetInvoices()
        {
            string invoiceDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Invoices");

            if (!Directory.Exists(invoiceDirectory))
                return Ok(new List<object>()); // Empty if folder doesn't exist

            // Get base URL (e.g., https://localhost:7001)
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            // Get all .pdf files in the Invoices folder
            var invoiceFiles = Directory.GetFiles(invoiceDirectory, "*.pdf")
                .Select(filePath => new
                {
                    FileName = Path.GetFileName(filePath),
                    Url = $"{baseUrl}/api/Invoice/DownloadInvoice?fileName={Path.GetFileName(filePath)}"
                })
                .ToList();

            return Ok(invoiceFiles);
        }

        [HttpGet("GetInvoicesByMobileUserKey")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPhonePe(Guid? mobileUserKey, int pageNumber, int pagiSize)
        {
#if DEBUG
            mobileUserKey = Guid.Parse("9d083eda-01ab-ef11-b335-dd38ec0f4d1d");
#endif

            var response = await _purchaseOrderService.GetInvoicesByMobileUserKeyAsync(mobileUserKey,pageNumber,pagiSize);

            return Ok(response);
        }

        [HttpGet("DownloadInvoice")]
        [AllowAnonymous]
        public IActionResult DownloadInvoice(string fileName)
        {
            // Prevent directory traversal
            if (string.IsNullOrWhiteSpace(fileName) || fileName.Contains(".."))
                return BadRequest("Invalid file name.");

            string sharedInvoiceDirectory = @"D:\SharedFiles\Invoices";
            string filePath = Path.Combine(sharedInvoiceDirectory, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("Invoice file not found.");

            var bytes = System.IO.File.ReadAllBytes(filePath);
            return File(bytes, "application/pdf", fileName);
        }
    }
}
