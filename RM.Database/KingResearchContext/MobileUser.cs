using System;
namespace RM.Database.KingResearchContext;

public partial class MobileUser
{
    public long Id { get; set; }
    public Guid PublicKey { get; set; }
    public Guid? LeadKey { get; set; }
    public string FullName { get; set; }
    public string EmailId { get; set; }
    public string Mobile { get; set; }
    public string City { get; set; }
    public string Gender { get; set; }
    public string Password { get; set; }
    public string? OneTimePassword { get; set; }
    public bool? IsOtpVerified { get; set; }
    public string? MobileToken { get; set; }
    public string? FirebaseFcmToken { get; set; }
    public string DeviceType { get; set; }
    public string? Imei { get; set; }
    public string? StockNature { get; set; }
    public bool? AgreeToTerms { get; set; }
    public bool? SameForWhatsApp { get; set; }
    public bool? IsActive { get; set; }

    public DateTime? CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string ProfileImage { get; set; }
    public string? About { get; set; }
    public bool? CanCommunityPost { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public DateTime? Dob { get; set; }
    public bool? IsDelete { get; set; }
    public bool? SelfDeleteRequest { get; set; }
    public DateTime? SelfDeleteRequestDate { get; set; }
    public string? SelfDeleteReason { get; set; }
    public string CountryCode { get; set; }
    public string DeviceVersion { get; set; }
}