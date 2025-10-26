using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BoardGamesStore.Services;

namespace BoardGamesStore.Controllers
{
    [ApiController]
    [Route("api/cart/{cartId:int}/items")]
    public class CartItemController : ControllerBase
    {
        private readonly ICartItemService _items;
        public CartItemController(ICartItemService items) => _items = items;

        // // MVC View:
        // public async Task<IActionResult> Index() => View(...)

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> List(int cartId)
        {
            var list = await _items.GetItemsAsync(cartId);
            return Ok(list);
        }

        // // MVC View (GET):
        // public IActionResult Create() => View();

        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> Add(int cartId, [FromBody] AddDto body)
        {
            // body: { "productId": 1, "quantity": 2 }
            var item = await _items.AddItemAsync(cartId, body.ProductId, body.Quantity);
            return Ok(item);
        }
        public record AddDto(int ProductId, int Quantity);

        // // MVC View (GET):
        // public async Task<IActionResult> Edit(int? id) => View(cartItem);

        [HttpPut]
        [Route("{cartItemId:int}/update")]
        public async Task<IActionResult> Update(int cartId, int cartItemId, [FromBody] UpdateDto body)
        {
            var ok = await _items.UpdateQuantityAsync(cartItemId, body.Quantity);
            return ok ? NoContent() : NotFound();
        }
        public record UpdateDto(int Quantity);

        // // MVC View:
        // public async Task<IActionResult> Delete(int? id) => View(cartItem);

        [HttpDelete]
        [Route("{cartItemId:int}/delete")]
        public async Task<IActionResult> Remove(int cartId, int cartItemId)
        {
            var ok = await _items.RemoveItemAsync(cartItemId);
            return ok ? NoContent() : NotFound();
        }

        [HttpPost]
        [Route("clear")]
        public async Task<IActionResult> Clear(int cartId)
        {
            await _items.ClearAsync(cartId);
            return NoContent();
        }
    }
}
