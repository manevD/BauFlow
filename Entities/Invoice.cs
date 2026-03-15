namespace BauFlow.Entities
{
    public class Invoice : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Guid? QuoteId { get; set; }

        public string InvoiceNumber { get; set; }

        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }

        public InvoiceStatus Status { get; set; }

        public decimal NetAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrossAmount { get; set; }

        public Customer Customer { get; set; }
        public ICollection<QuoteItem> Items { get; set; }

    }

    public enum InvoiceStatus
    {
        Draft = 1,
        Sent = 2,
        Paid = 3,
        Overdue = 4,
        Cancelled = 5
    }
}
