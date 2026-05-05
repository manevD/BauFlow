using BauFlow.Data;
using BauFlow.Security;
using BauFlow.Services;
using BauFlow.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace BauFlow.Controllers
{
    [RequireTenant]
    [Authorize("OwnerOnly")]
    public class CompanyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailEncryptionService _encryptionService;
        public CompanyController(ApplicationDbContext context, EmailEncryptionService encryptionService)
        {
            _encryptionService = encryptionService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var company = _context.Companies.FirstOrDefault(x => x.Id == _context.CurrentCompanyId);
            if (company == null)
            {
                return NotFound();
            }
            var companyViewModel = new CompanyViewModel()
            {
                Name = company.Name,
                Address = company.Address,
                PostalCode = company.PostalCode,
                City = company.City,
                Country = company.Country,
                TaxNumber = company.TaxNumber,
                IBAN = company.IBAN,
            };
            return View(companyViewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(CompanyViewModel companyViewModel)
        {
            if (!ModelState.IsValid)
                return View(companyViewModel);

            var company = _context.Companies
                .FirstOrDefault(x => x.Id == _context.CurrentCompanyId);

            if (company == null)
                return NotFound();

            // update
            company.Name = companyViewModel.Name;
            company.Address = companyViewModel.Address;
            company.PostalCode = companyViewModel.PostalCode;
            company.City = companyViewModel.City;
            company.Country = companyViewModel.Country;
            company.TaxNumber = companyViewModel.TaxNumber;
            company.IBAN = companyViewModel.IBAN.Replace(" ", "");

            await _context.SaveChangesAsync(); 

            TempData["Success"] = "Податоците се успешно зачувани";

            return RedirectToAction("Edit"); 
        }
        public async Task<IActionResult> EmailSettings()
        {
            var company = await _context.Companies.FindAsync(_context.CurrentCompanyId);

            if (company == null) return NotFound();

            var vm = new CompanyEmailSettingsVM
            {
                Id = company.Id,
                EmailHost = company.EmailHost,
                EmailPort = company.EmailPort,
                EmailUser = company.EmailUser,
                EmailPassword = _encryptionService.Decrypt(company.EmailPassword),
                EmailSSL = company.EmailSSL,
                EmailFrom = company.EmailFrom,
                EmailFromName = company.EmailFromName
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> EmailSettings(CompanyEmailSettingsVM vm)
        {
            var company = await _context.Companies.FindAsync(vm.Id);

            if (company == null) return NotFound();

            company.EmailHost = vm.EmailHost;
            company.EmailPort = vm.EmailPort;
            company.EmailUser = vm.EmailUser;
            company.EmailPassword = _encryptionService.Encrypt(vm.EmailPassword);
            company.EmailSSL = vm.EmailSSL;
            company.EmailFrom = vm.EmailFrom;
            company.EmailFromName = vm.EmailFromName;
            if (!string.IsNullOrEmpty(vm.EmailPassword))
            {
                company.EmailPassword = _encryptionService.Encrypt(vm.EmailPassword);
            }
            await _context.SaveChangesAsync();

            return RedirectToAction("EmailSettings");
        }

        [HttpPost]
        public async Task<IActionResult> TestEmailSettings([FromBody] CompanyEmailSettingsVM vm)
        {
            try
            {
                using var smtp = new SmtpClient(vm.EmailHost, vm.EmailPort)
                {
                    Credentials = new NetworkCredential(vm.EmailUser, vm.EmailPassword),
                    EnableSsl = vm.EmailSSL
                };

                // Dummy Mail (wird NICHT gesendet, nur Verbindung getestet)
                await smtp.SendMailAsync(new MailMessage
                {
                    From = new MailAddress(vm.EmailFrom),
                    Subject = "Test",
                    Body = "Test",
                    To = { vm.EmailUser } // an sich selbst
                });

                return Json(new { success = true, message = "Verbindung erfolgreich ✅" });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Fehler: " + ex.Message
                });
            }
        }
    }
}
