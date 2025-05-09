namespace SRMCore.Services;

public interface ITokenService
{
    Task<string> RequestTokenAsync(int userId, string role, int customerId);
}
