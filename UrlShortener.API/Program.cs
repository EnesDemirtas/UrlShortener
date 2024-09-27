using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using UrlShortener.API.Data;
using UrlShortener.API.Helpers;
using UrlShortener.API.Models.DTO;
using UrlShortener.API.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IUrlDb, MongoDbService>();

builder.Services.AddSingleton<IConnection>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    Task.Delay(5_000);
    var factory = new ConnectionFactory { Uri = new Uri(config["RabbitMQ:ConnectionUrl"]) };
    return factory.CreateConnection();
});

builder.Services.AddSingleton<IModel>(sp =>
{
    var connection = sp.GetRequiredService<IConnection>();
    var channel = connection.CreateModel();

    channel.QueueDeclare(queue: "url_shortener_logs", durable: true, exclusive: false, autoDelete: false, arguments: null);
    channel.QueueDeclare(queue: "url_access_count", durable: true, exclusive: false, autoDelete: false, arguments: null);
    var properties = channel.CreateBasicProperties();
    properties.Persistent = true;
    return channel;
});

builder.Services.AddHostedService<RabbitMQConsumerService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Endpoints
app.MapPost("/shorten", async (IUrlDb _db, IModel channel, UrlShortenRequest request) =>
{
    var shortUrl = UrlGenerator.Generate(request.OriginalUrl);
    var result = await _db.InsertShortUrlAsync(shortUrl, request.OriginalUrl);

    var logMessage = new { Action = "ShortenUrl", Url = request.OriginalUrl, ShortUrl = shortUrl, CreatedAt = DateTime.UtcNow };
    var messageBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(logMessage));
    channel.BasicPublish(exchange: "", routingKey: "url_shortener_logs", basicProperties: null, body: messageBody);


    return Results.Ok(new { result });
});

app.MapGet("/{shortUrl}", async (IUrlDb _db, IModel channel, string shortUrl) =>
{
    var originalUrl = await _db.GetOriginalUrlAsync(shortUrl);
    if (string.IsNullOrEmpty(originalUrl))
        return Results.NotFound();

    var accessMessage = new { Action = "AccessUrl", ShortUrl = shortUrl, AccessedAt = DateTime.UtcNow };
    var messageBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(accessMessage));
    channel.BasicPublish(exchange: "", routingKey: "url_access_count", basicProperties: null, body: messageBody);

    return Results.Redirect(originalUrl);
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

app.Run();
