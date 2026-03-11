using BauFlow.Data;
using BauFlow.Entities;
using BauFlow.Security;
using BauFlow.Services;
using BauFlow.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace BauFlow.Controllers
{
    [RequireTenant]

    [Authorize(Policy= "OwnerOnly")]
    public class UsersController : Controller
    {
        private readonly PlanService _planService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailService _emailService;

        public UsersController(
            ApplicationDbContext context,
            PlanService planService,
            UserManager<ApplicationUser> userManager,
            EmailService emailService)
        {
            _context = context;
            _planService = planService;
            _userManager = userManager;
            _emailService = emailService;
        }

        // =============================
        // USER LIST
        // =============================

        public IActionResult Index()
        {
            var users = _context.AspNetUsers
                .Where(u => u.CompanyId == _context.CurrentCompanyId &&
                            u.Role != UserRole.Owner)
                .ToList();

            return View(users);
        }

        // =============================
        // CREATE USER + INVITE
        // =============================

        [HttpGet]
        public IActionResult Create()
        {
            return View(new UserCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);

            if (!_planService.CanCreateUser())
            {
                ModelState.AddModelError("", "Ihr Plan erlaubt keine weiteren Benutzer.");
                return View(model);
            }

            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Role = model.Role,
                CompanyId = companyId,
                IsInviteAccepted = false,
                InviteSentAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            await SendInvite(user);

            return RedirectToAction(nameof(Index));
        }

        // =============================
        // RESEND INVITE
        // =============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendInvite(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            await SendInvite(user);

            user.InviteSentAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }

        // =============================
        // SET PASSWORD (INVITE)
        // =============================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult SetPassword(string userId, string token)
        {
            var model = new SetPasswordViewModel
            {
                UserId = userId,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
                return NotFound();

            var decodedToken = Encoding.UTF8.GetString(
                WebEncoders.Base64UrlDecode(model.Token));

            var result = await _userManager.ResetPasswordAsync(
                user,
                decodedToken,
                model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }
            user.IsInviteAccepted = true;
            await _userManager.UpdateAsync(user);
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        // =============================
        // DELETE USER
        // =============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
                await _userManager.DeleteAsync(user);

            return RedirectToAction(nameof(Index));
        }

        // =============================
        // Edit 
        // =============================
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();
            return View(user);
        }
   
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ApplicationUser model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
                return NotFound();

            user.Email = model.Email;
            user.UserName = model.Email;
            user.FullName = model.FullName;
            user.Role = model.Role;

            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }
        // =============================
        // INVITE HELPER
        // =============================

        private async Task SendInvite(ApplicationUser user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var encodedToken = WebEncoders.Base64UrlEncode(
                Encoding.UTF8.GetBytes(token));

            var link = Url.Action(
                "SetPassword",
                "Users",
                new { userId = user.Id, token = encodedToken },
                Request.Scheme);

            await _emailService.SendInvite(user.Email, user.FullName, link);
        }
    }
}