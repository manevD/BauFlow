namespace BauFlow.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }

        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public string PaymentMethod { get; set; } // Bank, Cash, Stripe

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
