using System.ComponentModel.DataAnnotations;

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
        public ICollection<QuoteItem> Items { get; set; } = new List<QuoteItem>();

    }
    public enum QuoteStatus
    {
        [Display(Name = "Entwurf")]
        Draft = 0,

        [Display(Name = "Gesendet")]
        Sent = 1,

        [Display(Name = "Angenommen")]
        Accepted = 2,

        [Display(Name = "Abgelehnt")]
        Rejected = 3
    }
}
