using Microsoft.EntityFrameworkCore;
using UrlManagement.Api.Models;

namespace UrlManagement.Api.Data;

public class UrlDbContext : DbContext
{
    public UrlDbContext(DbContextOptions<UrlDbContext> options) : base(options)
    {
    }

    public DbSet<ShortUrl> ShortUrls { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ShortUrl>()
            .HasIndex(u => u.ShortCode)
            .IsUnique();

        modelBuilder.Entity<ShortUrl>()
            .HasIndex(u => u.UserId);
    }
}
