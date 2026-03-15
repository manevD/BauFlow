using BauFlow.Data;
using BauFlow.Entities;
using BauFlow.Security;
using BauFlow.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace BauFlow.Controllers
{
    [RequireTenant]
    public class InvoicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly NumberService _numberService;
        public InvoicesController(ApplicationDbContext context, NumberService numberService)
        {
            _context = context;
            _numberService = numberService;
        }

        // GET: Invoices
        public async Task<IActionResult> Index()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Customer)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();

            return View(invoices);
        }

        // GET: Invoices/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                return NotFound();

            return View(invoice);
        }

        // GET: Invoices/Create
        public IActionResult Create()
        {
            ViewBag.CustomerId = _context.Customers.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
            return View();
        }
        public async Task<IActionResult> CreateFromQuote(Guid quoteId)
        {
            var quote = await _context.Quotes
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == quoteId);

            if (quote == null)
                return NotFound();

            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                CustomerId = quote.CustomerId,
                QuoteId = quote.Id,
                InvoiceDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14),
                Status = InvoiceStatus.Draft
            };

            invoice.Items = (ICollection<QuoteItem>)quote.Items.Select(x => new InvoiceItem
            {
                Id = Guid.NewGuid(),
                Description = x.Description,
                Quantity = x.Quantity,
                Unit = x.Unit,
                UnitPrice = x.UnitPrice,
                TotalPrice = x.TotalPrice
            }).ToList();

            invoice.NetAmount = invoice.Items.Sum(x => x.TotalPrice);
            invoice.TaxAmount = invoice.NetAmount * 0.19m;
            invoice.GrossAmount = invoice.NetAmount + invoice.TaxAmount;

            invoice.InvoiceNumber =
                await _numberService.GetNextInvoiceNumber(_context.CurrentCompanyId.Value);

            _context.Invoices.Add(invoice);

            await _context.SaveChangesAsync();

            return RedirectToAction("Edit", "Invoices", new { id = invoice.Id });
        }
        // POST: Invoices/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Invoice invoice)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CustomerId = _context.Customers.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();
                return View(invoice);
            }

            invoice.Id = Guid.NewGuid();
      
            invoice.InvoiceNumber = await _numberService.GetNextInvoiceNumber(_context.CurrentCompanyId.Value);

            invoice.InvoiceDate = DateTime.UtcNow;
            invoice.NetAmount = invoice.Items.Sum(x => x.TotalPrice);
            invoice.TaxAmount = invoice.NetAmount * 0.19m;
            invoice.GrossAmount = invoice.NetAmount + invoice.TaxAmount;

            _context.Invoices.Add(invoice);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        
        }
        public async Task<IActionResult> Pdf(Guid id)
        {
            var invoice = await _context.Invoices
                .Include(x => x.Customer)
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);

            var document = new InvoiceDocument(invoice);

            var pdf = document.GeneratePdf();

            return File(pdf, "application/pdf",
                $"Rechnung-{invoice.InvoiceNumber}.pdf");
        }
        // GET: Invoices/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }
            ViewBag.CustomerId = _context.Customers.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
            return View(invoice);
        }

        // POST: Invoices/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Invoice invoice)
        {
            if (id != invoice.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Invoices.Update(invoice);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvoiceExists(invoice.Id))
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

            ViewBag.CustomerId = _context.Customers
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();
            return View(invoice);
        }

        // GET: Invoices/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        // POST: Invoices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice != null)
            {
                _context.Invoices.Remove(invoice);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InvoiceExists(Guid id)
        {
            return _context.Invoices.Any(e => e.Id == id);
        }
    }
}
