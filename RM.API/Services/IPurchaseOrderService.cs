using DocumentFormat.OpenXml;
using RM.API.Hub;
using RM.API.Models;
using RM.API.Models.Mail;
using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.KingResearchContext;
using RM.Database.KingResearchContext.Tables;
using RM.Database.MongoDbContext;
using RM.Model;
using RM.Model.Common;
using RM.Model.DB.Tables;
using RM.Model.ResponseModel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RM.API.Services
{
    public interface IPurchaseOrderService
    {
        /// <summary>
        /// Update or Modify the Purchase Order Request
        /// </summary>
        /// <param name="pr"></param>
        /// <returns></returns>
        Task<ApiCommonResponseModel> ManagePurchaseOrder(PurchaseOrder pr);

        /// <summary>
        /// Get the filtered Purchase Order Data
        /// </summary>
        /// <param name="queryValues"></param>
        /// <returns></returns>
        Task<ApiCommonResponseModel> GetFilteredPurchaseOrders(QueryValues queryValues ,Guid loggedInUser);

        /// <summary>
        /// Update the Purchase Order Status
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ApiCommonResponseModel> ManagePurchaseOrderStatus(PurchaseOrderStatusRequestModel request);

        Task<ApiCommonResponseModel> UpdateStartEndPurchaseOrderDate(PurchaseOrder request);

        Task<ApiCommonResponseModel> GetStatusForPurchaseOrder(string category);

        Task<ApiCommonResponseModel> GetPurchaseOrderDetails(string? purchaseOrderKey);

        Task<ApiCommonResponseModel> GetPurchaseOrdersByLead(Guid leadPublicKey);

        Task<ApiCommonResponseModel> GetPoStatus(string v);

         Task<ApiCommonResponseModel> GetUsers(string userType, int? loginUserId );

        Task<ApiCommonResponseModel> VerifyInstaMojoPaymentId(QueryValues queryValues);

        Task<ApiCommonResponseModel> GetInstaMojoPayments(QueryValues request);

        Task<ApiCommonResponseModel> PoStatusList();

        Task<ApiCommonResponseModel> SendQrCodeMail(string instamojoId, string mailId, string sendType, string purpose, DateTime fromDate, DateTime toDate);

        Task<ApiCommonResponseModel> InstaMojoUserEntered(QueryValues queryValues);

        Task<ApiCommonResponseModel> GetInstaMojoPaymentIdDetails(string paymentId);

        Task<ApiCommonResponseModel> GetPoReport(GetPoReportRequestModel request);
    }

    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly KingResearchContext _context;
        private readonly IPushNotificationService _pushNotification;
        private readonly NotificationHub hub = new();
        private readonly ApiCommonResponseModel responseModel = new();
        private readonly IConfiguration _configuration;
        private readonly IMailService _mailService;
        private readonly IActivityService _activityService;


        public PurchaseOrderService(KingResearchContext context, IPushNotificationService pushNotification, IConfiguration configuration, IMailService mailService, IActivityService activityService)
        {
            _context = context;

            _pushNotification = pushNotification;

            _configuration = configuration;
            _mailService = mailService;
            _activityService = activityService;
        }

        /// <summary>
        /// To Get the Filtered purchase order from the Purchaseorders Table
        /// </summary>
        /// <param name="queryValues"></param>
        /// <returns></returns>
        public async Task<ApiCommonResponseModel> GetFilteredPurchaseOrders(QueryValues queryValues ,Guid loggedInUser)
        {
            responseModel.Message = "Successfully";
            responseModel.StatusCode = HttpStatusCode.OK;

            List<SqlParameter> sqlParameters2 = ProcedureCommonSqlParameters.GetCommonSqlParameters(queryValues);

            SqlParameter parameterOutValue = new()
            {
                ParameterName = "TotalCount",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output,
            };

            SqlParameter parameterOutValue2 = new()
            {
                ParameterName = "TotalSales",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output,
            };

            sqlParameters2.AddRange(new SqlParameter[] {
            //service value
                new SqlParameter { ParameterName = "PrimaryKey", Value = string.IsNullOrEmpty(queryValues.PrimaryKey) ?  DBNull.Value :  queryValues.PrimaryKey, SqlDbType = System.Data.SqlDbType.NVarChar },
                new SqlParameter { ParameterName = "SecondaryKey",Value =  queryValues.SecondaryKey != null ? queryValues.SecondaryKey : DBNull.Value  , SqlDbType = SqlDbType.VarChar}, //Po status value
                new SqlParameter { ParameterName = "ThirdKey", Value = queryValues.ThirdKey != null ?  queryValues.ThirdKey  :  DBNull.Value, SqlDbType = SqlDbType.VarChar},//Assigned To value
                new SqlParameter
                {
                    ParameterName = "FourthKey",
                    Value = string.IsNullOrEmpty(queryValues.FourthKey) ? DBNull.Value : queryValues.FourthKey,
                    SqlDbType = SqlDbType.VarChar
                },
                 new SqlParameter
                {
                    ParameterName = "FourthKey",
                    Value = string.IsNullOrEmpty(queryValues.FourthKey) ? DBNull.Value : queryValues.FourthKey,
                    SqlDbType = SqlDbType.VarChar
                }, new SqlParameter
                {
                    ParameterName = "FifthKey",
                    Value = string.IsNullOrEmpty(queryValues.FifthKey) ? DBNull.Value : queryValues.FifthKey,
                    SqlDbType = SqlDbType.VarChar
                },
                new SqlParameter { ParameterName = "LoggedInUser",Value = loggedInUser == Guid.Empty ? DBNull.Value : loggedInUser.ToString(), SqlDbType = SqlDbType.VarChar, Size = 50},

                parameterOutValue,
                parameterOutValue2
            });

            List<PurchaseOrderResponseSPModel> result = await _context.SqlQueryToListAsync<PurchaseOrderResponseSPModel>("exec GetPurchaseOrders @IsPaging , @PageSize, @PageNumber , @SortExpression , @SortOrder , @RequestedBy,  @FromDate,@ToDate,@SearchText, @PrimaryKey, @SecondaryKey , @ThirdKey,@FourthKey,@FifthKey,@LoggedInUser,@TotalCount  OUTPUT, @TotalSales OUTPUT ",
                sqlParameters2.ToArray());

            decimal totalRecords = Convert.ToDecimal(parameterOutValue?.Value);
            decimal totalSales = 0;
            if (totalRecords > 0)
            {
                totalSales = Convert.ToDecimal(parameterOutValue2?.Value);
            }

            dynamic runTimeObject = new ExpandoObject();
            runTimeObject.data = result;
            runTimeObject.totalRecords = totalRecords;
            runTimeObject.totalSales = totalSales;

            responseModel.Data = runTimeObject;

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetPurchaseOrderDetails(string? purchaseOrderKey)//leadId
        {
            if (!string.IsNullOrEmpty(purchaseOrderKey))
            {
                PurchaseOrder pr = await _context.PurchaseOrders.Where(item => item.PublicKey == Guid.Parse(purchaseOrderKey) && item.IsExpired == false
                                && item.Status != (int)PurchaseOrdersStatusEnum.Rejected).FirstOrDefaultAsync();
                responseModel.Message = responseModel.Message = "Successfully";
                responseModel.Data = pr;
                responseModel.StatusCode = HttpStatusCode.OK;
            }
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetStatusForPurchaseOrder(string category)
        {
            responseModel.Message = "Successfully";
            responseModel.Data = await _context.Status.Where(item => item.Category.ToLower() == category.ToLower()).ToListAsync();
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> ManagePurchaseOrderOld(PurchaseOrder request)
        {
            responseModel.Message = "Added Successfully";
            //try
            //{
            //ToDo : Change hard coded Days
            PurchaseOrder duplicateCheck = await _context.PurchaseOrders.Where(pr => pr.LeadId == request.LeadId && pr.CreatedOn.AddDays(35) > System.DateTime.Now).FirstOrDefaultAsync();
            Lead lead = await _context.Leads.FindAsync(request.LeadId);
            PushNotification notification = new();
            string adminList = _configuration.GetSection("AppSettings:FinanceAdmin").Value;
            var currentDateTime = DateTime.Now;
            if (duplicateCheck == null)
            {
                request.Status = (int)PurchaseOrdersStatusEnum.Pending;
                request.CreatedOn = currentDateTime;
                request.ModifiedOn = currentDateTime;
                _ = _context.PurchaseOrders.Add(request);
                responseModel.Data = await _context.SaveChangesAsync();
                responseModel.StatusCode = HttpStatusCode.OK;

                notification.Message = "PR Request for " + lead.FullName + " has been moved to status: " + request.Status + ".";
                notification.CreatedDate = currentDateTime;
                notification.IsActive = true;
                notification.IsImportant = false;
                notification.IsRead = false;
                notification.ModifiedDate = currentDateTime;
                notification.Source = "ManagePurchaseOrderOld";
                notification.Destination = NotificationScreenEnum.PurchaseOrder.ToString();
                //CreatedBy = Guid.Parse(request.CreatedBy)

                // Send notification to signalr server
                //await hub.SendMessage(lead.PublicKey.ToString() , "PR Request for " + lead.FullName + "has been " + request.Status + ".");
                //await hub.SendToAll("BB74D26F-AA28-EB11-BEE5-00155D53687A", "PR Request for " + lead.FullName + "has been " + request.Status + ".");
            }
            else
            {
                if (duplicateCheck.CreatedOn.AddDays(35) >= currentDateTime)
                {
                    responseModel.Message = "Alredy PR Created.. Now you have to wait for " + duplicateCheck.CreatedOn.AddDays(35).Subtract(System.DateTime.Now).Days + " days to Create new PR";
                    responseModel.StatusCode = HttpStatusCode.AlreadyReported;
                }
                else if ((int)PurchaseOrdersStatusEnum.Pending > duplicateCheck.Status)
                {
                    responseModel.Message = "Already Approved, Now Modification not allowed";
                    responseModel.StatusCode = HttpStatusCode.AlreadyReported;
                }
                else
                {
                    duplicateCheck.ServiceId = request.ServiceId;
                    duplicateCheck.Service = request.Service;
                    duplicateCheck.PaidAmount = request.PaidAmount;
                    duplicateCheck.Status = request.Status; //ToDo: check the logic
                    duplicateCheck.State = request.State;
                    duplicateCheck.Email = request.Email.Trim();
                    duplicateCheck.City = request.City;
                    duplicateCheck.PaymentDate = request.PaymentDate;
                    duplicateCheck.BankName = request.BankName;
                    duplicateCheck.ClientName = request.ClientName;
                    duplicateCheck.Dob = request.Dob;
                    duplicateCheck.Mobile = request.Mobile.Trim();
                    duplicateCheck.ModeOfPayment = request.ModeOfPayment;
                    duplicateCheck.TransasctionReference = request.TransasctionReference;
                    duplicateCheck.TransactionRecipt = request.TransactionRecipt;
                    duplicateCheck.NetAmount = request.NetAmount;
                    duplicateCheck.PaidAmount = request.PaidAmount;
                    duplicateCheck.Pan = request.Pan;
                    duplicateCheck.Remark = request.Remark;
                    duplicateCheck.StartDate = request.StartDate;
                    duplicateCheck.EndDate = request.EndDate;

                    responseModel.StatusCode = HttpStatusCode.OK;
                    responseModel.Message = "Updated your pending PR successfully";
                    _ = _context.PurchaseOrders.Add(duplicateCheck);
                    _ = await _context.SaveChangesAsync();

                    notification.Message = "PR Approval pending for " + lead.FullName + ".";
                    notification.CreatedDate = currentDateTime;
                    notification.IsActive = true;
                    notification.IsImportant = false;
                    notification.IsRead = false;
                    notification.ModifiedDate = currentDateTime;
                    notification.Source = "ManagePurchaseOrderOld";
                    notification.Destination = NotificationScreenEnum.PurchaseOrder.ToString();
                }
            }

            _ = await _pushNotification.PostPushNotification(notification, adminList.Split(",").ToList());

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> ManagePurchaseOrder(PurchaseOrder request)
        {
            try
            {
                responseModel.Message = "Added Successfully";
                if (request != null)
                {
                    Lead lead = await _context.Leads.FirstOrDefaultAsync(item => item.Id == request.LeadId);

                    PurchaseOrder poexist = null;
                    // if this is false then the lead is a customer OR first time PR creation for a lead
                    // if this is true then  Regenerating PR
                    if (lead.PurchaseOrderKey is not null)
                    {
                        poexist = await _context.PurchaseOrders.Where(x => x.PublicKey == lead.PurchaseOrderKey
                        //&& x.IsExpired == false
                        //&& x.IsActive == true
                        && x.Status != (int)PurchaseOrdersStatusEnum.Customer).FirstOrDefaultAsync();
                        if (poexist == null)
                        {
                            responseModel.StatusCode = HttpStatusCode.NotFound;
                            responseModel.Message = "No active purchase order found for the given lead.";
                            return responseModel;
                        }
                        else if (poexist.Status == (int)PurchaseOrdersStatusEnum.Rejected)
                        {
                            if (!String.IsNullOrEmpty(request.Remark.Trim()))
                            {
                                lead.Remarks = request.Remark;
                            }
                        }
                    }

                    //Updating Lead Table

                    #region LeadModifiedDate Update

                    lead.EmailId = string.IsNullOrEmpty(lead.EmailId) || string.IsNullOrWhiteSpace(lead.EmailId) ? request.Email : lead.EmailId;
                    lead.ModifiedOn = DateTime.Now;
                    lead.ModifiedBy = request.ModifiedBy?.ToString();

                    _ = await _context.SaveChangesAsync();

                    #endregion LeadModifiedDate Update

                    SqlParameter[] sqlParameters = new[]
                    {new SqlParameter { ParameterName = "InstaMojoId", Value = request.TransasctionReference  ,SqlDbType = SqlDbType.VarChar,Size = 100,}};

                    List<ValidateInstaMojoPaymentResponseModel> TransactionExists = await _context.SqlQueryToListAsync<ValidateInstaMojoPaymentResponseModel>(ProcedureCommonSqlParametersText.ValidateInstaMojoPayment, sqlParameters);

                    if (poexist is not null)
                    {
                        if (TransactionExists != null && TransactionExists.Count > 1 && TransactionExists.Any(item => item.PublicKey != request.PublicKey))
                        {
                            responseModel.Message = "Request with same instamojo transaction already exists.";
                            responseModel.StatusCode = HttpStatusCode.Ambiguous;
                            return responseModel;
                        }
                        else
                        {
                            #region PO model update
                            PurchaseOrder newPO = null;

                            if (poexist.Status == (int)PurchaseOrdersStatusEnum.Rejected)
                            {
                                // Update the existing purchase order
                                poexist.Status = (int)PurchaseOrdersStatusEnum.Pending;
                                poexist.ModifiedOn = DateTime.Now;
                                poexist.ClientName = request.ClientName;
                                poexist.Dob = request.Dob;
                                poexist.Email = request.Email.Trim();
                                poexist.Mobile = request.Mobile.Trim();
                                poexist.ModeOfPayment = request.ModeOfPayment;
                                poexist.NetAmount = request.NetAmount;
                                poexist.PaidAmount = request.PaidAmount;
                                poexist.Pan = request.Pan;
                                poexist.Remark = request.Remark;
                                poexist.TransactionRecipt = request.TransactionRecipt;
                                poexist.TransasctionReference = request.TransasctionReference;
                                poexist.State = request.State;
                                poexist.ServiceId = request.ServiceId;
                                poexist.PaymentDate = request.PaymentDate;

                                _context.Entry(poexist).State = EntityState.Modified;
                                await _context.SaveChangesAsync();

                                responseModel.Message = "Purchase order updated successfully";
                            }
                            else if (poexist.Status == (int)PurchaseOrdersStatusEnum.Completed)
                            {
                                newPO = new()
                                {
                                    LeadId = request.LeadId,
                                    PublicKey = Guid.NewGuid(),
                                    CreatedOn = DateTime.Now,
                                    ModifiedOn = DateTime.Now,
                                    ClientName = request.ClientName,
                                    Dob = request.Dob,
                                    Email = request.Email.Trim(),
                                    Mobile = request.Mobile.Trim(),
                                    ModeOfPayment = request.ModeOfPayment,
                                    NetAmount = request.NetAmount,
                                    PaidAmount = request.PaidAmount,
                                    Pan = request.Pan,
                                    Remark = request.Remark,
                                    TransactionRecipt = request.TransactionRecipt,
                                    TransasctionReference = request.TransasctionReference,
                                    State = request.State,
                                    ServiceId = request.ServiceId,
                                    PaymentDate = request.PaymentDate,
                                    Status = (int)PurchaseOrdersStatusEnum.Pending,
                                    CreatedBy = Guid.Parse(request.CreatedBy.ToString()),
                                    IsActive = true,
                                    IsExpired = false,
                                    CountryCode = request.CountryCode,
                                    PaymentActionDate = request.PaymentActionDate,
                                    City = request.City,
                                };

                                await _context.PurchaseOrders.AddAsync(newPO);
                                await _context.SaveChangesAsync();

                                responseModel.Message = "New purchase order created successfully";
                            }
                            #endregion PO model update

                            #region Add Lead Activity & User Log for PO Creation or Modification

                            User assignedTo = await _context.Users
                                .FirstOrDefaultAsync(item => item.PublicKey == request.CreatedBy);

                            bool isNewPO = newPO != null;
                            Guid destinationKey = isNewPO ? newPO.PublicKey ?? Guid.Empty : poexist.PublicKey ?? Guid.Empty;
                            ActivityTypeEnum activityType = isNewPO ? ActivityTypeEnum.PoCreated : ActivityTypeEnum.PoModified;
                            string message = isNewPO ? "New PO created" : "PO modified";

                            // Add to Lead Activity
                            LeadActivity leadActivity = new()
                            {
                                LeadKey = (Guid)lead.PublicKey,
                                ActivityType = (int)activityType,
                                Message = message,
                                Destination = destinationKey,
                                CreatedOn = DateTime.Now,
                                CreatedBy = Guid.Parse(request.CreatedBy.ToString())
                            };
                            _context.LeadActivity.Add(leadActivity);

                            // Add User Log
                            await _activityService.UserLog(
                                request.CreatedBy.ToString(),
                                Guid.Parse(lead.PublicKey.ToString()),
                                activityType,
                                "1.ManagePurchaseOrder"
                            );


                            #region Update Lead Data

                            lead.EmailId = string.IsNullOrWhiteSpace(lead.EmailId) ? request.Email : lead.EmailId;
                            lead.ModifiedOn = DateTime.Now;
                            lead.ModifiedBy = request.ModifiedBy?.ToString();

                            if (isNewPO)
                            {
                                lead.ServiceKey = await _context.Services
                                    .Where(s => s.Id == request.ServiceId)
                                    .Select(s => s.PublicKey.ToString())
                                    .FirstOrDefaultAsync();
                                lead.LeadTypeKey = null;
                                lead.StatusId = (int)PurchaseOrdersStatusEnum.Pending;
                                lead.PurchaseOrderKey = newPO.PublicKey;
                            }

                            await _context.SaveChangesAsync();

                            #endregion Update Lead Data

                            #endregion Add Lead Activity & User Log for PO Creation or Modification


                            await _context.SaveChangesAsync();

                            responseModel.Message = "Purchase order updated Successfully";
                            responseModel.StatusCode = HttpStatusCode.OK;

                            #region PushNotification on PO Modification

                            string adminList = _configuration.GetSection("AppSettings:FinanceAdmin").Value;

                            responseModel.Message = "Purchase order updated Successfully";
                            responseModel.StatusCode = HttpStatusCode.OK;
                            //var adminList = _configuration.GetSection("AppSettings:FinanceAdmin").Value;
                            PushNotification notification = new()
                            {
                                Userkey = Guid.Parse(adminList.Split(",")[0]),
                                Message = "PR Request for " + lead.FullName + " has been modified to " + EnumFinder.FindEnumByIdForPO(Convert.ToInt32(poexist.Status)) + ". By " + assignedTo.FirstName + " [ ₹ " + request.PaidAmount + "]",
                                CreatedDate = DateTime.Now,
                                IsActive = true,
                                IsImportant = false,
                                IsRead = false,
                                ModifiedDate = DateTime.Now,
                                CreatedBy = Guid.Parse(request.CreatedBy.ToString()),
                                Source = "ManagePurchaseOrder",
                                Destination = NotificationScreenEnum.PurchaseOrder.ToString()
                            };

                            CRMPushNotificationCollection pushNotificationCollection = new()
                            {
                                Userkey = Guid.Parse(adminList.Split(",")[0]),
                                Message = "PR Request for " + lead.FullName + " has been modified to " + EnumFinder.FindEnumByIdForPO(Convert.ToInt32(poexist.Status)) + ". By " + assignedTo.FirstName + " [ ₹ " + request.PaidAmount + "]",
                                CreatedDate = DateTime.Now,
                                IsActive = true,
                                IsImportant = false,
                                IsRead = false,
                                ModifiedDate = DateTime.Now,
                                CreatedBy = Guid.Parse(request.CreatedBy.ToString()),
                                Source = "ManagePurchaseOrder",
                                Destination = NotificationScreenEnum.PurchaseOrder.ToString()
                            };

                            //_ = await _mongoDbService.InsertPushNotification(pushNotificationCollection);
                            //_ = await _pushNotification.PostPushNotification(notification, adminList.Split(",").ToList());

                            #endregion PushNotification on PO Modification
                        }
                    }
                    // new po creation [ first time generating OR creating a new Pr for existing customer]
                    else
                    {
                        if (TransactionExists != null && TransactionExists.Count > 0)
                        {
                            responseModel.Message = "Request with same instamojo transaction already exists.";
                            responseModel.StatusCode = HttpStatusCode.Ambiguous;
                            return responseModel;
                        }
                        //when pr is generated for the first time purchaseOrderKey is null for that lead OR when creating a new service for a customer
                        if (lead.PurchaseOrderKey == null)
                        {
                            #region PurchaseOrder Create

                            request.Id = 0; // If we are creating new PO then the ID should be always 0
                            request.Status = (int)PurchaseOrdersStatusEnum.Pending;
                            request.CreatedOn = DateTime.Now;
                            request.ModifiedOn = DateTime.Now;
                            request.IsExpired = false;
                            responseModel.StatusCode = HttpStatusCode.OK;
                            try
                            {
                                _context.PurchaseOrders.Add(request);
                                await _context.SaveChangesAsync();
                            }
                            catch (DbUpdateException ex)
                            {
                                var inner = ex.InnerException?.Message;
                                Console.WriteLine($"DB Save Error: {inner}");
                                throw;
                            }

                            // adding into userActivity when creating PR check if startDate and endDate is null  then it is created for the first time and
                            // if it is not null then it is an existing customer and the service is modified
                            // adding into leadActivity
                            var createdPR = await _context.PurchaseOrders.Where(item => item.PublicKey == lead.PublicKey).FirstOrDefaultAsync();

                            if (createdPR.EndDate is null && createdPR.StartDate is null)
                            {
                                await _activityService.UserLog(request.CreatedBy.ToString(), lead.PublicKey, ActivityTypeEnum.PoCreated, "4.ManagePurchaseOrder");
                                await _activityService.LeadLog(lead.PublicKey.ToString(), request.CreatedBy.ToString(), ActivityTypeEnum.PoCreated, null, createdPR.PublicKey.ToString(), "2.ManagePurchaseOrder");
                            }
                            else
                            {
                                await _activityService.UserLog(request.CreatedBy.ToString(), lead.PublicKey, ActivityTypeEnum.ServiceModified, "ManagePurchaseOrderStatus");
                                await _activityService.LeadLog(lead.PublicKey.ToString(), request.CreatedBy.ToString(), ActivityTypeEnum.ServiceModified, null, createdPR.PublicKey.ToString(), "3.ManagePurchaseOrder");
                            }

                            //User assignedTo = await _context.Users.FirstOrDefaultAsync(item => item.PublicKey == request.CreatedBy);

                            //update Lead tbl
                            lead.PurchaseOrderKey = request.PublicKey;
                            lead.StatusId = (int)PurchaseOrdersStatusEnum.Fresh;
                            _context.Entry(lead).State = EntityState.Modified;
                            _ = await _context.SaveChangesAsync();

                            // Adding into leadActivity for the purchase order also save the purchase order key in the destination
                            //Lead generatedPOsLead = _context.Leads.FirstOrDefault(item => item.Id == request.LeadId);

                            #endregion PurchaseOrder Create

                            #region Log Lead Activity

                            //if (generatedPOsLead is not null)
                            //{
                            //    LeadActivity leadActivity = new()
                            //    {
                            //        LeadKey = (Guid)request.PublicKey,
                            //        ActivityType = 6,
                            //        Message = "PO created",
                            //        Destination = (Guid)generatedPOsLead.PurchaseOrderKey,
                            //        CreatedOn = DateTime.Now,
                            //        CreatedBy = Guid.Parse(request.CreatedBy.ToString())
                            //    };
                            //    //_ = _context.LeadActivity.Add(leadActivity);
                            //    //_ = await _context.SaveChangesAsync();
                            //}

                            #endregion Log Lead Activity

                            //if (generatedPOsLead is null)
                            //{
                            //PurchaseOrder customerDetails = _context.PurchaseOrders.FirstOrDefault(item => item.PublicKey == request.PublicKey);
                            //Lead leadDetails = _context.Leads.FirstOrDefault(item => item.Id == request.LeadId);

                            //LeadActivity leadActivityForCustomer = new()
                            //{
                            //    LeadKey = leadDetails.PublicKey,
                            //    ActivityType = 18,
                            //    Message = "Service modified",
                            //    Destination = leadDetails.PurchaseOrderKey,
                            //    CreatedOn = DateTime.Now,
                            //    CreatedBy = Guid.Parse(request.CreatedBy.ToString())
                            //};
                            //_ = _context.LeadActivity.Add(leadActivityForCustomer);
                            //_ = await _context.SaveChangesAsync();
                            //await _activityService.UserLog(request.PublicKey.ToString(), Guid.Parse(leadDetails.PublicKey.ToString()), ActivityTypeEnum.ServiceModified);

                            //}

                            #region PushNotification on PO Creation

                            string adminList = _configuration.GetSection("AppSettings:FinanceAdmin").Value;

                            //PushNotification notification = new()
                            //{
                            //    Userkey = Guid.Parse(adminList.Split(",")[0]),
                            //    Message = "New PR Request for " + lead.FullName + " has been Created by " + assignedTo.FirstName + ". [ ₹ " + request.PaidAmount + " ]",
                            //    CreatedDate = DateTime.Now,
                            //    IsActive = true,
                            //    IsImportant = false,
                            //    IsRead = false,
                            //    ModifiedDate = DateTime.Now,
                            //    ModifiedBy = request.ModifiedBy,
                            //    CreatedBy = Guid.Parse(request.CreatedBy.ToString()),
                            //    Source = "ManagePurchaseOrder",
                            //    Destination = NotificationScreenEnum.PurchaseOrder.ToString()
                            //};
                            //CRMPushNotificationCollection pushNotificationCollection = new()
                            //{
                            //    Userkey = Guid.Parse(adminList.Split(",")[0]),
                            //    Message = "New PR Request for " + lead.FullName + " has been Created by " + assignedTo.FirstName + ". [ ₹ " + request.PaidAmount + " ]",
                            //    CreatedDate = DateTime.Now,
                            //    IsActive = true,
                            //    IsImportant = false,
                            //    IsRead = false,
                            //    ModifiedDate = DateTime.Now,
                            //    ModifiedBy = request.ModifiedBy,
                            //    CreatedBy = Guid.Parse(request.CreatedBy.ToString()),
                            //    Source = "ManagePurchaseOrder",
                            //    Destination = NotificationScreenEnum.PurchaseOrder.ToString()
                            //};

                            //_ = await _mongoDbService.InsertPushNotification(pushNotificationCollection);
                            //_ = await _pushNotification.PostPushNotification(notification, adminList.Split(",").ToList());

                            #endregion PushNotification on PO Creation
                        }
                        else
                        {
                            responseModel.Message = "Active PO Already exists.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Message = ex.Message;
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                return responseModel;
            }
            return responseModel;
        }

        /// <summary>
        /// This method is used to approve or reject the Purchase Order by finance admin
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>

        // This is used when admin changed any PO from Purchase Order Screen
        public async Task<ApiCommonResponseModel> ManagePurchaseOrderStatus(PurchaseOrderStatusRequestModel request)
        {
            try
            {
                PurchaseOrder result = _context.PurchaseOrders.Where(pr => pr.LeadId == request.LeadId && pr.Id == request.Id).FirstOrDefault();

                if (result == null)
                {
                    responseModel.Message = "Record Not Found";
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                }
                else if (result.Status == (int)PurchaseOrdersStatusEnum.Approved)
                {
                    responseModel.Message = "Already Approved";
                    responseModel.StatusCode = HttpStatusCode.AlreadyReported;
                }
                else   // (result.Status == PurchaseOrdersStatusEnum.Active)
                {
                    result.Status = request.Status;
                    result.ModifiedOn = System.DateTime.Now;
                    result.EndDate = request.EndDate;
                    result.StartDate = request.StartDate;
                    result.Mobile = request.Mobile.Trim();
                    result.CountryCode = request.CountryCode.Trim();
                    result.Email = request.Email.Trim();
                    result.Remark = request.Remark;
                    result.Pan = request.Pan;
                    result.PaidAmount = request.PaidAmount;
                    result.NetAmount = request.NetAmount;
                    result.ClientName = request.ClientName;
                    result.City = request.City;
                    result.State = request.State;
                    result.TransasctionReference = request.TransasctionReference;
                    result.TransactionRecipt = request.TransactionRecipt;
                    result.Dob = request.DOB;
                    result.PaymentDate = request.PaymentDate;
                    result.ModeOfPayment = request.ModeOfPayment;
                    result.ModifiedBy = Guid.Parse(request.ModifiedBy);
                    result.PublicKey = request.PublicKey;
                    result.PaymentActionDate = DateTime.Now; // will come when did this get approved by admin
                                                             ////update Lead tbl

                    //var lead = await _context.Leads.FindAsync(request.LeadId);
                    //lead.PurchaseOrderKey = result.PublicKey;
                    _context.Entry(result).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                    Lead leadDetails = _context.Leads.FirstOrDefault(item => item.Id == request.LeadId);

                    // update lead remarks when pr is rejected

                    if (result.Status == (int)PurchaseOrdersStatusEnum.Rejected)
                    {
                        if (!String.IsNullOrEmpty(request.Remark.Trim()))
                        {
                            leadDetails.Remarks = "Rejected PR Comment: " + request.Remark;
                        }
                    }

                    // Add into lead activity when a PO is modified
                    LeadActivity leadActivity = new()
                    {
                        LeadKey = request.PublicKey
                    };
                    // Added switch case according to Status Type
                    switch (request.Status)
                    {
                        //when status is chnaged to pending status = 3
                        case 3:
                            leadActivity.ActivityType = 10;
                            leadActivity.Message = "PO pending";
                            await _activityService.UserLog(request.ModifiedBy.ToString(), Guid.Parse(leadDetails.PublicKey.ToString()), ActivityTypeEnum.PoPending, "15.ManagePurchaseOrderStatus");
                            break;

                        case 10:
                            leadActivity.ActivityType = 8;
                            leadActivity.Message = "PO approved";
                            await _activityService.UserLog(request.ModifiedBy.ToString(), Guid.Parse(leadDetails.PublicKey.ToString()), ActivityTypeEnum.PoApproved, "16.ManagePurchaseOrderStatus");

                            break;

                        case 11:
                            leadActivity.ActivityType = 9;
                            leadActivity.Message = "PO rejected";
                            await _activityService.UserLog(request.ModifiedBy.ToString(), Guid.Parse(leadDetails.PublicKey.ToString()), ActivityTypeEnum.PoRejected, "17.ManagePurchaseOrderStatus");

                            break;

                        default:
                            leadActivity.ActivityType = 7;
                            leadActivity.Message = "PO modified";
                            await _activityService.UserLog(request.ModifiedBy.ToString(), Guid.Parse(leadDetails.PublicKey.ToString()), ActivityTypeEnum.PoModified, "19.ManagePurchaseOrderStatus");

                            break;
                    }
                    leadActivity.Destination = request.PublicKey;
                    leadActivity.CreatedOn = DateTime.Now;
                    leadActivity.CreatedBy = Guid.Parse(request.ModifiedBy.ToString());
                    _ = _context.LeadActivity.Add(leadActivity);
                    _ = await _context.SaveChangesAsync();
                    // Send notification to signalr server
                    Lead lead = await _context.Leads.FindAsync(request.LeadId);
                    //await hub.SendToAll("BB74D26F-AA28-EB11-BEE5-00155D53687A", "PR Request for " + lead.FullName + "has been change to " + result.Status + ".");

                    List<string> receiverList = new();

                    Lead leadId = await _context.Leads.Where(lead => lead.Id == result.LeadId).FirstOrDefaultAsync();
                    if (leadId.AssignedTo is not null)
                    {
                        receiverList.Add(leadId.AssignedTo);

                        PushNotification notification = new()
                        {
                            Message = "PR Request for " + lead.FullName + " is " + EnumFinder.FindEnumByIdForPO(request.Status) + ".",
                            CreatedDate = DateTime.Now,
                            Userkey = Guid.Parse(request.ModifiedBy),
                            IsActive = true,
                            IsImportant = false,
                            IsRead = false,
                            ModifiedDate = DateTime.Now,
                            ModifiedBy = Guid.Parse(request.ModifiedBy),
                            CreatedBy = Guid.Parse(request.ModifiedBy),
                            Source = "ManagePurchaseOrder",
                            Destination = NotificationScreenEnum.Lead.ToString()
                        };
                        CRMPushNotificationCollection pushNotificationCollection = new()
                        {
                            Message = "PR Request for " + lead.FullName + " is " + EnumFinder.FindEnumByIdForPO(request.Status) + ".",
                            CreatedDate = DateTime.UtcNow,
                            Userkey = Guid.Parse(request.ModifiedBy),
                            IsActive = true,
                            IsImportant = false,
                            IsRead = false,
                            ModifiedDate = DateTime.UtcNow,
                            ModifiedBy = Guid.Parse(request.ModifiedBy),
                            CreatedBy = Guid.Parse(request.ModifiedBy),
                            Source = "ManagePurchaseOrder",
                            Destination = NotificationScreenEnum.Lead.ToString()
                        };

                        //_ = await _mongoDbService.InsertPushNotification(pushNotificationCollection);
                        //_ = await _pushNotification.PostPushNotification(notification, receiverList);
                    }
                    responseModel.Message = "Successfully Update the Status";
                    responseModel.StatusCode = HttpStatusCode.OK;
                }
            }
            catch (System.Exception ex)
            {
                responseModel.Message = "Exception Occured.";
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                responseModel.Data = ex;
            }
            return responseModel;
        }

        // Update start and end date and then convert the leads to customer
        public async Task<ApiCommonResponseModel> UpdateStartEndPurchaseOrderDate(PurchaseOrder request)
        {
            PurchaseOrder result = await (from s in _context.PurchaseOrders
                                          join r in _context.Leads on s.LeadId equals r.Id
                                          where s.PublicKey == r.PurchaseOrderKey && s.PublicKey == request.PublicKey && s.Status == (int)PurchaseOrdersStatusEnum.Approved
                                          select s).FirstOrDefaultAsync();

            if (result == null)
            {
                responseModel.Message = "Record Not Found";
                responseModel.StatusCode = HttpStatusCode.NotFound;
            }
            else
            {
                if (result.Status == (int)PurchaseOrdersStatusEnum.Approved && result.PublicKey == request.PublicKey)
                {
                    var currentDateTime = DateTime.Now;
                    result.ModifiedOn = currentDateTime;
                    result.EndDate = request.EndDate;
                    result.StartDate = request.StartDate;
                    result.Status = (int)PurchaseOrdersStatusEnum.Customer;
                    result.LTCDate = currentDateTime;
                    result.ModifiedBy = request.ModifiedBy;
                    _context.Entry(result).State = EntityState.Modified;

                    // insert into customer table
                    Lead lead = await _context.Leads.FindAsync(request.LeadId);

                    lead.PurchaseOrderKey = null;
                    _context.Entry(lead).State = EntityState.Modified;
                    _ = await _context.SaveChangesAsync();

                    Lead leadDetails = await _context.Leads.FirstOrDefaultAsync(item => item.Id == request.LeadId);
                    await _activityService.UserLog(request.ModifiedBy.ToString(), Guid.Parse(leadDetails.PublicKey.ToString()), ActivityTypeEnum.LTCCreated, "18.LTC");

                    LeadActivity leadActivityForCustomer = new()
                    {
                        LeadKey = leadDetails.PublicKey,
                        ActivityType = (int)ActivityTypeEnum.LTCCreated,
                        Message = "LTC created",
                        Destination = null,
                        CreatedOn = currentDateTime,
                        CreatedBy = Guid.Parse(request.CreatedBy.ToString())
                    };
                    _ = _context.LeadActivity.Add(leadActivityForCustomer);
                    _ = await _context.SaveChangesAsync();

                    List<SqlParameter> sqlParameters = new()
                    {
                        new SqlParameter { ParameterName = "EmailTemplateName",      Value = EmailTemplatesEnum.LTC.ToString() ,SqlDbType = System.Data.SqlDbType.VarChar, Size = 100},
                        new SqlParameter { ParameterName = "Category",   Value = ""  ,SqlDbType = System.Data.SqlDbType.VarChar, Size = 50},
                        new SqlParameter { ParameterName = "RequestedBy",   Value =  request.ModifiedBy   ,SqlDbType = System.Data.SqlDbType.UniqueIdentifier},
                        new SqlParameter { ParameterName = "PurchaseOrderKey",   Value = request.PublicKey  ,SqlDbType = System.Data.SqlDbType.UniqueIdentifier},
                    };

                    List<LTCMailResponseModel> emailTemplate = await _context.SqlQueryToListAsync<LTCMailResponseModel>(ProcedureCommonSqlParametersText.GetEmailTemplate, sqlParameters.ToArray());

                    if (emailTemplate != null)
                    {
                        MailRequest mailBody = new()
                        {
                            Body = emailTemplate.FirstOrDefault().Body,
                            CcEmail = emailTemplate.FirstOrDefault().Cc,
                            Subject = emailTemplate.FirstOrDefault().Subject,
                            ToEmail = emailTemplate.FirstOrDefault().EmailId
                        };
                        await _mailService.SendEmailAsync(mailBody);
                    }
                }

                responseModel.Message = "Successfully Update the PO";
                responseModel.StatusCode = HttpStatusCode.OK;
            }

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetPurchaseOrdersByLead(Guid leadPublicKey)
        {
            responseModel.Message = "Successfully";
            responseModel.StatusCode = HttpStatusCode.OK;
            SqlParameter parameterOutValue = new()
            {
                ParameterName = "TotalCount",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output,
            };

            SqlParameter[] sqlParameters = new[]
            {
                    new SqlParameter { ParameterName = "LeadPublicKey", Value = leadPublicKey ,SqlDbType = System.Data.SqlDbType.UniqueIdentifier},
                    parameterOutValue,
                };

            List<GetProcedureJsonResponse> customers = await _context.SqlQueryFirstOrDefaultAsync<GetProcedureJsonResponse>(
                @"exec GetPurchaseOrdersByLead  @LeadPublicKey={0}, @TotalCount={1} OUTPUT", sqlParameters);
            _ = Convert.ToDecimal(parameterOutValue.Value);
            responseModel.Data = customers.FirstOrDefault().JsonData;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetPoStatus(string v)
        {
            responseModel.Data = await _context.Status.Where(item => (item.Category == v && item.Code != null) || item.Code == "cus").ToListAsync();
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetUsers(string userType, int? loginUserId = null)
        {
            var sqlParameters = new List<SqlParameter>
    {
        new SqlParameter("@userType", SqlDbType.VarChar, 50) { Value = userType },
        new SqlParameter("@loginUserId", SqlDbType.Int)
        {
            Value = loginUserId.HasValue ? loginUserId.Value : (object)DBNull.Value
        }
    };

            var responseModel = new ApiCommonResponseModel();

            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Data = await _context.SqlQueryToListAsync<Users>(
                "EXEC GetFilterUsersBy @userType, @loginUserId",
                sqlParameters.ToArray()
            );

            return responseModel;
        }






        public async Task<ApiCommonResponseModel> VerifyInstaMojoPaymentId(QueryValues queryValues)
        {
            if (queryValues.PrimaryKey is not null && queryValues.SecondaryKey is not null)
            {
                double.TryParse(queryValues.SecondaryKey, out double paymentAmount);
                var verifiedId = await _context.InstaMojos.Where(item => item.PaymentId == queryValues.PrimaryKey.ToUpper() && item.Status == "SUCCESS" && item.Amount == paymentAmount).FirstOrDefaultAsync();
                if (verifiedId != null)
                {
                    responseModel.Data = verifiedId;
                    responseModel.Message = "PaymentId exists";
                    responseModel.StatusCode = HttpStatusCode.OK;
                    return responseModel;
                }
                responseModel.Message = "PaymentId doesn't exist";
                responseModel.Data = null;
                responseModel.StatusCode = HttpStatusCode.NotFound;
                return responseModel;
            }

            responseModel.Message = "No Parameters Sent In Payload";
            responseModel.Data = null;
            responseModel.StatusCode = HttpStatusCode.BadRequest;

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetInstaMojoPayments(QueryValues request)
        {
            try
            {
                responseModel.Message = "List Fetched Successfully";

                SqlParameter Purpose = new()
                {
                    ParameterName = "SecondaryKey",
                    Direction = ParameterDirection.Output,
                    SqlDbType = SqlDbType.VarChar,
                    Size = 100
                };

                List<SqlParameter> sqlParameters = new()
                {
                    new SqlParameter { ParameterName = "PageSize", Value = request.PageSize ,SqlDbType = System.Data.SqlDbType.Int},
                    new SqlParameter { ParameterName = "PageNumber", Value = request.PageNumber  ,SqlDbType = System.Data.SqlDbType.Int},
                    new SqlParameter { ParameterName = "SortOrder", Value = request.SortOrder ,SqlDbType = System.Data.SqlDbType.VarChar, Size = 50},
                    new SqlParameter { ParameterName = "SortExpression", Value = request.SortExpression  ,SqlDbType = System.Data.SqlDbType.VarChar, Size = 50},
                    new SqlParameter { ParameterName = "PrimaryKey", Value = request.PrimaryKey == "" ? DBNull.Value : request.PrimaryKey ,SqlDbType = System.Data.SqlDbType.VarChar, Size = 50},
                    new SqlParameter { ParameterName = "SearchText", Value = request.SearchText == null ? DBNull.Value : request.SearchText ,SqlDbType = System.Data.SqlDbType.VarChar, Size = 50},
                    new SqlParameter { ParameterName = "FromDate", Value = request.FromDate == null ? DBNull.Value  : request.FromDate ,SqlDbType = System.Data.SqlDbType.DateTime },
                    new SqlParameter { ParameterName = "ToDate", Value = request.ToDate == null ? DBNull.Value : request.ToDate ,SqlDbType = System.Data.SqlDbType.DateTime },
                };
                responseModel.Data = await _context.SqlQueryToListAsync<InstaMojoPaymentsResponse>(ProcedureCommonSqlParametersText.GetInstaMojoPayments, sqlParameters.ToArray());

                if (responseModel.Data is null)
                {
                    responseModel.Message = "No Data For Current Parameters";
                }
                responseModel.Message = "Data Fetch Successfull.";
                responseModel.StatusCode = HttpStatusCode.OK;
                return responseModel;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ApiCommonResponseModel> PoStatusList()
        {
            responseModel.Data = await _context.Status.Where(item => item.Category == "PO").ToListAsync();
            if (responseModel.Data is null)
            {
                responseModel.Message = "No Status List Found.";
                responseModel.StatusCode = HttpStatusCode.NotFound;
            }
            responseModel.Message = "Status List Fetch Successfull.";
            responseModel.StatusCode = HttpStatusCode.OK;

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> SendQrCodeMail(string instamojoId, string mailId, string sendType, string purpose, DateTime fromDate, DateTime toDate)
        {
            try
            {
                List<InstaMojoQrCodeResponse> customerDetails = new();
                string emailSubject = "";
                string emailBody = "";
                string redirectUrl = "";

                if (sendType.ToUpper() != "GETALL")
                {
                    SqlParameter EmailSubject = new()
                    {
                        ParameterName = "FourthKey",
                        Direction = ParameterDirection.Output,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 100
                    };
                    SqlParameter EmailBody = new()
                    {
                        ParameterName = "FifthKey",
                        Direction = ParameterDirection.Output,
                        SqlDbType = SqlDbType.VarChar,
                        Size = int.MaxValue
                    };
                    SqlParameter RedirectUrl = new()
                    {
                        ParameterName = "SixthKey",
                        Direction = ParameterDirection.Output,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 100
                    };

                    List<SqlParameter> sqlParameters = new()
                    {
                        new SqlParameter { ParameterName = "PrimaryKey", Value = sendType.ToUpper() , SqlDbType = System.Data.SqlDbType.VarChar, Size = 50 },
                        new SqlParameter { ParameterName = "ThirdKey", Value = purpose  == "" ? DBNull.Value : purpose, SqlDbType = System.Data.SqlDbType.VarChar, Size = 100 },
                        new SqlParameter { ParameterName = "FromDate", Value = fromDate    ,SqlDbType = System.Data.SqlDbType.DateTime },
                        new SqlParameter { ParameterName = "ToDate", Value = toDate  ,SqlDbType = System.Data.SqlDbType.DateTime },
                        EmailSubject,
                        EmailBody,
                        RedirectUrl
                    };
                    customerDetails = await _context.SqlQueryToListAsync<InstaMojoQrCodeResponse>(ProcedureCommonSqlParametersText.InstaMojoQrCodeMail, sqlParameters.ToArray());
                    emailSubject = Convert.ToString(EmailSubject.Value);
                    emailBody = Convert.ToString(EmailBody.Value);
                    redirectUrl = Convert.ToString(RedirectUrl.Value);
                }
                else if (sendType == "GETALL")
                {
                    SqlParameter EmailSubject = new()
                    {
                        ParameterName = "FourthKey",
                        Direction = ParameterDirection.Output,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 100
                    };
                    SqlParameter EmailBody = new()
                    {
                        ParameterName = "FifthKey",
                        Direction = ParameterDirection.Output,
                        SqlDbType = SqlDbType.VarChar,
                        Size = int.MaxValue
                    };
                    SqlParameter RedirectUrl = new()
                    {
                        ParameterName = "SixthKey",
                        Direction = ParameterDirection.Output,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 100
                    };

                    List<SqlParameter> sqlParameters = new()
                    {
                        new SqlParameter { ParameterName = "PrimaryKey", Value = instamojoId == null ? DBNull.Value : instamojoId, SqlDbType = System.Data.SqlDbType.VarChar, Size = 50 },
                        new SqlParameter { ParameterName = "ThirdKey", Value = purpose  == "" ? DBNull.Value : purpose, SqlDbType = System.Data.SqlDbType.VarChar, Size = 100 },
                        EmailSubject,
                        EmailBody,
                        RedirectUrl
                    };

                    customerDetails = await _context.SqlQueryToListAsync<InstaMojoQrCodeResponse>(ProcedureCommonSqlParametersText.InstaMojoQrCodeMail, sqlParameters.ToArray());

                    emailSubject = Convert.ToString(EmailSubject.Value);
                    emailBody = Convert.ToString(EmailBody.Value);
                }

                int count = 0;
                foreach (var customer in customerDetails) //ToDo: Change the first or default
                {
                    if (customer.MailSent > 0)
                    {
                        continue;
                    }

                    MailRequest mailParams = new()
                    {
                        ToEmail = customer.Email,
                        Subject = emailSubject,
                    };

                    // Create the HTML content for the email body.
                    var emailBodyTemplate = emailBody;
                    emailBodyTemplate = emailBodyTemplate.Replace("{customerName}", customer.Name);
                    mailParams.Body = emailBodyTemplate;

                    var mailResponse = await _mailService.SendEmailAsyncMarketManthan(mailParams);
                    if (mailResponse)
                    {
                        var insta = new InstaMojosMarketManthan
                        {
                            Id = customer.Id,
                            MailSent = customer.MailSent + 1
                        };
                        _context.Entry(insta).Property(e => e.MailSent).IsModified = mailResponse;
                        count++;
                    }
                }
                await _context.SaveChangesAsync();

                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Message = "Mail Sent Successfully.";
                responseModel.Data = count;
                return responseModel;
            }
            catch (Exception ex)
            {
                responseModel.Message = ex.Message;
                return responseModel;
            }
        }

        public async Task<ApiCommonResponseModel> InstaMojoUserEntered(QueryValues queryValues)
        {
            //primaryKey = paymentId
            //secondaryKey = boolean value
            try
            {
                OctEventRegistration instaMojoToUpdate = await _context.OctEventRegistration.FirstOrDefaultAsync(item => item.PaymentId.ToLower() == queryValues.PrimaryKey.ToLower());
                if (instaMojoToUpdate is not null)
                {
                    instaMojoToUpdate.hasEntered = Convert.ToInt32(queryValues.SecondaryKey);
                    await _context.SaveChangesAsync();

                    await _pushNotification.InstaMojoEvent("Binod");

                    responseModel.Message = "Update Successfull.";
                    responseModel.StatusCode = HttpStatusCode.OK;
                    return responseModel;
                }
                else
                {
                    responseModel.Message = "PaymentID not found";
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                    return responseModel;
                }
            }
            catch (Exception ex)
            {
                responseModel.Message = ex.Message;
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                return responseModel;
            }
        }

        public async Task<ApiCommonResponseModel> GetInstaMojoPaymentIdDetails(string paymentId)
        {
            try
            {
                InstaMojo getInstaMojoPaymentIdDetails = await _context.InstaMojos.FirstOrDefaultAsync(item => item.PaymentId.ToLower() == paymentId.ToLower());
                if (getInstaMojoPaymentIdDetails is not null)
                {
                    responseModel.Data = getInstaMojoPaymentIdDetails;
                    responseModel.Message = "Details Fetched Successfully.";
                    responseModel.StatusCode = HttpStatusCode.OK;
                    return responseModel;
                }
                else
                {
                    responseModel.Message = "Payment ID not Found.";
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                    return responseModel;
                }
            }
            catch (Exception ex)
            {
                responseModel.Message = ex.Message;
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                return responseModel;
            }
        }

        public async Task<ApiCommonResponseModel> GetPoReport(GetPoReportRequestModel request)
        {
            // Output parameters for TotalSalesAmount and TotalCount
            SqlParameter totalSalesAmountParam = new SqlParameter
            {
                ParameterName = "TotalSalesAmount",
                SqlDbType = SqlDbType.Decimal,
                Direction = ParameterDirection.Output,
            };

            SqlParameter totalCountParam = new SqlParameter
            {
                ParameterName = "TotalCount",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output,
            };

            // List of SQL parameters for the stored procedure
            List<SqlParameter> sqlParameters = new List<SqlParameter>
            {
                new SqlParameter { ParameterName = "StartDate", Value = request.StartDate, SqlDbType = SqlDbType.Date },
                new SqlParameter { ParameterName = "EndDate", Value = request.EndDate, SqlDbType = SqlDbType.Date },
                new SqlParameter { ParameterName = "StatusId", Value = request.StatusId == 0 ? DBNull.Value : request.StatusId, SqlDbType = SqlDbType.Int },
                new SqlParameter { ParameterName = "LeadSourceId", Value = request.LeadSourceId == 0 ? DBNull.Value : request.LeadSourceId, SqlDbType = SqlDbType.Int },
                new SqlParameter { ParameterName = "PageNumber", Value = request.PageNumber == 0 ? 1 : request.PageNumber, SqlDbType = SqlDbType.Int },  // Default to 1 if 0
                new SqlParameter { ParameterName = "PageSize", Value = request.PageSize == 0 ? 10 : request.PageSize, SqlDbType = SqlDbType.Int },  // Default to 10 if 0
                totalSalesAmountParam,
                totalCountParam
            };

            // Execute stored procedure and get results
            var spResult = await _context.SqlQueryToListAsync<GetPoReportSpResponseModel>(
                ProcedureCommonSqlParametersText.GetPOReport,
                sqlParameters.ToArray()
            );


            // Get the output values for TotalSalesAmount and TotalCount
            decimal totalSalesAmount = (totalSalesAmountParam.Value != DBNull.Value)
                    ? Convert.ToDecimal(totalSalesAmountParam.Value)
                    : 0;

            int totalCount = (totalCountParam.Value != DBNull.Value)
                ? Convert.ToInt32(totalCountParam.Value)
                : 0;

            // Prepare the response model
            var responseModel = new ApiCommonResponseModel
            {
                Data = new
                {
                    list = spResult,               // List of Purchase Orders
                    totalSales = totalSalesAmount
                },
                StatusCode = HttpStatusCode.OK,
                Total = totalCount
            };
            return responseModel;
        }
    }
}