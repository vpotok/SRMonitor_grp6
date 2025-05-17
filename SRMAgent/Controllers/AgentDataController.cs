using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;

namespace SRMAgent.Controllers;

[ApiController]
[Route("[controller]")]
public class AgentDataController : ControllerBase
{
    private readonly HttpClient _httpClient;

    public AgentDataController(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(10);

        string headerName = "X-AGENT-TOKEN";
        string headerValue = "BrYp8vv8Dle6iLCYHu9i8RLoKfLjmJdU0F8EYZz8lcKV8gQsKft0RWlrginBhK4o"; // Ersetze dies durch deinen tatsächlichen Token
        if (!_httpClient.DefaultRequestHeaders.Contains(headerName))
        {
            _httpClient.DefaultRequestHeaders.Add(headerName, headerValue);
        }
    }

    // 1. Empfängt Daten vom Shelly
    [HttpGet("receive")]
    public async Task<IActionResult> ReceiveSensorData(
        [FromQuery] double hum,
        [FromQuery] double temp,
        [FromQuery] string id,
        [FromQuery] double lux,
        [FromQuery] bool state)
    {
        var receivedData = new AgentData
        {
            ShellyId = id,
            CurrentTemp = (float)temp,
            //CurrentHumidity = (float)hum,
            //Lux = (float)lux,
            DoorOpen = state,
            KeepAliveTimestamp = DateTime.Now
        };

        // Log the created object
        Console.WriteLine($"Created AgentData object: {JsonSerializer.Serialize(receivedData)}");

        var json = JsonSerializer.Serialize(new
        {
            shellyId = receivedData.ShellyId,
            currentTemp = receivedData.CurrentTemp,
            doorOpen = receivedData.DoorOpen,
            keepAliveTimestamp = receivedData.KeepAliveTimestamp
        });
        Console.WriteLine($"Outgoing JSON: {json}");

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("http://192.168.4.12:5001/api/CoreService/shelly", content);
        response.EnsureSuccessStatusCode();

        return Ok("Data received and forwarded successfully");
    }

    // 2. Ruft die IP-Liste vom Core-Service ab
    public async Task<List<string>> FetchIpList()
    {
        try
        {
            var response = await _httpClient.GetAsync("http://192.168.4.12:5001/api/IP/agent");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Received IP list JSON: {json}");

            var ipResponse = JsonSerializer.Deserialize<IpResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return ipResponse?.Ips ?? new List<string>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching IP list: {ex.Message}");
            return new List<string>();
        }
    }

    // 3. Führt Pings aus und sendet JEDE IP einzeln an den Core-Service
    public async Task PerformPing(List<string> ipList)
    {
        Console.WriteLine($"Pinging {ipList.Count} IPs...");
        var ping = new Ping();

        foreach (var ip in ipList)
        {
            PingResult result;
            try
            {
                var reply = await ping.SendPingAsync(ip, 1000);
                result = new PingResult
                {
                    Ip = ip,
                    Success = reply.Status == IPStatus.Success
                };
            }
            catch
            {
                result = new PingResult
                {
                    Ip = ip,
                    Success = false
                };
            }

            var json = JsonSerializer.Serialize(new {
                ipAddress = result.Ip,
                success = result.Success
            });
            Console.WriteLine($"Outgoing Ping JSON: {json}");
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("http://192.168.4.12:5001/api/CoreService/ping", content);
            response.EnsureSuccessStatusCode();
        }
    }
}