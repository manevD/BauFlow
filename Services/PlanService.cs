using BauFlow.Data;
using BauFlow.Entities;
using BauFlow.Interfaces;

namespace BauFlow.Services
{
    public class PlanService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenant;

        public PlanService(ApplicationDbContext context, ITenantProvider tenant)
        {
            _context = context;
            _tenant = tenant;
        }

        private PlanLimits GetLimits()
        {
            var companyId = _tenant.GetCompanyId();

            var plan = _context.Companies
                .Where(c => c.Id == companyId)
                .Select(c => c.Plan)
                .First();

            return PlanConfig.Plans[plan];
        }

        public bool CanCreateUser()
        {
            var limits = GetLimits();
            var companyId = _tenant.GetCompanyId();

            var userCount = _context.Users.Count(u => u.CompanyId == companyId);

            return userCount < limits.MaxUsers;
        }

        public bool CanCreateQuote()
        {
            var limits = GetLimits();
            var companyId = _tenant.GetCompanyId();

            var monthCount = _context.Quotes.Count(q =>
                q.CompanyId == companyId &&
                q.CreatedAt.Month == DateTime.UtcNow.Month &&
                q.CreatedAt.Year == DateTime.UtcNow.Year);

            return monthCount < limits.MaxQuotesPerMonth;
        }

        public bool HasApiAccess()
            => GetLimits().ApiAccess;

        public bool HasBranding()
            => GetLimits().Branding;
    }
}
