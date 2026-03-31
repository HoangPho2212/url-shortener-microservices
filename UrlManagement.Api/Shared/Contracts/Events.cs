namespace Shared.Contracts;



public interface IUserRegisteredEvent

{

    Guid UserId { get; }

    string Email { get; }

    string Username { get; }

}



public interface IUrlShortenedEvent

{

    string ShortCode { get; }

    string OriginalUrl { get; }

    string CreatedBy { get; }

}



public interface IUrlClickedEvent

{

    string ShortCode { get; }

    DateTime ClickedAt { get; }

    string? IpAddress { get; }

}



public interface IUserAccountDeletedEvent

{

    Guid UserId { get; }

    string Email { get; }

}