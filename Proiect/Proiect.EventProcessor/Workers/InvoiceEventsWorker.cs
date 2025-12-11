using Proiect.Messaging.Events;
using Microsoft.Extensions.Hosting;

namespace Proiect.EventProcessor.Workers;

/// <summary>
/// Worker that listens to invoice-events topic
/// Processes InvoiceGeneratedDto events
/// </summary>
internal class InvoiceEventsWorker : IHostedService
{
    private readonly IEventListener _eventListener;

    public InvoiceEventsWorker(IEventListener eventListener)
    {
        _eventListener = eventListener;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("InvoiceEventsWorker started - listening to invoice-events...");
        return _eventListener.StartAsync("invoice-events", "invoice-subscription", cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("InvoiceEventsWorker stopped!");
        return _eventListener.StopAsync(cancellationToken);
    }
}
