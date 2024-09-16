namespace UrlShortener.API.Models.DTO;

public class UrlResponse
{
    public int Id { get; set; }
    public required string OriginalUrl {get;set;}
    public required string ShortUrl {get;set;}
    public DateTime CreatedAt { get; set;}
    public DateTime UpdatedAt { get; set; }
}
