using BauFlow.Data;
using BauFlow.Entities;
using BauFlow.Models;
using BauFlow.Security;
using BauFlow.Services;

using Microsoft.AspNetCore.Authorization;
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



        // =====================
        // INVOICE TEXT
        // =====================

        [HttpGet]
        [Authorize(Roles = "Owner")]
        public IActionResult SetInvoiceText()
        {
            if (!_context.CurrentCompanyId.HasValue)
                return Unauthorized();


            var text =
                _context.InvoiceTexts
                .FirstOrDefault(x =>
                    x.CompanyId ==
                    _context.CurrentCompanyId.Value);


            text ??= new InvoiceText
            {
                CompanyId =
                    _context.CurrentCompanyId.Value,

                Text = ""
            };


            return PartialView(
                "_SetInvoiceTextModal",
                text);
        }



        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> SetInvoiceText(
            InvoiceText model)
        {
            if (!_context.CurrentCompanyId.HasValue)
                return Unauthorized();



            var existing =
                await _context.InvoiceTexts
                .FirstOrDefaultAsync(x =>
                    x.CompanyId ==
                    _context.CurrentCompanyId.Value);



            if (existing == null)
            {
                model.CompanyId =
                    _context.CurrentCompanyId.Value;


                _context.InvoiceTexts.Add(model);
            }
            else
            {
                existing.Text =
                    model.Text ?? "";
            }



            await _context.SaveChangesAsync();


            return RedirectToAction(
                nameof(Index));
        }





        // =====================
        // INDEX
        // =====================

        public async Task<IActionResult> Index()
        {
            var invoices =
                await _context.Invoices
                .Include(x => x.Customer)
                .OrderByDescending(
                    x => x.InvoiceDate)
                .ToListAsync();



            return View(invoices);
        }




        // =====================
        // DETAILS
        // =====================

        public async Task<IActionResult> Details(Guid id)
        {
            var invoice =
                await _context.Invoices

                .Include(x => x.Customer)

                .Include(x => x.Company)

                .Include(x => x.Items)

                .FirstOrDefaultAsync(
                    x => x.Id == id);



            if (invoice == null)
                return NotFound();



            return View(invoice);
        }
        // =====================
        // CREATE GET
        // =====================

        public IActionResult Create()
        {
            if (!_context.CurrentCompanyId.HasValue)
                return Unauthorized();


            ViewBag.CustomerId =
                _context.Customers
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList();


            ViewBag.TaxRates = GetTaxRates();


            var existing =
                _context.InvoiceTexts
                .FirstOrDefault(x =>
                    x.CompanyId ==
                    _context.CurrentCompanyId.Value);



            return View(new Invoice
            {
                TaxRate = 0,

                Description =
                    existing?.Text ?? "",

                DueDate =
                    DateTime.UtcNow.AddDays(7)
            });
        }




        // =====================
        // CREATE POST
        // =====================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Invoice invoice)
        {
            try
            {
                if (!_context.CurrentCompanyId.HasValue)
                    return Unauthorized();



                invoice.Items ??=
                    new List<InvoiceItem>();



                if (!ModelState.IsValid)
                {
                    ViewBag.CustomerId =
                        _context.Customers
                        .Select(c => new SelectListItem
                        {
                            Value = c.Id.ToString(),
                            Text = c.Name
                        })
                        .ToList();


                    ViewBag.TaxRates =
                        GetTaxRates();


                    return View(invoice);
                }



                invoice.Id =
                    Guid.NewGuid();


                invoice.InvoiceNumber =
                    await _numberService
                    .GetNextInvoiceNumber(
                        _context.CurrentCompanyId.Value);



                invoice.InvoiceDate =
                    DateTime.UtcNow;



                invoice.Status =
                    InvoiceStatus.Draft;



                foreach (var item in invoice.Items)
                {
                    item.Id = Guid.NewGuid();

                    item.InvoiceId =
                        invoice.Id;

                    item.TotalPrice =
                        item.Quantity *
                        item.UnitPrice;
                }



                invoice.NetAmount =
                    invoice.Items.Sum(
                        x => x.TotalPrice);



                invoice.TaxAmount =
                    invoice.NetAmount *
                    (invoice.TaxRate / 100m);



                invoice.GrossAmount =
                    Math.Round(
                        invoice.NetAmount +
                        invoice.TaxAmount,
                        0,
                        MidpointRounding.AwayFromZero);



                _context.Invoices.Add(invoice);


                await _context.SaveChangesAsync();



                return RedirectToAction(
                    nameof(Index));

            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.GetBaseException().Message;


                return View(invoice);
            }
        }







        // =====================
        // CREATE FROM QUOTE
        // =====================

        public async Task<IActionResult> CreateFromQuote(
            Guid quoteId)
        {
            if (!_context.CurrentCompanyId.HasValue)
                return Unauthorized();



            var quote =
                await _context.Quotes

                .Include(x => x.Items)

                .FirstOrDefaultAsync(
                    x => x.Id == quoteId);



            if (quote == null)
                return NotFound();




            var invoice = new Invoice
            {
                Id =
                    Guid.NewGuid(),


                CustomerId =
                    quote.CustomerId,


                QuoteId =
                    quote.Id,


                InvoiceDate =
                    DateTime.UtcNow,


                DueDate =
                    DateTime.UtcNow.AddDays(14),


                Status =
                    InvoiceStatus.Draft,


                TaxRate =
                    quote.TaxRate
            };



            invoice.Items =
                (quote.Items ?? new List<QuoteItem>())

                .Select(x => new InvoiceItem
                {
                    Id =
                        Guid.NewGuid(),


                    InvoiceId =
                        invoice.Id,


                    Description =
                        x.Description,


                    Quantity =
                        x.Quantity,


                    Unit =
                        x.Unit,


                    UnitPrice =
                        x.UnitPrice,


                    TotalPrice =
                        x.TotalPrice

                }).ToList();




            invoice.NetAmount =
                invoice.Items.Sum(
                    x => x.TotalPrice);



            invoice.TaxAmount =
                invoice.NetAmount *
                (invoice.TaxRate / 100m);



            invoice.GrossAmount =
                Math.Round(
                    invoice.NetAmount +
                    invoice.TaxAmount,
                    0,
                    MidpointRounding.AwayFromZero);



            invoice.InvoiceNumber =
                await _numberService
                .GetNextInvoiceNumber(
                    _context.CurrentCompanyId.Value);




            _context.Invoices.Add(invoice);


            await _context.SaveChangesAsync();




            return RedirectToAction(
                "Edit",
                new { id = invoice.Id });
        }        // =====================
        // SEND INVOICE
        // =====================

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

                    .FirstOrDefaultAsync(
                        x => x.Id == invoiceId);



                if (invoice == null)
                {
                    TempData["Error"] =
                        "Фактурата не е пронајдена";

                    return RedirectToAction(nameof(Index));
                }



                var company = invoice.Company;


                if (company == null)
                {
                    TempData["Error"] =
                        "Компанијата не е пронајдена";

                    return RedirectToAction(
                        nameof(Details),
                        new { id = invoiceId });
                }




                if (string.IsNullOrWhiteSpace(company.EmailHost) ||
                    string.IsNullOrWhiteSpace(company.EmailUser) ||
                    string.IsNullOrWhiteSpace(company.EmailFrom))
                {
                    TempData["Error"] =
                        "SMTP податоците не се внесени";


                    return RedirectToAction(
                        nameof(Details),
                        new { id = invoiceId });
                }




                string password = "";


                if (!string.IsNullOrWhiteSpace(company.EmailPassword))
                {
                    try
                    {
                        password =
                            _encryptionService.Decrypt(
                                company.EmailPassword);
                    }
                    catch
                    {
                        TempData["Error"] =
                            "Внесете ја е-маил лозинката повторно";


                        return RedirectToAction(
                            nameof(Details),
                            new { id = invoiceId });
                    }
                }




                var receiver =
                    sendToAccountant
                    ? company.Accountant
                    : invoice.Customer?.Email;




                if (string.IsNullOrWhiteSpace(receiver))
                {
                    TempData["Error"] =
                        "Нема внесено е-маил адреса";


                    return RedirectToAction(
                        nameof(Details),
                        new { id = invoiceId });
                }




                var settings = new EmailSettings
                {
                    Host =
                        company.EmailHost,


                    Port =
                        company.EmailPort,


                    UserName =
                        company.EmailUser,


                    Password =
                        password,


                    EnableSSL =
                        company.EmailSSL,


                    From =
                        company.EmailFrom,


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
                    $"Е-маилот е испратен до {receiver}";



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







        // =====================
        // PDF
        // =====================

        public async Task<IActionResult> Pdf(Guid id)
        {
            try
            {
                var invoice =
                    await _context.Invoices

                    .Include(x => x.Customer)

                    .Include(x => x.Items)

                    .FirstOrDefaultAsync(
                        x => x.Id == id);



                if (invoice == null)
                    return NotFound();




                var company =
                    await _context.Companies

                    .FirstOrDefaultAsync(
                        x => x.Id == invoice.CompanyId);



                if (company == null)
                    return NotFound();




                var document =
                    new InvoiceDocument(
                        invoice,
                        company);



                var pdf =
                    document.GeneratePdf();



                return File(
                    pdf,
                    "application/pdf",
                    $"Invoice-{invoice.InvoiceNumber}.pdf");
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.GetBaseException().Message;


                return RedirectToAction(
                    nameof(Index));
            }
        }








        // =====================
        // EDIT GET
        // =====================

        public async Task<IActionResult> Edit(Guid id)
        {
            var invoice =
                await _context.Invoices

                .Include(x => x.Items)

                .Include(x => x.Customer)

                .FirstOrDefaultAsync(
                    x => x.Id == id);



            if (invoice == null)
                return NotFound();




            ViewBag.CustomerId =
                _context.Customers

                .Select(c => new SelectListItem
                {
                    Value =
                        c.Id.ToString(),

                    Text =
                        c.Name

                }).ToList();



            ViewBag.TaxRates =
                GetTaxRates();



            return View(invoice);
        }







        // =====================
        // EDIT POST
        // =====================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Guid id,
            Invoice model)
        {
            var invoice =
                await _context.Invoices

                .FirstOrDefaultAsync(
                    x => x.Id == id);



            if (invoice == null)
                return NotFound();




            invoice.CustomerId =
                model.CustomerId;


            invoice.InvoiceNumber =
                model.InvoiceNumber;


            invoice.InvoiceDate =
                model.InvoiceDate;


            invoice.DueDate =
                model.DueDate;


            invoice.Status =
                model.Status;


            invoice.TaxRate =
                model.TaxRate;




            var oldItems =
                await _context.InvoiceItems

                .Where(x =>
                    x.InvoiceId == invoice.Id)

                .ToListAsync();



            _context.InvoiceItems
                .RemoveRange(oldItems);




            var newItems =
                (model.Items ?? new List<InvoiceItem>())

                .Select(x => new InvoiceItem
                {
                    Id =
                        Guid.NewGuid(),

                    InvoiceId =
                        invoice.Id,

                    Description =
                        x.Description,

                    Quantity =
                        x.Quantity,

                    Unit =
                        x.Unit,

                    UnitPrice =
                        x.UnitPrice,

                    TotalPrice =
                        x.Quantity * x.UnitPrice

                }).ToList();




            await _context.InvoiceItems
                .AddRangeAsync(newItems);




            invoice.NetAmount =
                newItems.Sum(
                    x => x.TotalPrice);



            invoice.TaxAmount =
                invoice.NetAmount *
                (invoice.TaxRate / 100m);



            invoice.GrossAmount =
                Math.Round(
                    invoice.NetAmount +
                    invoice.TaxAmount,
                    0,
                    MidpointRounding.AwayFromZero);




            await _context.SaveChangesAsync();




            return RedirectToAction(
                nameof(Index));
        }








        // =====================
        // DELETE
        // =====================

        public async Task<IActionResult> Delete(Guid id)
        {
            var invoice =
                await _context.Invoices

                .Include(x => x.Customer)

                .FirstOrDefaultAsync(
                    x => x.Id == id);



            if (invoice == null)
                return NotFound();



            return View(invoice);
        }




        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            Guid id)
        {
            var invoice =
                await _context.Invoices
                .FindAsync(id);



            if (invoice != null)
            {
                _context.Invoices
                    .Remove(invoice);


                await _context.SaveChangesAsync();
            }




            return RedirectToAction(
                nameof(Index));
        }






        // =====================
        // HELPERS
        // =====================

        private List<SelectListItem> GetTaxRates()
        {
            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value="0",
                    Text="0%"
                },

                new SelectListItem
                {
                    Value="7",
                    Text="7%"
                },

                new SelectListItem
                {
                    Value="19",
                    Text="19%"
                }
            };
        }
    }
}