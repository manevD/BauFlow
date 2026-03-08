using BauFlow.Entities;
using System.ComponentModel.DataAnnotations;

namespace BauFlow.ViewModels
{
    public class UserCreateViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public string FullName { get; set; }

        public UserRole Role { get; set; } = UserRole.Member;
    }
}
