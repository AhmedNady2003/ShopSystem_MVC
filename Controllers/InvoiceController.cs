using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopWebMVC.Context;
using ShopWebMVC.Models;

namespace ShopWebMVC.Controllers
{
    public class InvoiceController : Controller

    {
        private readonly ILogger<HomeController> _logger;

        private readonly ShopDbContext _context;
        public InvoiceController(ILogger<HomeController> logger, ShopDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: InvoiceController
        public ActionResult Index()
        {
            return View();
        }

        // GET: InvoiceController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }


        // Method to create a new invoice for a card
        [HttpPost]
        public async Task CreateInvoiceAsync(Card card, decimal? breadAmount)
        {
            try
            {
                var invoice = new Invoice
                {
                    CardId = card.Id,
                    InvoiceDate = DateTime.Now,
                    DebtValue = card.Debt?.MonetaryDebt ?? 0,
                    SupportAmount = CalculateSupportAmount(card.NumberOfIndividuals),
                    BreadAmount = breadAmount ?? 0,
                    InvoiceItems = new List<InvoiceItem>()
                };

                // Add debt items to invoice items
                if (card.Debt?.DebtItems != null)
                {
                    foreach (var debtItem in card.Debt.DebtItems)
                    {
                        if (debtItem.Item != null)
                        {
                            var item = new InvoiceItem
                            {
                                ItemId = debtItem.ItemId,
                                Quantity = debtItem.Quantity,
                                Item = debtItem.Item,
                                Price = debtItem.Item.Price
                            };
                            invoice.InvoiceItems.Add(item);
                        }
                    }
                }

                // Calculate amounts
                CalculateInvoiceTotals(ref invoice, card.NumberOfIndividuals);

                // Reset debt values for card
                ResetCardDebt(card);

                // Save invoice to database
                await _context.Invoices.AddAsync(invoice);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Handle exceptions as appropriate
                throw new Exception($"خطأ في انشاء الفاتورة: {ex.Message}");
            }
        }

        private void ResetCardDebt(Card card)
        {
            if (card.Debt != null)
            {
                card.Debt.MonetaryDebt = 0;
                if (card.Debt.DebtItems != null)
                {
                    _context.DebtItems.RemoveRange(card.Debt.DebtItems);
                }
            }
        }

     

        private void CalculateInvoiceTotals(ref Invoice invoice, int NumberOfIndividuals )
        {
            
            decimal itemAmount = invoice.InvoiceItems?.Sum(i => i.Quantity * (i.Price)) ?? 0;
            invoice.TotalAmount = invoice.DebtValue + itemAmount;
            if(((invoice.SupportAmount + invoice.BreadAmount) - invoice.TotalAmount) > 0)
            {
                invoice.DifferenceAmount = 0;
                invoice.RemainingAmount = ((invoice.SupportAmount + invoice.BreadAmount) - invoice.TotalAmount);
            }
            else
            {
                invoice.RemainingAmount = 0;
                invoice.DifferenceAmount = ((invoice.SupportAmount + invoice.BreadAmount) - invoice.TotalAmount);
                
            }
             
            invoice.Paide = CalculatePaide(NumberOfIndividuals) - invoice.DifferenceAmount;
           
        }

        
        public async Task<IActionResult> AddInvoice(int cardId)
        {
            try
            {
                var card = await _context.Cards
                    .Include(c => c.Debt)
                        .ThenInclude(d => d.DebtItems)
                            .ThenInclude(di => di.Item)
                    .Include(r => r.Invoices)
                        .ThenInclude(b => b.InvoiceItems)
                            .ThenInclude(bi => bi.Item)
                    .FirstOrDefaultAsync(c => c.Id == cardId);

                if (card == null)
                    return NotFound();

                if (card.HasInvoice)
                {
                    TempData["Message"] = "تم إنشاء فاتورة لهذا الشهر بالفعل.";
                    return RedirectToAction("ViewInvoices", "Card", new { id = cardId });
                }
                    
                

                await CreateInvoiceAsync(card, 0); // Call the async version of CreateInvoice
                TempData["SuccessMessage"] = "تم إنشاء الفاتورة بنجاح.";

                return RedirectToAction("ViewInvoices", "Card", new { id = cardId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطأ في حفظ البيانات: {ex.Message}");
            }
        }


        public async Task<IActionResult> StrikeCard(int id, decimal beardAmount)
        {
            try
            {
                var card = await _context.Cards
                    .Include(c => c.Debt)
                        .ThenInclude(d => d.DebtItems)
                            .ThenInclude(di => di.Item)
                    .Include(r => r.Invoices)
                        .ThenInclude(b => b.InvoiceItems)
                            .ThenInclude(bi => bi.Item)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (card == null)
                    return NotFound();

                if (!card.HasInvoice)
                {
                    await CreateInvoiceAsync(card, beardAmount); // Call the async version of CreateInvoice
                    card.IsStruck = true;
                }
                else if (!card.IsStruck)
                {
                    card.IsStruck = true;
                    // Retrieve the current month's invoice
                    var invoice = card.Invoices.SingleOrDefault(i => i.InvoiceDate.Month == DateTime.Now.Month && i.InvoiceDate.Year == DateTime.Now.Year);

                    if (invoice != null)
                    {
                        invoice.BreadAmount = beardAmount;
                        CalculateInvoiceTotals(ref invoice, card.NumberOfIndividuals);
                        _context.Update(invoice); // Update the single invoice entity
                        await _context.SaveChangesAsync();

                    }
                }

                _context.Cards.Update(card);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Card");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطأ في حفظ البيانات: {ex.Message}");
            }
        }


        // Calculate support amount based on number of individuals
        private decimal CalculateSupportAmount(int numberOfIndividuals)
        {
            if (numberOfIndividuals <= 4)
                return (50 * numberOfIndividuals) - 2;
            else
                return (200 + ((numberOfIndividuals - 4) * 25)) - 2;
        }

        // Calculate paid amount based on number of individuals
        private decimal CalculatePaide(int numberOfIndividuals)
        {
            return numberOfIndividuals > 1 ? 3 + numberOfIndividuals : 3;
        }
   


        // GET: Edit Invoice
        public async Task<IActionResult> Edit(int id)
        {
            // جلب الفاتورة المطلوبة مع البيانات المرتبطة
            var invoice = await _context.Invoices
                .Include(i => i.Card) // تضمين بيانات البطاقة
                .Include(i => i.InvoiceItems) // تضمين عناصر الفاتورة
                .ThenInclude(ii => ii.Item) // تضمين تفاصيل العنصر
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                TempData["ErrorMessage"] = "الفاتورة غير موجودة.";
                return RedirectToAction("Index", "Card");
            }
            var items = await _context.Items.ToListAsync();
            ViewData["Items"] = items;
            // تمرير الفاتورة إلى العرض
            return View(invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            decimal SupportAmount,
            decimal BreadAmount,
            decimal DebtValue,
            List<InvoiceItem> newItems)
        {
            // البحث عن الفاتورة الحالية
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .ThenInclude (ii => ii.Item)
                .Include(i=>i.Card)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound("الفاتورة غير موجودة.");
            }

            // تحديث القيم الأساسية للفاتورة
            invoice.SupportAmount = SupportAmount;
            invoice.BreadAmount = BreadAmount;
            invoice.DebtValue = DebtValue;

            // التعامل مع العناصر الجديدة
            if (newItems != null && newItems.Any())
            {
                foreach (var newItem in newItems)
                {
                    // البحث عن العنصر في الفاتورة الحالية
                    var existingItem = invoice.InvoiceItems
                        .FirstOrDefault(i => i.ItemId == newItem.ItemId);

                    if (existingItem != null)
                    {
                        // إذا كان العنصر موجودًا، قم بتحديث الكمية فقط
                        existingItem.Quantity = newItem.Quantity;
                    }
                    else
                    {
                        // إذا كان العنصر غير موجود، أضفه إلى الفاتورة
                        var itemToAdd = new InvoiceItem
                        {
                            InvoiceId = id,
                            ItemId = newItem.ItemId,
                            Quantity = newItem.Quantity,
                            Price = _context.Items.Find(newItem.ItemId)?.Price ?? 0 // جلب السعر من قاعدة البيانات
                        };
                        invoice.InvoiceItems.Add(itemToAdd);
                    }
                }
            }
            // حفظ التعديلات في قاعدة البيانات
            try
            {
                CalculateInvoiceTotals(ref invoice, invoice.Card.NumberOfIndividuals);

                _context.Update(invoice);
                await _context.SaveChangesAsync();
                return RedirectToAction("ViewInvoices", "Card", new { id = invoice.CardId });
            }
            catch 
            {
                return RedirectToAction("Edit");

            }
        }





        // GET: Spent Invoice
        public async Task<IActionResult> Spent(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .ThenInclude(ii => ii.Item)
                .Include(i => i.Card)
                .ThenInclude(d=>d.Debt)
                .ThenInclude(di=>di.DebtItems)
                .ThenInclude(i=>i.Item)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                TempData["ErrorMessage"] = "الفاتورة غير موجودة.";
                return RedirectToAction("Index", "Card");
            }
            if (!invoice.IsSpent)
            {
                invoice.DebtValue += invoice.Card.Debt.MonetaryDebt;
                if (invoice.Card.Debt?.DebtItems != null)
                {
                    foreach (var debtItem in invoice.Card.Debt.DebtItems)
                    {

                        if (debtItem.Item != null)
                        {

                            var existingItem = invoice.InvoiceItems.FirstOrDefault(x => x.ItemId == debtItem.ItemId);
                            if (existingItem == null)
                            {
                                var invoiceItem = new InvoiceItem
                                {
                                    ItemId = debtItem.ItemId,
                                    Quantity = debtItem.Quantity,
                                    Item = debtItem.Item,
                                    Price = debtItem.Item.Price
                                };
                                invoice.InvoiceItems.Add(invoiceItem);
                            }
                            else
                            {
                                existingItem.Quantity += debtItem.Quantity;

                            }
                        }
                    }
                }
            }
            ResetCardDebt(invoice.Card);
            CalculateInvoiceTotals(ref invoice, invoice.Card.NumberOfIndividuals); // Refactored calculation logic

            _context.Update(invoice);
            _context.SaveChanges();
            // Load all available items
            var items = await _context.Items.ToListAsync();
            ViewData["Items"] = items;

            // Send the invoice to the view for editing
            return View(invoice);
        }

