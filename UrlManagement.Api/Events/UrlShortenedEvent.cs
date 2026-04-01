namespace UrlManagement.Api.Events;

public record UrlShortenedEvent
{
    public Guid Id { get; init; }
    public string OriginalUrl { get; init; } = string.Empty;
    public string ShortCode { get; init; } = string.Empty;
    public Guid? UserId { get; init; }
}
