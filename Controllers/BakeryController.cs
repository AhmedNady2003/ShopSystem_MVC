using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.EntityFrameworkCore;
using ShopWebMVC.Context;
using ShopWebMVC.Models;
using System.Linq.Expressions;

namespace ShopWebMVC.Controllers
{
    public class BakeryController : Controller
    {
        private readonly ShopDbContext _context;

        public BakeryController(ShopDbContext context)
        {
            _context = context;
        }
        
        // GET: Bakery
        public async Task<IActionResult> Index()
        {
            return View(await _context.Bakeries.ToListAsync());
        }

        // GET: Bakery/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var bakery = await _context.Bakeries
                .Include(b => b.Cards) // Include associated cards
                .FirstOrDefaultAsync(m => m.Id == id);

            if (bakery == null)
                return NotFound();

            return View(bakery);
        }

        // GET: Bakery/Create
        public async Task<IActionResult> Create()
        {
            int maxId = await _context.Bakeries.MaxAsync(b => (int?)b.Id) ?? 0;


            ViewBag.NextId = maxId + 1;
            return View();
        }

        // POST: Bakery/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( Bakery bakery)
        {
            try
            {
                
                _context.Add(bakery);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch 
            {
                return View(bakery);
            }
            
            
        }

        // GET: Bakery/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var bakery = await _context.Bakeries.FindAsync(id);
            if (bakery == null)
                return NotFound();

            return View(bakery);
        }

        // POST: Bakery/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address")] Bakery bakery)
        {
            if (id != bakery.Id)
                return NotFound();
            try
            {
                try
                {
                    _context.Update(bakery);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BakeryExists(bakery.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(bakery);
            }

           
        }

        // GET: Bakery/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var bakery = await _context.Bakeries
                .FirstOrDefaultAsync(m => m.Id == id);

            if (bakery == null)
                return NotFound();

            return View(bakery);
        }

        // POST: Bakery/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bakery = await _context.Bakeries.FindAsync(id);
            if (bakery != null)
            {
                _context.Bakeries.Remove(bakery);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BakeryExists(int id)
        {
            return _context.Bakeries.Any(e => e.Id == id);
        }
    }
}
