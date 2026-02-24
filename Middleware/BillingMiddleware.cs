using BauFlow.Data;
using Microsoft.AspNetCore.Authorization;

namespace BauFlow.Middleware
{
    public class BillingMiddleware
    {
        private readonly RequestDelegate _next;

        public BillingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
        {
            var endpoint = context.GetEndpoint();

            // ✅ AllowAnonymous respektieren (Login, Register etc.)
            if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
            {
                await _next(context);
                return;
            }

            // ✅ Nicht eingeloggte durchlassen (Authorize kümmert sich)
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await _next(context);
                return;
            }

            var companyIdClaim = context.User.FindFirst("CompanyId");

            if (companyIdClaim == null)
            {
                await _next(context);
                return;
            }

            var companyId = Guid.Parse(companyIdClaim.Value);
            var company = await db.Companies.FindAsync(companyId);

            if (company == null)
            {
                context.Response.Redirect("/");
                return;
            }

            bool isApi = context.Request.Path.StartsWithSegments("/api");

            if (!company.IsActive || company.IsSuspended)
            {
                if (isApi)
                    context.Response.StatusCode = 403;
                else
                    context.Response.Redirect("/Billing/Locked");

                return;
            }

            if (company.IsTrial && company.TrialEndsAt < DateTime.UtcNow)
            {
                context.Response.Redirect("/Billing/Expired");
                return;
            }

            if (!company.IsTrial && company.SubscriptionEndDate < DateTime.UtcNow)
            {
                context.Response.Redirect("/Billing/Expired");
                return;
            }

            await _next(context);
        }
    }
}
