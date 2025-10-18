using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BoardGamesStore.Data;
using BoardGamesStore.Models;
using BoardGamesStore.Services;

namespace BoardGamesStore.Controllers
{
    // public class OrderController : Controller
    // {
    //     private readonly ApplicationDbContext _context;

    //     public OrderController(ApplicationDbContext context)
    //     {
    //         _context = context;
    //     }

    //     // GET: Order
    //     public async Task<IActionResult> Index()
    //     {
    //         var applicationDbContext = _context.Orders.Include(o => o.Status).Include(o => o.User);
    //         return View(await applicationDbContext.ToListAsync());
    //     }

    //     // GET: Order/Details/5
    //     public async Task<IActionResult> Details(int? id)
    //     {
    //         if (id == null)
    //         {
    //             return NotFound();
    //         }

    //         var order = await _context.Orders
    //             .Include(o => o.Status)
    //             .Include(o => o.User)
    //             .FirstOrDefaultAsync(m => m.Id == id);
    //         if (order == null)
    //         {
    //             return NotFound();
    //         }

    //         return View(order);
    //     }

    //     // GET: Order/Create
    //     public IActionResult Create()
    //     {
    //         ViewData["StatusId"] = new SelectList(_context.OrderStatuses, "Id", "Id");
    //         ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
    //         return View();
    //     }

    //     // POST: Order/Create
    //     // To protect from overposting attacks, enable the specific properties you want to bind to.
    //     // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    //     [HttpPost]
    //     [ValidateAntiForgeryToken]
    //     public async Task<IActionResult> Create([Bind("Id,UserId,StatusId,Total,CreatedAt,UpdatedAt")] Order order)
    //     {
    //         if (ModelState.IsValid)
    //         {
    //             _context.Add(order);
    //             await _context.SaveChangesAsync();
    //             return RedirectToAction(nameof(Index));
    //         }
    //         ViewData["StatusId"] = new SelectList(_context.OrderStatuses, "Id", "Id", order.StatusId);
    //         ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
    //         return View(order);
    //     }

    //     // GET: Order/Edit/5
    //     public async Task<IActionResult> Edit(int? id)
    //     {
    //         if (id == null)
    //         {
    //             return NotFound();
    //         }

    //         var order = await _context.Orders.FindAsync(id);
    //         if (order == null)
    //         {
    //             return NotFound();
    //         }
    //         ViewData["StatusId"] = new SelectList(_context.OrderStatuses, "Id", "Id", order.StatusId);
    //         ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
    //         return View(order);
    //     }

    //     // POST: Order/Edit/5
    //     // To protect from overposting attacks, enable the specific properties you want to bind to.
    //     // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    //     [HttpPost]
    //     [ValidateAntiForgeryToken]
    //     public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,StatusId,Total,CreatedAt,UpdatedAt")] Order order)
    //     {
    //         if (id != order.Id)
    //         {
    //             return NotFound();
    //         }

    //         if (ModelState.IsValid)
    //         {
    //             try
    //             {
    //                 _context.Update(order);
    //                 await _context.SaveChangesAsync();
    //             }
    //             catch (DbUpdateConcurrencyException)
    //             {
    //                 if (!OrderExists(order.Id))
    //                 {
    //                     return NotFound();
    //                 }
    //                 else
    //                 {
    //                     throw;
    //                 }
    //             }
    //             return RedirectToAction(nameof(Index));
    //         }
    //         ViewData["StatusId"] = new SelectList(_context.OrderStatuses, "Id", "Id", order.StatusId);
    //         ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
    //         return View(order);
    //     }

    //     // GET: Order/Delete/5
    //     public async Task<IActionResult> Delete(int? id)
    //     {
    //         if (id == null)
    //         {
    //             return NotFound();
    //         }

    //         var order = await _context.Orders
    //             .Include(o => o.Status)
    //             .Include(o => o.User)
    //             .FirstOrDefaultAsync(m => m.Id == id);
    //         if (order == null)
    //         {
    //             return NotFound();
    //         }

    //         return View(order);
    //     }

    //     // POST: Order/Delete/5
    //     [HttpPost, ActionName("Delete")]
    //     [ValidateAntiForgeryToken]
    //     public async Task<IActionResult> DeleteConfirmed(int id)
    //     {
    //         var order = await _context.Orders.FindAsync(id);
    //         if (order != null)
    //         {
    //             _context.Orders.Remove(order);
    //         }

    //         await _context.SaveChangesAsync();
    //         return RedirectToAction(nameof(Index));
    //     }

    //     private bool OrderExists(int id)
    //     {
    //         return _context.Orders.Any(e => e.Id == id);
    //     }
    // }

    [Route("api/[controller]")]
    public class OrderController : Controller
    {
        private readonly IGenericService<Order> _orderService;
        private readonly ApplicationDbContext _context;

        public OrderController(IGenericService<Order> orderService, ApplicationDbContext context)
        {
            _orderService = orderService;
            _context = context;
        }

        // GET /order
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderService.GetAllAsync(q => q
                .Include(o => o.Status)
                .Include(o => o.User)
                .Include(o => o.OrderItems));

            return Ok(orders);
        }

        // GET api/orders/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Status)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            return Ok(order);
        }

        // POST api/orders
        [HttpPost]
        public async Task<IActionResult> Create(Order order)
        {
            await _orderService.AddAsync(order);
            return Ok(order);
        }

        // PUT api/orders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Order order)
        {
            if (id != order.Id) return BadRequest();
            await _orderService.UpdateAsync(order);
            return Ok(order);
        }

        // DELETE api/orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _orderService.DeleteAsync(id);
            return NoContent();
        }
    }
}
