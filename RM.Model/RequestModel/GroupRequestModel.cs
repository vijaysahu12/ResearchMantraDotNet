namespace RM.Model.RequestModel
{
    public class GroupRequestModel
    {

        public int? GroupId { get; set; }
        public string GroupName { get; set; }
        public bool? IsCommentEnabled { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
    }

}
