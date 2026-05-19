using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace BauFlow.Entities
{
    public class Invoice : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Guid? QuoteId { get; set; }
        [MaxLength(1024)]
        public string? Description { get; set; }
        [ValidateNever]
        public string InvoiceNumber { get; set; }

        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public Company? Company { get; set; }
        public InvoiceStatus Status { get; set; }

        public decimal NetAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrossAmount { get; set; }
        public int  TaxRate { get; set; }
        [ValidateNever]
        public Customer Customer { get; set; }
        public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

    }

    public enum InvoiceStatus
    {
        [Display(Name = "Нацрт")]
        Draft = 1,

        [Display(Name = "Испратена")]
        Sent = 2,

        [Display(Name = "Платена")]
        Paid = 3,

        [Display(Name = "Доцни")]
        Overdue = 4,

        [Display(Name = "Откажана")]
        Cancelled = 5
    }
}
