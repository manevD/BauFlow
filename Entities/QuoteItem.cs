namespace BauFlow.Entities
{
    public class QuoteItem
    {
        public Guid Id { get; set; }
        public Guid QuoteId { get; set; }

        public string Description { get; set; }

        public decimal Quantity { get; set; }
        public string Unit { get; set; }

        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
