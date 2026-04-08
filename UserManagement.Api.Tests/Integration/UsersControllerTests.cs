using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using UserManagement.Api.Models;
using UserManagement.Api.Tests.Helpers;

namespace UserManagement.Api.Tests.Integration;

public class UsersControllerTests : BaseIntegrationTest
{
    public UsersControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetUsers_WhenAuthenticated_ReturnsList()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<User>>();
        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetUsers_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_ReturnsCorrectUserDetails()
    {
        // Arrange
        var email = "me@example.com";
        await AuthenticateAsync(email);

        // Act
        var response = await Client.GetAsync("/api/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<MeResponse>();
        result!.Username.Should().Be("testuser");
    }

    private class MeResponse
    {
        public string UserId { get; set; } = null!;
        public string Username { get; set; } = null!;
    }

    [Fact]
    public async Task GetUser_WithValidId_ReturnsUser()
    {
        // Arrange
        await AuthenticateAsync();
        var users = await Client.GetFromJsonAsync<List<User>>("/api/users");
        var userId = users![0].Id;

        // Act
        var response = await Client.GetAsync($"/api/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<User>();
        user!.Id.Should().Be(userId);
    }

    [Fact]
    public async Task GetUser_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();
        var invalidId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/users/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
