using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace RM.Database.MongoDbContext
{
    public class MobileUserNotificationCollection
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("ReceivedById")]
        public string ReceivedById { get; set; }

        [BsonElement("Title")]
        public string Title { get; set; }

        [BsonElement("Body")]
        public string Body { get; set; }

        [BsonElement("CategoryId")]
        public string CategoryId { get; set; }

        [BsonElement("CreatedOn")]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime CreatedOn { get; set; }
    }
}
