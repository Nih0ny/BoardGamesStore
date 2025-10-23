using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BoardGamesStore.Services;

namespace BoardGamesStore.Controllers
{
    [Route("Cart/{cartId:int}/Items")]
    public class CartItemController : Controller
    {
        private readonly ICartItemService _items;

        public CartItemController(ICartItemService items)
        {
            _items = items;
        }

        // POST: /Cart/{cartId}/Items/Add
        [HttpPost("Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int cartId, int productId, int quantity = 1)
        {
            await _items.AddItemAsync(cartId, productId, quantity);
            return RedirectToAction("Details", "Cart", new { id = cartId });
        }

        // POST: /Cart/{cartId}/Items/{cartItemId}/Update
        [HttpPost("{cartItemId:int}/Update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int cartId, int cartItemId, int quantity)
        {
            var ok = await _items.UpdateQuantityAsync(cartItemId, quantity);
            if (!ok) return NotFound();

            return RedirectToAction("Details", "Cart", new { id = cartId });
        }

        // POST: /Cart/{cartId}/Items/{cartItemId}/Remove
        [HttpPost("{cartItemId:int}/Remove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartId, int cartItemId)
        {
            var ok = await _items.RemoveItemAsync(cartItemId);
            if (!ok) return NotFound();

            return RedirectToAction("Details", "Cart", new { id = cartId });
        }

        // POST: /Cart/{cartId}/Items/Clear
        [HttpPost("Clear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear(int cartId)
        {
            await _items.ClearAsync(cartId);
            return RedirectToAction("Details", "Cart", new { id = cartId });
        }
    }
}
