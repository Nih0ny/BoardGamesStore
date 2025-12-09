using BoardGamesStore.Models;
using FluentResults;

namespace BoardGamesStore.Interfaces;

public interface IUserService
{
  Task<PagedResult<UserDto>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 20, CancellationToken ct = default);
  Task<UserDto?> GetUserByIdAsync(string userId, CancellationToken ct = default);
}