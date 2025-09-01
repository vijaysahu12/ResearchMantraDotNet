using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace RM.Model.MongoDbCollection
{
    public class UserBlock
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId BlockerId { get; set; }

        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId BlockedId { get; set; }


        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}

