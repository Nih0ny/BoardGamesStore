using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BoardGamesStore.Models;
using BoardGamesStore.Services;
using BoardGamesStore.Data;               // only for the SelectList (users dropdown)
using Microsoft.EntityFrameworkCore;     // only for users dropdown
using Microsoft.AspNetCore.Authorization;

namespace BoardGamesStore.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _carts;
        private readonly ICartItemService _items;
        private readonly ApplicationDbContext _db; // for Users dropdown in Create/Edit

        public CartController(ICartService carts, ICartItemService items, ApplicationDbContext db)
        {
            _carts = carts;
            _items = items;
            _db = db;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var list = await _carts.GetAllAsync();
            return View(list);
        }

        // GET: /Cart/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null) return NotFound();

            var cart = await _carts.GetByIdAsync(id.Value);
            if (cart is null) return NotFound();

            // Load items for this cart
            ViewData["Items"] = await _items.GetItemsAsync(cart.Id);
            return View(cart);
        }

        // GET: /Cart/Create
        // (If you want this for Admin only, add: [Authorize(Roles = "Admin")])
        public async Task<IActionResult> Create()
        {
            // Identity UserId is a string
            var users = await _db.Users.AsNoTracking().ToListAsync();
            ViewData["UserId"] = new SelectList(users, "Id", "Id");
            return View();
        }

        // POST: /Cart/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId")] Cart cart)
        {
            if (!ModelState.IsValid)
            {
                var users = await _db.Users.AsNoTracking().ToListAsync();
                ViewData["UserId"] = new SelectList(users, "Id", "Id", cart.UserId);
                return View(cart);
            }

            // CartService handles timestamps & required navs
            var created = await _carts.CreateAsync(cart.UserId);
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }

        // GET: /Cart/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null) return NotFound();

            var cart = await _carts.GetByIdAsync(id.Value);
            if (cart is null) return NotFound();

            var users = await _db.Users.AsNoTracking().ToListAsync();
            ViewData["UserId"] = new SelectList(users, "Id", "Id", cart.UserId);
            return View(cart);
        }

        // POST: /Cart/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId")] Cart cart)
        {
            if (id != cart.Id) return NotFound();
            if (!ModelState.IsValid)
            {
                var users = await _db.Users.AsNoTracking().ToListAsync();
                ViewData["UserId"] = new SelectList(users, "Id", "Id", cart.UserId);
                return View(cart);
            }

            var ok = await _carts.UpdateAsync(cart);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize] // user must be logged in
        [HttpPost("Cart/Pay/{cartId:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int cartId)
        {
            var cart = await _carts.GetByIdAsync(cartId);
            if (cart == null)
                return NotFound();

            var items = await _items.GetItemsAsync(cartId);
            if (items == null || items.Count == 0)
                return BadRequest("Cart is empty.");

            // Compute total
            var total = items.Sum(i => i.Product.Price * i.Quantity);

            // TODO: plug in payment gateway here
            
            await _items.ClearAsync(cartId);

            TempData["Message"] = $"Payment successful. Total paid: {total:C2}";
            return RedirectToAction(nameof(Details), new { id = cartId });
        }

        // ==== NO CART DELETE ====

        [NonAction]
        public Task<IActionResult> Delete(int? id) => Task.FromResult<IActionResult>(BadRequest("Cart deletion is disabled."));
        [NonAction]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> DeleteConfirmed(int id) => Task.FromResult<IActionResult>(BadRequest("Cart deletion is disabled."));
    }
}
