namespace BauFlow.Entities
{
    public class Customer : BaseEntity
    {

        public string Name { get; set; }
        public string ContactPerson { get; set; }

        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }

        public string Notes { get; set; }

    }
}
