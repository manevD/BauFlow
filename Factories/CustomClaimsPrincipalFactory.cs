namespace BauFlow.Factories
{
    using BauFlow.Entities;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Options;
    using System.Security.Claims;

    public class CustomClaimsPrincipalFactory
        : UserClaimsPrincipalFactory<ApplicationUser>
    {
        public CustomClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            IOptions<IdentityOptions> options)
            : base(userManager, options)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            identity.AddClaim(new Claim("CompanyId", user.CompanyId.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()));

            return identity;
        }
    }

}
