using RM.API.Dtos;
using RM.API.Helpers;
using RM.API.Models.Mail;
using RM.API.Services;
using RM.Database.KingResearchContext;
using RM.Model.MongoDbCollection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValuesController : ControllerBase
    {
        private readonly KingResearchContext _gurujiDbContext;
        private readonly IMailService _mailService;
        public ValuesController(KingResearchContext gurujiDbContext, IMailService mailService)
        {
            _gurujiDbContext = gurujiDbContext;
            _mailService = mailService;
        }

        [AllowAnonymous]
        // GET api/values
        [HttpGet]
        public async Task<IActionResult> GetValues(string email, string body)
        {
            await _mailService.SendEmailAsync(new Models.Mail.MailRequest
            {

                Body = body,
                Subject = "Kingresearch Test",
                ToEmail = email
            });

            return Ok();


        }

        //public async Task<ActionResult<User>> GetUserById(int UserId)
        //{
        //    var userParam = new SqlParameter("@Key", UserId);
        //    var user = await _gurujiDbContext.Users.FromSqlRaw($"sp_GetUsersbyKey", userParam).ToListAsync();
        //    //return user;

        //}


        // GET api/values/5

        [HttpGet("MobileProductTopics")]
        public async Task<IActionResult> MobileProductTopics()
        {
            var codes = _gurujiDbContext.ProductsM
                .Where(item => !item.IsDeleted && item.IsActive)
                .Select(x => new { x.Code, x.Name, x.Id })
                .OrderBy(x => x.Name);

            return Ok(codes);
        }



        [HttpGet("{id}")]
        public IActionResult GetValue(string id)
        {
            // var identity = HttpContext.User.Identity as ClaimsIdentity;
            // //if (identity != null)
            // //{
            //     IEnumerable<Claim> claims = identity.Claims;
            //     // or
            //    var usernameClaim = claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();

            // //}

            //// var values = await _gurujiDbContext.Pincodes.Where(p => p.PincodeValue.Contains(id)).ToListAsync();
            // return Ok(usernameClaim.Value);


            TokenAnalyser tokenAnalyser = new();
            TokenVariables tokenVariables = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                tokenVariables = tokenAnalyser.FetchTokenValues(identity);
            }

            return tokenVariables != null ? Ok(tokenVariables.PublicKey) : (IActionResult)null;
        }

        [AllowAnonymous]
        [Route("/krinstamojo")]
        [HttpPost]
        public async Task<IActionResult> KrInstamojo()
        {
            //try
            //{
                using StreamReader reader = new(HttpContext.Request.Body);
                // You now have the body string raw
                string queryString = await reader.ReadToEndAsync();

                // queryString = "amount=4999.00&buyer=contactakshay3%40gmail.com&buyer_name=Akshay+Pawar&buyer_phone=%2B919833961033&currency=INR&fees=94.98&longurl=https%3A%2F%2Fwww.instamojo.com%2F%40kingresearch_financial%2F83f108ede1f74782ab315c36750cef52&payment_id=MOJO8402030271546002&payment_request_id=83f108ede1f74782ab315c36750cef52&purpose=No+Brainer+Masterclass++19th+August+2023&shorturl=&status=Credit&mac=ca6e154828b1025775e306b09eb989d38dadc0c9";
                //queryString = "{'payment': {'payment_id': 'MOJO3816I05Q64432608', 'status': 'SUCCESS', 'amount': '9.00', 'currency': 'INR', 'completed_at': '2023-08-16 06:14:15.616903+00:00', 'resource_uri': 'https://api.instamojo.com/v2/payments/MOJO3816I05Q64432608/'}, 'buyer': {'name': 'kumud saini', 'email': 'kumud@yopmail.com', 'phone': '+919411122233'}, 'seller': {'account_id': '553c0c00877d4faa93a7a1b0f08a41cf'}, 'details': [{'amount': '9.00', 'purpose': 'Testing Webhook Kingresearch', 'quantity': 1}], 'page_id': 'kingresear-4b2893f6dd964cfa8161', 'discount_code': null, 'custom_fields': {'shipping_address': {'address': null, 'city': null, 'postal_code': null, 'state': null, 'country': null}, 'additional_custom_fields': null, 'gstin': null, 'company_name': null, 'billing_address': {'address': null, 'city': null, 'postal_code': null, 'state': null, 'country': null}}, 'message_authentication_code': '6e2d8ef0ac8848c8c41487cdf4cf8c24e961c789'}";
                NameValueCollection queryParams = System.Web.HttpUtility.ParseQueryString(queryString);
                //_ = _gurujiDbContext.Logs.Add(new Log
                //{

                //    CreatedDate = DateTime.Now,
                //    Description = queryString,
                //    Source = "InstaMojos"
                //});

                _ = await _gurujiDbContext.SaveChangesAsync();


                InstaMojoRequestModel instaMojoBody = JsonConvert.DeserializeObject<InstaMojoRequestModel>(queryString);

                InstaMojo instaMojoModel = new()
                {
                    Name = instaMojoBody.buyer.name,
                    Email = instaMojoBody.buyer.email,
                    Phone = instaMojoBody.buyer.phone,
                    Status = instaMojoBody.payment.status,
                    PaymentId = instaMojoBody.payment.payment_id,
                    Amount = instaMojoBody.payment.amount,
                    Currency = instaMojoBody.payment.currency
                };


                if (instaMojoBody.details != null && instaMojoBody.details.Count > 0)
                {
                    instaMojoModel.Purpose = instaMojoBody.details[0].purpose;
                    instaMojoModel.Quantity = instaMojoBody.details[0].quantity.ToString();
                }


                instaMojoModel.PaymentDate = Convert.ToDateTime(instaMojoBody.payment.completed_at);
                instaMojoModel.CreatedOn = DateTime.Now;


                //instaMojoModel.Amount = Convert.ToDouble(queryParams["amount"]);
                //instaMojoModel.Buyer = queryParams["buyer"];
                //instaMojoModel.BuyerName = queryParams["buyer_name"];
                //instaMojoModel.BuyerPhone = queryParams["buyer_phone"];
                //instaMojoModel.Currency = queryParams["currency"];
                //instaMojoModel.Fees = Convert.ToDouble(queryParams["fees"]);
                //instaMojoModel.Longurl = queryParams["longurl"];
                //instaMojoModel.PaymentId = queryParams["payment_id"];
                //instaMojoModel.Purpose = queryParams["purpose"];
                //instaMojoModel.Shorturl = queryParams["shorturl"];
                //instaMojoModel.Status = queryParams["status"];
                //instaMojoModel.Mac = queryParams["mac"];
                //instaMojoModel.CreatedOn = DateTime.Now;

                _ = _gurujiDbContext.InstaMojos.Add(instaMojoModel);

                _ = await _gurujiDbContext.SaveChangesAsync();

            //}
            //catch (DbException ex)
            //{
            //    ExceptionLog exceptionLog = new()
            //    {
            //        CreatedDate = DateTime.Now,
            //        Description = ex?.InnerException?.ToString(),
            //        ErrorMessage = ex.Message,
            //        ExceptionType = "instamojoex",
            //        Notes = "",
            //        StackTrace = ex.StackTrace
            //    };
            //    _ = _gurujiDbContext.ExceptionLogs.Add(exceptionLog);
            //    _ = await _gurujiDbContext.SaveChangesAsync();
            //}
            return Ok();
        }
        [HttpPost("GetMail")]
        public ActionResult GetMailTest(MailRequest mailRequest)
        {
            Task rr = _mailService.SendEmailAsync(mailRequest);
            return Ok(rr);
        }
    }
}