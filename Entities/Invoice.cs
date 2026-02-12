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

    }

    public enum InvoiceStatus
    {
        Open,
        Paid,
        Overdue,
        Cancelled
    }
}
