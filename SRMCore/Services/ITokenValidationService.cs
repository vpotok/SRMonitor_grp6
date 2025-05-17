using SRMCore.Models;

namespace SRMCore.Services;

public interface ITokenValidationService
{
    Task<TokenValidationResult?> ValidateTokenAsync(string token);
}
