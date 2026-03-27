using System.ComponentModel.DataAnnotations;

namespace BauFlow.Entities
{
    public class Company
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // BASIC
        [Required, MaxLength(200)]
        public string Name { get; set; }

        // ADDRESS
        [Required, MaxLength(300)]
        public string Address { get; set; }

        [Required, MaxLength(20)]
        public string PostalCode { get; set; }

        [Required, MaxLength(150)]
        public string City { get; set; }

        [Required, MaxLength(150)]
        public string Country { get; set; }

        // TAX
        [MaxLength(50)]
        public string? TaxNumber { get; set; }

        [MaxLength(50)]
        public string? VatId { get; set; }

        public bool IsSmallBusiness { get; set; }

        // BRANDING
        [MaxLength(500)]
        public string? LogoPath { get; set; }

        // SUBSCRIPTION
        public Plan Plan { get; set; }

        public string? StripeCustomerId { get; set; }
        public string? StripeSubscriptionId { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime? SubscriptionEndDate { get; set; }
        public bool IsTrial { get; set; }
        public DateTime? TrialEndsAt { get; set; }
        public bool IsSuspended { get; set; }

        // AUDIT
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string EmailHost { get; set; }
        public int EmailPort { get; set; }
        public string EmailUser { get; set; }
        [DataType(DataType.Password)]
        public string EmailPassword { get; set; }
        public bool EmailSSL { get; set; }

        public string EmailFrom { get; set; }
        public string EmailFromName { get; set; }
    }
    public enum Plan
    {
        [Display(Name = "Kostenlos")]
        Free,

        [Display(Name = "Starter")]
        Starter,

        [Display(Name = "Pro")]
        Pro,

        [Display(Name = "Enterprise")]
        Enterprise
    }
}
