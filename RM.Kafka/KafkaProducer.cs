
using Confluent.Kafka;
using System.Text.Json;

namespace RM.Kafka
{
    public class KafkaProducer : IDisposable
    {
        private readonly IProducer<string, string> _producer;

        public KafkaProducer()
        {
            var config = new ProducerConfig
            {
                BootstrapServers = "jarvisalgoIpAddress:9092",
                BatchSize = 32 * 1024, // 32 KB
                LingerMs = 5, // 5 ms
                CompressionType = CompressionType.Snappy,
                Acks = Acks.All // Ensure all replicas acknowledge the message
            }; 
            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task SendOrderAsync(Order order)
        {
            var message = new Message<string, string>
            {
                Key = order.Symbol,
                Value = JsonSerializer.Serialize(order)
            };
            await _producer.ProduceAsync("order-requests", message);
        }

        public void Dispose()
        {
            _producer?.Dispose();
        }
    }

    public class Order
    {
        public string Exchange { get; set; }
        public string Symbol { get; set; }
        public string SymbolToken { get; set; }
        public int Qty { get; set; }
        public double Price { get; set; }
        public double TriggerPrice { get; set; }
        public string Label{ get; set; }
    }
}
