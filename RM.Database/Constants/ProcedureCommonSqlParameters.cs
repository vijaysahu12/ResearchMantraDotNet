using RM.Database.ResearchMantraContext;
using RM.Model.Common;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace RM.Database.Constants
{
    public static class ProcedureCommonSqlParameters
    {
        public static List<SqlParameter> GetCommonSqlParameters(QueryValues queryValues)
        {
            var sqlParameters = new List<SqlParameter>
           {
                    new SqlParameter { ParameterName = "IsPaging",      Value = Convert.ToBoolean(queryValues.IsPaging) ,SqlDbType = System.Data.SqlDbType.Bit},
                    new SqlParameter { ParameterName = "PageSize",      Value = queryValues.PageSize == 0 ? 25:  queryValues.PageSize,SqlDbType = System.Data.SqlDbType.Int},
                    new SqlParameter { ParameterName = "PageNumber ",   Value = queryValues.PageNumber == 0 ? 1 : queryValues.PageNumber ,SqlDbType = System.Data.SqlDbType.Int},
                    new SqlParameter { ParameterName = "SortExpression",Value = string.IsNullOrEmpty(queryValues.SortExpression) ?  DBNull.Value : queryValues.SortExpression,SqlDbType = System.Data.SqlDbType.VarChar, Size = 50},
                    new SqlParameter { ParameterName = "SortOrder",     Value = string.IsNullOrEmpty(queryValues.SortOrder) ?   DBNull.Value : queryValues.SortOrder,SqlDbType = System.Data.SqlDbType.VarChar, Size = 50},
                    new SqlParameter { ParameterName = "RequestedBy",   Value = string.IsNullOrEmpty(queryValues.RequestedBy) ? DBNull.Value : queryValues.RequestedBy   ,SqlDbType = System.Data.SqlDbType.VarChar, Size = 100},
                    new SqlParameter { ParameterName = "SearchText",    Value = string.IsNullOrEmpty(queryValues.SearchText) ? DBNull.Value : queryValues.SearchText  ,SqlDbType = System.Data.SqlDbType.VarChar, Size = 100},
                    new SqlParameter { ParameterName = "FromDate",      Value = queryValues.FromDate == null ? DBNull.Value : queryValues.FromDate  ,SqlDbType = System.Data.SqlDbType.DateTime},
                    new SqlParameter { ParameterName = "ToDate",        Value = queryValues.ToDate == null ? DBNull.Value : queryValues.ToDate  ,SqlDbType = System.Data.SqlDbType.DateTime}
            };
            return sqlParameters;
        }
    }

    public static class ProcedureCommonSqlParametersText
    {
        public static string Parameters = "@IsPaging={0} , @PageSize={1}, @PageNumber={2} , @SortExpression={3} , @SortOrder={4}  , @RequestedBy={5} , @SearchText={6} , @FromDate={7}, @ToDate={8} ";
        public static string Parameters2 = "{0} , {1}, {2} , {3} , {4}  , {5} , {6} , {7} , {8} ";
        public static string GetCallPerformance = "EXEC GetCallPerformance {0} ,{1}, {2} , {3} , {4}  , {5} , {6} , {7}, {8} , {9} , @TotalCount OUTPUT";
        public static string GetCallPerformanceExcel = "EXEC GetCallPerformanceExcel {0} ,{1}, {2} , {3} , {4}  , {5} , {6} , {7}, {8} , {9} , @TotalCount OUTPUT";
        public static string GetCallPerformanceReport = "EXEC GetCallPerformanceReport {0} , {1} , @TotalM2MWithHighPrice OUTPUT , @TotalM2MWithExitPrice OUTPUT  ";
        public static string GetExpiredServices = "EXEC GetExpiredServices {0}";
        public static string GetFollowUpReminder = "EXEC GetFollowUpReminder @Param1=null ";
        public static string GetEmailTemplate = "EXEC GetEmailTemplate @EmailTemplateName={0} , @Category={1} , @RequestedBy={2},@PurchaseOrderKey={3}";
        public static string GetUntouchedLeads = "EXEC GetUntouchedLeads";
        public static string GetPromoImagesM = "EXEC GetPromoImagesM {0}";

        public static string GetFilterUsersBy = "EXEC GetFilterUsersBy @userType={0}";
        public static string UpdateUntouchedLeadsToNull = "EXEC UnTochedLeadsAssignedToNull ";
        public static string GetSalesDashboardReport = "EXEC GetSalesDashboardReport {0}, {1}, {2} , {3}, {4}, {5}, {6} ";
        public static string GetInstaMojoPayments = "EXEC GetInstaMojoPayments {0}, {1}, {2}, {3} , {4}  , {5} , {6} , {7}  ";
        public static string ValidateInstaMojoPayment = "EXEC ValidateInstaMojoPayment {0}";
        public static string GetUsers = "EXEC GetUsers {0}";
        public static string GetAnalysts = "EXEC GetAnalysts {0}";
        public static string GetJunkLeads = "EXEC GetJunkLeads {0},{1}, {2} , {3} , {4}  , {5} , {6} , {7}, {8} , {9} ,{10} ,{11},{12}, {13}, {14},{15},   @TotalCount OUTPUT";
        public static string GetActivityLogs = "EXEC GetActivityLogs {0},{1}, {2} , {3} , {4}, {5},{6},  @TotalCount OUTPUT";
        public static string GetLeadFreeTrials = "EXEC GetLeadFreeTrials {0},{1}, {2} , {3} , {4}, {5},{6},  @TotalCount OUTPUT";
        public static string GetFilteredStocks = "EXEC GetFilteredStocks {0},{1}, {2}";
        public static string GetUserActivity = "EXEC GetUserActivity {0},{1}, {2} , {3} , {4},{5}, {6},{7} , @TotalCount OUTPUT";
        public static string GetBDE = "EXEC GetBDE";
        public static string UpdateExpiredServices = "EXEC UpdateExpiredServices";
        public static string GetSelfAssignJunkLeads = "EXEC GetSelfAssignJunkLeads {0}";
        public static string CallPerformancePNLReport = "EXEC CallPerformancePNLReport {0},{1}, {2}";
        public static string GetCallDetails = "EXEC GetCallDetails {0},{1}";
        public static string InstaMojoQrCodeMail = "EXEC InstaMojoQrCodeMail {0},{1} ,{2} ,{3}, @FourthKey OUTPUT, @FifthKey OUTPUT, @SixthKey OUTPUT";

        public static string GetProductsM = "EXEC GetProductsM {0}, {1},@IsValidUser={2} OUTPUT";
        public static string CreateCommunityPost = "EXEC CreateUpdateDeleteCommunity {0},{1}, {2} , {3} , {4}  , {5} , {6},{7},{8}";
        public static string UpdateCommunityPostInteractionsAndStoreReplies = "EXEC UpdateCommunityPostInteractionsAndStoreReplies {0},{1}, {2} , {3} , {4},{5}";
        public static string GetRepliesForPost = "EXEC GetRepliesForPost {0},{1}, {2}";
        public static string GetAllCommunityPosts = "EXEC CommunityGetAllPosts {0},{1}";
        public static string GetRepliesByReplyId = "EXEC UpdateCommunityPostReply {0},{1},{2}";
        public static string ManageGroup = "EXEC ManageGroup {0},{1},{2},{3},{4}, ";
        public static string OtpLogin = "EXEC OtpLogin {0},{1},{2},{3},{4}, @Otp OUTPUT ";
        public static string GetCommunityPostLikesDislikesHeartsForPost = "EXEC GetPostReactions {0}";
        public static string GetProductById = "EXEC GetProductById {0}, {1}";
        public static string OtpLoginVerificationAndGetSubscription = "EXEC OtpLoginVerificationAndGetSubscription {0}, {1}, {2}, {3}, @oldDeviceFcmToken OUTPUT";
        public static string LikeProductM = "EXEC LikeProductM {0}, {1}, {2}, {3}";

        //public static string GetNotificationsM = "EXEC GetNotificationsM {0},{1}, {2} , {3} , @TotalUnreadCount OUTPUT, @Categories OUTPUT";
        public static string AddPushNotificationsM = "EXEC AddPushNotificationsM {0},{1}, {2} , {3} , {4}  , {5} , {6}";

        public static string ManagePurchaseOrdersM = "EXEC ManagePurchaseOrdersM {0},{1}, {2}, {3}, {4}, {5} , {6}";
        public static string GetUserTopics = "EXEC GetUserTopics {0}";
        public static string GetMyBucketContent = "EXEC GetMyBucketContent {0}";
        public static string GetLeadsForWhatsappMessage = "EXEC GetLeadsForWhatsappMessage {0}";
        public static string RateProductM = "EXEC RateProductM {0}, {1}, {2}";
        public static string GetTopGainerLoser = "EXEC GetTopGainerLoser {0} ";
        public static string GetUnreadNotificationCount = "EXEC GetUnreadNotificationCount {0}  ";
        public static string GetProductContentM = "EXEC GetProductContentM {0},{1}, @IsInMyBucket OUTPUT, @IsInValidity OUTPUT";
        public static string GetProductContentMV2 = "EXEC GetProductContentMV2 {0},{1}, @IsInMyBucket OUTPUT, @IsInValidity OUTPUT";
        public static string GetEmployeeWorkStatus = "EXEC GetEmployeeWorkStatus {0}, {1}, {2} ";
        public static string GetPOReport = "EXEC GetPOReport {0}, {1}, {2}, {3}, {4}, {5}, @TotalSalesAmount OUTPUT, @TotalCount OUTPUT";
        public static string GetKycDetails = "EXEC GetKycDetails {0}, {1}, {2}, {3},{4},{5}, @TotalCount OUTPUT";
        public static string GenerateCouponCode = "EXEC GenerateCouponCode {0},{1}, {2} , {3} , {4}  , {5} , {6}";
        public static string ValidateCouponM = "EXEC ValidateCouponM {0}, {1}, {2}, {3}";
        public static string GetMobileUserKeyForTopic = "EXEC GetMobileUserKeyForTopic {0}";
        public static string GetAdImagesM = "EXEC GetAdImagesM  {0}, {1}";
        public static string GetMobileProducts = "EXEC GetMobileProducts {0}";
        public static string GetFilteredMobileProducts = "EXEC GetFilteredMobileProducts {0}, {1} , {2}";
        public static string GetNotificationReceivers = "EXEC GetNotificationReceivers {0}, {1}";
        public static string GetTargetAudianceListForPushNotification = "EXEC GetTargetAudianceListForPushNotification {0}, {1}, {2}, {3}, {4}, {5}";
        public static string GetMobileUserDetails = "EXEC GetMobileUserDetails {0}";
        public static string GetBasketsM = "EXEC GetBasketsM @UserId";
        public static string GetCampaignFcmTokenM = "EXEC GetCampaignFcmTokenM {0}";
        public static string GetCompaniesM = "EXEC GetCompaniesM {0},{1}, {2} , {3} , {4}, @UserHasResearch OUTPUT";
        public static string GetFreeTrialM = "EXEC GetFreeTrialM {0} ";
        public static string ActivateFreeTrialM = "EXEC ActivateFreeTrialM {0} ";
        public static string ManageCouponM = "EXEC ManageCouponM {0},{1}, {2} , {3} , {4}  , {5} , {6},{7},{8}, {9}, {10}, {11} ";
        public static string GetTicketsM = "EXEC GetTicketsM {0},{1}, {2} , {3} , {4} , {5},{6}, @TotalCount OUTPUT,@TypeCount OUTPUT,@StatusCount OUTPUT,@PriorityCount OUTPUT";
        public static string AddTicketCommentM = "EXEC AddTicketCommentM {0},{1}, {2} , {3},{4}";
        public static string GetScreenerDetails = "EXEC GetScreenerDetails {0},{1},{2},{3},{4}";
        public static string GetSubscriptionPlanWithProduct = "EXEC GetSubscriptionPlanWithProduct {0},{1},{2},{3}";
        public static string ManageScreenerStocksData = "EXEC ManageScreenerStocksData {0},{1}";
        public static string GetCallPerformanceM = "EXEC GetCallPerformanceM {0},@TotalTrades OUTPUT, @TotalProfitable OUTPUT,@TotalLoss OUTPUT,@TradeClosed OUTPUT,@Balance OUTPUT,@TradeOpen OUTPUT";
        public static string GetPaginationCallPerformanceM = "EXEC GetPaginationCallPerformanceM {0},{1},{2},{3},@TotalTrades OUTPUT, @TotalProfitable OUTPUT,@TotalLoss OUTPUT,@TradeClosed OUTPUT,@Balance OUTPUT,@TradeOpen OUTPUT";
        public static string GetCouponsM = "EXEC GetCouponsM {0},{1},{2}";
        public static string DeleteMobileUserData = "EXEC DeleteMobileUserData {0}";
        public static string GetScreenerData = "exec GetScreenerData {0}";
        public static string GetSubscriptionDetails = "exec GetSubscriptionDetails";
        public static string GetExpiredServiceFromMyBucket = "EXEC GetExpiredServiceFromMyBucket";
        public static string GetCommunityDetails = "EXEC GetCommunityDetails {0}";
        public static string GetPlayList  = "EXEC GetPlayList {0},{1}";
        public static string GetDueNotifications  = "EXEC GetDueNotifications";

        public static string GetMobileDashboard = "EXEC GetMobileDashboard {0},{1},{2},{3},{4},{5},{6},{7}, {8}, {9}, {10}, {11}, {12}, {13}, {14} ";



        //public static string GetPaginatedScreenerData = "exec GetPaginatedScreenerData {0},{1},{2}";
    }
}