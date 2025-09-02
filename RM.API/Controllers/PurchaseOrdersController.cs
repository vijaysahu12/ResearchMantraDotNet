using RM.API.Dtos;
using RM.API.Helpers;
using RM.API.Models;
using RM.API.Services;
using RM.CommonServices.Helpers;
using RM.Database.ResearchMantraContext;
using RM.Model;
using RM.Model.Common;
using RM.Model.DB.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RM.API.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly IPurchaseOrderService _purchaseOrders;
        private readonly IActivityService _activityService;

        public PurchaseOrdersController(IPurchaseOrderService purchaseOrders, IActivityService activityService)
        {
            _purchaseOrders = purchaseOrders;
            _activityService = activityService;
        }

        [HttpPost("GetFilteredPurchaseOrders")]
        public async Task<IActionResult> GetFilteredPurchaseOrders(QueryValues queryValues )

        {
            _ = Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out Guid loggedInUser);

            ApiCommonResponseModel response = await _purchaseOrders.GetFilteredPurchaseOrders(queryValues , loggedInUser);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("ManagePurchaseOrder")]
        public async Task<IActionResult> ManagePurchaseOrder(PurchaseOrder pr)
        {
            string UserKey = GetLoggedInUserId();
            pr.CreatedBy = Guid.Parse(UserKey);
            pr.ModifiedBy = Guid.Parse(UserKey);
            ApiCommonResponseModel response = await _purchaseOrders.ManagePurchaseOrder(pr);
            //when creating pr adding into useractivity
            return Ok(response);
        }

        [HttpPost("ManagePurchaseOrderStatus")]
        public async Task<IActionResult> ManagePurchaseOrderStatus(PurchaseOrderStatusRequestModel request)
        {

            request.ModifiedBy = GetLoggedInUserId();
            return Ok(await _purchaseOrders.ManagePurchaseOrderStatus(request));
        }


        [HttpPost("UpdateStartEndPurchaseOrderDate")]
        public async Task<IActionResult> UpdateStartEndPurchaseOrderDate(PurchaseOrder po)
        {
            string UserKey = GetLoggedInUserId();
            po.CreatedBy = Guid.Parse(UserKey);
            po.ModifiedBy = Guid.Parse(UserKey);
            ApiCommonResponseModel response = await _purchaseOrders.UpdateStartEndPurchaseOrderDate(po);
            return Ok(response);
        }


        [HttpGet("GetStatusForPurchaseOrder")]
        public async Task<IActionResult> GetStatusForPurchaseOrder()
        {
            return Ok(await _purchaseOrders.GetStatusForPurchaseOrder("po"));
        }

        [HttpGet("GetPurchaseOrderDetails")]
        public async Task<IActionResult> GetPurchaseOrderDetails(string? purchaseOrderKey)
        {
            return Ok(await _purchaseOrders.GetPurchaseOrderDetails(purchaseOrderKey));
        }
        [NonAction]
        public string GetLoggedInUserId()
        {
            TokenAnalyser tokenAnalyser = new();
            TokenVariables tokenVariables = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                tokenVariables = tokenAnalyser.FetchTokenValues(identity);
            }
            return tokenVariables.PublicKey.ToString();
        }



        [HttpGet("GetPoStatus/{category}")]
        public async Task<IActionResult> GetPoStatus(string category)
        {
            return Ok(await _purchaseOrders.GetPoStatus(category));
        }
        [HttpGet("GetUserByTypes/{userType}")]
        public async Task<IActionResult> GetUsers(string userType)
        {
            int systemUserId = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));
            string roleName = UserClaimsHelper.GetClaimValue(User, "roleName");

            // For admin, treat loginUserId as null
            int? loginUserId = (roleName.ToLower() == "admin" || roleName.ToLower() == "globleadmin") ? null : systemUserId;

            return Ok(await _purchaseOrders.GetUsers(userType, loginUserId));
        }




        [HttpPost("InstaMojo")]
        public async Task<IActionResult> VerifyInstaMojoTransactionId(QueryValues queryValues)
        {
            ApiCommonResponseModel response = await _purchaseOrders.VerifyInstaMojoPaymentId(queryValues);
            return Ok(response);
        }

        [HttpPost("GetInstaMojoPayments")]
        public async Task<IActionResult> GetInstaMojoPayments(QueryValues request)
        {
            return Ok(await _purchaseOrders.GetInstaMojoPayments(request));
        }




        [HttpGet("GetPoStatusList")]
        public async Task<IActionResult> GetPoStatusList()
        {
            return Ok(await _purchaseOrders.PoStatusList());
        }

        [HttpPost("SendQrCodeEmail")]
        public async Task<IActionResult> SendQrCodeEmail(QueryValues queryValues)
        {
            return Ok(await _purchaseOrders.SendQrCodeMail(queryValues.PrimaryKey, queryValues.SecondaryKey, queryValues.ThirdKey, queryValues.FourthKey, (DateTime)queryValues.FromDate, (DateTime)queryValues.ToDate));
        }

        [HttpPost("InstaMojoUserEntered")]
        public async Task<IActionResult> InstaMojoUserEntered(QueryValues queryValues)
        {
            return Ok(await _purchaseOrders.InstaMojoUserEntered(queryValues));
        }

        [HttpGet("GetInstaMojoPaymentIdDetails/{paymentId}")]
        public async Task<IActionResult> GetInstaMojoPaymentIdDetails(string paymentId)
        {
            return Ok(await _purchaseOrders.GetInstaMojoPaymentIdDetails(paymentId));
        }

        [HttpPost("GetPoReport")]
        public async Task<IActionResult> GetPoReport(GetPoReportRequestModel request)
        {
            return Ok(await _purchaseOrders.GetPoReport(request));
        }


    }

}