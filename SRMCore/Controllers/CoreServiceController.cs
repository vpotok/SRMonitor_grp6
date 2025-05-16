using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SRMCore.Data;
using SRMCore.Models;
using SRMCore.Services;
using System.Text.Json;

namespace SRMCore.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoreServiceController : ControllerBase
{
    private readonly CoreDbContext _db;
    private readonly IAgentAuthService _agentAuth;
    private readonly IAlarmService _alarm;
    private readonly ITokenValidationService _tokenValidator;
    private readonly ILogger<CoreServiceController> _logger;

    private string? ExtractBearerToken()
    {
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        _logger.LogDebug("Authorization Header: {Header}", authHeader);

        if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader["Bearer ".Length..].Trim();
            _logger.LogDebug("JWT extrahiert aus Header: {Token}", token);
            return token;
        }

        var cookieToken = Request.Cookies["jwt"];
        Console.WriteLine($"Cookie-Token: {cookieToken}");
        if (!string.IsNullOrWhiteSpace(cookieToken))
        {
            _logger.LogDebug("JWT extrahiert aus Cookie: {Token}", cookieToken);
            return cookieToken;
        }

        _logger.LogWarning("Kein gültiger Authorization Header oder Cookie vorhanden.");
        return null;
    }

    public CoreServiceController(
        CoreDbContext db,
        IAgentAuthService agentAuth,
        IAlarmService alarm,
        ITokenValidationService tokenValidator,
        ILogger<CoreServiceController> logger)
    {
        _db = db;
        _agentAuth = agentAuth;
        _alarm = alarm;
        _tokenValidator = tokenValidator;
        _logger = logger;
    }

    [HttpPost("shelly")]
    public async Task<IActionResult> ReceiveShellyData([FromBody] ShellyDataDto dto)
    {
        var token = Request.Headers["X-AGENT-TOKEN"].ToString();
        if (string.IsNullOrWhiteSpace(token)) return Unauthorized();

        var comId = await _agentAuth.ValidateAgentAsync(token);
        if (comId == null) return Unauthorized();

        var log = new Log
        {
            ComId = comId.Value,
            Timestamp = DateTime.UtcNow,
            Message = JsonSerializer.Serialize(dto),
            Type = "shelly"
        };

        _db.Logs.Add(log);
        await _db.SaveChangesAsync();

        await _alarm.CheckAndTriggerRedmineIfNeededAsync(comId.Value, dto);

        return Ok(new { status = "saved", time = log.Timestamp });
    }


    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs()
    {
        var jwt = ExtractBearerToken();
        if (jwt == null)
        {
            return Unauthorized();
        }

        var token = await _tokenValidator.ValidateTokenAsync(jwt);
        Console.WriteLine($"Token: {token}");
        if (token == null || !token.Valid)
        {
            return Unauthorized();
        }

        var query = _db.Logs
            .Where(l => l.ComId == token.CustomerId)
            .OrderByDescending(l => l.Timestamp);

        if (token.Role?.ToLower() == "customer" || token.Role?.ToLower() == "customeradmin")
            query = (IOrderedQueryable<Log>)query.Take(100);

        var logs = await query
            .Select(l => new { l.Timestamp, l.Type, l.Message })
            .ToListAsync();

        return Ok(logs);
    }

    [HttpPost("ping")]
    public async Task<IActionResult> ReceivePingResult([FromBody] PingResultDto dto)
    {
        var token = Request.Headers["X-AGENT-TOKEN"].ToString();
        if (string.IsNullOrWhiteSpace(token)) return Unauthorized();

        var comId = await _agentAuth.ValidateAgentAsync(token);
        if (comId == null) return Unauthorized();

        var knownIp = await _db.IPs.AnyAsync(i => i.ComId == comId && i.IpAddress == dto.IpAddress);
        if (!knownIp) return BadRequest("IP-Adresse ist für diese Firma nicht registriert.");

        var resultMessage = $"Ping zu {dto.IpAddress}: {(dto.Success ? "OK" : "Fehlgeschlagen")} ";

        var log = new Log
        {
            ComId = comId.Value,
            Timestamp = DateTime.UtcNow,
            Message = resultMessage,
            Type = "ip"
        };

        _db.Logs.Add(log);
        await _db.SaveChangesAsync();

        return Ok(new { status = "logged", ip = dto.IpAddress });
    }
}
