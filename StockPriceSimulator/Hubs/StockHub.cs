using Microsoft.AspNetCore.SignalR;

namespace StockPriceSimulator.Hubs
{
    // Meetingroom for realtime communication
    // Client will come here to receieve the updates form the server.
    public class StockHub : Hub
    {
        // no custom method needed
        // Backgroung service will send the msgs to client
    }
}
