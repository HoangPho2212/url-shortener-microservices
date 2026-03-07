using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using UserManagement.Api.DTOs;

namespace UserManagement.Api.Tests.IntegrationTests;

public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UsersControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetUsers_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUsers_WithValidToken_ReturnsOk()
    {
        // Arrange
        var userDto = new UserCreateDto("apiuser", "apiuser@example.com", "Password123!");
        await _client.PostAsJsonAsync("/api/auth/register", userDto);

        var loginDto = new UserLoginDto("apiuser@example.com", "Password123!");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<object>>();
        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
    }
}
