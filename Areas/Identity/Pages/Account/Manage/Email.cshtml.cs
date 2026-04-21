using BauFlow.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace BauFlow.Areas.Identity.Pages.Account.Manage
{
    public class EmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public EmailModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        public string Email { get; set; }
        public bool IsEmailConfirmed { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Е-поштата е задолжителна.")]
            [EmailAddress(ErrorMessage = "Внесете валидна е-пошта.")]
            public string NewEmail { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var email = await _userManager.GetEmailAsync(user);
            Email = email;

            Input = new InputModel
            {
                NewEmail = email,
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Корисникот не може да се вчита (ID: '{_userManager.GetUserId(User)}').");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Корисникот не може да се вчита (ID: '{_userManager.GetUserId(User)}').");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var email = await _userManager.GetEmailAsync(user);
            if (Input.NewEmail != email)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmailChange",
                    null,
                    new { area = "Identity", userId = userId, email = Input.NewEmail, code = code },
                    Request.Scheme);

                await _emailSender.SendEmailAsync(
                    Input.NewEmail,
                    "Потврда за е-пошта",
                    $"Потврдете ја новата е-пошта со клик <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>тука</a>.");

                StatusMessage = "Испратен е линк за потврда. Проверете ја вашата е-пошта.";
                return RedirectToPage();
            }

            StatusMessage = "Е-поштата не е променета.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Корисникот не може да се вчита (ID: '{_userManager.GetUserId(User)}').");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                null,
                new { area = "Identity", userId = userId, code = code },
                Request.Scheme);

            await _emailSender.SendEmailAsync(
                email,
                "Потврда на е-пошта",
                $"Потврдете ја вашата е-пошта со клик <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>тука</a>.");

            StatusMessage = "Испратена е е-пошта за потврда.";
            return RedirectToPage();
        }
    }
}