// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using BauFlow.Data;
using BauFlow.Entities;
using BauFlow.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BauFlow.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            ApplicationDbContext context,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _context = context;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            // ========================
            // ACCOUNT
            // ========================

            [Required(ErrorMessage = "Bitte geben Sie einen Firmennamen ein.")]
            [Display(Name = "Firmenname")]
            public string CompanyName { get; set; }

            [Required(ErrorMessage = "Bitte geben Sie eine E-Mail-Adresse ein.")]
            [EmailAddress(ErrorMessage = "Bitte geben Sie eine gültige E-Mail-Adresse ein.")]
            [Display(Name = "E-Mail-Adresse")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Bitte geben Sie ein Passwort ein.")]
            [StringLength(100,
                ErrorMessage = "Das Passwort muss mindestens {2} und maximal {1} Zeichen lang sein.",
                MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Passwort")]
            public string Password { get; set; }

            [Required(ErrorMessage = "Bitte bestätigen Sie Ihr Passwort.")]
            [DataType(DataType.Password)]
            [Display(Name = "Passwort bestätigen")]
            [Compare("Password", ErrorMessage = "Die Passwörter stimmen nicht überein.")]
            public string ConfirmPassword { get; set; }


            // ========================
            // COMPANY ADDRESS
            // ========================

            [Required(ErrorMessage = "Bitte Adresse eingeben.")]
            [Display(Name = "Adresse")]
            public string Address { get; set; }

            [Required(ErrorMessage = "Bitte Postleitzahl eingeben.")]
            [Display(Name = "PLZ")]
            public string PostalCode { get; set; }

            [Required(ErrorMessage = "Bitte Stadt eingeben.")]
            [Display(Name = "Stadt")]
            public string City { get; set; }

            [Required(ErrorMessage = "Bitte Land eingeben.")]
            [Display(Name = "Land")]
            public string Country { get; set; }


            // ========================
            // TAX
            // ========================

            [Display(Name = "Steuernummer")]
            public string? TaxNumber { get; set; }

            [Display(Name = "USt-ID")]
            public string? VatId { get; set; }

            [Display(Name = "Kleinunternehmer")]
            public bool IsSmallBusiness { get; set; }


            // ========================
            // BRANDING
            // ========================

            [Display(Name = "Logo")]
            public IFormFile? Logo { get; set; }


            // ========================
            // SUBSCRIPTION (hidden/system)
            // ========================

            [Required]
            public Plan Plan { get; set; }
        }




        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
           
            if (!PlanConfig.Plans.ContainsKey(Input.Plan))
            {
                ModelState.AddModelError("", "Ungültiger Plan.");
                return Page();
            }
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    var company = new Company
                    {
                        Id = Guid.NewGuid(),
                        Name = Input.CompanyName,
                        Address = Input.Address,
                        PostalCode = Input.PostalCode,
                        City = Input.City,
                        Country = Input.Country,
                        TaxNumber = Input.TaxNumber,
                        VatId = Input.VatId,
                        IsSmallBusiness = Input.IsSmallBusiness,

                        Plan = Input.Plan, // 🔥 HIER wird Plan gesetzt

                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };


                    _context.Companies.Add(company);
                    await _context.SaveChangesAsync();

                    // 🔥 User Company zuweisen
                    user.CompanyId = company.Id;
                    user.Role = UserRole.Owner;

                    await _userManager.UpdateAsync(user);
                    return RedirectToPage("/");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
