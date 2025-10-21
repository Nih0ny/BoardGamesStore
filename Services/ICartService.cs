using BoardGamesStore.Models;

namespace BoardGamesStore.Services
{
    public interface ICartService
{
    Task<List<Cart>> GetAllAsync(CancellationToken ct = default);
    Task<Cart?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Cart> CreateAsync(string userId, DateTime? createdAt = null, CancellationToken ct = default); // string!
    Task<bool> UpdateAsync(Cart cart, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
}
}
