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

            return View(new Invoice
            {
                TaxRate = 0,
                DueDate = DateTime.Now.AddDays(7)
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


        [HttpPost]
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