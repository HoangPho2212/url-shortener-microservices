using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UrlManagement.Api.Services;

namespace UrlManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        Console.WriteLine($"[DEBUG] Received shorten request for: {request.OriginalUrl}");

        if (string.IsNullOrEmpty(request.OriginalUrl))
        {
            Console.WriteLine("[DEBUG] Original URL is empty");
            return BadRequest("Original URL is required.");
        }

        var userId = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                     ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? "anonymous";

        Console.WriteLine($"[DEBUG] Shortening for UserID: {userId}");

        try
        {
            var shortCode = await _urlService.ShortenUrlAsync(request.OriginalUrl, userId);
            // Trả về địa chỉ Gateway để browser có thể truy cập
            var shortUrl = $"http://localhost:5130/s/{shortCode}";
            Console.WriteLine($"[DEBUG] Successfully shortened to: {shortCode}");
            return Ok(new { shortCode, shortUrl });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] Error during shortening: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyUrls()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        Console.WriteLine($"[DEBUG] Authorization header received: '{authHeader}'");

        var userId = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                     ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var urls = await _urlService.GetUrlsByUserAsync(userId);

        // Trả về địa chỉ Gateway để browser có thể click redirect
        var response = urls.Select(u => new
        {
            u.Id,
            u.OriginalUrl,
            u.ShortUrl,
            FullShortUrl = $"http://localhost:5130/s/{u.ShortUrl}",
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
