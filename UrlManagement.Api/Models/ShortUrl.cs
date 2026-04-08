using System.ComponentModel.DataAnnotations;

namespace UrlManagement.Api.Models;

public class ShortUrl
{
    public Guid Id { get; set; }

    [Required]
    [Url]
    public string OriginalUrl { get; set; } = string.Empty;

    [Required]
    [StringLength(10, MinimumLength = 6)]
    public string ShortCode { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid? UserId { get; set; }
}
