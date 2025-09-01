using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace RM.Database.MongoDbContext
{
    public class CRMPushNotificationCollection
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("Userkey")]
        public Guid Userkey { get; set; }

        [BsonElement("Message")]
        public string Message { get; set; }

        [BsonElement("ReadDate")]
        public DateTime? ReadDate { get; set; }

        [BsonElement("IsRead")]
        public bool? IsRead { get; set; }

        [BsonElement("IsActive")]
        public bool? IsActive { get; set; }

        [BsonElement("IsImportant")]
        public bool? IsImportant { get; set; }

        [BsonElement("Source")]
        public string Source { get; set; }

        [BsonElement("Destination")]
        public string Destination { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("CreatedBy")]
        public Guid CreatedBy { get; set; }

        [BsonElement("CreatedDate")]
        public DateTime CreatedDate { get; set; }


        [BsonRepresentation(BsonType.String)]
        [BsonElement("ModifiedBy")]
        public Guid? ModifiedBy { get; set; }

        [BsonElement("ModifiedDate")]
        public DateTime? ModifiedDate { get; set; }
    }
}
