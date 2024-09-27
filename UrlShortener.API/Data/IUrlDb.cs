namespace UrlShortener.API.Data;

public interface IUrlDb
{
    Task<string> InsertShortUrlAsync(string shortUrl, string originalUrl);
    Task<string> GetOriginalUrlAsync(string shortUrl);
    Task IncrementAccessCountAsync(string shortUrl);
}
