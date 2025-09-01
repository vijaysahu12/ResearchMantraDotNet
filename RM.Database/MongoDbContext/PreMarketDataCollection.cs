using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RM.Database.MongoDbContext
{
    public class PreMarketReport
    {
        public class PreMarketCollection
        {
            [BsonId]
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id { get; set; }
            [Required]
            public Index IndianIndices { get; set; }
            [Required]
            public Index GlobalIndices { get; set; }
            [Required]
            public Commodities Commodities { get; set; } // Use appropriate type if commodities structure is defined.
            [Required]
            public List<FiiDiiData> FiiDiiData { get; set; }

            public MarketSentiment MarketSentiment { get; set; }

            [Required]
            public SupportResistance SupportResistance { get; set; }
            [Required] public MarketBulletins NewsBulletins { get; set; }
            [Required] public List<IndicatorSupport> IndicatorSupport { get; set; }
            [Required] public IndiaVix IndiaVix { get; set; }
            public string CreatedOn { get; set; }
            public double Bullish { get; set; }
            public double Bearish { get; set; }
        }
        public class Commodities
        {
            public double Bullish { get; set; }
            public double Bearish { get; set; }
            public Commodity Commodity { get; set; }



        }
        public class Commodity
        {
            public CommodityData GOLD { get; set; }
            public CommodityData SILVER { get; set; }
            public CommodityData CRUDEOIL { get; set; }


        }
        public class CommodityData
        {
            public string Name { get; set; }
            public decimal Close { get; set; }
            public decimal ChangePercentage { get; set; }
        }
        public class Index
        {
            public double Bullish { get; set; }
            public double Bearish { get; set; }
            public List<IndexData> Data { get; set; }

        }

        public class IndexData
        {
            [Required]
            public string Name { get; set; }
            [Required]
            public string Continent { get; set; }
            [Required]
            public double Close { get; set; }
            [Required]
            public double ChangePercentage { get; set; }
        }
        public class FiiDiiData
        {
            [Required] public string Name { get; set; }
            [Required] public string NetValue { get; set; }
        }
        public class SupportResistance
        {
            [Required] public string Name { get; set; }
            [Required] public double Support { get; set; }
            [Required] public double Resistance { get; set; }
            [Required] public double Pivot { get; set; }
        }
        public class MarketBulletins
        {
            public List<string> Bulletins { get; set; }
        }
        public class MarketSentiment
        {
            [Required]
            public List<string> SentimentAnalysis { get; set; }
        }
        public class IndicatorSupport
        {
            public string Name { get; set; }
            public decimal Close { get; set; }
            public decimal EMA_20 { get; set; }
            public decimal EMA_50 { get; set; }
            public decimal RSI { get; set; }
        }
        public class IndiaVix
        {
            [Required]
            public string Name { get; set; }
            [Required]
            public decimal Close { get; set; }
            [Required]
            public decimal Change { get; set; }
        }



        public class PreMarketSummaryResponseModel
        {
            public string Nifty { get; set; }
            public string BNF { get; set; }
            public string Vix { get; set; }
            public string HotNews { get; set; }
            public string Diis { get; set; }
            public string Fiis { get; set; }
            public string Oi { get; set; }
            public string ButtonText { get; set; }
            public string Id { get; set; }
            public DateTime CreatedOn { get; set; }
        }
    }

    public class PostMarketReport
    {
        public class PostMarketCollection
        {
            [BsonId]
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id { get; set; }
            public List<TopGainer> TopGainers { get; set; }
            public List<TopLoser> TopLosers { get; set; }
            public List<Performer> BestPerformer { get; set; }
            public List<Performer> WorstPerformer { get; set; }
            public List<VolumeShocker> VolumeShocker { get; set; }
            public string CreatedOn { get; set; }
        }

        public class TopGainer
        {
            public string Name { get; set; }
            public double Close { get; set; }
            public double ChangePercentage { get; set; }
        }

        public class TopLoser
        {
            public string Name { get; set; }
            public double Close { get; set; }
            public double ChangePercentage { get; set; }
        }

        public class Performer
        {
            public string Name { get; set; }
            public double Close { get; set; }
            public double ChangePercentage { get; set; }
        }

        public class VolumeShocker
        {
            public string Name { get; set; }
            public double Close { get; set; }
            public int Volume { get; set; }
        }

        public class PostMarketSummaryResponseModel
        {
            public string Id { get; set; }
            public string CreatedOn { get; set; }
            public Performer BestPerformer { get; set; }
            public Performer WorstPerformer { get; set; }
            public List<VolumeShocker> VolumeShocker { get; set; }
        }
    }

}
