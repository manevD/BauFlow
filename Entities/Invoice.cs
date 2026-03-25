using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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
        public int  TaxRate { get; set; } 

        public Customer Customer { get; set; }
        public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

    }

    public enum InvoiceStatus
    {
        [Display(Name = "Entwurf")]
        Draft = 1,

        [Display(Name = "Gesendet")]
        Sent = 2,

        [Display(Name = "Bezahlt")]
        Paid = 3,

        [Display(Name = "Überfällig")]
        Overdue = 4,

        [Display(Name = "Storniert")]
        Cancelled = 5
    }
}
