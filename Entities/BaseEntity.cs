namespace BauFlow.Entities
{
    public class BaseEntity
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
