namespace UrlShortener.API.Data;

public interface IUrlDb
{
    Task InsertShortUrlAsync(string shortUrl, string originalUrl);
    Task<string> GetOriginalUrlAsync(string shortUrl);
}
