using Microsoft.AspNetCore.Mvc;
using UrlManagement.Api.Utils;

namespace UrlManagement.Api.Controllers;

[ApiController]
[Route("api/urls")] // Task requires /api/urls
public class UrlController : ControllerBase
{
    // Mock Database using Dictionary (Since real DB is not required in this Sprint)
    private static readonly Dictionary<string, string> UrlStore = new();
    private static long _currentId = 1000; // Initial ID

    // 1. POST /api/urls: Create short link
    [HttpPost]
    public IActionResult CreateShortLink([FromBody] string longUrl)
    {
        if (string.IsNullOrEmpty(longUrl)) return BadRequest("URL cannot be empty");

        _currentId++;
        // Use the Base62 algorithm from ShortCodeGenerator
        string shortCode = ShortCodeGenerator.Encode(_currentId);

        // Save to "Mock Database"
        UrlStore[shortCode] = longUrl;

        return Ok(new
        {
            ShortCode = shortCode,
            ShortLink = $"https://localhost:5000/gateway/urls/{shortCode}"
        });
    }

    // 2. GET /{shortCode}: HTTP 302 redirect
    [HttpGet("{shortCode}")]
    public IActionResult RedirectUrl(string shortCode)
    {
        if (UrlStore.TryGetValue(shortCode, out var longUrl))
        {
            // Perform HTTP 302 Redirect as required by the task
            return Redirect(longUrl);
        }

        return NotFound(new { Message = "Short link not found" });
    }
}