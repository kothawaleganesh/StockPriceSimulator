
# StockPriceSimulator — Backend README

## Overview

`StockPriceSimulator` is the **backend** component of the SignalR Stock Demo. It’s an **ASP.NET Core Web API** that uses **SignalR** to broadcast simulated real-time stock price updates to connected clients (e.g., React frontend dashboards).

This project demonstrates:

* How to implement a **SignalR Hub** for real-time bi-directional communication.
* Using a **Background Service** to simulate and broadcast stock prices.
* Setting up **CORS**, hosting, and SignalR hub mapping.

---

## What’s Inside

* `Hubs/StockHub.cs` — Defines the SignalR hub where clients connect.
* `Services/StockPriceSimulator.cs` — Background service that generates and pushes price updates every 2 seconds.
* `Program.cs` — Configures services, middleware (CORS, SignalR), and starts the app.
* `*.csproj`, config files — Project setup for .NET and SignalR dependencies.

---

## Quickstart — Run Locally

### Prerequisites

* [.NET 6 SDK](https://dotnet.microsoft.com/) or higher installed.
* A frontend client (like the React app in the companion repo) ready to connect to the hub.

Check your .NET version:

```bash
dotnet --version
```

### Setup

1. **Clone the Repository**

   ```bash
   git clone https://github.com/kothawaleganesh/StockPriceSimulator.git
   cd StockPriceSimulator
   ```

2. **Restore NuGet Packages**

   ```bash
   dotnet restore
   ```

3. **Build & Run the Server**

   ```bash
   dotnet run
   ```

   The server will start, typically listening on `http://localhost:5000`.

4. **Connect via Frontend**

   Configure your frontend (e.g., React app) to connect to `http://localhost:5000/stockHub`.

   Once connected, the backend will auto-send stock updates every 2 seconds.

---

## How It Works (Theory)

1. **SignalR Hub**
   The `StockHub` is the real-time messaging endpoint. Clients connect here to receive messages.

2. **Background Service**
   `StockPriceSimulator` runs continuously in the background, updates stock prices randomly every few seconds, and broadcasts updates using `IHubContext<StockHub>`.

3. **CORS Setup**
   Configured to allow frontend clients (often running on a different port like 3000) to connect securely.

### Conceptual Flow

```
[ Background Service ]
        │
        ▼
[ SignalR Hub ]  ←→  [ React Frontend(s) ]
Broadcast updates        Instant, live UI updates
```

---

## Code Highlights

### **Hubs/StockHub.cs**

```csharp
using Microsoft.AspNetCore.SignalR;

namespace StockPriceSimulator.Hubs
{
    // Acts as a real-time communication endpoint.
    public class StockHub : Hub
    {
        // Empty by design — the background service pushes updates directly.
    }
}
```

### **Services/StockPriceSimulator.cs**

```csharp
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using StockPriceSimulator.Hubs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StockPriceSimulator.Services
{
    public class StockPriceSimulator : BackgroundService
    {
        private readonly IHubContext<StockHub> _hubContext;
        private readonly Random _random = new();
        private readonly Dictionary<string, decimal> _stocks = new()
        {
            { "AAPL", 150m }, { "GOOG", 2700m }, { "MSFT", 300m }
        };

        public StockPriceSimulator(IHubContext<StockHub> hubContext)
        {
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var symbol in _stocks.Keys)
                {
                    var delta = (decimal)(_random.NextDouble() - 0.5) * 2;
                    _stocks[symbol] = Math.Round(_stocks[symbol] + delta, 2);

                    await _hubContext.Clients.All.SendAsync(
                        "ReceiveStockUpdate", symbol, _stocks[symbol], cancellationToken: stoppingToken);
                }
                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}
```

### **Program.cs**

```csharp
using StockPriceSimulator.Hubs;
using StockPriceSimulator.Services;

var builder = WebApplication.CreateBuilder(args);

// Register SignalR and Background Service
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true)); // Use cautiously in production
});
builder.Services.AddHostedService<StockPriceSimulator>();

var app = builder.Build();
app.UseCors("CorsPolicy");
app.MapHub<StockHub>("/stockHub");
app.Run();
```

---

## Tips & Next Steps

* **Production Caution**: Replace the wide CORS policy seen here with stricter origin rules.
* **Extend Functionality**: Add HTTP endpoints to fetch initial stock data on page load.
* **Enhance Robustness**: Add error handling, logging, and configuration settings.
* **Stateful Data**: For a true stock system, integrate a real data source or API.

---

## Why SignalR?

* Simplifies building real-time features like live stock dashboards.
* Uses web-friendly transports (WebSockets, SSE, long polling) automatically.
* Great for educational projects—straightforward yet powerful.

---

