namespace UrlShortener.API.Models.Domain;
public class Url
{
    public int Id { get; set; }
    public required string OriginalUrl { get; set; }
    public required string ShortUrl { get; set; }
    public int AccessCount { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime DeletedAt { get; set; }
}
