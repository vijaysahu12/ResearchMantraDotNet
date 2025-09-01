using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;


namespace RM.Model.MongoDbCollection
{
    public class ExceptionLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string InnerException { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public string RequestBody { get; set; }
        public string Source { get; set; }
    }

}
