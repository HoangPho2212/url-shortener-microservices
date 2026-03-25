using MongoDB.Driver;
using UrlManagement.Api.Models;
using UrlManagement.Api.Settings;
using MassTransit;
using Shared.Contracts;

namespace UrlManagement.Api.Services;

public class UrlService : IUrlService
{
    private readonly IMongoCollection<UrlRecord> _urls;
    private readonly IPublishEndpoint _publishEndpoint;
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private readonly Random _random = new();

    public UrlService(IMongoClient mongoClient, MongoDbSettings settings, IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
        var database = mongoClient.GetDatabase(settings.DatabaseName);
        _urls = database.GetCollection<UrlRecord>(settings.CollectionName);

        // Ensure index on ShortUrl
        var indexKeysDefinition = Builders<UrlRecord>.IndexKeys.Ascending(u => u.ShortUrl);
        var indexOptions = new CreateIndexOptions { Unique = true };
        var indexModel = new CreateIndexModel<UrlRecord>(indexKeysDefinition, indexOptions);
        _urls.Indexes.CreateOne(indexModel);
    }

    public async Task<string> ShortenUrlAsync(string originalUrl, string userId)
    {
        string shortCode;
        bool exists;

        do
        {
            shortCode = GenerateShortCode(7);
            exists = await _urls.Find(u => u.ShortUrl == shortCode).AnyAsync();
        } while (exists);

        var urlRecord = new UrlRecord
        {
            ShortUrl = shortCode,
            OriginalUrl = originalUrl,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _urls.InsertOneAsync(urlRecord);

        // Publish UrlShortenedEvent
        await _publishEndpoint.Publish<IUrlShortenedEvent>(new
        {
            ShortCode = shortCode,
            OriginalUrl = originalUrl,
            CreatedBy = userId
        });

        return shortCode;
    }

    public async Task<string?> GetOriginalUrlAsync(string shortCode)
    {
        var filter = Builders<UrlRecord>.Filter.Eq(u => u.ShortUrl, shortCode);
        var update = Builders<UrlRecord>.Update.Inc(u => u.Clicks, 1);
        
        var urlRecord = await _urls.FindOneAndUpdateAsync(filter, update);

        if (urlRecord != null)
        {
            // Publish UrlClickedEvent
            await _publishEndpoint.Publish<IUrlClickedEvent>(new
            {
                ShortCode = shortCode,
                ClickedAt = DateTime.UtcNow,
                IpAddress = (string?)null // Could be added from HttpContext if needed
            });
        }

        return urlRecord?.OriginalUrl;
    }

    public async Task<List<UrlRecord>> GetUrlsByUserAsync(string userId)
    {
        return await _urls.Find(u => u.CreatedBy == userId).ToListAsync();
    }

    private string GenerateShortCode(int length)
    {
        var chars = new char[length];
        for (int i = 0; i < length; i++)
        {
            chars[i] = Alphabet[_random.Next(Alphabet.Length)];
        }
        return new string(chars);
    }
}
