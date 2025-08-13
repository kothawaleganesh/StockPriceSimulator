
using Microsoft.AspNetCore.SignalR;
using StockPriceSimulator.Hubs;

namespace StockPriceSimulator.Services
{
    public class StockPriceSimulatorService : BackgroundService
    {
        private readonly IHubContext<StockHub> _hubContext;
        private readonly Random _random = new Random();
        private readonly Dictionary<string, decimal> _stocks = new() {
            {"Apple",150.00m},
            { "Google", 201.00m},
            { "MS", 291.00m},
            };
        public StockPriceSimulatorService(IHubContext<StockHub> hubContext)
        {
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var symbol in _stocks.Keys)
                {
                    var data = (decimal)(_random.NextDouble() - 0.5) * 2;
                    _stocks[symbol] += Math.Round(data, 2);

                    await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", symbol, _stocks[symbol], stoppingToken);
                }
                await Task.Delay(500, stoppingToken);
            }
        }
    }
}
