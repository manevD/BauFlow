using BauFlow.Data;
using BauFlow.Entities;
using BauFlow.Security;
using BauFlow.Services;
using BauFlow.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BauFlow.Controllers
{
    [RequireTenant]

    public class UsersController(ApplicationDbContext context, PlanService planService) : Controller
    {
        private readonly PlanService _planService = planService;
        private readonly ApplicationDbContext _context = context;
        public IActionResult Index()
        {
            var users = _context.AspNetUsers.Where(u => u.CompanyId == _context.CurrentCompanyId && u.Role != UserRole.Owner).ToList();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View(new UserCreateViewModel());
        }
     
        [HttpPost]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);

            if (!_planService.CanCreateUser())
            {
                ModelState.AddModelError("",
                    "Ihr Plan erlaubt keine weiteren Benutzer. Bitte upgraden Sie.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                Email = model.Email,
                FullName = model.FullName,
                Role = model.Role,
                CompanyId = companyId
            };
            await _context.AspNetUsers.AddAsync(user);
            await _context.SaveChangesAsync();
            // User erstellen
            return RedirectToAction(nameof(Index));
        }
      
        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string? id)
        {
            var customer = await _context.AspNetUsers.FindAsync(id);
            if (customer != null)
            {
                _context.AspNetUsers.Remove(customer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(Guid id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }
    }
}
