public interface IRedmineService
{
    Task CreateTicketAsync(int comId, string subject, string description);
}
