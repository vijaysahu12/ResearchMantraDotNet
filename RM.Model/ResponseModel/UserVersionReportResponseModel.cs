namespace RM.Model.ResponseModel
{
    public class UserVersionReportResponseModel
    {
        public string MobileUserKey { get; set; }
        public long? MobileUserId { get; set; }
        public string DeviceType { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string CreatedOn { get; set; }
        public string FullName { get; set; }
        public string MobileNumber { get; set; }
    }
}