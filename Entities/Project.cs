using System.Collections;

namespace BauFlow.Entities
{
    public class Project
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid CustomerId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public string Status { get; set; } // Open, InProgress, Completed

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Customer Customer { get; set; }
        public ICollection<Quote> Quotes { get; set; }
    }
}
