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
        _httpClient.Timeout = TimeSpan.FromSeconds(10); // Setze den Timeout auf 30 Sekunden

        // Füge den X-AGENT-TOKEN-Header hinzu
        string headerName = "X-AGENT-TOKEN";
        string headerValue = "aBBtXTkzYArjpscJq5BKbLLYF3li0vLzSpzRbExbQlFCumPtiVausjxOukmnqePK"; // Ersetze dies durch deinen tatsächlichen Token
        if (!_httpClient.DefaultRequestHeaders.Contains(headerName))
        {
            _httpClient.DefaultRequestHeaders.Add(headerName, headerValue);
        }
    }

    // 1. Empfängt Daten vom Shelly
    [HttpGet("receive")]
    public IActionResult ReceiveSensorData([FromQuery] double hum, [FromQuery] double temp, [FromQuery] string id, [FromQuery] double lux, [FromQuery] bool state)
    {
        // Log the received data
        //Console.WriteLine($"Received data from Shelly sensor:");
        //Console.WriteLine($"  Humidity: {hum}%");
        //Console.WriteLine($"  Temperature: {temp}°C");
        //Console.WriteLine($"  Shelly ID: {id}");
        //Console.WriteLine($"  Light Intensity: {lux} lux");
        //Console.WriteLine($"  Door State: {(state ? "Open" : "Closed")}");

        // Create an AgentData object from the query parameters
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

        /*var json = JsonSerializer.Serialize(new
        {
            Type = "sensorData",
            Data = receivedData
        });*/
        // Call Core to forward the data
        _ = CallCore(receivedData);

        // Return a success response
        return Ok("Data received successfully");
    }

    // 2. Ruft die IP-Liste vom Core-Service ab
    public async Task<List<string>> FetchIpList()
    {
        try
        {
            // Sende die Anfrage an den Core-Service
            var response = await _httpClient.GetAsync("http://192.168.4.12:5001/api/IP/agent");
            response.EnsureSuccessStatusCode();

            // Lese die JSON-Antwort
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Received IP list JSON: {json}");

            // Deserialisiere die IP-Liste
            var ipResponse = JsonSerializer.Deserialize<IpResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Ignoriere Groß-/Kleinschreibung bei den JSON-Property-Namen
            });

            return ipResponse?.Ips ?? new List<string>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching IP list: {ex.Message}");
            return new List<string>();
        }
    }

    // 3. Führt Pings aus und gibt die Ergebnisse zurück
    public async Task<List<PingResult>> PerformPing(List<string> ipList)
    {
        var pingResults = new List<PingResult>();
        Console.WriteLine($"Pinging {ipList.Count} IPs...");
        var ping = new Ping();

        foreach (var ip in ipList)
        {
            try
            {
                var reply = await ping.SendPingAsync(ip, 1000);
                pingResults.Add(new PingResult
                {
                    Ip = ip,
                    Success = reply.Status == IPStatus.Success
                });
            }
            catch
            {
                // Falls der Ping fehlschlägt, füge die IP mit Success = false hinzu
                pingResults.Add(new PingResult
                {
                    Ip = ip,
                    Success = false
                });
            }
        }
        
        return pingResults;
    }

    // 4. Sendet Daten an den Core-Service
    public async Task CallCore(object receivedData)
    {
        try
        {
            var json = JsonSerializer.Serialize(receivedData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Console.WriteLine($"Sending data to Core Service: {json}");

            // Sende die Daten an den Core-Service
            var response = await _httpClient.PostAsync("http://192.168.4.12:5001/api/CoreService/shelly", content);
            response.EnsureSuccessStatusCode();

            Console.WriteLine("Data successfully forwarded to Core Service.");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP request error: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine($"Request timed out: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }
}

// Datenstruktur für die IP-Liste
public class IpResponse
{
    public List<string>? Ips { get; set; }
}

// Datenstruktur für die Ergebnisse eines Pings
public class PingResult
{
    public string? Ip { get; set; }
    public bool Success { get; set; }
}