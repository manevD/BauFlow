using BauFlow.Entities;
using DataAnnotationsExtensions;

namespace BauFlow.ViewModels
{
    public class UserCreateViewModel
    {
        [Email]
        public string Email { get; set; }

        public Guid CompanyId { get; set; }

        public string? FullName { get; set; }

        public UserRole Role { get; set; } = UserRole.Member;
    }
}
