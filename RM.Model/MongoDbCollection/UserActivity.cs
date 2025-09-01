using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace RM.Model.MongoDbCollection
{
    public class UserActivity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ObjectId { get; set; }

        [BsonElement("CreatedBy")]
        [BsonRepresentation(BsonType.String)]
        public Guid CreatedBy { get; set; }

        [BsonElement("ProductId")]
        public int? ProductId { get; set; }

        [BsonElement("Message")]
        public string? Message { get; set; }

        [BsonElement("Description")]
        public string? Description { get; set; }

        [BsonElement("ActivityType")]
        public int ActivityType { get; set; }

        [BsonElement("CreatedOn")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }

    public enum UserActivityEnum
    {
        ProductClicked = 1,
        ContentClicked = 2,
        GetLearningMaterial = 3
    }

    public class UserVersionReport
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ObjectId { get; set; }

        public string MobileUserKey { get; set; }
        public long MobileUserId { get; set; }
        public string DeviceType { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string CreatedOn { get; set; }
    }
}