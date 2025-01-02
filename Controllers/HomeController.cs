using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ShopWebMVC.Context;
using ShopWebMVC.Helpers;
using ShopWebMVC.Models;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ShopWebMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ShopDbContext _context;
      
        public HomeController(ILogger<HomeController> logger, ShopDbContext context)
        {
            _logger = logger;
            _context = context;
           
        }

        [HttpPost]
        public async Task<IActionResult> AddToQueue(int id)
        {
            var card = await _context.Cards.FindAsync(id);
            if (card == null || card.IsInQueue)
            {
                return Json(new { success = false, message = "البطاقة غير موجودة أو موجودة مسبقًا في الطابور." });
            }

            // تحديث حالة البطاقة وإضافتها للطابور
            card.IsInQueue = true;
            _context.Update(card);
            await _context.SaveChangesAsync();

            // الحصول على الطابور من الجلسة أو إنشاء طابور جديد
            var queue = HttpContext.Session.GetObjectFromJson<Queue<Card>>("CardQueue") ?? new Queue<Card>();

            // التحقق إذا كانت البطاقة موجودة بالفعل في الطابور
            var existingCardInQueue = queue.FirstOrDefault(c => c.Id == card.Id);
            if (existingCardInQueue != null)
            {
                return Json(new { success = false, message = "البطاقة موجودة بالفعل في الطابور." });
            }

            // إضافة البطاقة إلى الطابور
            queue.Enqueue(card);
            HttpContext.Session.SetObjectAsJson("CardQueue", queue);

            // إعادة عرض الطابور بعد إضافة البطاقة
            var queueCards = queue.ToList();
            return Json(new
            {
                success = true,
                message = "تمت إضافة البطاقة إلى الطابور بنجاح.",
                card = new
                {
                    card.Id,
                    FullName = $"{card.FirstName} {card.SecondName} {card.ThirdName} {card.FourthName}",
                    card.NumberOfIndividuals,
                    card.Notes,
                    BakeryName = card.Bakery?.Name
                },
               
            }) ;
        }

   

        [HttpPost]
        public async Task<IActionResult> RemoveFromQueue(int id)
        {
            var card = await _context.Cards.FindAsync(id);

            if (card == null)
            {
                return Json(new { success = false, message = "البطاقة غير موجودة." });
            }

            if (!card.IsInQueue)
            {
                return Json(new { success = false, message = "البطاقة غير موجودة في الطابور." });
            }

            // تحديث حالة البطاقة
            card.IsInQueue = false;
            _context.Update(card);
            await _context.SaveChangesAsync();

            // الحصول على الطابور من الجلسة
            var queue = HttpContext.Session.GetObjectFromJson<Queue<Card>>("CardQueue") ?? new Queue<Card>();

            // إعادة بناء الطابور بدون البطاقة المحذوفة
            var updatedQueue = new Queue<Card>(queue.Where(c => c.Id != id));
            HttpContext.Session.SetObjectAsJson("CardQueue", updatedQueue);

            // إرسال استجابة تؤكد الحذف
            return Json(new
            {
                success = true,
                message = "تمت إزالة البطاقة من الطابور بنجاح.",
                cardId = id
            });
        }

        public IActionResult ShowQueue()
        {
            // جلب الطابور من الجلسة، أو إنشاء طابور فارغ في حال عدم وجوده
            var queueList = HttpContext.Session.GetObjectFromJson<Queue<Card>>("CardQueue")?.ToList() ?? new List<Card>();

            // إذا كان الطابور فارغًا، قم بتحديث حالة `IsInQueue` في قاعدة البيانات
            if (!queueList.Any())
            {
                var allCards = _context.Cards.Where(c => c.IsInQueue).ToList();

                // تحديث حالة `IsInQueue` للبطاقات التي كانت في الطابور
                foreach (var card in allCards)
                {
                    card.IsInQueue = false;
                }

                // حفظ التغييرات في قاعدة البيانات إذا تم تحديث أي بطاقة
                if (allCards.Any())
                {
                    _context.SaveChanges();
                }
            }

            // إرجاع العرض الجزئي للطابور
            return PartialView("_QueuePartial", queueList);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string searchQuery)
        {
            if (string.IsNullOrEmpty(searchQuery))
            {
                ViewBag.Message = "يرجى إدخال استعلام البحث.";
                return View();
            }

            try
            {
                var cards = _context.Cards
                    .Include(c => c.Bakery)  // تضمين المخابز
                    .Include(c => c.Debt)    // تضمين الديون
                    .AsQueryable();

                if (int.TryParse(searchQuery, out int cardId)) // إذا كان المدخل رقمًا (ID)
                {
                    cards = cards.Where(c => c.Id == cardId); // تصفية حسب ID البطاقة
                }
                else // إذا كان المدخل نصًا (اسم البطاقة)
                {
                    var searchWords = searchQuery.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (searchWords.Length == 1)
                    {
                        cards = cards.Where(c =>
                            EF.Functions.Like(c.FirstName, $"%{searchWords[0]}%") ||
                            EF.Functions.Like(c.SecondName, $"%{searchWords[0]}%") ||
                            EF.Functions.Like(c.ThirdName, $"%{searchWords[0]}%") ||
                            EF.Functions.Like(c.FourthName, $"%{searchWords[0]}%"));
                    }
                    else if (searchWords.Length == 2)
                    {
                        cards = cards.Where(c =>
                            EF.Functions.Like(c.FirstName, $"%{searchWords[0]}%") &&
                            EF.Functions.Like(c.SecondName, $"%{searchWords[1]}%"));
                    }
                    else if (searchWords.Length == 3)
                    {
                        cards = cards.Where(c =>
                            EF.Functions.Like(c.FirstName, $"%{searchWords[0]}%") &&
                            EF.Functions.Like(c.SecondName, $"%{searchWords[1]}%") &&
                            EF.Functions.Like(c.ThirdName, $"%{searchWords[2]}%"));
                    }
                    else if (searchWords.Length == 4)
                    {
                        cards = cards.Where(c =>
                            EF.Functions.Like(c.FirstName, $"%{searchWords[0]}%") &&
                            EF.Functions.Like(c.SecondName, $"%{searchWords[1]}%") &&
                            EF.Functions.Like(c.ThirdName, $"%{searchWords[2]}%") &&
                            EF.Functions.Like(c.FourthName, $"%{searchWords[3]}%"));
                    }
                }

                var result = await cards.ToListAsync();

                if (!result.Any())
                {
                    ViewBag.Message = "لم يتم العثور على نتائج.";
                }

                return PartialView("_CardSearchResults", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during search.");
                ViewBag.ErrorMessage = "حدث خطأ أثناء البحث: " + ex.Message;
                return View();
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
