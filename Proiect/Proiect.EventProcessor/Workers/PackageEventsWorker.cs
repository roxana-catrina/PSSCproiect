using Proiect.Messaging.Events;
using Microsoft.Extensions.Hosting;

namespace Proiect.EventProcessor.Workers;

/// <summary>
/// Worker that listens to package-events topic
/// Processes PackageDeliveredDto events
/// </summary>
internal class PackageEventsWorker : IHostedService
{
    private readonly IEventListener _eventListener;

    public PackageEventsWorker(IEventListener eventListener)
    {
        _eventListener = eventListener;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("PackageEventsWorker started - listening to package-events...");
        return _eventListener.StartAsync("package-events", "package-subscription", cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("PackageEventsWorker stopped!");
        return _eventListener.StopAsync(cancellationToken);
    }
}
