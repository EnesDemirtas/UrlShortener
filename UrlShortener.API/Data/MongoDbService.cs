using MongoDB.Bson;
using MongoDB.Driver;

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

    public async Task InsertShortUrlAsync(string shortUrl, string originalUrl)
    {
        var urlRecord = new BsonDocument 
        {
            { "ShortUrl", shortUrl },
            { "OriginalUrl", originalUrl },
            { "AccessCount", 0 },
            { "CreatedAt", DateTime.UtcNow },
        };

        try
        {
            await _collection.InsertOneAsync(urlRecord);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new Exception("A url record with this ShortUrl already exists.");
        }
    }

    public async Task<string> GetOriginalUrlAsync(string shortUrl)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("ShortUrl", shortUrl);
        var urlRecord = await _collection.Find(filter).FirstOrDefaultAsync();
        if (urlRecord != null)
            return urlRecord["OriginalUrl"].AsString;
        return string.Empty;
    }
}
