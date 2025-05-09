using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SRMCore.Data;
using SRMCore.Models;
using SRMCore.Services;

namespace SRMCore.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoreServiceController : ControllerBase
{
    private readonly CoreDbContext _db;
    private readonly ITokenValidationService _tokenValidator;
    private readonly IAlarmService _alarmService;

    public CoreServiceController(CoreDbContext db, ITokenValidationService tokenValidator, IAlarmService alarmService)
    {
        _db = db;
        _tokenValidator = tokenValidator;
        _alarmService = alarmService;
    }

    // POST /api/coreservice/shelly/data
    [HttpPost("shelly/data")]
    public async Task<IActionResult> PostShellyData([FromBody] ShellyData incoming)
    {
        var token = Request.Headers.Authorization?.ToString()?.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token)) return Unauthorized();

        var validated = await _tokenValidator.ValidateTokenAsync(token);
        if (validated == null || !validated.Valid) return Unauthorized();

        incoming.CustomerId = validated.CustomerId;
        incoming.Timestamp = DateTime.UtcNow;

        _db.ShellyData.Add(incoming);
        await _db.SaveChangesAsync();

        await _alarmService.CheckAndSendAlertAsync(incoming); // Schritt 7

        return Ok();
    }

    // GET /api/coreservice/data (für Kunden)
    [HttpGet("data")]
    public async Task<IActionResult> GetMyData()
    {
        var token = Request.Headers.Authorization?.ToString()?.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token)) return Unauthorized();

        var validated = await _tokenValidator.ValidateTokenAsync(token);
        if (validated == null || !validated.Valid) return Unauthorized();

        var data = await _db.ShellyData
            .Where(d => d.CustomerId == validated.CustomerId)
            .OrderByDescending(d => d.Timestamp)
            .ToListAsync();

        return Ok(data);
    }

    // GET /api/coreservice/data/all (nur für Admins)
    [HttpGet("data/all")]
    public async Task<IActionResult> GetAllData()
    {
        var token = Request.Headers.Authorization?.ToString()?.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token)) return Unauthorized();

        var validated = await _tokenValidator.ValidateTokenAsync(token);
        if (validated == null || !validated.Valid || validated.Role != "Employee")
            return Forbid();

        var data = await _db.ShellyData
            .OrderByDescending(d => d.Timestamp)
            .ToListAsync();

        return Ok(data);
    }
}
