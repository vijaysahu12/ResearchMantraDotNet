using System;

namespace RM.Model.ResponseModel
{
    public class CommunityPostResponseModel
    {
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string PostContent { get; set; }
        public DateTime PostCreatedDate { get; set; }
        public bool PostIsDeleted { get; set; }
        public int TotalLikes { get; set; }
        public int TotalHearts { get; set; }
        public int TotalDislikes { get; set; }
        public int RepliesCount { get; set; }
    }
}
