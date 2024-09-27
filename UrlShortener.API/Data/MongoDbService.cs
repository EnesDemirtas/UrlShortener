using MongoDB.Bson;
using MongoDB.Driver;
using UrlShortener.API.Helpers;

namespace UrlShortener.API.Data;

public class MongoDbService : IUrlDb
{
    private readonly IMongoCollection<BsonDocument> _collection;

    public MongoDbService(IConfiguration config)
    {
        var client = new MongoClient(config["MongoDb:Client"]);
        var database = client.GetDatabase(config["MongoDb:Database"]);
        _collection = database.GetCollection<BsonDocument>(config["MongoDb:Collection"]);

        var indexOptions = new CreateIndexOptions { Unique = true };
        var indexKeys = Builders<BsonDocument>.IndexKeys.Ascending("ShortUrl");
        _collection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(indexKeys, indexOptions));
    }

    public async Task<string> InsertShortUrlAsync(string shortUrl, string originalUrl)
    {
        var urlRecord = new BsonDocument
        {
            { "ShortUrl", shortUrl },
            { "OriginalUrl", originalUrl },
            { "AccessCount", 0 },
            { "CreatedAt", DateTime.UtcNow },
        };

        for (int attempt = 1; ; attempt++)
        {
            try
            {
                await _collection.InsertOneAsync(urlRecord);
                break;
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                // If DuplicateKey exception is thrown, generate the short url
                // again but skip 'attempt' number of characters from beginning
                shortUrl = UrlGenerator.Generate(originalUrl, attempt); // attempt => skip
                urlRecord["ShortUrl"] = shortUrl;
            }
        }

        return shortUrl;
    }

    public async Task<string> GetOriginalUrlAsync(string shortUrl)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("ShortUrl", shortUrl);
        var urlRecord = await _collection.Find(filter).FirstOrDefaultAsync();
        if (urlRecord != null)
            return urlRecord["OriginalUrl"].AsString;
        return string.Empty;
    }

    public async Task IncrementAccessCountAsync(string shortUrl)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("ShortUrl", shortUrl);
        var update = Builders<BsonDocument>.Update.Inc("AccessCount", 1);
        await _collection.UpdateOneAsync(filter, update);
    }
}
