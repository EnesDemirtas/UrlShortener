using Microsoft.EntityFrameworkCore;
using UrlShortener.API.Models.Domain;

namespace UrlShortener.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Url> Urls { get; set; }
}
