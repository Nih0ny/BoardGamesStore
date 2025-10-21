using BoardGamesStore.Models;

namespace BoardGamesStore.Services
{
    public interface IOrderItemService
    {
        Task<IReadOnlyList<OrderItem>> GetByOrderAsync(int orderId, CancellationToken ct = default);
        Task<OrderItem> AddOrUpdateAsync(int orderId, int productId, int quantity, decimal price, CancellationToken ct = default);
        Task<bool> UpdateQuantityAsync(int orderItemId, int quantity, CancellationToken ct = default);
        Task<bool> RemoveAsync(int orderItemId, CancellationToken ct = default);
        Task<bool> ClearAsync(int orderId, CancellationToken ct = default);
    }
}
