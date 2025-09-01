using System;

namespace RM.Model.ResponseModel
{
    public class GetRepliesResponseModel
    {
        public string ObjectId { get; set; }
        public string BlogId { get; set; }
        public string CommentId { get; set; }
        public string Content { get; set; }
        public string Mention { get; set; }
        public string CreatedBy { get; set; }
        public string PostedAgo { get; set; }
        public DateTime CreatedOn { get; set; }
        public string UserFullName { get; set; }
        public string Gender { get; set; }
        public string UserProfileImage { get; set; }
    }
}
