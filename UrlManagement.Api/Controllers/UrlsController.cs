using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlManagement.Api.Data;
using UrlManagement.Api.Models;
using UrlManagement.Api.Services;
using UrlManagement.Api.Events;
using MassTransit;

namespace UrlManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UrlsController : ControllerBase
{
    private readonly UrlDbContext _context;
    private readonly IUrlShorteningService _urlShorteningService;
    private readonly IPublishEndpoint _publishEndpoint;

    public UrlsController(UrlDbContext context, IUrlShorteningService urlShorteningService, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _urlShorteningService = urlShorteningService;
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost("shorten")]
    public async Task<IActionResult> Shorten([FromBody] ShortenRequest request)
    {
        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
        {
            return BadRequest("Invalid URL format.");
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? userId = userIdClaim != null ? Guid.Parse(userIdClaim) : null;

        string shortCode;
        bool exists;
        do
        {
            shortCode = _urlShorteningService.GenerateShortCode();
            exists = await _context.ShortUrls.AnyAsync(u => u.ShortCode == shortCode);
        } while (exists);

        var shortUrl = new ShortUrl
        {
            Id = Guid.NewGuid(),
            OriginalUrl = request.Url,
            ShortCode = shortCode,
            UserId = userId
        };

        _context.ShortUrls.Add(shortUrl);
        await _context.SaveChangesAsync();

        await _publishEndpoint.Publish(new UrlShortenedEvent
        {
            Id = shortUrl.Id,
            OriginalUrl = shortUrl.OriginalUrl,
            ShortCode = shortUrl.ShortCode,
            UserId = shortUrl.UserId
        });

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        return Ok(new { ShortUrl = $"{baseUrl}/api/urls/{shortCode}", ShortCode = shortCode });
    }

    [HttpGet("{shortCode}")]
    public async Task<IActionResult> RedirectTo(string shortCode)
    {
        var shortUrl = await _context.ShortUrls.FirstOrDefaultAsync(u => u.ShortCode == shortCode);

        if (shortUrl == null)
        {
            return NotFound();
        }

        return Redirect(shortUrl.OriginalUrl);
    }

    [Authorize]
    [HttpGet("my-urls")]
    public async Task<IActionResult> GetMyUrls()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return Unauthorized();

        var userId = Guid.Parse(userIdClaim);
        var urls = await _context.ShortUrls
            .Where(u => u.UserId == userId)
            .ToListAsync();

        return Ok(urls);
    }
}

public record ShortenRequest(string Url);
