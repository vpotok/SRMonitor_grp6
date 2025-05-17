using SRMCore.Models;

namespace SRMCore.Services;

public interface IUserService
{
    Task<User?> AuthenticateAsync(string username, string password);
}
