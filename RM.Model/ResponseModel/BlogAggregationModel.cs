

using RM.Model.MongoDbCollection;
using System.Collections.Generic;

namespace RM.Model.ResponseModel
{
    public class BlogAggregationModel
    {
        public string ObjectId { get; set; }
        public string Content { get; set; }
        public string Hashtag { get; set; }
        public string CreatedBy { get; set; }
        public bool EnableComments { get; set; }
        public string CreatedOn { get; set; }
        public string? ModifiedOn { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public List<ImageModel> Image { get; set; }
        //public List<string> Image { get; set; }
        public string UserFullName { get; set; }
        public string UserProfileImage { get; set; }
        public string Gender { get; set; }
        public bool UserHasLiked { get; set; }
        public bool IsUserReported { get; set; }
        public bool IsPinned { get; set; }
    }

    //public class ImageModel
    //{
    //    public string Name { get; set; }
    //    public string AspectRatio { get; set; }
    //}
}
