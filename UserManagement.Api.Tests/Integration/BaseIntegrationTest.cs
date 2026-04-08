using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using UserManagement.Api.Data;
using UserManagement.Api.Models;
using UserManagement.Api.Tests.Helpers;

namespace UserManagement.Api.Tests.Integration;

public abstract class BaseIntegrationTest : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly HttpClient Client;
    protected readonly CustomWebApplicationFactory Factory;

    protected BaseIntegrationTest(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = Factory.CreateClient();

        // Ensure database is created and migrated
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureDeleted(); // Clear for each test to ensure isolation
        db.Database.EnsureCreated();
    }

    protected async Task<string> AuthenticateAsync(string email = "test@example.com", string password = "Password123!")
    {
        // First, register the user
        var registerDto = new UserCreateDto
        {
            Username = "testuser",
            Email = email,
            Password = password
        };
        await Client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Then, login
        var loginDto = new UserLoginDto
        {
            Email = email,
            Password = password
        };
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginDto);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.Token);
        return result.Token;
    }

    private class LoginResponse
    {
        public string Token { get; set; } = null!;
    }
}
