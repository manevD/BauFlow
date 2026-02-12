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

        public Guid GetCompanyId()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !user.Identity.IsAuthenticated)
                throw new Exception("User nicht authentifiziert.");

            var companyIdClaim = user.FindFirst("CompanyId");

            if (companyIdClaim == null)
                throw new Exception("CompanyId Claim fehlt.");

            return Guid.Parse(companyIdClaim.Value);
        }
    }

}
