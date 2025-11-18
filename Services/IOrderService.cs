using BoardGamesStore.Models;
using Microsoft.EntityFrameworkCore.Query;

namespace BoardGamesStore.Services
{
    public interface IOrderService
    {
        Task<List<Order>> GetAllAsync(
            Func<IQueryable<Order>, IIncludableQueryable<Order, object>>? include = null,
            CancellationToken ct = default);

        Task<Order?> GetByIdAsync(
            int id,
            Func<IQueryable<Order>, IIncludableQueryable<Order, object>>? include = null,
            CancellationToken ct = default);

        Task<Order> CreateAsync(string userId, int statusId, decimal total, CancellationToken ct = default);

        Task<bool> UpdateAsync(Order order, CancellationToken ct = default);

        Task<bool> DeleteAsync(int id, CancellationToken ct = default);

        Task<bool> ExistsAsync(int id, CancellationToken ct = default);

        Task<decimal> RecalculateTotalAsync(int orderId, bool save = true, CancellationToken ct = default);

        Task<bool> ChangeStatusAsync(int orderId, int statusId, CancellationToken ct = default);
    }
}
