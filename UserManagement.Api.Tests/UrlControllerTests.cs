using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using StackExchange.Redis;
using UrlManagement.Api.Controllers;
using UrlManagement.Api.Models;

namespace UserManagement.Api.Tests;

public class UrlControllerTests
{
    private readonly Mock<IConnectionMultiplexer> _mockRedis;
    private readonly Mock<IDatabase> _mockRedisDb;
    private readonly Mock<IMongoClient> _mockMongoClient;
    private readonly Mock<IMongoDatabase> _mockMongoDb;
    private readonly Mock<IMongoCollection<UrlMapping>> _mockCollection;
    private readonly UrlController _controller;

    public UrlControllerTests()
    {

        _mockRedis = new Mock<IConnectionMultiplexer>();
        _mockRedisDb = new Mock<IDatabase>();
        _mockRedis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_mockRedisDb.Object);


        _mockMongoClient = new Mock<IMongoClient>();
        _mockMongoDb = new Mock<IMongoDatabase>();
        _mockCollection = new Mock<IMongoCollection<UrlMapping>>();

        _mockMongoClient.Setup(c => c.GetDatabase(It.IsAny<string>(), null)).Returns(_mockMongoDb.Object);
        _mockMongoDb.Setup(db => db.GetCollection<UrlMapping>(It.IsAny<string>(), null)).Returns(_mockCollection.Object);

        _controller = new UrlController(_mockRedis.Object, _mockMongoClient.Object);
    }

    [Fact]
    public async Task CreateShortLink_ReturnsOk_WithShortCode()
    {

        var longUrl = "https://example.com";

        var result = await _controller.CreateShortLink(longUrl);


        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockCollection.Verify(m => m.InsertOneAsync(It.IsAny<UrlMapping>(), null, default), Times.Once);
    }

    [Fact]
    public async Task RedirectUrl_ReturnsRedirect_WhenCacheHit()
    {

        var shortCode = "abc123";
        var expectedUrl = "https://original.com";
        _mockRedisDb.Setup(db => db.StringGetAsync(shortCode, It.IsAny<CommandFlags>()))
                    .ReturnsAsync(expectedUrl);


        var result = await _controller.RedirectUrl(shortCode);


        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal(expectedUrl, redirectResult.Url);
    }
}