using Microsoft.AspNetCore.Mvc;
using UserManagement.Api.DTOs;
using UserManagement.Api.Models;
using UserManagement.Api.Services;
using MassTransit;
using Shared.Contracts;
using BC = BCrypt.Net.BCrypt;

namespace UserManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuthController(IUserRepository userRepository, ITokenService tokenService, IPublishEndpoint publishEndpoint)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(UserCreateDto userDto)
    {
        Console.WriteLine($"[DEBUG] Registering user: {userDto.Email}");
        if (await _userRepository.GetByEmailAsync(userDto.Email) != null)
        {
            Console.WriteLine($"[DEBUG] User already exists: {userDto.Email}");
            return BadRequest("Email is already taken");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = userDto.Username,
            Email = userDto.Email,
            PasswordHash = BC.HashPassword(userDto.Password),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);
        Console.WriteLine($"[DEBUG] User created in MongoDB with ID: {user.Id}");

        // Publish Event to RabbitMQ
        try 
        {
            await _publishEndpoint.Publish<IUserRegisteredEvent>(new
            {
                UserId = user.Id,
                user.Email,
                user.Username
            });
            Console.WriteLine("[DEBUG] Published UserRegisteredEvent to RabbitMQ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] Failed to publish event: {ex.Message}");
        }

        return CreatedAtAction(nameof(Register), new { id = user.Id }, new { user.Id, user.Username, user.Email });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(UserLoginDto loginDto)
    {
        Console.WriteLine($"[DEBUG] Login attempt for: {loginDto.Email}");
        var user = await _userRepository.GetByEmailAsync(loginDto.Email);

        if (user == null)
        {
            Console.WriteLine($"[DEBUG] Login failed: User {loginDto.Email} not found in database.");
            return Unauthorized();
        }

        if (!BC.Verify(loginDto.Password, user.PasswordHash))
        {
            Console.WriteLine($"[DEBUG] Login failed: Incorrect password for {loginDto.Email}.");
            return Unauthorized();
        }

        Console.WriteLine($"[DEBUG] Login successful for: {loginDto.Email}");
        var token = _tokenService.CreateToken(user);
        return Ok(new AuthResponseDto(token, user.Username, user.Email));
    }
}
