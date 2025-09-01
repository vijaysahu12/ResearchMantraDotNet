using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace RM.Model.MongoDbCollection
{
    public class Like
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ObjectId { get; set; }

        [BsonElement("BlogId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string BlogId { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("CreatedBy")]
        public string CreatedBy { get; set; }

        [BsonElement("CreatedOn")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
