namespace RM.Model.ResponseModel
{
    public class OtpLogin
    {
        public string Result { get; set; }
        public string? Message { get; set; }
        public string? Otp { get; set; }
        public System.Guid? PublicKey { get; set; }
        public string? OneTimePassword { get; set; }
        public string? ProfileImage { get; set; }
        public string? FirebaseFcmToken { get; set; }
        public string? FullName { get; set; }
        public string? EmailId { get; set; }
        public bool? IsExistingUser { get; set; }
        public bool IsSelfDeleteRequested { get; set; }
    }
}
