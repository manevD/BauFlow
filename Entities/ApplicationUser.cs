using Microsoft.AspNetCore.Identity;

namespace BauFlow.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public Guid CompanyId { get; set; }

        public string FullName { get; set; }

        public UserRole Role { get; set; }
    }
    public enum UserRole
    {
        Owner,
        Admin,
        Member
    }
}
