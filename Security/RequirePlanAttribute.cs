using BauFlow.Entities;
using Microsoft.AspNetCore.Authorization;

namespace BauFlow.Security
{
    public class RequirePlanAttribute : AuthorizeAttribute
    {
        public RequirePlanAttribute(Plan plan)
        {
            Policy = $"Plan_{plan}";
        }
    }
}
