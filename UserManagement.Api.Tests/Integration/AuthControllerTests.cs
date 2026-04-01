using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using UserManagement.Api.Models;
using UserManagement.Api.Tests.Helpers;
using UserManagement.Api.Events;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace UserManagement.Api.Tests.Integration;

public class AuthControllerTests : BaseIntegrationTest
{
    public AuthControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsOk_AndPublishesEvent()
    {
        // Arrange
        var harness = Factory.Services.GetRequiredService<ITestHarness>();
        var userDto = new UserCreateDto
        {
            Username = "newuser",
            Email = "new@example.com",
            Password = "Password123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", userDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("User registered successfully");

        // Verify Event
        (await harness.Published.Any<UserRegisteredEvent>(x => x.Context.Message.Email == userDto.Email)).Should().BeTrue();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var userDto = new UserCreateDto
        {
            Username = "user1",
            Email = "user1@example.com",
            Password = "Password123!"
        };
        await Client.PostAsJsonAsync("/api/auth/register", userDto);

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", userDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("User already exists");
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var email = "login@example.com";
        var password = "Password123!";
        var registerDto = new UserCreateDto
        {
            Username = "loginuser",
            Email = email,
            Password = password
        };
        await Client.PostAsJsonAsync("/api/auth/register", registerDto);

        var loginDto = new UserLoginDto
        {
            Email = email,
            Password = password
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result!.Token.Should().NotBeNullOrEmpty();
    }

    private class LoginResponse
    {
        public string Token { get; set; } = null!;
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new UserLoginDto
        {
            Email = "wrong@example.com",
            Password = "WrongPassword!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
