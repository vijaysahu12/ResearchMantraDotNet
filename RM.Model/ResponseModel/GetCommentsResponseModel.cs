using System;

namespace RM.Model.ResponseModel
{
    public class GetCommentsResponseModel
    {
        public string ObjectId { get; set; }
        public string BlogId { get; set; }
        public string Content { get; set; }
        public string Mention { get; set; }
        public string CreatedBy { get; set; }
        public string PostedAgo { get; set; }
        public DateTime CreatedOn { get; set; }
        public long ReplyCount { get; set; }
        public string UserFullName { get; set; }
        public string Gender { get; set; }
        public string UserProfileImage { get; set; }

    }
}
