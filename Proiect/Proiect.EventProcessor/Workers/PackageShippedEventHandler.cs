using Proiect.Domain.Models.Events;
using Proiect.Messaging.Events;
using Proiect.Messaging.Events.Models;

namespace Proiect.EventProcessor.Workers;

/// <summary>
/// Event handler for processing PackageShipped events
/// Listens to events published after packages are successfully shipped
/// FINAL STEP: Marks the completion of the entire order workflow chain
/// </summary>
internal class PackageShippedEventHandler : AbstractEventHandler<PackageShippedDto>
{
    public override string[] EventTypes => new[] { typeof(PackageShippedDto).Name };

    protected override async Task<EventProcessingResult> OnHandleAsync(PackageShippedDto eventData)
    {
        Console.WriteLine($"\n[PackageShippedEventHandler] ✅ Package shipped successfully!");
        Console.WriteLine($"  Order: {eventData.OrderNumber}");
        Console.WriteLine($"  Tracking Number: {eventData.TrackingNumber}");
        Console.WriteLine($"  Shipped At: {eventData.ShippedAt:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"  Delivery Address: {eventData.DeliveryAddress.Street}, {eventData.DeliveryAddress.City}");
        Console.WriteLine($"  📞 {eventData.CourierMessage}");
        
        Console.WriteLine($"\n🎉 WORKFLOW CHAIN COMPLETED for order {eventData.OrderNumber}!");
        Console.WriteLine($"   1. ✅ Order Placed");
        Console.WriteLine($"   2. ✅ Invoice Generated");
        Console.WriteLine($"   3. ✅ Package Shipped");
        Console.WriteLine($"   ═══════════════════════════════════════════════════\n");

        await Task.CompletedTask;
        return EventProcessingResult.Completed;
    }
}
