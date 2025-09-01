using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM.Model.MongoDbCollection
{
    public class CommunityReport
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("Id")]
        public string Id { get; set; }

        [BsonElement("communityId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CommunityId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("reportedBy")]
        public ObjectId ReportedBy { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("reasonId")]
        public ObjectId ReasonId { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("createdOn")]
        public DateTime CreatedOn { get; set; }

        [BsonElement("status")]
        public bool Status { get; set; }
    }
}