        // POST: Spent Invoice
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Spent(int id, decimal cashValue, List<InvoiceItem> newItems)
        {
            try
            {
               
                var existingInvoice = await _context.Invoices
                    .Include(i => i.InvoiceItems)
                    .ThenInclude(ii => ii.Item)
                    .Include(c => c.Card)
                    .ThenInclude(d => d.Debt)
                    .ThenInclude(di => di.DebtItems)
                    .ThenInclude(dti => dti.Item) // Explicitly load Item in DebtItems
                    .FirstOrDefaultAsync(i => i.Id == id);
                

                if (existingInvoice == null)
                {
                    return NotFound("الفاتورة غير موجودة.");
                }

               

                // Add new items to the invoice
                foreach (var newItem in newItems)
                {
                     Item itemToAdd = await _context.Items.FindAsync(newItem.ItemId);
                        var existingItem = existingInvoice.InvoiceItems.FirstOrDefault(x => x.ItemId == newItem.ItemId);
                        if (existingItem == null)
                        {
                            var invoiceItem = new InvoiceItem
                            {
                                ItemId = newItem.ItemId,
                                Quantity = newItem.Quantity,
                                Item = itemToAdd,
                                Price = itemToAdd.Price
                            };
                            existingInvoice.InvoiceItems.Add(invoiceItem);
                        }
                        else
                        {
                            existingItem.Quantity += newItem.Quantity;
                            
                        }
                    
                }

                // Update invoice values
                existingInvoice.DebtValue += cashValue; // Update debt value as necessary
                CalculateInvoiceTotals(ref existingInvoice, existingInvoice.Card.NumberOfIndividuals); // Refactored calculation logic
               
                // Save changes
                await _context.SaveChangesAsync();
                return RedirectToAction("ViewInvoices", "Card", new { id = existingInvoice.CardId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطأ في حفظ البيانات: {ex.Message}";
                return RedirectToAction("ViewInvoices", "Card");
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(m => m.Id == id);

            if (invoice == null)
                return NotFound();

            return View(invoice);
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var invoice = await _context.Invoices.Include(i => i.InvoiceItems)
                    .Include(c=>c.Card)
                    .FirstOrDefaultAsync(i => i.Id == id);
                if (invoice != null)
                {
                    if (invoice.InvoiceItems != null && invoice.InvoiceItems.Any())
                    {
                         _context.InvoiceItems.RemoveRange(invoice.InvoiceItems);
                    }

                    // حذف الفاتورة
                    _context.Invoices.Remove(invoice);
                    
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("ViewInvoices", "Card", new { id = invoice.CardId });
            }
            catch
            {
                return View();
            }
           
        }

        [HttpGet]
        public IActionResult GetInvoiceItems(int id)
        {
            var invoice = _context.Invoices
                .Include(i => i.InvoiceItems)
                .ThenInclude(ii => ii.Item)
                .FirstOrDefault(i => i.Id == id);

            if (invoice == null || invoice.InvoiceItems == null)
            {
                return Json(new List<object>());
            }

            var items = invoice.InvoiceItems.Select(ii => new
            {
                name = ii.Item.Name,
                quantity = ii.Quantity,
                price = ii.Item.Price
            }).ToList();

            return Json(items);
        }

        private bool InvoiceExists(int id)
        {
            return _context.Invoices.Any(e => e.Id == id);
        }

    }
}
