using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace RM.Model.MongoDbCollection
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ObjectId { get; set; }

        [BsonElement("FullName")]
        public string FullName { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("PublicKey")]
        public Guid PublicKey { get; set; }

        [BsonElement("CreatedOn")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [BsonElement("ModifiedOn")]
        public DateTime? ModifiedOn { get; set; }

        [BsonElement("ProfileImage")]
        public string? ProfileImage { get; set; }

        [BsonElement("CanCommunityPost")]
        public bool CanCommunityPost { get; set; }

        [BsonElement("Gender")]
        public string Gender { get; set; }


        [BsonElement("IsCrmUser")]
        public bool IsCrmUser { get; set; }
    }
}
