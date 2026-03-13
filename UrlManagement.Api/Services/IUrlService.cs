namespace UrlManagement.Api.Services;

public interface IUrlService
{
    Task<string> ShortenUrlAsync(string originalUrl, string userId);
    Task<string?> GetOriginalUrlAsync(string shortCode);
}
