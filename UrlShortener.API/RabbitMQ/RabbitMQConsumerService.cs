using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using UrlShortener.API.Data;

namespace UrlShortener.API.RabbitMQ;

public class RabbitMQConsumerService : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IUrlDb _db;

    public RabbitMQConsumerService(IConnection connection, IModel channel, IUrlDb db)
    {
        _connection = connection;
        _channel = channel;
        _db = db;

        _channel.QueueDeclare(queue: "url_shortener_logs", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "url_access_count", durable: true, exclusive: false, autoDelete: false, arguments: null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        StartLogConsumer(stoppingToken);
        StartAccessCountConsumer(stoppingToken);

        return Task.CompletedTask;
    }

    private void StartLogConsumer(CancellationToken stoppingToken) 
    {
        var logConsumer = new EventingBasicConsumer(_channel);
        logConsumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            var log = JsonSerializer.Deserialize<LogMessage>(message);
            Console.WriteLine($"[LOG] {log.Action} - {log.Url} (Short: {log.ShortUrl}) at {log.CreatedAt}");

            _channel.BasicAck(ea.DeliveryTag, false);
        };

        _channel.BasicConsume(queue: "url_shortener_logs", autoAck: false, consumer: logConsumer);
    }

    private void StartAccessCountConsumer(CancellationToken stoppingToken)
    {
        var accessCountConsumer = new EventingBasicConsumer(_channel);
        accessCountConsumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            var access = JsonSerializer.Deserialize<AccessMessage>(message);
            Console.WriteLine($"[ACCESS] Incrementing access count for {access.ShortUrl}");

            await _db.IncrementAccessCountAsync(access.ShortUrl);

            _channel.BasicAck(ea.DeliveryTag, false);
        };

        _channel.BasicConsume(queue: "url_access_count", autoAck: false, consumer: accessCountConsumer);
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }

    private class LogMessage
    {
        public string Action { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ShortUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    private class AccessMessage
    {
        public string Action { get; set; } = string.Empty;
        public string ShortUrl { get; set; } = string.Empty;
        public DateTime AccessedAt { get; set; }
    }
}
