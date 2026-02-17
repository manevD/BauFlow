using Microsoft.AspNetCore.Identity;

namespace BauFlow.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public Guid CompanyId { get; set; }

        public string? FullName { get; set; }

        public UserRole Role { get; set; } = UserRole.Member;
    }
    public enum UserRole
    {
        Owner = 1,
        Admin = 2,
        Member = 3
    }
}
