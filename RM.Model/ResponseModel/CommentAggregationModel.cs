namespace RM.Model.ResponseModel
{
    public class CommentAggregationModel
    {
        public string BlogId { get; set; }
        public string Content { get; set; }
        public string Mention { get; set; }
        public string ObjectId { get; set; }
        public int ReplyCount { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedOn { get; set; }
        public string UserFullName { get; set; }
        public string Gender { get; set; }
        public string? UserProfileImage { get; set; }
    }
}
