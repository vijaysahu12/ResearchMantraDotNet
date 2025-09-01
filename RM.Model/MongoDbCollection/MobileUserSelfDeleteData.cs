using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace RM.Model.MongoDbCollection
{
    public class MobileUserSelfDeleteData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("Id")]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.String)] // Ensures correct serialization
        [BsonElement("MobileUserKey")]
        public string MobileUserKey { get; set; }
        [BsonElement("FullName")]
        public string FullName { get; set; }
        [BsonElement("Mobile")]
        public string Mobile { get; set; }
        [BsonElement("EmailId")]
        public string EmailId { get; set; }
        [BsonElement("RegistrationDate")]
        public DateTime? RegistrationDate { get; set; }

        [BsonElement("LeadKey")]
        public string? LeadKey { get; set; }

        [BsonElement("CreatedOn")]
        public DateTime? CreatedOn { get; set; }
        [BsonElement("LastLoginDate")]
        public DateTime? LastLoginDate { get; set; }
    }
}
