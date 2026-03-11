using System.Collections;

namespace BauFlow.Entities
{
    public class Project : BaseEntity
    {
        public Guid CustomerId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public string Status { get; set; } // Open, InProgress, Completed


        public Customer Customer { get; set; }
        public ICollection<Quote> Quotes { get; set; }
    }
}
