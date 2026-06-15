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

        public CompanyController(
            ApplicationDbContext context,
            EmailEncryptionService encryptionService)
        {
            _context = context;
            _encryptionService = encryptionService;
        }


        // =========================
        // COMPANY EDIT GET
        // =========================
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            try
            {
                var company = _context.Companies
                    .FirstOrDefault(x =>
                        x.Id == _context.CurrentCompanyId);


                if (company == null)
                    return NotFound();


                return View(new CompanyViewModel
                {
                    Name = company.Name,
                    Address = company.Address,
                    PostalCode = company.PostalCode,
                    City = company.City,
                    Country = company.Country,
                    CEO = company.CEO,
                    Accountant = company.Accountant,
                    TaxNumber = company.TaxNumber,
                    IBAN = company.IBAN,
                    BankName = company.BankName
                });
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.GetBaseException().Message;

                return RedirectToAction("Index", "Home");
            }
        }



        // =========================
        // COMPANY EDIT POST
        // =========================
        [HttpPost]
        public async Task<IActionResult> Edit(
            CompanyViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(vm);


                var company = _context.Companies
                    .FirstOrDefault(x =>
                        x.Id == _context.CurrentCompanyId);


                if (company == null)
                    return NotFound();



                company.Name = vm.Name;
                company.Address = vm.Address;
                company.PostalCode = vm.PostalCode;
                company.City = vm.City;
                company.Country = vm.Country;
                company.TaxNumber = vm.TaxNumber;
                company.IBAN = vm.IBAN;
                company.Accountant = vm.Accountant;
                company.CEO = vm.CEO;
                company.BankName = vm.BankName;


                await _context.SaveChangesAsync();


                TempData["Success"] =
                    "Податоците се успешно зачувани";


                return RedirectToAction("Edit");
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.GetBaseException().Message;

                return View(vm);
            }
        }



        // =========================
        // EMAIL SETTINGS GET
        // =========================
        [HttpGet]
        public async Task<IActionResult> EmailSettings()
        {
            try
            {
                if (!_context.CurrentCompanyId.HasValue)
                    return Unauthorized();


                var company =
                    await _context.Companies
                    .FindAsync(_context.CurrentCompanyId);


                if (company == null)
                    return NotFound();



                var password = "";


                try
                {
                    if (!string.IsNullOrWhiteSpace(
                        company.EmailPassword))
                    {
                        password =
                            _encryptionService.Decrypt(
                                company.EmailPassword);
                    }
                }
                catch
                {
                    // alter kaputter DataProtection Key
                    password = "";
                }



                return View(
                    new CompanyEmailSettingsVM
                    {
                        Id = company.Id,

                        EmailHost = company.EmailHost,

                        EmailPort = company.EmailPort,

                        EmailUser = company.EmailUser,

                        EmailPassword = password,

                        EmailSSL = company.EmailSSL,

                        EmailFrom = company.EmailFrom,

                        EmailFromName =
                            company.EmailFromName
                    });
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.GetBaseException().Message;


                return RedirectToAction(
                    "Index",
                    "Home");
            }
        }



        // =========================
        // EMAIL SETTINGS POST
        // =========================
        [HttpPost]
        public async Task<IActionResult> EmailSettings(
            CompanyEmailSettingsVM vm)
        {
            try
            {
                var company =
                    await _context.Companies
                    .FindAsync(vm.Id);


                if (company == null)
                    return NotFound();



                company.EmailHost =
                    vm.EmailHost;

                company.EmailPort =
                    vm.EmailPort;

                company.EmailUser =
                    vm.EmailUser;

                company.EmailSSL =
                    vm.EmailSSL;

                company.EmailFrom =
                    vm.EmailFrom;

                company.EmailFromName =
                    vm.EmailFromName;



                if (!string.IsNullOrWhiteSpace(
                    vm.EmailPassword))
                {
                    company.EmailPassword =
                        _encryptionService.Encrypt(
                            vm.EmailPassword);
                }



                await _context.SaveChangesAsync();


                TempData["Success"] =
                    "Е-маил податоците се зачувани";


                return RedirectToAction(
                    "EmailSettings");
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.GetBaseException().Message;


                return View(vm);
            }
        }



        // =========================
        // TEST EMAIL
        // =========================
        [HttpPost]
        public async Task<IActionResult> TestEmailSettings(
            [FromBody] CompanyEmailSettingsVM vm)
        {
            try
            {
                using var smtp =
                    new SmtpClient(
                        vm.EmailHost,
                        vm.EmailPort)
                    {
                        Credentials =
                            new NetworkCredential(
                                vm.EmailUser,
                                vm.EmailPassword),

                        EnableSsl =
                            vm.EmailSSL
                    };



                await smtp.SendMailAsync(
                    new MailMessage
                    {
                        From =
                            new MailAddress(
                                vm.EmailFrom),

                        Subject =
                            "Test",

                        Body =
                            "Test",

                        To =
                        {
                            vm.EmailUser
                        }
                    });


                return Json(new
                {
                    success = true,
                    message =
                        "Verbindung erfolgreich ✅"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,

                    message =
                        ex.GetBaseException()
                          .Message
                });
            }
        }
    }
}