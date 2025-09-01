using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace RM.Model.MongoDbCollection
{
    public class CommunityComments : MongoDbCommonCollection 
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ObjectId { get; set; }

        [BsonElement("CommunityPostId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CommunityPostId { get; set; }

        [BsonElement("Content")]
        public string Content { get; set; }

        [BsonElement("Mention")]
        public string Mention { get; set; }

        [BsonElement("ReplyCount")]
        public int ReplyCount { get; set; }
    }
}
