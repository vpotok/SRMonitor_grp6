using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SRMCore.Controllers
{
    [ApiController]
    [Route("api/protected")]
    public class ProtectedController : ControllerBase
    {
        [Authorize]
        [HttpGet("devices")]
        public IActionResult GetDevices()
        {
            return Ok(new { devices = new[] { "Device1", "Device2" } });
        }
    }
}