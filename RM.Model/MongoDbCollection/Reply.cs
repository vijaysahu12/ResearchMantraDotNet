using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace RM.Model.MongoDbCollection
{
    public class Reply
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ObjectId { get; set; }

        [BsonElement("CommentId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CommentId { get; set; }
        [BsonElement("Content")]

        public string Content { get; set; }
        [BsonElement("Mention")]
        public string Mention { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("CreatedBy")]
        public string CreatedBy { get; set; }

        [BsonElement("IsActive")]
        public bool IsActive { get; set; }

        [BsonElement("IsDelete")]
        public bool IsDelete { get; set; }

        [BsonElement("ModifiedOn")]
        public DateTime? ModifiedOn { get; set; }

        [BsonElement("CreatedOn")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }



}
