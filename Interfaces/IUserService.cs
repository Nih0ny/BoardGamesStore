using BoardGamesStore.Models;
using FluentResults;

namespace BoardGamesStore.Interfaces;

public interface IUserService
{
  Task<PagedResult<UserDto>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 20, CancellationToken ct = default);
  Task<UserDto?> GetUserByIdAsync(string userId, CancellationToken ct = default);
  Task<Result> UploadAvatarAsync(string userId, IFormFile file, CancellationToken ct = default);
  Task<Result> DeleteAvatarAsync(string userId, CancellationToken ct = default);
}