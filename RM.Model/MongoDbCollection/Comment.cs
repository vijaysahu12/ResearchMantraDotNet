using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace RM.Model.MongoDbCollection
{
    public class Comment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ObjectId { get; set; }

        [BsonElement("BlogId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string BlogId { get; set; }

        [BsonElement("Content")]
        public string Content { get; set; }


        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("CreatedBy")]
        public string CreatedBy { get; set; }

        [BsonElement("Mention")]
        public string Mention { get; set; }


        [BsonElement("CreatedOn")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [BsonElement("IsActive")]
        public bool IsActive { get; set; }

        [BsonElement("IsDelete")]
        public bool IsDelete { get; set; }

        [BsonElement("ModifiedOn")]
        public DateTime? ModifiedOn { get; set; }

        public long ReplyCount { get; set; }
    }
}
