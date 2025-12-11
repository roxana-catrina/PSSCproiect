using Proiect.Messaging.Events;
using Microsoft.Extensions.Hosting;

namespace Proiect.EventProcessor.Workers;

/// <summary>
/// Worker that listens to order-events topic
/// Processes OrderPlacedDto events
/// </summary>
internal class OrderEventsWorker : IHostedService
{
    private readonly IEventListener _eventListener;

    public OrderEventsWorker(IEventListener eventListener)
    {
        _eventListener = eventListener;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("OrderEventsWorker started - listening to order-events...");
        return _eventListener.StartAsync("order-events", "order-subscription", cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("OrderEventsWorker stopped!");
        return _eventListener.StopAsync(cancellationToken);
    }
}
