#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using BauFlow.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace BauFlow.Areas.Identity.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResetPasswordModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }


        public class InputModel
        {
            [Required(ErrorMessage = "Bitte E-Mail eingeben")]
            [EmailAddress(ErrorMessage = "Ungültige E-Mail-Adresse")]
            [Display(Name = "E-Mail")]
            public string Email { get; set; }


            [Required(ErrorMessage = "Bitte Passwort eingeben")]
            [StringLength(100,
                ErrorMessage = "{0} muss mindestens {2} und maximal {1} Zeichen lang sein.",
                MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Neues Passwort")]
            public string Password { get; set; }


            [DataType(DataType.Password)]
            [Display(Name = "Passwort bestätigen")]
            [Compare("Password", ErrorMessage = "Passwort und Bestätigung stimmen nicht überein.")]
            public string ConfirmPassword { get; set; }


            [Required]
            public string Code { get; set; }
        }


        public IActionResult OnGet(string code = null)
        {
            if (code == null)
                return BadRequest("Ein Reset-Code muss angegeben werden.");

            Input = new InputModel
            {
                Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
            };

            return Page();
        }



        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();


            var user = await _userManager.FindByEmailAsync(Input.Email);

            // Sicherheitsmaßnahme → nicht verraten ob User existiert
            if (user == null)
                return RedirectToPage("./ResetPasswordConfirmation");


            var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);

            if (result.Succeeded)
                return RedirectToPage("./ResetPasswordConfirmation");


            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);


            return Page();
        }
    }
}