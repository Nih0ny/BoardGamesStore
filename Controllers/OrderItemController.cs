using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BoardGamesStore.Data;
using BoardGamesStore.Models;
using BoardGamesStore.Services;

namespace BoardGamesStore.Controllers
{
    public class OrderItemController : Controller
    {
        private readonly IOrderItemService _orderItems;
        private readonly ApplicationDbContext _db; // for dropdowns

        public OrderItemController(IOrderItemService orderItems, ApplicationDbContext db)
        {
            _orderItems = orderItems;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            // show all items with order & product
            var list = await _db.OrderItems.Include(o => o.Order).Include(o => o.Product).AsNoTracking().ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var item = await _db.OrderItems.Include(o => o.Order).Include(o => o.Product)
                                           .AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            return item == null ? NotFound() : View(item);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["OrderId"]   = new SelectList(_db.Orders, "Id", "Id");
            ViewData["ProductId"] = new SelectList(_db.Products, "Id", "Id");
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,ProductId,Quantity,Price")] OrderItem vm)
        {
            if (!ModelState.IsValid)
            {
                ViewData["OrderId"]   = new SelectList(_db.Orders, "Id", "Id", vm.OrderId);
                ViewData["ProductId"] = new SelectList(_db.Products, "Id", "Id", vm.ProductId);
                return View(vm);
            }

            await _orderItems.AddOrUpdateAsync(vm.OrderId, vm.ProductId, vm.Quantity, vm.Price);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _db.OrderItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (item == null) return NotFound();

            ViewData["OrderId"]   = new SelectList(_db.Orders, "Id", "Id", item.OrderId);
            ViewData["ProductId"] = new SelectList(_db.Products, "Id", "Id", item.ProductId);
            return View(item);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrderId,ProductId,Quantity,Price")] OrderItem vm)
        {
            if (id != vm.Id) return NotFound();
            if (!ModelState.IsValid)
            {
                ViewData["OrderId"]   = new SelectList(_db.Orders, "Id", "Id", vm.OrderId);
                ViewData["ProductId"] = new SelectList(_db.Products, "Id", "Id", vm.ProductId);
                return View(vm);
            }

            var ok = await _orderItems.UpdateQuantityAsync(vm.Id, vm.Quantity);
            if (!ok) return NotFound();

            // price change via AddOrUpdate if you want to allow editors to reset price:
            // await _orderItems.AddOrUpdateAsync(vm.OrderId, vm.ProductId, vm.Quantity, vm.Price);

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var item = await _db.OrderItems.Include(o => o.Order).Include(o => o.Product)
                                           .AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            return item == null ? NotFound() : View(item);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _orderItems.RemoveAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
