using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SRMCore.Data;
using SRMCore.Models;
using SRMCore.Services;

namespace SRMCore.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IPController : ControllerBase
{
    private readonly CoreDbContext _db;
    private readonly ITokenValidationService _tokenValidator;
    private readonly IAgentAuthService _agentAuth;

    public IPController(CoreDbContext db, ITokenValidationService tokenValidator, IAgentAuthService agentAuth)
    {
        _db = db;
        _tokenValidator = tokenValidator;
        _agentAuth = agentAuth;
    }

    [HttpGet]
    public async Task<IActionResult> GetForUser()
    {
        var jwt = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var token = await _tokenValidator.ValidateTokenAsync(jwt);
        if (token == null || !token.Valid) return Unauthorized();

        var ips = await _db.IPs
            .Where(i => i.ComId == token.CustomerId)
            .Select(i => i.IpAddress)
            .ToListAsync();

        return Ok(ips);
    }

    [HttpPost]
    public async Task<IActionResult> AddIp([FromBody] string ip)
    {
        var jwt = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var token = await _tokenValidator.ValidateTokenAsync(jwt);
        if (token == null || !token.Valid || token.Role != "customeradmin") return Forbid();

        if (await _db.IPs.AnyAsync(i => i.ComId == token.CustomerId && i.IpAddress == ip))
            return Conflict("IP existiert bereits.");

        _db.IPs.Add(new IP { ComId = token.CustomerId, IpAddress = ip });
        await _db.SaveChangesAsync();

        return Ok(new { status = "added", ip });
    }

    [HttpDelete("{ip}")]
    public async Task<IActionResult> DeleteIp(string ip)
    {
        var jwt = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var token = await _tokenValidator.ValidateTokenAsync(jwt);
        if (token == null || !token.Valid || token.Role != "customeradmin") return Forbid();

        var entry = await _db.IPs.FirstOrDefaultAsync(i => i.ComId == token.CustomerId && i.IpAddress == ip);
        if (entry == null) return NotFound();

        _db.IPs.Remove(entry);
        await _db.SaveChangesAsync();

        return Ok(new { status = "deleted", ip });
    }

    [HttpGet("agent")]
    public async Task<IActionResult> GetForAgent()
    {
        var token = Request.Headers["X-AGENT-TOKEN"].ToString();
        if (string.IsNullOrWhiteSpace(token)) return Unauthorized();

        var comId = await _agentAuth.ValidateAgentAsync(token);
        if (comId == null) return Unauthorized();

        var ips = await _db.IPs
            .Where(i => i.ComId == comId)
            .Select(i => i.IpAddress)
            .ToListAsync();

        return Ok(ips);
    }
}
