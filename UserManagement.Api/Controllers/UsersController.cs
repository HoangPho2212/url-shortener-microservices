using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using UserManagement.Api.DTOs;
using UserManagement.Api.Models;
using BC = BCrypt.Net.BCrypt;

namespace UserManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMongoCollection<User> _users;

    public UsersController(IMongoCollection<User> users)
    {
        _users = users;
    }

    [HttpGet]
    public async Task<ActionResult> GetUsers()
    {
        var users = await _users.Find(_ => true)
            .Project(u => new { u.Id, u.Username, u.Email, u.CreatedAt })
            .ToListAsync();
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetUser(Guid id)
    {
        var user = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        if (user == null) return NotFound();

        return Ok(new { user.Id, user.Username, user.Email, user.CreatedAt });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateUser(Guid id, UserCreateDto inputUser)
    {
        var user = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        if (user == null) return NotFound();

        var updateDefinition = Builders<User>.Update
            .Set(u => u.Username, inputUser.Username)
            .Set(u => u.Email, inputUser.Email);

        if (!string.IsNullOrWhiteSpace(inputUser.Password))
        {
            updateDefinition = updateDefinition.Set(u => u.PasswordHash, BC.HashPassword(inputUser.Password));
        }

        await _users.UpdateOneAsync(u => u.Id == id, updateDefinition);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        var user = await _users.FindOneAndDeleteAsync(u => u.Id == id);
        if (user == null) return NotFound();

        return Ok(new { user.Id, user.Username, user.Email });
    }
}
