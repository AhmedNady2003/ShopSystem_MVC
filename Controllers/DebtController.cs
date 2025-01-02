using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopWebMVC.Context;
using ShopWebMVC.Models;
using System.Linq.Expressions;

namespace ShopWebMVC.Controllers
{
    public class DebtController : Controller
    {
        private readonly ShopDbContext _context;
        public DebtController(ShopDbContext context)
        {
            _context = context;
        }
        // GET: Debt - عرض جميع الديون
        public async Task<IActionResult> Index()
        {
            var debts = await _context.Debts
                .Include(d => d.Card)
                .Include(d => d.DebtItems)
                .ThenInclude(di => di.Item)
                .ToListAsync();
            return View(debts);
        }

        // GET: Debt/Details/5 - عرض تفاصيل دين معين
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var debt = await _context.Debts
                .Include(d => d.Card)
                .Include(d => d.DebtItems)
                .ThenInclude(di => di.Item)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (debt == null) return NotFound();

            return View(debt);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var debt = await _context.Debts.Include(d => d.Card)
                .Include(d => d.DebtItems)
                .ThenInclude(di => di.Item) // تحميل الأصناف المرتبطة
                .FirstOrDefaultAsync(d => d.Id == id);

            if (debt == null)
            {
                return NotFound();
            }

            // تحميل جميع العناصر المتاحة
            var items = await _context.Items.ToListAsync();
            ViewData["Items"] = items;

            return View(debt);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Debt debt, List<DebtItem> DebtItems)
        {
            if (id != debt.Id)
            {
                return NotFound();
            }

            try
            {
                var existingDebt = await _context.Debts
                    .Include(d => d.DebtItems)
                    .ThenInclude(i=>i.Item)// تأكد من تضمين العناصر المرتبطة
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (existingDebt == null)
                {
                    return NotFound();
                }

                // تحديث القيم
                existingDebt.MonetaryDebt = debt.MonetaryDebt;

                // تحديث العناصر
                existingDebt.DebtItems.Clear(); // مسح العناصر الحالية
                foreach (var item in DebtItems)
                {
                    if (item.Quantity > 0) // أضف فقط العناصر ذات الكمية أكبر من 0
                    {
                        existingDebt.DebtItems.Add(item);
                    }
                }

                _context.Debts.Update(existingDebt);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Card");
            }
            catch
            {
                // إذا كان هناك خطأ في النموذج، أعيد البيانات
                ViewData["Items"] = await _context.Items.ToListAsync();
                return View(debt);
            }
            
        }



        // GET: Debt/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var debt = await _context.Debts
                .Include(d => d.Card) // تضمين البطاقة المرتبطة بالدين
                .Include(d => d.DebtItems)
                    .ThenInclude(di => di.Item) // تضمين الأصناف المرتبطة بالعناصر
                .FirstOrDefaultAsync(d => d.Id == id);

            if (debt == null)
            {
                return NotFound();
            }

            return View(debt);
        }


        // POST: Debt/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var debt = await _context.Debts
                    .Include(d => d.DebtItems)
                    .ThenInclude(di => di.Item) // تضمين الأصناف المرتبطة بكل عنصر من عناصر الدين
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (debt == null)
                {
                    return NotFound();
                }

                // حذف العناصر المرتبطة بالدين
                if (debt.DebtItems != null && debt.DebtItems.Count > 0)
                {
                    _context.DebtItems.RemoveRange(debt.DebtItems);
                }

                // حذف الدين نفسه
                debt.MonetaryDebt = 0;
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // سجل الاستثناء إذا لزم الأمر
                ModelState.AddModelError("", "حدث خطأ أثناء حذف الدين. يرجى المحاولة لاحقاً.");
                return View("Error"); // عرض صفحة خطأ
            }
        }
        private bool DebtExists(int id)
        {
            return _context.Debts.Any(d => d.Id == id);
        }
    }

   
    
}
