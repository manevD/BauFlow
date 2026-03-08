using Microsoft.AspNetCore.Identity;
using System.ComponentModel;

namespace BauFlow.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public Guid CompanyId { get; set; }

        [DisplayName("Vollname")]
        public string? FullName { get; set; }

        [DisplayName("Rolle")]
        public UserRole Role { get; set; } = UserRole.Member;
        // Invite System
        public bool IsInviteAccepted { get; set; }

        public DateTime? InviteSentAt { get; set; }

        public DateTime? InviteAcceptedAt { get; set; }
    }
    public enum UserRole
    {
        Owner = 1,
        Admin = 2,
        Member = 3
    }
}
