
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
        var user = await _db.Users
            .Where(u => u.UserName == username)
            .Select(u => new User
            {
                UserId = u.UserId,
                ComId = u.ComId,
                Role = u.Role,
                UserName = u.UserName,
                PasswordHash = u.PasswordHash
            })
            .FirstOrDefaultAsync();

        if (user == null) return null;

        var passwordMatches = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (!passwordMatches) return null;

        Console.WriteLine($"✅ LOGIN: user_id={user.UserId}, com_id={user.ComId}, role={user.Role}");
        return user;
    }
}