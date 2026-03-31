using MassTransit;
using MongoDB.Driver;
using StackExchange.Redis;
using Shared.Contracts;
using UrlManagement.Api.Models;

namespace UrlManagement.Api.Services;

public class UserAccountDeletedConsumer : IConsumer<IUserAccountDeletedEvent>
{
    private readonly IMongoCollection<UrlMapping> _urlCollection;
    private readonly IDatabase _cache;
    private readonly ILogger<UserAccountDeletedConsumer> _logger;

    public UserAccountDeletedConsumer(IMongoClient mongoClient, IConnectionMultiplexer redis, ILogger<UserAccountDeletedConsumer> logger)
    {
        var database = mongoClient.GetDatabase("UrlShortenerDb");
        _urlCollection = database.GetCollection<UrlMapping>("Urls");
        _cache = redis.GetDatabase();
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IUserAccountDeletedEvent> context)
    {
        var userId = context.Message.UserId.ToString();
        _logger.LogInformation("Bắt đầu dọn dẹp dữ liệu cho User bị xóa: {UserId}", userId);

        // 1. Tìm danh sách URL của User này để xóa Cache Redis
        var userUrls = await _urlCollection.Find(u => u.CreatedBy == userId).ToListAsync();

        foreach (var url in userUrls)
        {
            if (!string.IsNullOrEmpty(url.ShortCode))
            {
                await _cache.KeyDeleteAsync(url.ShortCode);
            }
        }

        // 2. Xóa sạch trong MongoDB
        var deleteResult = await _urlCollection.DeleteManyAsync(u => u.CreatedBy == userId);

        _logger.LogInformation("Completed! Removed {Count} URLs belonging to User {UserId} from DB and Cache.", deleteResult.DeletedCount, userId);
    }
}