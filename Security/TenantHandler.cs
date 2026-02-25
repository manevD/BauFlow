using BauFlow.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BauFlow.Security
{
   
    public class TenantHandler(ApplicationDbContext db) : AuthorizationHandler<TenantRequirement>
    {
        private readonly ApplicationDbContext _db = db;

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            TenantRequirement requirement)
        {
            var companyIdClaim = context.User.FindFirst("CompanyId");

            if (companyIdClaim == null)
                return;

            var companyId = Guid.Parse(companyIdClaim.Value);

            var company = await _db.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == companyId);

            if (company == null)
                return;

            if (!company.IsActive || company.IsSuspended)
                return;

            if (!company.IsTrial &&
                company.SubscriptionEndDate < DateTime.UtcNow)
                return;

            if (company.IsTrial &&
                company.TrialEndsAt < DateTime.UtcNow)
                return;

            context.Succeed(requirement);
        }
    }
}
