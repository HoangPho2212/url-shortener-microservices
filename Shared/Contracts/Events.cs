using System;

namespace Shared.Contracts;

public record IUserRegisteredEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
}

public record IUrlShortenedEvent
{
    public Guid Id { get; init; }
    public string OriginalUrl { get; init; } = string.Empty;
    public string ShortCode { get; init; } = string.Empty;
    public Guid? UserId { get; init; }
}

public record IUrlClickedEvent
{
    public string ShortCode { get; init; } = string.Empty;
    public DateTime ClickedAt { get; init; }
    public string? IpAddress { get; init; }
}

public record IUserAccountDeletedEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
}