using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BoardGamesStore.Models;
using BoardGamesStore.Services; // IProductService
using Microsoft.EntityFrameworkCore;       // only if you keep any EF helpers (not strictly needed)

namespace BoardGamesStore.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _products;

        public ProductController(IProductService products)
        {
            _products = products;
        }

        // GET: Product
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var list = await _products.GetAllAsync();
            return View(list);
        }

        // GET: Product/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null) return NotFound();

            var product = await _products.GetByIdAsync(id.Value);
            if (product is null) return NotFound();

            return View(product);
        }

        // GET: Product/Create  (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Product/Create  (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Price,Stock,Category,ImageUrl,BonusRate,MaxBonusPaymentPercent")] Product product)
        {
            if (!ModelState.IsValid) return View(product);

            await _products.CreateAsync(product);
            return RedirectToAction(nameof(Index));
        }

        // GET: Product/Edit/5  (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null) return NotFound();

            var product = await _products.GetByIdAsync(id.Value);
            if (product is null) return NotFound();

            return View(product);
        }

        // POST: Product/Edit/5  (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,Stock,Category,ImageUrl,BonusRate,MaxBonusPaymentPercent,CreatedAt")] Product product)
        {
            if (id != product.Id) return NotFound();
            if (!ModelState.IsValid) return View(product);

            var ok = await _products.UpdateAsync(product);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(Index));
        }

        // GET: Product/Delete/5  (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return NotFound();

            var product = await _products.GetByIdAsync(id.Value);
            if (product is null) return NotFound();

            return View(product);
        }

        // POST: Product/Delete/5  (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _products.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // If you still need this helper:
        private Task<bool> ProductExists(int id) => _products.ExistsAsync(id);
    }
}
