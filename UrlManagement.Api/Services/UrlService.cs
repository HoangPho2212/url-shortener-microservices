using MongoDB.Driver;
using UrlManagement.Api.Models;
using UrlManagement.Api.Settings;

namespace UrlManagement.Api.Services;

public class UrlService : IUrlService
{
    private readonly IMongoCollection<UrlRecord> _urls;
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private readonly Random _random = new();

    public UrlService(IMongoClient mongoClient, MongoDbSettings settings)
    {
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
        return shortCode;
    }

    public async Task<string?> GetOriginalUrlAsync(string shortCode)
    {
        var urlRecord = await _urls.Find(u => u.ShortUrl == shortCode).FirstOrDefaultAsync();
        return urlRecord?.OriginalUrl;
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
