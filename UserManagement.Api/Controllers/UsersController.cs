using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Api.DTOs;
using UserManagement.Api.Models;
using UserManagement.Api.Services;
using BC = BCrypt.Net.BCrypt;

namespace UserManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<ActionResult> GetUsers()
    {
        var users = await _userRepository.GetAllAsync();
        var response = users.Select(u => new { u.Id, u.Username, u.Email, u.CreatedAt });
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetUser(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound();

        return Ok(new { user.Id, user.Username, user.Email, user.CreatedAt });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateUser(Guid id, UserCreateDto inputUser)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound();

        user.Username = inputUser.Username;
        user.Email = inputUser.Email;
        if (!string.IsNullOrWhiteSpace(inputUser.Password))
        {
            user.PasswordHash = BC.HashPassword(inputUser.Password);
        }

        await _userRepository.UpdateAsync(id, user);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound();

        await _userRepository.DeleteAsync(id);
        return Ok(new { user.Id, user.Username, user.Email });
    }
}
