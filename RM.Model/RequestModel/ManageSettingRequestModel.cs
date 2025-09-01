namespace RM.Model.RequestModel
{
    public class ManageSettingRequestModel
    {
        public int? Id { get; set; }
        public string Value { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
    }
}