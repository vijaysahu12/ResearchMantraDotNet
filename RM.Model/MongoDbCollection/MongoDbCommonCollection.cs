using MongoDB.Bson.Serialization.Attributes;
using System;

namespace RM.Model.MongoDbCollection
{
    public class MongoDbCommonCollection
    {
        //[BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("CreatedBy")]
        public string CreatedBy { get; set; }
        [BsonElement("CreatedOn")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [BsonElement("ModifiedOn")]
        public DateTime? ModifiedOn { get; set; }

        [BsonElement("ModifiedBy")]
        public string ModifiedBy { get; set; }
        [BsonElement("IsActive")]
        public bool IsActive { get; set; }

        [BsonElement("IsDelete")]
        public bool IsDelete { get; set; }
    }
   
}