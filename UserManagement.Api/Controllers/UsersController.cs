using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Data;
using UserManagement.Api.DTOs;
using UserManagement.Api.Models;
using BC = BCrypt.Net.BCrypt;

namespace UserManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserDbContext _db;

    public UsersController(UserDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult> GetUsers()
    {
        var users = await _db.Users
            .Select(u => new { u.Id, u.Username, u.Email, u.CreatedAt })
            .ToListAsync();
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetUser(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        return Ok(new { user.Id, user.Username, user.Email, user.CreatedAt });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateUser(Guid id, UserCreateDto inputUser)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.Username = inputUser.Username;
        user.Email = inputUser.Email;
        if (!string.IsNullOrWhiteSpace(inputUser.Password))
        {
            user.PasswordHash = BC.HashPassword(inputUser.Password);
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        return Ok(new { user.Id, user.Username, user.Email });
    }
}
