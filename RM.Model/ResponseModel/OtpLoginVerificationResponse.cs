using System;
using System.Collections.Generic;

namespace RM.Model.ResponseModel
{
    public class OtpLoginVerificationResponse
    {
        public string? Result { get; set; }
        public Guid MobileUserKey { get; set; }
        public long MobileUserId { get; set; }
        public string MobileNumber { get; set; }
        public string? EmailTemp { get; set; }
        public string? FullName { get; set; }
        public string? EventSubscription { get; set; }
        public string? Message { get; set; }
        public string? Gender { get; set; }
        public string? ProfileImage { get; set; }
        public bool? IsExistingUser { get; set; }
        public bool IsFreeTrialActivated { get; set; }
        public string? LeadKey { get; set; }
        public int FreeTrialDays { get; set; }
        public string productNames { get; set; }
        public string WelcomeMessage { get; set; }
    }

    public class GetFreeTrialRootResponseModel
    {
        public string TrialName { get; set; }
        public int TrialInDays { get; set; }
        public List<string> Data { get; set; }
    }
    public class GetFreeTrialResponseModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int DaysInNumber { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
    }


    public class ActivateFreeTrialResponseModel
    {
        public string Result { get; set; }
        public string Message { get; set; }
    }
}