#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using BauFlow.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace BauFlow.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Bitte E-Mail eingeben")]
            [EmailAddress(ErrorMessage = "Ungültige E-Mail-Adresse")]
            [Display(Name = "E-Mail")]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();


            var user = await _userManager.FindByEmailAsync(Input.Email);

            // Sicherheitsmaßnahme:
            // Nicht verraten ob User existiert oder bestätigt ist
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                return RedirectToPage("./ForgotPasswordConfirmation");


            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                null,
                new { area = "Identity", code },
                Request.Scheme);


            await _emailSender.SendEmailAsync(
                Input.Email,
                "Passwort zurücksetzen",
                $"<p>Hallo,</p>" +
                $"<p>Sie haben angefordert, Ihr Passwort zurückzusetzen.</p>" +
                $"<p>Klicken Sie auf den folgenden Link:</p>" +
                $"<p><a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Passwort zurücksetzen</a></p>" +
                $"<p>Falls Sie diese Anfrage nicht gestellt haben, können Sie diese E-Mail ignorieren.</p>"
            );

            return RedirectToPage("./ForgotPasswordConfirmation");
        }
    }
}