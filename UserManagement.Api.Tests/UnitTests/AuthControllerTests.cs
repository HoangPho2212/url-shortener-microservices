using Microsoft.AspNetCore.Mvc;
using Moq;
using UserManagement.Api.Controllers;
using UserManagement.Api.DTOs;
using UserManagement.Api.Models;
using UserManagement.Api.Services;
using FluentAssertions;

namespace UserManagement.Api.Tests.UnitTests;

public class AuthControllerTests
{
    private readonly Mock<IUserRepository> _mockRepo;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<MassTransit.IPublishEndpoint> _mockPublishEndpoint;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockRepo = new Mock<IUserRepository>();
        _mockTokenService = new Mock<ITokenService>();
        _mockPublishEndpoint = new Mock<MassTransit.IPublishEndpoint>();

        
        _controller = new AuthController(
            _mockRepo.Object,_mockTokenService.Object,_mockPublishEndpoint.Object
        );
    }

    [Fact]
    public async Task Register_UserAlreadyExists_ReturnsBadRequest()
    {
        // Arrange
        var userDto = new UserCreateDto("existinguser", "test@example.com", "password");
        _mockRepo.Setup(r => r.GetByEmailAsync(userDto.Email)).ReturnsAsync(new User());

        // Act
        var result = await _controller.Register(userDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Email is already taken");
    }

    [Fact]
    public async Task Register_NewUser_ReturnsCreatedAtAction()
    {
        // Arrange
        var userDto = new UserCreateDto("newuser", "new@example.com", "password");
        _mockRepo.Setup(r => r.GetByEmailAsync(userDto.Email)).ReturnsAsync((User?)null);
        _mockRepo.Setup(r => r.CreateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Register(userDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        _mockRepo.Verify(r => r.CreateAsync(It.Is<User>(u => u.Email == userDto.Email)), Times.Once);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        // Arrange
        var loginDto = new UserLoginDto("test@example.com", "password");
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password");
        var user = new User { Username = "testuser", Email = "test@example.com", PasswordHash = passwordHash };

        _mockRepo.Setup(r => r.GetByEmailAsync(loginDto.Email)).ReturnsAsync(user);
        _mockTokenService.Setup(s => s.CreateToken(It.IsAny<User>())).Returns("fake-token");

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as AuthResponseDto;
        response!.Token.Should().Be("fake-token");
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new UserLoginDto("test@example.com", "wrongpassword");
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword");
        var user = new User { Email = "test@example.com", PasswordHash = passwordHash };

        _mockRepo.Setup(r => r.GetByEmailAsync(loginDto.Email)).ReturnsAsync(user);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedResult>();
    }
}