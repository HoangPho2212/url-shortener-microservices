using UserManagement.Api.Models;

namespace UserManagement.Api.Services;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<List<User>> GetAllAsync();
    Task CreateAsync(User user);
    Task UpdateAsync(Guid id, User user);
    Task DeleteAsync(Guid id);
}
