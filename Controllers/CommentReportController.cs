// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.Rendering;
// using Microsoft.EntityFrameworkCore;
// using BoardGamesStore.Data;
// using BoardGamesStore.Models;
// using BoardGamesStore.Models.Entities;

// namespace BoardGamesStore.Controllers
// {
//     public class CommentReportController : Controller
//     {
//         private readonly ApplicationDbContext _context;

//         public CommentReportController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         // GET: CommentReport
//         public async Task<IActionResult> Index()
//         {
//             var applicationDbContext = _context.CommentReports.Include(c => c.Comment).Include(c => c.User);
//             return View(await applicationDbContext.ToListAsync());
//         }

//         // GET: CommentReport/Details/5
//         public async Task<IActionResult> Details(int? id)
//         {
//             if (id == null)
//             {
//                 return NotFound();
//             }

//             var commentReport = await _context.CommentReports
//                 .Include(c => c.Comment)
//                 .Include(c => c.User)
//                 .FirstOrDefaultAsync(m => m.Id == id);
//             if (commentReport == null)
//             {
//                 return NotFound();
//             }

//             return View(commentReport);
//         }

//         // GET: CommentReport/Create
//         public IActionResult Create()
//         {
//             ViewData["CommentId"] = new SelectList(_context.Comments, "Id", "Id");
//             ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
//             return View();
//         }

//         // POST: CommentReport/Create
//         // To protect from overposting attacks, enable the specific properties you want to bind to.
//         // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Create([Bind("Id,UserId,CommentId,Reason,Status,CreatedAt")] CommentReport commentReport)
//         {
//             if (ModelState.IsValid)
//             {
//                 _context.Add(commentReport);
//                 await _context.SaveChangesAsync();
//                 return RedirectToAction(nameof(Index));
//             }
//             ViewData["CommentId"] = new SelectList(_context.Comments, "Id", "Id", commentReport.CommentId);
//             ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", commentReport.UserId);
//             return View(commentReport);
//         }

//         // GET: CommentReport/Edit/5
//         public async Task<IActionResult> Edit(int? id)
//         {
//             if (id == null)
//             {
//                 return NotFound();
//             }

//             var commentReport = await _context.CommentReports.FindAsync(id);
//             if (commentReport == null)
//             {
//                 return NotFound();
//             }
//             ViewData["CommentId"] = new SelectList(_context.Comments, "Id", "Id", commentReport.CommentId);
//             ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", commentReport.UserId);
//             return View(commentReport);
//         }

//         // POST: CommentReport/Edit/5
//         // To protect from overposting attacks, enable the specific properties you want to bind to.
//         // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,CommentId,Reason,Status,CreatedAt")] CommentReport commentReport)
//         {
//             if (id != commentReport.Id)
//             {
//                 return NotFound();
//             }

//             if (ModelState.IsValid)
//             {
//                 try
//                 {
//                     _context.Update(commentReport);
//                     await _context.SaveChangesAsync();
//                 }
//                 catch (DbUpdateConcurrencyException)
//                 {
//                     if (!CommentReportExists(commentReport.Id))
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
//             ViewData["CommentId"] = new SelectList(_context.Comments, "Id", "Id", commentReport.CommentId);
//             ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", commentReport.UserId);
//             return View(commentReport);
//         }

//         // GET: CommentReport/Delete/5
//         public async Task<IActionResult> Delete(int? id)
//         {
//             if (id == null)
//             {
//                 return NotFound();
//             }

//             var commentReport = await _context.CommentReports
//                 .Include(c => c.Comment)
//                 .Include(c => c.User)
//                 .FirstOrDefaultAsync(m => m.Id == id);
//             if (commentReport == null)
//             {
//                 return NotFound();
//             }

//             return View(commentReport);
//         }

//         // POST: CommentReport/Delete/5
//         [HttpPost, ActionName("Delete")]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> DeleteConfirmed(int id)
//         {
//             var commentReport = await _context.CommentReports.FindAsync(id);
//             if (commentReport != null)
//             {
//                 _context.CommentReports.Remove(commentReport);
//             }

//             await _context.SaveChangesAsync();
//             return RedirectToAction(nameof(Index));
//         }

//         private bool CommentReportExists(int id)
//         {
//             return _context.CommentReports.Any(e => e.Id == id);
//         }
//     }
// }
