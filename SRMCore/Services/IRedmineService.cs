namespace SRMCore.Services;

public interface IRedmineService
{
    Task CreateTicketAsync(string subject, string description);
}
