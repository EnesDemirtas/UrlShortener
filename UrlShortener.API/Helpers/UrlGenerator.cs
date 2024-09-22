using Base62;

namespace UrlShortener.API.Helpers;

public static class UrlGenerator
{
    public static string Generate(string originalUrl)
    {
        return originalUrl.ToBase62()[..6];
    }

    public static string Generate(string originalUrl, int skip)
    {
        return originalUrl.ToBase62().Substring(skip, 6);
    }
}
