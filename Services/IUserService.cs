using BoardGamesStore.Models;

namespace BoardGamesStore.Services;

public interface IUserService
{
  Task<List<User>> GetAllAsync();
  Task<User?> GetByIdAsync(int id);
  Task AddAsync(User user);
  Task UpdateAsync(User user);
  Task DeleteAsync(int id);
}
