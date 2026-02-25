using BauFlow.Entities;
using Microsoft.AspNetCore.Authorization;
namespace BauFlow.Security
{


    public class PlanRequirement : IAuthorizationRequirement
    {
        public Plan RequiredPlan { get; }

        public PlanRequirement(Plan plan)
        {
            RequiredPlan = plan;
        }
    }
}
