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
    public class PaymentTransactionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentTransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PaymentTransaction
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.PaymentTransactions.Include(p => p.Order);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: PaymentTransaction/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentTransaction = await _context.PaymentTransactions
                .Include(p => p.Order)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (paymentTransaction == null)
            {
                return NotFound();
            }

            return View(paymentTransaction);
        }

        // GET: PaymentTransaction/Create
        public IActionResult Create()
        {
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id");
            return View();
        }

        // POST: PaymentTransaction/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OrderId,PaymentSystem,TransactionId,Amount,Status,CreatedAt")] PaymentTransaction paymentTransaction)
        {
            if (ModelState.IsValid)
            {
                _context.Add(paymentTransaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", paymentTransaction.OrderId);
            return View(paymentTransaction);
        }

        // GET: PaymentTransaction/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentTransaction = await _context.PaymentTransactions.FindAsync(id);
            if (paymentTransaction == null)
            {
                return NotFound();
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", paymentTransaction.OrderId);
            return View(paymentTransaction);
        }

        // POST: PaymentTransaction/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrderId,PaymentSystem,TransactionId,Amount,Status,CreatedAt")] PaymentTransaction paymentTransaction)
        {
            if (id != paymentTransaction.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(paymentTransaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentTransactionExists(paymentTransaction.Id))
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
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", paymentTransaction.OrderId);
            return View(paymentTransaction);
        }

        // GET: PaymentTransaction/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentTransaction = await _context.PaymentTransactions
                .Include(p => p.Order)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (paymentTransaction == null)
            {
                return NotFound();
            }

            return View(paymentTransaction);
        }

        // POST: PaymentTransaction/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var paymentTransaction = await _context.PaymentTransactions.FindAsync(id);
            if (paymentTransaction != null)
            {
                _context.PaymentTransactions.Remove(paymentTransaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentTransactionExists(int id)
        {
            return _context.PaymentTransactions.Any(e => e.Id == id);
        }
    }
}
