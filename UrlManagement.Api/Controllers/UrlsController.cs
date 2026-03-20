using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UrlManagement.Api.Services;

namespace UrlManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UrlsController : ControllerBase
{
    private readonly IUrlService _urlService;

    public UrlsController(IUrlService urlService)
    {
        _urlService = urlService;
    }

    [HttpPost("shorten")]
    public async Task<IActionResult> Shorten([FromBody] ShortenRequest request)
    {
        if (string.IsNullOrEmpty(request.OriginalUrl))
        {
            return BadRequest("Original URL is required.");
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        
        var shortCode = await _urlService.ShortenUrlAsync(request.OriginalUrl, userId);
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        return Ok(new { shortCode, shortUrl = $"{baseUrl}/{shortCode}" });
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyUrls()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var urls = await _urlService.GetUrlsByUserAsync(userId);
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        
        var response = urls.Select(u => new 
        {
            u.Id,
            u.OriginalUrl,
            u.ShortUrl,
            FullShortUrl = $"{baseUrl}/{u.ShortUrl}",
            u.Clicks,
            u.CreatedAt
        });

        return Ok(response);
    }

    [HttpGet("{shortCode}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetOriginal(string shortCode)
    {
        var originalUrl = await _urlService.GetOriginalUrlAsync(shortCode);
        if (originalUrl == null)
        {
            return NotFound();
        }

        return Ok(new { originalUrl });
    }
}

public class ShortenRequest
{
    public string OriginalUrl { get; set; } = null!;
}
