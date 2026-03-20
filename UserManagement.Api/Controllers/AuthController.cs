using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using UserManagement.Api.DTOs;
using UserManagement.Api.Models;
using UserManagement.Api.Services;
using BC = BCrypt.Net.BCrypt;

namespace UserManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMongoCollection<User> _users;
    private readonly ITokenService _tokenService;

    public AuthController(IMongoCollection<User> users, ITokenService tokenService)
    {
        _users = users;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(UserCreateDto userDto)
    {
        if (await _users.Find(u => u.Email == userDto.Email).AnyAsync())
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

        await _users.InsertOneAsync(user);

        return CreatedAtAction(nameof(Register), new { id = user.Id }, new { user.Id, user.Username, user.Email });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(UserLoginDto loginDto)
    {
        var user = await _users.Find(u => u.Email == loginDto.Email).SingleOrDefaultAsync();

        if (user == null || !BC.Verify(loginDto.Password, user.PasswordHash))
        {
            return Unauthorized();
        }

        var token = _tokenService.CreateToken(user);
        return Ok(new AuthResponseDto(token, user.Username, user.Email));
    }
}
