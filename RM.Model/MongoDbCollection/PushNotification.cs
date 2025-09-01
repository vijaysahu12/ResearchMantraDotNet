using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace RM.Model.MongoDbCollection
{
    public class PushNotification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ObjectId { get; set; }

        [BsonElement("Title")]
        public string Title { get; set; }

        [BsonElement("Message")]
        public string? Message { get; set; }

        [BsonElement("EnableTradingButton")]
        public bool EnableTradingButton { get; set; }

        [BsonElement("Scanner")]
        public bool Scanner { get; set; }

        [BsonElement("Token")]
        public string Token { get; set; }

        [BsonElement("AppCode")]
        public string AppCode { get; set; }

        [BsonElement("Exchange")]
        public string Exchange { get; set; }

        [BsonElement("TradingSymbol")]
        public string TradingSymbol { get; set; }

        [BsonElement("TransactionType")]
        public string TransactionType { get; set; }

        [BsonElement("Quantity")]
        public int Quantity { get; set; }

        [BsonElement("OrderType")]
        public string OrderType { get; set; }

        [BsonElement("Price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; }

        [BsonElement("Validity")]
        public string Validity { get; set; }


        [BsonElement("ProductId")]
        [BsonRepresentation(BsonType.String)]
        public string? ProductId { get; set; }

        [BsonElement("ProductName")]
        public string? ProductName { get; set; }

        [BsonElement("Complexity")]
        public string Complexity { get; set; }

        [BsonElement("CategoryId")]
        public int CategoryId { get; set; }

        [BsonElement("Topic")]
        public string Topic { get; set; }

        [BsonElement("ScreenName")]
        public string? ScreenName { get; set; }

        [BsonElement("CreatedOn")]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime CreatedOn { get; set; }

        [BsonElement("IsPinned")]
        public bool IsPinned { get; set; }

        [BsonElement("IsDelete")]
        public bool IsDelete { get; set; }

        [BsonElement("Screen")]
        public string? Screen { get; set; }

        [BsonElement("ImageUrl")]
        public string ImageUrl { get; set; } = string.Empty;
    }
    public class PushNotificationReceiver
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ObjectId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string NotificationId { get; set; }

        [BsonElement("ReceivedBy")]
        [BsonRepresentation(BsonType.String)]
        public Guid ReceivedBy { get; set; }

        [BsonElement("IsRead")]
        public bool IsRead { get; set; }

        [BsonElement("IsDelete")]
        public bool IsDelete { get; set; }

        [BsonElement("ReadDate")]
        public string? ReadDate { get; set; }
    }

    public class ScannerNotification
    {
        public bool IsRead { get; set; }
        public string ObjectId { get; set; }
        public string TradingSymbol { get; set; }
        public string CreatedOn { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public int Price { get; set; }
        public string TransactionType { get; set; }
        public string Topic { get; set; }
        public string ViewChart { get; set; }
        public string Created { get; set; }
    }

    public class PushNotificationResponse
    {
        public string NotificationId { get; set; }
        public string ObjectId { get; set; }
        public string ReceivedBy { get; set; }

        public string Message { get; set; }
        public string Title { get; set; }
        public bool EnableTradingButton { get; set; }
        public string AppCode { get; set; }
        public string Exchange { get; set; }
        public string TradingSymbol { get; set; }
        public string TransactionType { get; set; }
        public string OrderType { get; set; }
        public int Price { get; set; } // Converted to int using $toInt in the projection
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string Complexity { get; set; }
        public int CategoryId { get; set; }
        public bool IsRead { get; set; }
        public bool IsDelete { get; set; }
        public string? ReadDate { get; set; } // Nullable DateTime for potential null values
        public string Topic { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? ScreenName { get; set; }
        public bool? IsPinned { get; set; }
    }


}
