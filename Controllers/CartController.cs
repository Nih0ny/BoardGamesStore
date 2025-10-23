using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BoardGamesStore.Models;
using BoardGamesStore.Services;

namespace BoardGamesStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _carts;
        private readonly ICartItemService _items;

        public CartController(ICartService carts, ICartItemService items)
        {
            _carts = carts;
            _items = items;
        }

        // // MVC View:
        // public async Task<IActionResult> Index() => View(await _carts.GetAllAsync());

        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _carts.GetAllAsync();
            return Ok(list);
        }

        // // MVC View:
        // public async Task<IActionResult> Details(int? id) => View(cart);

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cart = await _carts.GetByIdAsync(id);
            if (cart is null) return NotFound();
            var items = await _items.GetItemsAsync(id);
            return Ok(new { cart, items });
        }

        // // MVC View (GET):
        // public IActionResult Create() => View();

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] CartCreateDto body)
        {
            // body: { "userId": "<string>" }
            var created = await _carts.CreateAsync(body.UserId);
            return Ok(created);
        }

        public record CartCreateDto(string UserId);

        // // MVC View (GET):
        // public async Task<IActionResult> Edit(int? id) => View(cart);

        [HttpPut]
        [Route("{id:int}/update")]
        public async Task<IActionResult> Update(int id, [FromBody] Cart dto)
        {
            if (id != dto.Id) return BadRequest("Mismatched id.");
            var ok = await _carts.UpdateAsync(dto);
            return ok ? Ok(dto) : NotFound();
        }

        // ==== Cart deletion disabled by requirement ====
        // [HttpDelete("{id:int}/delete")] -> intentionally omitted

        // PAY: compute total, (TODO: call payment), clear items
        [Authorize]
        [HttpPost]
        [Route("pay/{cartId:int}")]
        public async Task<IActionResult> Pay(int cartId)
        {
            var cart = await _carts.GetByIdAsync(cartId);
            if (cart is null) return NotFound();

            var items = await _items.GetItemsAsync(cartId);
            if (items.Count == 0) return BadRequest("Cart is empty.");

            var total = items.Sum(i => i.Product.Price * i.Quantity);

            // TODO: integrate payment gateway here
            // if (!paymentSuccess) return BadRequest("Payment failed.");

            await _items.ClearAsync(cartId);
            return Ok(new { cartId, total, message = "Payment simulated and cart cleared." });
        }
    }
}
