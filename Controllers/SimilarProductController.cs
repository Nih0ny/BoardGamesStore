// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.Rendering;
// using Microsoft.EntityFrameworkCore;
// using BoardGamesStore.Data;
// using BoardGamesStore.Models;

// namespace BoardGamesStore.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class SimilarProductController(ApplicationDbContext context) : Controller
//     {
//         private readonly ApplicationDbContext _context = context;

//         // GET: SimilarProduct
//         public async Task<IActionResult> Index()
//         {
//             var applicationDbContext = _context.SimilarProducts.Include(s => s.Product).Include(s => s.SimilarTo);
//             return View(await applicationDbContext.ToListAsync());
//         }

//         // GET: SimilarProduct/Details/5
//         public async Task<IActionResult> Details(int? id)
//         {
//             if (id == null)
//             {
//                 return NotFound();
//             }

//             var similarProduct = await _context.SimilarProducts
//                 .Include(s => s.Product)
//                 .Include(s => s.SimilarTo)
//                 .FirstOrDefaultAsync(m => m.Id == id);
//             if (similarProduct == null)
//             {
//                 return NotFound();
//             }

//             return View(similarProduct);
//         }

//         // GET: SimilarProduct/Create
//         public IActionResult Create()
//         {
//             ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id");
//             ViewData["SimilarProductId"] = new SelectList(_context.Products, "Id", "Id");
//             return View();
//         }

//         // POST: SimilarProduct/Create
//         // To protect from overposting attacks, enable the specific properties you want to bind to.
//         // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Create([Bind("Id,ProductId,SimilarProductId")] SimilarProduct similarProduct)
//         {
//             if (ModelState.IsValid)
//             {
//                 _context.Add(similarProduct);
//                 await _context.SaveChangesAsync();
//                 return RedirectToAction(nameof(Index));
//             }
//             ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id", similarProduct.ProductId);
//             ViewData["SimilarProductId"] = new SelectList(_context.Products, "Id", "Id", similarProduct.SimilarProductId);
//             return View(similarProduct);
//         }

//         // GET: SimilarProduct/Edit/5
//         public async Task<IActionResult> Edit(int? id)
//         {
//             if (id == null)
//             {
//                 return NotFound();
//             }

//             var similarProduct = await _context.SimilarProducts.FindAsync(id);
//             if (similarProduct == null)
//             {
//                 return NotFound();
//             }
//             ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id", similarProduct.ProductId);
//             ViewData["SimilarProductId"] = new SelectList(_context.Products, "Id", "Id", similarProduct.SimilarProductId);
//             return View(similarProduct);
//         }

//         // POST: SimilarProduct/Edit/5
//         // To protect from overposting attacks, enable the specific properties you want to bind to.
//         // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Edit(int id, [Bind("Id,ProductId,SimilarProductId")] SimilarProduct similarProduct)
//         {
//             if (id != similarProduct.Id)
//             {
//                 return NotFound();
//             }

//             if (ModelState.IsValid)
//             {
//                 try
//                 {
//                     _context.Update(similarProduct);
//                     await _context.SaveChangesAsync();
//                 }
//                 catch (DbUpdateConcurrencyException)
//                 {
//                     if (!SimilarProductExists(similarProduct.Id))
//                     {
//                         return NotFound();
//                     }
//                     else
//                     {
//                         throw;
//                     }
//                 }
//                 return RedirectToAction(nameof(Index));
//             }
//             ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id", similarProduct.ProductId);
//             ViewData["SimilarProductId"] = new SelectList(_context.Products, "Id", "Id", similarProduct.SimilarProductId);
//             return View(similarProduct);
//         }

//         // GET: SimilarProduct/Delete/5
//         public async Task<IActionResult> Delete(int? id)
//         {
//             if (id == null)
//             {
//                 return NotFound();
//             }

//             var similarProduct = await _context.SimilarProducts
//                 .Include(s => s.Product)
//                 .Include(s => s.SimilarTo)
//                 .FirstOrDefaultAsync(m => m.Id == id);
//             if (similarProduct == null)
//             {
//                 return NotFound();
//             }

//             return View(similarProduct);
//         }

//         // POST: SimilarProduct/Delete/5
//         [HttpPost, ActionName("Delete")]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> DeleteConfirmed(int id)
//         {
//             var similarProduct = await _context.SimilarProducts.FindAsync(id);
//             if (similarProduct != null)
//             {
//                 _context.SimilarProducts.Remove(similarProduct);
//             }

//             await _context.SaveChangesAsync();
//             return RedirectToAction(nameof(Index));
//         }

//         private bool SimilarProductExists(int id)
//         {
//             return _context.SimilarProducts.Any(e => e.Id == id);
//         }
//     }
// }
