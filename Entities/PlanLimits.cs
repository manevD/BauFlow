namespace BauFlow.Entities
{
    public class PlanLimits 
    {
        public int MaxUsers { get; init; }
        public int MaxQuotesPerMonth { get; init; }
        public bool ApiAccess { get; init; }
        public bool Branding { get; init; }
    }
}
