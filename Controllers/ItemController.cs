using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopWebMVC.Context;
using ShopWebMVC.Models;

namespace ShopWebMVC.Controllers
{
    public class ItemController : Controller
    {
        private readonly ShopDbContext _context;
        public ItemController(ShopDbContext context)
        {
            _context = context;
        }
        // GET: ItemController
        // GET: Item - عرض جميع العناصر
        public async Task<IActionResult> Index()
        {
            var items = await _context.Items.ToListAsync();
            return View(items);
        }

        // GET: Item/Details/5 - عرض تفاصيل عنصر معين
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var item = await _context.Items.FirstOrDefaultAsync(m => m.Id == id);

            if (item == null)
                return NotFound();

            return View(item);
        }

        // GET: Item/Create - عرض واجهة إضافة عنصر جديد
        public async Task<IActionResult> Create()
        {
            int maxId = await _context.Items.MaxAsync(b => (int?)b.Id) ?? 0;


            ViewBag.NextId = maxId + 1;
            return View();
        }

        // POST: Item/Create - إضافة عنصر جديد إلى قاعدة البيانات
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( Item item)
        {
            try 
            {
                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(item);

            }
        }

        // GET: Item/Edit/5 - عرض واجهة تعديل عنصر
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var item = await _context.Items.FindAsync(id);

            if (item == null)
                return NotFound();

            return View(item);
        }

        // POST: Item/Edit/5 - حفظ التعديلات على عنصر
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price")] Item item)
        {
            if (id != item.Id)
                return NotFound();

            try 
            {
                try
                {
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(item.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            catch 
            {

                return View(item);
            }
        }

        // GET: Item/Delete/5 - عرض واجهة حذف عنصر
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var item = await _context.Items.FirstOrDefaultAsync(m => m.Id == id);

            if (item == null)
                return NotFound();

            return View(item);
        }

        // POST: Item/Delete/5 - تأكيد حذف عنصر
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.Id == id);
        }
    }
}
