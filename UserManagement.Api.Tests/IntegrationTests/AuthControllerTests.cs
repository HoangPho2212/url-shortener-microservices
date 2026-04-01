using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using UserManagement.Api.DTOs;

namespace UserManagement.Api.Tests.IntegrationTests;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ValidUser_ReturnsCreated()
    {
        // Arrange
        var userDto = new UserCreateDto("testuser", "test@example.com", "Password123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", userDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var userDto = new UserCreateDto("loginuser", "login@example.com", "Password123!");
        await _client.PostAsJsonAsync("/api/auth/register", userDto);

        var loginDto = new UserLoginDto("login@example.com", "Password123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.Email.Should().Be("login@example.com");
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new UserLoginDto("nonexistent@example.com", "WrongPassword");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
