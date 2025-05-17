using Microsoft.EntityFrameworkCore;
using SRMCore.Data;

namespace SRMCore.Services;

public class AgentAuthService : IAgentAuthService
{
    private readonly CoreDbContext _db;

    public AgentAuthService(CoreDbContext db)
    {
        _db = db;
    }

    public async Task<int?> ValidateAgentAsync(string token)
    {
        var agent = await _db.Agents.FirstOrDefaultAsync(a => a.AuthToken == token && a.Enabled);
        return agent?.ComId;
    }
}
