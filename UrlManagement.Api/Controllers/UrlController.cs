using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using StackExchange.Redis;
using UrlManagement.Api.Models;
using UrlManagement.Api.Utils;

namespace UrlManagement.Api.Controllers;

[ApiController]
[Route("api/urls")]
public class UrlController : ControllerBase
{
    private readonly IDatabase _cache;
    private readonly IMongoCollection<UrlMapping> _urlCollection;

    public UrlController(IConnectionMultiplexer redis, IMongoClient mongoClient)
    {
        _cache = redis.GetDatabase();
        var database = mongoClient.GetDatabase("UrlShortenerDb");
        _urlCollection = database.GetCollection<UrlMapping>("Urls");
    }

    [HttpPost]
    public async Task<IActionResult> CreateShortLink([FromBody] string longUrl)
    {
      
        long newId = DateTime.UtcNow.Ticks;

        var urlMapping = new UrlMapping
        {
            NumericId = newId,
            LongUrl = longUrl,
            ShortCode = ShortCodeGenerator.Encode(newId) 
        };

        await _urlCollection.InsertOneAsync(urlMapping);

        return Ok(new { urlMapping.ShortCode });
    }

    [HttpGet("{shortCode}")]
    public async Task<IActionResult> RedirectUrl(string shortCode)
    {
      
        string cachedUrl = await _cache.StringGetAsync(shortCode);
        if (!string.IsNullOrEmpty(cachedUrl)) return Redirect(cachedUrl);

  
        var urlMapping = await _urlCollection
            .Find(u => u.ShortCode == shortCode)
            .FirstOrDefaultAsync();

        if (urlMapping != null)
        {
       
            await _cache.StringSetAsync(shortCode, urlMapping.LongUrl, TimeSpan.FromHours(24));
            return Redirect(urlMapping.LongUrl);
        }

        return NotFound();
    }
}