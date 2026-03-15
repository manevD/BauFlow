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
        public async Task<IActionResult> Create(Quote quote)
        {
            quote.Id = Guid.NewGuid();
            quote.QuoteDate = DateTime.UtcNow;

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
            var quote = await _context.Quotes.FindAsync(id);

            ViewBag.CustomerId = _context.Customers
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();

            return View(quote);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Quote quote)
        {
            _context.Update(quote);
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
