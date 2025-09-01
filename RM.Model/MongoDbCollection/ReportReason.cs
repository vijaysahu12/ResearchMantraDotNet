using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace RM.Model.MongoDbCollection
{
    public class ReportReason
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [Required]
        public string Reason { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
