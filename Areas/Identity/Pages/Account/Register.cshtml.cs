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

            [Required(ErrorMessage = "Внесете име на фирма.")]
            [Display(Name = "Име на фирма")]
            public string CompanyName { get; set; }

            [Required(ErrorMessage = "Внесете е-пошта.")]
            [EmailAddress(ErrorMessage = "Внесете валидна е-пошта.")]
            [Display(Name = "Е-пошта")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Внесете лозинка.")]
            [StringLength(100,
                ErrorMessage = "Лозинката мора да има најмалку {2} и најмногу {1} карактери.",
                MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Лозинка")]
            public string Password { get; set; }

            [Required(ErrorMessage = "Потврдете ја лозинката.")]
            [DataType(DataType.Password)]
            [Display(Name = "Потврди лозинка")]
            [Compare("Password", ErrorMessage = "Лозинките не се совпаѓаат.")]
            public string ConfirmPassword { get; set; }


            // ========================
            // COMPANY ADDRESS
            // ========================

            [Required(ErrorMessage = "Внесете адреса.")]
            [Display(Name = "Адреса")]
            public string Address { get; set; }

            [Required(ErrorMessage = "Внесете поштенски број.")]
            [Display(Name = "Поштенски број")]
            public string PostalCode { get; set; }

            [Required(ErrorMessage = "Внесете град.")]
            [Display(Name = "Град")]
            public string City { get; set; }

            [Required(ErrorMessage = "Внесете држава.")]
            [Display(Name = "Држава")]
            public string Country { get; set; }


            // ========================
            // TAX
            // ========================

            [Display(Name = "Даночен број")]
            public string? TaxNumber { get; set; }

            [Display(Name = "ДДВ број")]
            public string? VatId { get; set; }

            [Display(Name = "Мало претпријатие")]
            public bool IsSmallBusiness { get; set; }

            [Required(ErrorMessage = "Жиросметката е задолжителна")]
            public string IBAN { get; set; }


            // ========================
            // BRANDING
            // ========================

            [Display(Name = "Лого")]
            public IFormFile? Logo { get; set; }


            // ========================
            // SUBSCRIPTION
            // ========================

            [Required(ErrorMessage = "Изберете план.")]
            public Plan Plan { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = [.. (await _signInManager.GetExternalAuthenticationSchemesAsync())];
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            _ = returnUrl ?? Url.Content("~/");
            ExternalLogins = [.. (await _signInManager.GetExternalAuthenticationSchemesAsync())];
           
            if (!PlanConfig.Plans.ContainsKey(Input.Plan))
            {
                ModelState.AddModelError("", "Невалиден план.");
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
                        IBAN = Input.IBAN,
                        VatId = Input.VatId,
                        IsSmallBusiness = Input.IsSmallBusiness,
                        IsTrial = false,
                        Plan = Input.Plan,
                        SubscriptionEndDate = DateTime.UtcNow.AddYears(1),
                        IsSuspended = false,
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
