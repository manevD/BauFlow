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
            [Required(ErrorMessage = "Внесете е-пошта")]
            [EmailAddress(ErrorMessage = "Невалидна е-пошта")]
            [Display(Name = "Е-пошта")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Внесете лозинка")]
            [StringLength(100,
                ErrorMessage = "{0} мора да има најмалку {2} и најмногу {1} карактери.",
                MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Нова лозинка")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Потврди лозинка")]
            [Compare("Password", ErrorMessage = "Лозинките не се совпаѓаат.")]
            public string ConfirmPassword { get; set; }

            [Required]
            public string Code { get; set; }
        }

        public IActionResult OnGet(string code = null)
        {
            if (code == null)
                return BadRequest("Потребен е код за ресетирање.");

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

            // Безбедност → не откривај дали постои корисник
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