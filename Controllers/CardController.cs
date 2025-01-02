using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopWebMVC.Context;
using ShopWebMVC.Models;

namespace ShopWebMVC.Controllers
{
    
    public class CardController : Controller
    {
        private readonly ShopDbContext _context;


        public CardController(ShopDbContext context)
        {
            _context = context;

        }
        // GET: Card
        public async Task<IActionResult> Index()
        {
            var cards = await _context.Cards
                .Include(c => c.Bakery)
                .Include(c => c.Debt)
                .ToListAsync();
            return View(cards);
        }


        // GET: Card/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var card = await _context.Cards
                .Include(c => c.Bakery)
                .Include(c => c.Debt)
                .Include(c => c.Invoices)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (card == null)
            {
                return NotFound();
            }

            return View(card);
        }

        // GET: Card/Create
        public async Task<IActionResult> Create()
        {
            int maxId = await _context.Cards.MaxAsync(b => (int?)b.Id) ?? 0;


            ViewBag.NextId = maxId + 1;
            ViewData["BakeryId"] = new SelectList(_context.Bakeries, "Id", "Name");
            return View();
        }

        // POST: Card/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Card card)
        {
            ViewData["BakeryId"] = new SelectList(_context.Bakeries, "Id", "Name", card.BakeryId);

            try
            {
                _context.Add(card);
                await _context.SaveChangesAsync();
                var newDebt = new Debt
                {
                    CardId = card.Id,   // ربط الدين بالبطاقة الجديدة
                    MonetaryDebt = 0,   // قيمة الدين المالية الافتراضية (يمكنك تخصيصها حسب الحاجة)
                    IsSettled = false,  // تعيين الحالة الافتراضية للدين
                    DebtItems = new List<DebtItem>() // إنشاء قائمة فارغة للعناصر
                };

                _context.Debts.Add(newDebt);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(card);
            }

            
        }

        // GET: Card/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var card = await _context.Cards.FindAsync(id);
            if (card == null)
            {
                return NotFound();
            }
            ViewData["BakeryId"] = new SelectList(_context.Bakeries, "Id", "Name", card.BakeryId);
            return View(card);
        }

        // POST: Card/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,  Card card)
        {

            if (id != card.Id)
            {
                return BadRequest();
            }
            try
            {
                try
                {
                    ViewData["BakeryId"] = new SelectList(_context.Bakeries, "Id", "Name", card.BakeryId);

                    _context.Update(card);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CardExists(card.Id))
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
            catch
            {
                return View(card);
            }

            
        }

        // GET: Card/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var card = await _context.Cards
                .Include(c => c.Bakery)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (card == null)
            {
                return NotFound();
            }

            return View(card);
        }

        // POST: Card/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var card = await _context.Cards.FindAsync(id);
            _context.Cards.Remove(card);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CardExists(int id)
        {
            return _context.Cards.Any(e => e.Id == id);
        }

        public async Task<IActionResult> ViewInvoices(int id)
        {
            var card = await _context.Cards
                .Include(c => c.Debt)
                .Include(c => c.Invoices)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (card == null)
            {
                return NotFound();
            }
           

            return View(card);
        }
        






    }
}
