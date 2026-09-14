namespace FleetTelemetryPlatform.Messaging
{
    // Keeps the application independent from the RabbitMQ client API.
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(T message, string queueName);
    }
}