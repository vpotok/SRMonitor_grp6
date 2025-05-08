using SRMCore.Data;
using SRMCore.Models;

namespace SRMCore.Services;

public class AuthService
{
    private readonly AppDbContext _dbContext;

    public AuthService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public User? Authenticate(string username, string password)
    {
        var user = _dbContext.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == password);
        return user;
    }
}