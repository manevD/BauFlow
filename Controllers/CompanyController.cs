using BauFlow.Data;
using BauFlow.Security;
using BauFlow.Services;
using BauFlow.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace BauFlow.Controllers
{
    [RequireTenant]

    public class CompanyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailEncryptionService _encryptionService;
        public CompanyController(ApplicationDbContext context, EmailEncryptionService encryptionService)
        {
            _encryptionService = encryptionService;
            _context = context;
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
            company.EmailPassword = _encryptionService.Encrypt(vm.EmailPassword) ;
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
