using Proiect.Domain.Models.Events;
using Proiect.Messaging.Events;
using Proiect.Messaging.Events.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Proiect.EventProcessor.Workers;

/// <summary>
/// Event handler for processing PackageDelivered events
/// Listens to events published after packages are successfully delivered
/// FINAL STEP: Marks the completion of the entire order workflow chain
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
        Console.WriteLine($"\n[PackageDeliveredEventHandler] ✅ Package delivered successfully!");
        Console.WriteLine($"  Order: {eventData.OrderNumber}");
        Console.WriteLine($"  Tracking Number: {eventData.TrackingNumber}");
        Console.WriteLine($"  Recipient: {eventData.RecipientName}");
        Console.WriteLine($"  Delivered At: {eventData.DeliveredAt:yyyy-MM-dd HH:mm:ss}");
        
        Console.WriteLine($"\n🎉 WORKFLOW CHAIN COMPLETED for order {eventData.OrderNumber}!");
        Console.WriteLine($"   1. ✅ Order Placed");
        Console.WriteLine($"   2. ✅ Invoice Generated");
        Console.WriteLine($"   3. ✅ Package Delivered");
        Console.WriteLine($"   ═══════════════════════════════════════════════════\n");

        await Task.CompletedTask;
        return EventProcessingResult.Completed;
    }
}
