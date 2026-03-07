using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Data;
using UserManagement.Api.DTOs;
using UserManagement.Api.Models;
using UserManagement.Api.Services;
using BC = BCrypt.Net.BCrypt;

namespace UserManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserDbContext _db;
    private readonly ITokenService _tokenService;

    public AuthController(UserDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(UserCreateDto userDto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == userDto.Email))
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

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Register), new { id = user.Id }, new { user.Id, user.Username, user.Email });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(UserLoginDto loginDto)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == loginDto.Email);

        if (user == null || !BC.Verify(loginDto.Password, user.PasswordHash))
        {
            return Unauthorized();
        }

        var token = _tokenService.CreateToken(user);
        return Ok(new AuthResponseDto(token, user.Username, user.Email));
    }
}
