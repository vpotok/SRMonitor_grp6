using SRMCore.Models;

namespace SRMCore.Services;

public interface ITokenValidationService
{
    Task<TokenValidationResponse?> ValidateTokenAsync(string token);
}
