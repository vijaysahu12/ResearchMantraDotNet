namespace RM.Model.RequestModel
{
    public class DeleteBlogCommentOrReplyRequest
    {
        public string ObjectId { get; set; }
        public string UserObjectId { get; set; }
        public string Type { get; set; }
    }
}