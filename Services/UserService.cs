namespace BoardGamesStore.Services;

using BoardGamesStore.Data;
using BoardGamesStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


public class UserService : IUserService
{
  private readonly ApplicationDbContext _db;
  private readonly UserManager<User> _userManager;

  public UserService(ApplicationDbContext db, UserManager<User> userManager)
  {
    _db = db;
    _userManager = userManager;
  }

  public async Task<List<User>> GetAllAsync() => await _db.Users.ToListAsync();

  public async Task<User?> GetByIdAsync(int id) => await _db.Users.FindAsync(id);

  public async Task AddAsync(User user)
  {
    await _userManager.CreateAsync(user, "TempPassword123"); // TODO: Replace with actual password handling
  }

  public async Task UpdateAsync(User user)
  {
    _db.Users.Update(user);
    await _db.SaveChangesAsync();
  }

  public async Task DeleteAsync(int id)
  {
    var user = await _db.Users.FindAsync(id);
    if (user != null)
    {
      await _userManager.DeleteAsync(user);
    }
  }

  public async Task AddCoinsAsync(int userId, decimal amount)
  {
    var user = await _db.Users.FindAsync(userId);
    if (user != null)
    {
      user.Coins += amount;
      await _db.SaveChangesAsync();
    }
  }

  public async Task RemoveCoinsAsync(int userId, decimal amount)
  {
    var user = await _db.Users.FindAsync(userId);
    if (user != null)
    {
      user.Coins -= amount;
      await _db.SaveChangesAsync();
    }
  }
}
