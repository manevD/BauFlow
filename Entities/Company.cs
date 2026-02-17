using System.ComponentModel.DataAnnotations;

namespace BauFlow.Entities
{
    public class Company
    {
        public Guid Id { get; set; } = Guid.NewGuid();


        // =========================
        // BASIC INFO
        // =========================

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }


        // =========================
        // ADDRESS
        // =========================

        [Required]
        [MaxLength(300)]
        public string Address { get; set; }

        [Required]
        [MaxLength(20)]
        public string PostalCode { get; set; }

        [Required]
        [MaxLength(150)]
        public string City { get; set; }

        [Required]
        [MaxLength(150)]
        public string Country { get; set; }


        // =========================
        // TAX INFO
        // =========================

        [MaxLength(50)]
        public string? TaxNumber { get; set; }

        [MaxLength(50)]
        public string? VatId { get; set; }

        public bool IsSmallBusiness { get; set; }


        // =========================
        // BRANDING
        // =========================

        [MaxLength(500)]
        public string? LogoPath { get; set; }


        // =========================
        // SUBSCRIPTION
        // =========================

        [MaxLength(100)]
        public string? StripeCustomerId { get; set; }

        [MaxLength(100)]
        public string? StripeSubscriptionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Plan { get; set; } 

        public bool IsActive { get; set; } = true;


        // =========================
        // AUDIT
        // =========================

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
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
