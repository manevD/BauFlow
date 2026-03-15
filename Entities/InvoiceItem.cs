namespace BauFlow.Entities
{
    public class InvoiceItem
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }

        public string Description { get; set; }

        public decimal Quantity { get; set; }
        public string Unit { get; set; }

        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public Invoice Invoice { get; set; }
    }
}
