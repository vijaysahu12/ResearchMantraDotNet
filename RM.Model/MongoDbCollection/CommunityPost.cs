using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace RM.Model.MongoDbCollection
{
    // Model class to represent a Community Post in MongoDB
    public class CommunityPost
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("productId")] // Using ProductId instead of aCommunityId
        public int ProductId { get; set; } // Links to Product table

        [BsonElement("postTypeId")]
        public int PostTypeId { get; set; } // 1: Post, 2: Recorded Video, 3: Upcoming Event

        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("content")]
        public string Content { get; set; }

        [BsonElement("createdBy")]
        public long CreatedBy { get; set; }

        [BsonElement("ModifiedBy")]
        public int ModifiedBy { get; set; }

        [BsonElement("createdOn")]
        public DateTime CreatedOn { get; set; }

        [BsonElement("ModifiedOn")]
        public DateTime? ModifiedOn { get; set; }

        [BsonElement("UpComingEvent")]
        public DateTime? UpComingEvent { get; set; }
        public string Url { get; set; }

        [BsonElement("ImageUrls")]
        public List<ImageModel>? ImageUrls { get; set; } = new List<ImageModel>();

        [BsonElement("ImageUrl")]
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public bool IsQueryFormEnabled { get; set; }
        public bool IsJoinNowEnabled { get; set; }

        [BsonElement("IsApproved")]
        public string IsApproved { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("userObjectId")]
        public string? UserObjectId { get; set; }


        [BsonElement("Isadminposted")]
        public bool Isadminposted { get; set; }

        [BsonElement("Likecount")]
        public long Likecount { get; set; }

        [BsonElement("CommentsCount")]
        public long CommentsCount { get; set; }

        [BsonElement("EnableComments")]
        public bool EnableComments { get; set; }

        [BsonElement("reportsCount")]
        public int ReportsCount { get; set; }
    }

    // Model class to represent Community Post Type in MongoDB
    public class CommunityPostType
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("postTypeId")]
        public int PostTypeId { get; set; }

        [BsonElement("postTypeName")]
        public string PostTypeName { get; set; }
    }


    public class CreateCommunityPostRequest
    {
        public int Id { get; set; }
        public List<IFormFile> NewImages { get; set; }
        public List<string> AspectRatios { get; set; }
        public string Url { get; set; } // Recording session link, Upcoming event link 
        public IFormFile ImageUrl { get; set; }
        public string Hashtags { get; set; }
        public string Mentions { get; set; }
        public int ProductId { get; set; }
        public CommunityPostTypeEnum PostTypeId { get; set; }
        public string Title { get; set; }
        public int MobileUserId { get; set; }
        public string Content { get; set; }
        public DateTime? UpComingEvent { get; set; }
        public bool IsQueryFormEnabled { get; set; }
        public bool IsJoinNowEnabled { get; set; }
        public string IsApproved { get; set; }
        public bool Isadminposted { get; set; }
        public long Likecount { get; set; }

    }
    public class ImageModelRequest
    {
        public IFormFile ImageFile { get; set; }
        public string ImagePath { get; set; }
    }

    public class ImageResponseModel
    {
        public string Name { get; set; }
        public string AspectRatio { get; set; }
    }
    public class CommunityDetailsResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
    }

    public enum CommunityPostTypeEnum
    {
        Post = 1,
        RecordedVideo = 2,
        UpcomingEvent = 3
    }

    public class CommunityPostTypeResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class GetCommunityRequestModel
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int CommunityProductId { get; set; }
        public Guid LoggedInUserId { get; set; }
        public int PostTypeId { get; set; }
    }

    public class CommunityPostDto
    {
        public string Id { get; set; }
        public string ObjectId { get; set; }
        public int ProductId { get; set; }
        public int PostTypeId { get; set; }
        public long CreatedBy { get; set; }
        public long ModifiedBy { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? UpComingEvent { get; set; }
        public bool IsQueryFormEnabled { get; set; }
        public bool IsJoinNowEnabled { get; set; }
        public string Url { get; set; }
        public List<ImageModel> ImageUrls { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public string IsApproved { get; set; }
        public string UserObjectId { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string ProfileImage { get; set; }
        public long Likecount { get; set; }
        public long CommentsCount { get; set; }
        public bool EnableComments { get; set; }
        public bool Isadminposted { get; set; }
        public bool IsUserReported { get; set; }
        public bool UserHasLiked { get; set; }
        public int ReportsCount { get; set; }


    }

    public class UserDto
    {
        public string? FullName { get; set; }
        public string? Gender { get; set; }
        public string? ProfileImage { get; set; }
    }

}
