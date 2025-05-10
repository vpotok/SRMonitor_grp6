using Microsoft.AspNetCore.Mvc;

namespace SRMCore.Controllers
{
    [ApiController]
    [Route("api/devices")]
    public class DevicesController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetDevices()
        {
            // Updated example response
            var devices = new[]
            {
                new { Id = 1, InstalledDate = "2023-05-01", BatteryState = "Good", Version = "1.0.0", Owner = "John Doe" },
                new { Id = 2, InstalledDate = "2023-06-15", BatteryState = "Low", Version = "1.1.0", Owner = "Jane Smith" }
            };

            return Ok(devices);
        }
    }
}