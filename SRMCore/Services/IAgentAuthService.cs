namespace SRMCore.Services;

public interface IAgentAuthService
{
    Task<int?> ValidateAgentAsync(string token);
}
