using BauFlow.Data;
using BauFlow.Entities;
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
    public class InvoicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly NumberService _numberService;
        private readonly EmailService _emailService;
        private readonly EmailEncryptionService _encryptionService;

        public InvoicesController(
            ApplicationDbContext context,
            NumberService numberService,
            EmailService emailService,
            EmailEncryptionService encryptionService)
        {
            _context = context;
            _numberService = numberService;
            _emailService = emailService;
            _encryptionService = encryptionService;
        }

        public async Task<IActionResult> Index()
        {
            var invoices = await _context.Invoices
                .Include(x => x.Customer)
                .OrderByDescending(x => x.InvoiceDate)
                .ToListAsync();

            return View(invoices);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var invoice = await _context.Invoices
                .Include(x => x.Customer)
                .Include(x => x.Company)
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (invoice == null)
                return NotFound();

            return View(invoice);
        }

        public async Task<IActionResult> Pdf(Guid id)
        {
            try
            {
                var invoice = await _context.Invoices
                    .Include(x => x.Customer)
                    .Include(x => x.Items)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (invoice == null)
                    return NotFound();

                var company = await _context.Companies
                    .FirstOrDefaultAsync(x => x.Id == invoice.CompanyId);

                if (company == null)
                    throw new Exception("Company missing");

                byte[] pdf;

                try
                {
                    pdf = new InvoiceDocument(invoice, company)
                        .GeneratePdf();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "PDF ERROR: " +
                        ex.GetBaseException().Message);
                }

                return File(
                    pdf,
                    "application/pdf",
                    $"Invoice-{invoice.InvoiceNumber}.pdf");
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.GetBaseException().Message;

                return RedirectToAction(nameof(Index));
            }
        }


        public async Task<IActionResult> SendInvoice(
            Guid invoiceId,
            bool sendToAccountant = false)
        {
            try
            {
                var invoice = await _context.Invoices
                    .Include(x => x.Customer)
                    .Include(x => x.Company)
                    .Include(x => x.Items)
                    .FirstOrDefaultAsync(x => x.Id == invoiceId);

                if (invoice == null)
                    return NotFound();


                var company = invoice.Company;

                if (company == null)
                    throw new Exception("Company missing");


                string password = "";

                if (!string.IsNullOrWhiteSpace(company.EmailPassword))
                {
                    try
                    {
                        password =
                            _encryptionService
                            .Decrypt(company.EmailPassword);
                    }
                    catch
                    {
                        TempData["Error"] =
                            "Внесете ја лозинката повторно";

                        return RedirectToAction(
                            nameof(Details),
                            new { id = invoiceId });
                    }
                }


                var receiver = sendToAccountant
                    ? company.Accountant
                    : invoice.Customer?.Email;


                if (string.IsNullOrWhiteSpace(receiver))
                    throw new Exception("Email missing");


                var settings = new EmailSettings
                {
                    Host = company.EmailHost,
                    Port = company.EmailPort,
                    UserName = company.EmailUser,
                    Password = password,
                    EnableSSL = company.EmailSSL,
                    From = company.EmailFrom,
                    FromName =
                        string.IsNullOrWhiteSpace(company.EmailFromName)
                        ? company.Name
                        : company.EmailFromName
                };


                await _emailService.SendInvoice(
                    receiver,
                    invoice,
                    settings,
                    company.Name,
                    sendToAccountant);


                TempData["Success"] =
                    "Е-маилот е испратен";


                return RedirectToAction(
                    nameof(Details),
                    new { id = invoiceId });
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.GetBaseException().Message;


                return RedirectToAction(
                    nameof(Details),
                    new { id = invoiceId });
            }
        }


        public IActionResult Create()
        {
            ViewBag.CustomerId = GetCustomers();
            ViewBag.TaxRates = GetTaxRates();

            var existing = _context.InvoiceTexts
               .FirstOrDefault(x =>
                   x.CompanyId == _context.CurrentCompanyId.Value);
            return View(new Invoice
            {
                TaxRate = 0,
                Description = existing?.Text ?? "",
                DueDate = DateTime.UtcNow.AddDays(7)
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Invoice invoice)
        {
            invoice.Items ??= new List<InvoiceItem>();

            invoice.Id = Guid.NewGuid();
            invoice.InvoiceDate = DateTime.Now;
            invoice.Status = InvoiceStatus.Draft;

            if (_context.CurrentCompanyId.HasValue)
            {
                invoice.InvoiceNumber =
                    await _numberService.GetNextInvoiceNumber(
                        _context.CurrentCompanyId.Value);
            }

            foreach (var item in invoice.Items)
            {
                item.Id = Guid.NewGuid();
                item.InvoiceId = invoice.Id;
                item.TotalPrice =
                    item.Quantity * item.UnitPrice;
            }

            invoice.NetAmount =
                invoice.Items.Sum(x => x.TotalPrice);

            invoice.TaxAmount =
                invoice.NetAmount *
                invoice.TaxRate / 100m;

            invoice.GrossAmount =
                Math.Round(
                    invoice.NetAmount +
                    invoice.TaxAmount);

            _context.Invoices.Add(invoice);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Delete(Guid id)
        {
            var invoice = await _context.Invoices
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (invoice == null)
                return NotFound();

            return View(invoice);
        }


        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var invoice =
                await _context.Invoices.FindAsync(id);

            if (invoice != null)
            {
                _context.Invoices.Remove(invoice);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        private List<SelectListItem> GetCustomers()
        {
            return _context.Customers
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                }).ToList();
        }
        // GET: Invoices/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ModelState.Clear(); // 💥 DAS IST DER FIX
            var invoice = await _context.Invoices
                .Include(i => i.Items)
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (invoice == null)
            {
                return NotFound();
            }

            ViewBag.TaxRates = GetTaxRates();

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
        public async Task<IActionResult> Edit(Guid id, Invoice model)
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(x => x.Id == id);

            if (invoice == null)
                return NotFound();

            // ========= Stammdaten =========
            invoice.CustomerId = model.CustomerId;
            invoice.InvoiceNumber = model.InvoiceNumber;
            invoice.InvoiceDate = model.InvoiceDate;
            invoice.DueDate = model.DueDate;
            invoice.Status = model.Status;
            invoice.TaxRate = model.TaxRate;

            // ========= Items SAFE =========
            var existingItems = await _context.InvoiceItems
                .Where(x => x.InvoiceId == invoice.Id)
                .ToListAsync();

            _context.InvoiceItems.RemoveRange(existingItems);

            var newItems = model.Items.Select(x => new InvoiceItem
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoice.Id,
                Description = x.Description,
                Quantity = x.Quantity,
                Unit = x.Unit,
                UnitPrice = x.UnitPrice,
                TotalPrice = x.TotalPrice
            }).ToList();

            await _context.InvoiceItems.AddRangeAsync(newItems);

            // ========= Totals =========
            invoice.NetAmount = newItems.Sum(x => x.TotalPrice);
            invoice.TaxAmount = invoice.NetAmount * (invoice.TaxRate / 100m);
            invoice.GrossAmount = Math.Round(invoice.NetAmount + invoice.TaxAmount, 0, MidpointRounding.AwayFromZero);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        private List<SelectListItem> GetTaxRates()
        {
            return new()
            {
                new(){Value="0",Text="0%"},
                new(){Value="7",Text="7%"},
                new(){Value="19",Text="19%"}
            };
        }
    }
}