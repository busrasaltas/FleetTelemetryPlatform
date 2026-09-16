using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using FleetTelemetryPlatform.Data;
using FleetTelemetryPlatform.Messaging.Events;
using FleetTelemetryPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetTelemetryPlatform.Workers
{ 
    public class TelemetryConsumerService : BackgroundService
    {
        private const string QueueName = "telemetry-received";
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _host;
        private readonly ILogger<TelemetryConsumerService> _logger;

        private IConnection? _connection;
        private IChannel? _channel;

        public TelemetryConsumerService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<TelemetryConsumerService> logger)
        {
            _scopeFactory = scopeFactory;
            _host = configuration["RabbitMq:Host"] ?? "localhost";
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory { HostName = _host };
            
            _connection = await ConnectWithRetryAsync(factory, stoppingToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await _channel.QueueDeclareAsync(queue: QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);

            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += OnMessageReceivedAsync;

            await _channel.BasicConsumeAsync(queue: QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

            await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { });
        }

        private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
        {
            try
            {
                var json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                var telemetryEvent = JsonSerializer.Deserialize<TelemetryReceivedEvent>(json);

                if (telemetryEvent is not null)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    db.TelemetryReadings.Add(new TelemetryReading
                    {
                        DeviceId = telemetryEvent.DeviceId,
                        Timestamp = telemetryEvent.Timestamp,
                        Latitude = telemetryEvent.Latitude,
                        Longitude = telemetryEvent.Longitude,
                        Altitude = telemetryEvent.Altitude,
                        BatteryLevel = telemetryEvent.BatteryLevel,
                        Speed = telemetryEvent.Speed
                    });

                    await db.Devices
                        .Where(d => d.Id == telemetryEvent.DeviceId)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(d => d.LastSeenAt, telemetryEvent.Timestamp));

                    await db.SaveChangesAsync();
                }

                await _channel!.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process telemetry message, requeueing");
               
                await _channel!.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: true);
            }
        }

        private async Task<IConnection> ConnectWithRetryAsync(ConnectionFactory factory, CancellationToken stoppingToken)
        {
            const int maxAttempts = 10;
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    return await factory.CreateConnectionAsync(stoppingToken);
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    _logger.LogWarning("RabbitMQ connection attempt {Attempt} failed: {Message}. Retrying...", attempt, ex.Message);
                    await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
                }
            }

            return await factory.CreateConnectionAsync(stoppingToken);
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}