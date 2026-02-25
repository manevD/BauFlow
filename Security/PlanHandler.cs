using BauFlow.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
namespace BauFlow.Security
{
    public class PlanHandler : AuthorizationHandler<PlanRequirement>
    {
        private readonly ApplicationDbContext _db;

        public PlanHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PlanRequirement requirement)
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

            if (company.Plan >= requirement.RequiredPlan)
                context.Succeed(requirement);
        }
    }

}