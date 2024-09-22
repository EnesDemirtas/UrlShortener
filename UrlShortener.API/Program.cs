using Amazon.Runtime.Internal.Util;
using UrlShortener.API.Data;
using UrlShortener.API.Helpers;
using UrlShortener.API.Models.DTO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IUrlDb, MongoDbService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Endpoints
app.MapPost("/shorten", async (IUrlDb _db, UrlShortenRequest request) =>
{
    var shortUrl = UrlGenerator.Generate(request.OriginalUrl);
    var result = await _db.InsertShortUrlAsync(shortUrl, request.OriginalUrl);
    return Results.Ok(new { result });
});

app.MapGet("/{shortUrl}", async (IUrlDb _db, string shortUrl) =>
{
    var originalUrl = await _db.GetOriginalUrlAsync(shortUrl);
    if (string.IsNullOrEmpty(originalUrl))
        return Results.NotFound();
    
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
