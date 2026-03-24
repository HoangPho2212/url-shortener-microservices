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
        if (await _userRepository.GetByEmailAsync(userDto.Email) != null)
        {
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

        // Publish Event to RabbitMQ
        await _publishEndpoint.Publish<IUserRegisteredEvent>(new
        {
            UserId = user.Id,
            user.Email,
            user.Username
        });

        return CreatedAtAction(nameof(Register), new { id = user.Id }, new { user.Id, user.Username, user.Email });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(UserLoginDto loginDto)
    {
        var user = await _userRepository.GetByEmailAsync(loginDto.Email);

        if (user == null || !BC.Verify(loginDto.Password, user.PasswordHash))
        {
            return Unauthorized();
        }

        var token = _tokenService.CreateToken(user);
        return Ok(new AuthResponseDto(token, user.Username, user.Email));
    }
}
