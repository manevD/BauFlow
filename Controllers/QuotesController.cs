using BauFlow.Data;
using BauFlow.Entities;
using BauFlow.Interfaces;
using BauFlow.Models;
using BauFlow.Security;
using BauFlow.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace BauFlow.Controllers
{
    [RequireTenant]
    public class QuotesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly NumberService _numberService;
        private readonly EmailService _emailService;
        private readonly EmailEncryptionService _encryptionService;
        public QuotesController(ApplicationDbContext context, NumberService numberService, EmailService emailService, EmailEncryptionService encryptionService)
        {
            _context = context;
            _emailService = emailService;
            _numberService = numberService;
            _encryptionService = encryptionService;
        }
        public async Task<IActionResult> Pdf(Guid id)
        {
            var quote = await _context.Quotes
                .Include(x => x.Customer)
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);

            var document = new QuotesDocument(quote, _context.Companies.Find(_context.CurrentCompanyId.Value));

            var pdf = document.GeneratePdf();

            return File(pdf, "application/pdf",
                $"{quote.QuoteNumber}.pdf");
        }

        // ===================== LIST =====================
        [Route("ponuda")]
        public async Task<IActionResult> Index()
        {
            var quotes = await _context.Quotes
                .Include(x => x.Customer)
                .OrderByDescending(x => x.QuoteDate)
                .ToListAsync();

            return View(quotes);
        }

        // ===================== CREATE =====================
        public IActionResult Create()
        {
            ViewBag.CustomerId = _context.Customers.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            ViewBag.TaxRates = GetTaxRates() ?? new List<SelectListItem>();

            return View(new Quote
            {
                TaxRate = 19,
                ValidUntil = DateTime.Today.AddDays(7)
            });
        }
        public async Task<IActionResult> SendInvoice(Guid invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(x => x.Id == invoiceId);

            if (invoice == null)
                return NotFound();

            var company = await _context.Companies.FirstOrDefaultAsync(x => x.Id == invoice.CompanyId);

            var settings = new EmailSettings
            {
                Host = company.EmailHost,
                Port = company.EmailPort,
                UserName = company.EmailUser,
                Password = _encryptionService.Decrypt(company.EmailPassword),
                EnableSSL = company.EmailSSL,
                From = company.EmailFrom,
                FromName = company.EmailFromName
            };

            await _emailService.SendInvoice(
                invoice.Customer.Email,
                invoice,
                settings, company.Name,false
            );

            return RedirectToAction("Details", new { id = invoiceId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Quote quote)
        {
            ModelState.Remove("Customer");
            ModelState.Remove("QuoteNumber");
            ModelState.Remove("Quote");

            if (!ModelState.IsValid)
            {
                ViewBag.CustomerId = GetCustomers();
                ViewBag.TaxRates = GetTaxRates();
                return View(quote);
            }

            quote.Id = Guid.NewGuid();
            quote.QuoteDate = DateTime.UtcNow;
            quote.Status = QuoteStatus.Draft;

            // 🔥 WICHTIG: TaxRate normalisieren
            quote.TaxRate = (int)quote.TaxRate;

            quote.QuoteNumber = $"Понуда-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4)}";

            if (quote.Items == null)
                quote.Items = new List<QuoteItem>();

            foreach (var item in quote.Items)
            {
                item.Id = Guid.NewGuid();
                item.QuoteId = quote.Id;
                item.TotalPrice = item.Quantity * item.UnitPrice;
            }

            // 🔥 Totals IMMER serverseitig
            quote.NetAmount = quote.Items.Sum(x => x.TotalPrice);
            quote.TaxAmount = quote.NetAmount * (quote.TaxRate / 100m);
            quote.GrossAmount = Math.Round(quote.NetAmount + quote.TaxAmount, 0, MidpointRounding.AwayFromZero);

            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ===================== DETAILS =====================
        public async Task<IActionResult> Details(Guid id)
        {
            var quote = await _context.Quotes
                .Include(x => x.Customer)
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (quote == null)
                return NotFound();

            return View(quote);
        }

        // ===================== EDIT =====================
        public async Task<IActionResult> Edit(Guid id)
        {
            var quote = await _context.Quotes
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (quote == null)
                return NotFound();

            // 🔥 WICHTIG
            quote.TaxRate = (int)quote.TaxRate;

            ViewBag.CustomerId = GetCustomers();
            ViewBag.TaxRates = GetTaxRates();

            return View(quote);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Quote model)
        {
            var quote = await _context.Quotes
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (quote == null)
                return NotFound();

            // ========= Stammdaten =========
            quote.CustomerId = model.CustomerId;
            quote.ValidUntil = model.ValidUntil;
            quote.Status = model.Status;
            quote.TaxRate = model.TaxRate;

            // ========= Items SAFE =========
            var existingItems = await _context.QuoteItems
                .Where(x => x.QuoteId == quote.Id)
                .ToListAsync();

            _context.QuoteItems.RemoveRange(existingItems);

            var newItems = (model.Items ?? new List<QuoteItem>())
                .Select(x => new QuoteItem
                {
                    Id = Guid.NewGuid(),
                    QuoteId = quote.Id,
                    Description = x.Description,
                    Quantity = x.Quantity,
                    Unit = x.Unit,
                    UnitPrice = x.UnitPrice,
                    TotalPrice = x.Quantity * x.UnitPrice
                })
                .ToList();

            await _context.QuoteItems.AddRangeAsync(newItems);

            // ========= Totals =========
            quote.NetAmount = newItems.Sum(x => x.TotalPrice);
            quote.TaxAmount = quote.NetAmount * (quote.TaxRate / 100m);
            quote.GrossAmount = Math.Round(quote.NetAmount + quote.TaxAmount, 0, MidpointRounding.AwayFromZero);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ===================== DELETE =====================
        public async Task<IActionResult> Delete(Guid id)
        {
            var quote = await _context.Quotes
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.Id == id);

            return View(quote);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var quote = await _context.Quotes.FindAsync(id);

            if (quote != null)
                _context.Quotes.Remove(quote);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ===================== HELPERS =====================
        private List<SelectListItem> GetCustomers()
        {
            return _context.Customers.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
        }

        private List<SelectListItem> GetTaxRates()
        {
            return new List<SelectListItem>
        {
            new SelectListItem { Value = "0", Text = "0%" },
            new SelectListItem { Value = "7", Text = "7%" },
            new SelectListItem { Value = "19", Text = "19%" }
        };
        }
    }

}
