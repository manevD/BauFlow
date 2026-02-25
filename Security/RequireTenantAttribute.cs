using Microsoft.AspNetCore.Authorization;

namespace BauFlow.Security
{
    public class RequireTenantAttribute : AuthorizeAttribute
    {
        public RequireTenantAttribute()
        {
            Policy = "TenantActive";
        }
    }
}
