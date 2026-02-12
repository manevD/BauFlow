namespace BauFlow.Entities
{
    public class Company
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public string TaxNumber { get; set; }
        public string VatId { get; set; }
        public bool IsSmallBusiness { get; set; }

        public string LogoPath { get; set; }

        // Subscription
        public string StripeCustomerId { get; set; }
        public string StripeSubscriptionId { get; set; }
        public string Plan { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
