using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RM.Model.MongoDbCollection;

namespace RM.Model.ResponseModel
{
    public class CommunityPostResponse
    {
        public string Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string PostTypeName { get; set; }
        public int PostTypeId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string Url { get; set; }
        public List<ImageModel> ImageUrls { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime? UpComingEvent { get; set; }
        public bool IsQueryFormEnabled { get; set; }
        public bool IsJoinNowEnabled { get; set; }
        public string IsApproved { get; set; }
        public string UserObjectId { get; set; }
        public bool Isadminposted { get; set; }
        public long Likecount { get; set; }
        public long CommentsCount { get; set; }
        public string Gender { get; set; }
    }
}
