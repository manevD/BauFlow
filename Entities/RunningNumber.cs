namespace BauFlow.Entities
{
    public class RunningNumber
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        public string Type { get; set; } // Quote, Invoice

        public int CurrentNumber { get; set; }
    }
}
