using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace RM.Model.MongoDbCollection
{
    public class Blog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ObjectId { get; set; }

        [BsonElement("Content")]
        public string? Content { get; set; }

        [BsonElement("Hashtag")]
        public string Hashtag { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("CreatedBy")]
        public string CreatedBy { get; set; }

        [BsonElement("CreatedOn")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [BsonElement("ModifiedOn")]
        public DateTime? ModifiedOn { get; set; }

        [BsonElement("EnableComments")]
        public bool EnableComments { get; set; }

        [BsonElement("IsActive")]
        public bool IsActive { get; set; }

        [BsonElement("IsDelete")]
        public bool IsDelete { get; set; }

        [BsonElement("LikesCount")]
        public long LikesCount { get; set; }

        [BsonElement("CommentsCount")]
        public long CommentsCount { get; set; }

        [BsonElement("Image")]
        public List<ImageModel> Image { get; set; }

        //public List<string> Image { get; set; }

        [BsonElement("ReportsCount")]
        public int ReportsCount { get; set; }

        [BsonElement("Status")]
        public string Status { get; set; }

        [BsonElement("ModifiedBy")]
        public string ModifiedBy { get; set; }

        [BsonElement("IsPinned")]
        public bool IsPinned { get; set; }


    }

    public class ImageModel
    {
        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("AspectRatio")]
        public string AspectRatio { get; set; }
    }
}