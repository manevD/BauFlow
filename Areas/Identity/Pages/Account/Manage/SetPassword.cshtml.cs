using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BauFlow.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BauFlow.Areas.Identity.Pages.Account.Manage
{
    public class SetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public SetPasswordModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Лозинката е задолжителна.")]
            [StringLength(100, ErrorMessage = "Лозинката мора да има најмалку {2} и најмногу {1} карактери.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Нова лозинка")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Потврди лозинка")]
            [Compare("NewPassword", ErrorMessage = "Лозинките не се совпаѓаат.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Корисникот не може да се вчита (ID: '{_userManager.GetUserId(User)}').");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);

            if (hasPassword)
            {
                return RedirectToPage("./ChangePassword");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Корисникот не може да се вчита (ID: '{_userManager.GetUserId(User)}').");
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(user, Input.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                foreach (var error in addPasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Лозинката е успешно поставена.";

            return RedirectToPage();
        }
    }
}