namespace BauFlow.Providers
{
    using BauFlow.Interfaces;
    using Microsoft.AspNetCore.Http;
    using System.Security.Claims;

    public class TenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? GetCompanyId()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user?.Identity?.IsAuthenticated != true)
                return null;

            var claim = user.FindFirst("CompanyId");
            if (claim == null)
                return null;

            return Guid.Parse(claim.Value);
        }
    }

}
