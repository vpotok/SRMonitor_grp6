using System.Net.Http;
using System.Text.Json;
using SRMAgent.Controllers;

namespace SRMAgent
{
    public class WorkerService : BackgroundService
    {
        private const int generalDelay = 10 * 1000; // 10 Sekunden
        private readonly AgentDataController _controller;

        public WorkerService(HttpClient httpClient)
        {
            // Initialisiere den Controller mit dem HttpClient
            _controller = new AgentDataController(httpClient);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine("WorkerService is running...");

                try
                {
                    // 1. IP-Liste abrufen
                    var ipList = await _controller.FetchIpList();
                    //Console.WriteLine($"Fetched IP List: {JsonSerializer.Serialize(ipList)}");

                    // 2. Pings durchführen
                    var pingResults = await _controller.PerformPing(ipList);
                    Console.WriteLine($"Ping Results: {JsonSerializer.Serialize(pingResults)}");

                    // 3. Ergebnisse an den Core-Service senden
                    /*var payload = new
                    {
                        Type = "ip", // Füge das zusätzliche Feld hinzu
                        Results = pingResults
                    };*/
                    var json = JsonSerializer.Serialize(pingResults);
                    Console.WriteLine($"Sending JSON to Core Service: {json}");
                    //await _controller.CallCore(json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in WorkerService: {ex.Message}");
                }

                await Task.Delay(generalDelay, stoppingToken);
            }
        }
    }
}
