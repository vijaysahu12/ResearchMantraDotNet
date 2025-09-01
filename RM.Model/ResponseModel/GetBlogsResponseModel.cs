using RM.Model.MongoDbCollection;
using System;
using System.Collections.Generic;

namespace RM.Model.ResponseModel
{
    public class GetBlogsResponseModel
    {
        public string ObjectId { get; set; }
        public string Content { get; set; }
        public string Hashtag { get; set; }
        public string CreatedBy { get; set; }
        public bool EnableComments { get; set; }
        public string PostedAgo { get; set; }
        public DateTime CreatedOn { get; set; }
        public long LikesCount { get; set; }
        public long CommentsCount { get; set; }
        //public List<string> Image { get; set; }
        public List<ImageModel> Image { get; set; }
        public string UserFullName { get; set; }
        public string UserProfileImage { get; set; }
        public string Gender { get; set; }
        public bool UserHasLiked { get; set; }
        public bool IsUserReported { get; set; }
        public bool IsPinned { get; set; }
    }
}
