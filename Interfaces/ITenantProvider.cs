namespace BauFlow.Interfaces
{
    public interface ITenantProvider
    {
        Guid? GetCompanyId();
    }
}
