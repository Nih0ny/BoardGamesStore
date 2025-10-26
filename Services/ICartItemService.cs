using BoardGamesStore.Models;

namespace BoardGamesStore.Services
{
    public interface ICartItemService
    {
        Task<IReadOnlyList<CartItem>> GetItemsAsync(int cartId, CancellationToken ct = default);
        Task<CartItem> AddItemAsync(int cartId, int productId, int quantity, CancellationToken ct = default);
        Task<bool> UpdateQuantityAsync(int cartItemId, int quantity, CancellationToken ct = default);
        Task<bool> RemoveItemAsync(int cartItemId, CancellationToken ct = default);
        Task<bool> ClearAsync(int cartId, CancellationToken ct = default);
        Task<int> GetItemCountAsync(int cartId, CancellationToken ct = default);
    }
}
