using Proiect.Domain.Models.Events;
using Proiect.Messaging.Events;
using Proiect.Messaging.Events.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Proiect.EventProcessor.Workers;

/// <summary>
/// Event handler for processing PackageDelivered events
/// Listens to events published after packages are successfully delivered
/// </summary>
internal class PackageDeliveredEventHandler : AbstractEventHandler<PackageDeliveredDto>
{
    private readonly IServiceProvider _serviceProvider;

    public PackageDeliveredEventHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override string[] EventTypes => new[] { typeof(PackageDeliveredDto).Name };

    protected override async Task<EventProcessingResult> OnHandleAsync(PackageDeliveredDto eventData)
    {
        Console.WriteLine($"[PackageDeliveredEventHandler] Processing package delivery");
        Console.WriteLine($"  Order: {eventData.OrderNumber}");
        Console.WriteLine($"  Tracking Number: {eventData.TrackingNumber}");
        Console.WriteLine($"  Recipient: {eventData.RecipientName}");
        Console.WriteLine($"  Delivered At: {eventData.DeliveredAt:yyyy-MM-dd HH:mm:ss}");

        await Task.CompletedTask;
        return EventProcessingResult.Completed;
    }
}
