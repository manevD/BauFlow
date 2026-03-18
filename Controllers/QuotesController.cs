using Microsoft.AspNetCore.Mvc;

namespace BauFlow.Controllers
{
    using BauFlow.Data;
    using BauFlow.Entities;
    using BauFlow.Security;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;

    [RequireTenant]
    public class QuotesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuotesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LIST

        [Route("Angebote")]
        public async Task<IActionResult> Index()
        {
            var quotes = await _context.Quotes
                .Include(x => x.Customer)
                .OrderByDescending(x => x.QuoteDate)
                .ToListAsync();

            return View(quotes);
        }

        // CREATE

        public IActionResult Create()
        {
            ViewBag.CustomerId = _context.Customers
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();

            return View();
        }

        [HttpPost]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Quote quote)
        {
            ModelState.Remove("Customer");
            ModelState.Remove("QuoteNumber");
            if (!ModelState.IsValid)
            {
                ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "Name", quote.CustomerId);
                return View(quote);
            }

            quote.Id = Guid.NewGuid();
            quote.QuoteDate = DateTime.UtcNow;
            quote.Status = QuoteStatus.Draft;

            // QuoteNumber vergeben
            quote.QuoteNumber = $"ANG-{DateTime.UtcNow:yyyyMMddHHmmss}";

            // Falls keine Items gesendet wurden
            if (quote.Items == null)
                quote.Items = new List<QuoteItem>();

            decimal net = 0;

            foreach (var item in quote.Items)
            {
                item.Id = Guid.NewGuid();
                item.QuoteId = quote.Id;

                item.TotalPrice = item.Quantity * item.UnitPrice;
                net += item.TotalPrice;
            }

            quote.NetAmount = net;
            quote.TaxAmount = net * 0.19m;
            quote.GrossAmount = quote.NetAmount + quote.TaxAmount;

            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        // DETAILS

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

        // EDIT

        public async Task<IActionResult> Edit(Guid id)
        {
            var quote = await _context.Quotes
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);

            ViewBag.CustomerId = _context.Customers
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();

            return View(quote);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Quote quote)
        {
            var existingQuote = await _context.Quotes
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == quote.Id);

            if (existingQuote == null)
                return NotFound();

            existingQuote.CustomerId = quote.CustomerId;
            existingQuote.ValidUntil = quote.ValidUntil;
            existingQuote.Status = quote.Status;

            existingQuote.Items.Clear();

            if (quote.Items != null)
            {
                foreach (var item in quote.Items)
                {
                    existingQuote.Items.Add(new QuoteItem
                    {
                        Description = item.Description,
                        Quantity = item.Quantity,
                        Unit = item.Unit,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    });
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // DELETE

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

            _context.Quotes.Remove(quote);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
