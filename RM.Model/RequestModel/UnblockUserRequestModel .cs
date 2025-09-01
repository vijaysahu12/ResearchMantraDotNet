namespace RM.Model.RequestModel
{
    public class UnblockUserRequestModel
    {
        public string UserId { get; set; }
        public string BlockedId { get; set; }
    }
}
