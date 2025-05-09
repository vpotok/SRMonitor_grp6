using Microsoft.EntityFrameworkCore;
using SRMCore.Data;
using SRMCore.Models;

namespace SRMCore.Services;

public class UserService : IUserService
{
    private readonly CoreDbContext _db;

    public UserService(CoreDbContext db)
    {
        _db = db;
    }

    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        var user = await _db.Users.Include(u => u.Customer)
                                  .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
            return null;

        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash) ? user : null;
    }
}
