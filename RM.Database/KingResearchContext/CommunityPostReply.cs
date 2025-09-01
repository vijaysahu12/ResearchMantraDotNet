using System;
using System.Collections.Generic;

namespace RM.Database.KingResearchContext
{
    public class CommunityPostReply
    {

        public int Id { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public int? ParentReplyId { get; set; }
        public int TotalLikesCount { get; set; } = 0;
        public int TotalDislikesCount { get; set; } = 0;
        public int HeartLikeCount { get; set; } = 0;
        public string ReplyContent { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public string Hashtags { get; set; }
        public string Mentions { get; set; }

        public List<CommunityPostReply> ChildReplies { get; set; } = new List<CommunityPostReply>();
    }

}
