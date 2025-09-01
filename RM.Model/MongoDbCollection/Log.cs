using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace RM.Model.MongoDbCollection
{
    public class Log
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ObjectId { get; set; }

        [BsonElement("Message")]
        [BsonRepresentation(BsonType.String)]
        public string Message { get; set; }

        [BsonElement("Source")]
        [BsonRepresentation(BsonType.String)]
        public string? Source { get; set; }

        [BsonElement("Category")]
        [BsonRepresentation(BsonType.String)]
        public string? Category { get; set; }
        [BsonElement("CreatedOn")]
        public DateTime CreatedOn { get; set; }
    }


}
