namespace BauFlow.Entities
{
    public class Quote : BaseEntity
    {
        public Guid CustomerId { get; set; }

        public string QuoteNumber { get; set; }

        public DateTime QuoteDate { get; set; }
        public DateTime? ValidUntil { get; set; }

        public QuoteStatus Status { get; set; }

        public decimal NetAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrossAmount { get; set; }

        public Customer Customer { get; set; }
        public ICollection<QuoteItem> Items { get; set; }

    }
    public enum QuoteStatus
    {
        Draft,
        Sent,
        Accepted,
        Rejected
    }
}
