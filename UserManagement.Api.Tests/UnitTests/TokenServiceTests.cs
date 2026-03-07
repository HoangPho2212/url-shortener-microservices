using Microsoft.Extensions.Configuration;
using UserManagement.Api.Models;
using UserManagement.Api.Services;
using FluentAssertions;
using System.IdentityModel.Tokens.Jwt;

namespace UserManagement.Api.Tests.UnitTests;

public class TokenServiceTests
{
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _config;

    public TokenServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string> {
            {"Jwt:Key", "A_Very_Long_And_Secure_Key_For_Jwt_Authentication_2026"},
            {"Jwt:Issuer", "UserManagementApi"},
            {"Jwt:Audience", "UrlShortenerPlatform"}
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _tokenService = new TokenService(_config);
    }

    [Fact]
    public void CreateToken_ShouldReturnValidToken()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com"
        };

        // Act
        var token = _tokenService.CreateToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        jwtToken.Issuer.Should().Be("UserManagementApi");
        jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.UniqueName).Value.Should().Be("testuser");
        jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value.Should().Be("test@example.com");
    }
}
