using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<IPController> _logger;

    public IPController(CoreDbContext db, ITokenValidationService tokenValidator, IAgentAuthService agentAuth, ILogger<IPController> logger)
    {
        _db = db;
        _tokenValidator = tokenValidator;
        _agentAuth = agentAuth;
        _logger = logger;
    }

    private string? ExtractBearerToken()
    {
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        _logger.LogDebug("Authorization Header: {Header}", authHeader);

        var cookieToken = Request.Cookies["jwt"];
        if (!string.IsNullOrWhiteSpace(cookieToken))
        {
            _logger.LogDebug("JWT extrahiert aus Cookie: {Token}", cookieToken);
            return cookieToken;
        }

        _logger.LogWarning("Kein gültiger Authorization Header oder Cookie vorhanden.");
        return null;
    }

    [HttpGet]
    public async Task<IActionResult> GetForUser()
    {
        _logger.LogInformation("📥 [GET] /api/ip aufgerufen.");

        var jwt = ExtractBearerToken();
        if (jwt == null)
        {
            _logger.LogWarning("🔒 Zugriff ohne JWT");
            return Unauthorized();
        }

        var token = await _tokenValidator.ValidateTokenAsync(jwt);
        if (token == null || !token.Valid)
        {
            _logger.LogWarning("🔑 Ungültiger oder abgelaufener Token.");
            return Unauthorized();
        }

        _logger.LogInformation("✅ Gültiger Token für ComId={ComId}, Role={Role}", token.CustomerId, token.Role);

        var ips = await _db.IPs
            .Where(i => i.ComId == token.CustomerId)
            .Select(i => i.IpAddress)
            .ToListAsync();

        _logger.LogInformation("📤 {Count} IPs gefunden für ComId={ComId}", ips.Count, token.CustomerId);
        return Ok(ips);
    }

    [HttpPost]
    public async Task<IActionResult> AddIp([FromBody] string ip)
    {
        _logger.LogInformation("📥 [POST] /api/ip - Hinzufügen von IP: {Ip}", ip);

        var jwt = ExtractBearerToken();
        if (jwt == null)
        {
            _logger.LogWarning("🔒 Zugriff ohne JWT");
            return Unauthorized();
        }

        var token = await _tokenValidator.ValidateTokenAsync(jwt);
        if (token == null || !token.Valid)
        {
            _logger.LogWarning("🔑 Ungültiger oder abgelaufener Token.");
            return Unauthorized();
        }

        if (token.Role != "customeradmin")
        {
            _logger.LogWarning("🚫 Zugriff verweigert – Rolle ist nicht 'customeradmin'. Aktuelle Rolle: {Role}", token.Role);
            return Forbid();
        }

        if (await _db.IPs.AnyAsync(i => i.ComId == token.CustomerId && i.IpAddress == ip))
        {
            _logger.LogWarning("⚠️ IP {Ip} existiert bereits für ComId={ComId}", ip, token.CustomerId);
            return Conflict("IP existiert bereits.");
        }

        _db.IPs.Add(new IP { ComId = token.CustomerId, IpAddress = ip });
        await _db.SaveChangesAsync();

        _logger.LogInformation("✅ IP {Ip} hinzugefügt für ComId={ComId}", ip, token.CustomerId);
        return Ok(new { status = "added", ip });
    }

    [HttpDelete("{ip}")]
    public async Task<IActionResult> DeleteIp(string ip)
    {
        _logger.LogInformation("📥 [DELETE] /api/ip/{Ip}", ip);

        var jwt = ExtractBearerToken();
        if (jwt == null)
        {
            _logger.LogWarning("🔒 Zugriff ohne JWT");
            return Unauthorized();
        }

        var token = await _tokenValidator.ValidateTokenAsync(jwt);
        if (token == null || !token.Valid)
        {
            _logger.LogWarning("🔑 Ungültiger oder abgelaufener Token.");
            return Unauthorized();
        }

        if (token.Role != "customeradmin")
        {
            _logger.LogWarning("🚫 Zugriff verweigert – Rolle ist nicht 'customeradmin'. Aktuelle Rolle: {Role}", token.Role);
            return Forbid();
        }

        var entry = await _db.IPs.FirstOrDefaultAsync(i => i.ComId == token.CustomerId && i.IpAddress == ip);
        if (entry == null)
        {
            _logger.LogWarning("❌ IP {Ip} nicht gefunden für ComId={ComId}", ip, token.CustomerId);
            return NotFound();
        }

        _db.IPs.Remove(entry);
        await _db.SaveChangesAsync();

        _logger.LogInformation("🗑️ IP {Ip} gelöscht für ComId={ComId}", ip, token.CustomerId);
        return Ok(new { status = "deleted", ip });
    }

    [HttpGet("agent")]
    public async Task<IActionResult> GetForAgent()
    {
        _logger.LogInformation("📥 [GET] /api/ip/agent");

        var token = Request.Headers["X-AGENT-TOKEN"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("🔒 Kein Agent-Token übergeben.");
            return Unauthorized();
        }

        var comId = await _agentAuth.ValidateAgentAsync(token);
        if (comId == null)
        {
            _logger.LogWarning("🔑 Ungültiges Agent-Token: {Token}", token);
            return Unauthorized();
        }

        var ips = await _db.IPs
            .Where(i => i.ComId == comId)
            .Select(i => i.IpAddress)
            .ToListAsync();

        _logger.LogInformation("📤 {Count} IPs gefunden für Agent mit ComId={ComId}", ips.Count, comId);
        return Ok(new { ips });
    }
}
