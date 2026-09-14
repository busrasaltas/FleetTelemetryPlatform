using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace FleetTelemetryPlatform.Messaging
{
    public class RabbitMqPublisher : IMessagePublisher
    {
        private readonly string _host;

        public RabbitMqPublisher(IConfiguration configuration)
        {
            _host = configuration["RabbitMq:Host"] ?? "localhost";
        }

        public async Task PublishAsync<T>(T message, string queueName)
        {
            var factory = new ConnectionFactory { HostName = _host };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties { Persistent = true }; // persist message to disk, not just memory

            await channel.BasicPublishAsync(exchange: "", routingKey: queueName, mandatory: false, basicProperties: properties, body: body);
        }
    }
}