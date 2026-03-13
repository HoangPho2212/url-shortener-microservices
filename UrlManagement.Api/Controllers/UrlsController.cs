using Microsoft.AspNetCore.Mvc;
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
        if (string.IsNullOrEmpty(request.OriginalUrl))
        {
            return BadRequest("Original URL is required.");
        }

        // For now, use a hardcoded user ID. 
        // In a real scenario, this would come from the JWT token.
        var userId = "anonymous"; 
        
        var shortCode = await _urlService.ShortenUrlAsync(request.OriginalUrl, userId);
        return Ok(new { shortCode, shortUrl = $"{Request.Scheme}://{Request.Host}/{shortCode}" });
    }

    [HttpGet("{shortCode}")]
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
