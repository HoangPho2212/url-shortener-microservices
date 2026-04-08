using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using UrlManagement.Api.Controllers;
using UrlManagement.Api.Data;
using Shared.Contracts;
using MassTransit.Testing;
using FluentAssertions;
using Xunit;

namespace UrlManagement.Api.Tests;

public class UrlsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public UrlsControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Clear database before each test
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UrlDbContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task Shorten_ValidUrl_ReturnsShortUrl_AndPublishesEvent()
    {
        // Arrange
        var harness = _factory.Services.GetRequiredService<ITestHarness>();
        var request = new ShortenRequest("https://www.google.com");

        // Act
        var response = await _client.PostAsJsonAsync("/api/urls/shorten", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ShortenResponse>();
        Assert.NotNull(result);

        // Verify Event
        (await harness.Published.Any<IUrlShortenedEvent>(x => x.Context.Message.ShortCode == result.ShortCode)).Should().BeTrue();
    }

    [Fact]
    public async Task RedirectTo_ExistingShortCode_RedirectsToOriginalUrl()
    {
        // Arrange
        var request = new ShortenRequest("https://www.microsoft.com");
        var shortenResponse = await _client.PostAsJsonAsync("/api/urls/shorten", request);
        var shortenResult = await shortenResponse.Content.ReadFromJsonAsync<ShortenResponse>();
        var shortCode = shortenResult!.ShortCode;

        // Act
        var response = await _client.GetAsync($"/api/urls/{shortCode}");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://www.microsoft.com/", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task RedirectTo_NonExistentShortCode_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/urls/nonexistent");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private record ShortenResponse(string ShortUrl, string ShortCode);
}
