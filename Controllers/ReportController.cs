using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopWebMVC.Context;

namespace ShopWebMVC.Controllers
{
    public class ReportController : Controller
    {
        private readonly ShopDbContext _context;
        public ReportController(ShopDbContext context)
        {
            _context = context;
        }
        // GET: ReportController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ReportController/BakeriesReport
        public async Task<IActionResult> BakeriesReport()
        {
            return View(await _context.Bakeries.ToListAsync());
        }
        public async Task<IActionResult> CardsNotStruck(int? id)
        {
             var cards = await _context.Cards
     .Include(c => c.Bakery)
     .Where(c => id == null ? c.BakeryId == null : c.BakeryId == id)
     .ToListAsync(); // جلب البيانات من قاعدة البيانات


            cards = cards
                .AsQueryable() // تحويل النتائج إلى العميل
                .Where(c => c.HasInvoice && !c.IsStruck) // فلترة على العميل
                .ToList(); // استخدام ToList هنا

            return View(cards);
        }

        public async Task<IActionResult> CardsNotSpent(int? id)
        {
            var cards = await _context.Cards
    .Include(c => c.Bakery)
    .Where(c => id == null ? c.BakeryId == null : c.BakeryId == id)
    .ToListAsync(); // جلب البيانات من قاعدة البيانات

            cards = cards
                .AsEnumerable() // تحويل النتائج إلى العميل
                .Where(c => !c.HasInvoice && c.IsStruck) // فلترة على العميل
                .ToList(); // استخدام ToList هنا

            return View(cards);
        }



        // GET: ReportController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ReportController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ReportController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ReportController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ReportController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ReportController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
