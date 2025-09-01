using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace RM.Model.MongoDbCollection
{
    public class BlogReport
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("Id")]
        public string Id { get; set; }

        [BsonElement("BlogId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string BlogId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("ReportedBy")]
        public ObjectId ReportedBy { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("ReasonId")]
        public ObjectId ReasonId { get; set; }

        [BsonElement("Description")]
        public string Description { get; set; }

        [BsonElement("CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [BsonElement("Status")]
        public bool Status { get; set; }
    }
}