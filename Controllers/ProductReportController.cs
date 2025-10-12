using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BoardGamesStore.Data;
using BoardGamesStore.Models;

namespace BoardGamesStore.Controllers
{
    public class ProductReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ProductReport
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ProductReports.Include(p => p.Product).Include(p => p.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ProductReport/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productReport = await _context.ProductReports
                .Include(p => p.Product)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productReport == null)
            {
                return NotFound();
            }

            return View(productReport);
        }

        // GET: ProductReport/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: ProductReport/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,ProductId,Reason,Status,CreatedAt")] ProductReport productReport)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productReport);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id", productReport.ProductId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", productReport.UserId);
            return View(productReport);
        }

        // GET: ProductReport/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productReport = await _context.ProductReports.FindAsync(id);
            if (productReport == null)
            {
                return NotFound();
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id", productReport.ProductId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", productReport.UserId);
            return View(productReport);
        }

        // POST: ProductReport/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,ProductId,Reason,Status,CreatedAt")] ProductReport productReport)
        {
            if (id != productReport.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productReport);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductReportExists(productReport.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id", productReport.ProductId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", productReport.UserId);
            return View(productReport);
        }

        // GET: ProductReport/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productReport = await _context.ProductReports
                .Include(p => p.Product)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productReport == null)
            {
                return NotFound();
            }

            return View(productReport);
        }

        // POST: ProductReport/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productReport = await _context.ProductReports.FindAsync(id);
            if (productReport != null)
            {
                _context.ProductReports.Remove(productReport);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductReportExists(int id)
        {
            return _context.ProductReports.Any(e => e.Id == id);
        }
    }
}
