using BauFlow.Entities;

namespace BauFlow.Services
{
    public static class PlanConfig
    {
        public static readonly Dictionary<Plan, PlanLimits> Plans =
            new()
            {
                [Plan.Free] = new PlanLimits
                {
                    MaxUsers = 1,
                    MaxQuotesPerMonth = 10,
                    ApiAccess = false,
                    Branding = false
                },

                [Plan.Starter] = new PlanLimits
                {
                    MaxUsers = 1,
                    MaxQuotesPerMonth = 50,
                    ApiAccess = false,
                    Branding = false
                },

                [Plan.Pro] = new PlanLimits
                {
                    MaxUsers = 5,
                    MaxQuotesPerMonth = int.MaxValue,
                    ApiAccess = false,
                    Branding = true
                },

                [Plan.Enterprise] = new PlanLimits
                {
                    MaxUsers = int.MaxValue,
                    MaxQuotesPerMonth = int.MaxValue,
                    ApiAccess = true,
                    Branding = true
                }
            };
    }
}
