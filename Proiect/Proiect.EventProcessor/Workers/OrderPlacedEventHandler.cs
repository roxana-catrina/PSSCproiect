using Proiect.Domain.Models.Events;
using Proiect.Messaging.Events;
using Proiect.Messaging.Events.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Proiect.EventProcessor.Workers;

/// <summary>
/// Event handler for processing OrderPlaced events
/// Listens to events published after orders are successfully placed
/// </summary>
internal class OrderPlacedEventHandler : AbstractEventHandler<OrderPlacedDto>
{
    private readonly IServiceProvider _serviceProvider;

    public OrderPlacedEventHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override string[] EventTypes => new[] { typeof(OrderPlacedDto).Name };

    protected override async Task<EventProcessingResult> OnHandleAsync(OrderPlacedDto eventData)
    {
        Console.WriteLine($"[OrderPlacedEventHandler] Processing order: {eventData.OrderNumber}");
        Console.WriteLine($"  Customer: {eventData.CustomerName} ({eventData.CustomerEmail})");
        Console.WriteLine($"  Total Amount: {eventData.TotalAmount:C}");
        Console.WriteLine($"  Delivery: {eventData.DeliveryAddress.Street}, {eventData.DeliveryAddress.City}");
        Console.WriteLine($"  Placed At: {eventData.PlacedAt:yyyy-MM-dd HH:mm:ss}");

        // Aici poți adăuga logică suplimentară, de exemplu:
        // - Trimitere notificare către client
        // - Trigger pentru generarea automată a facturii
        // - Actualizare dashboard în timp real
        // - Logging în sistem extern

        await Task.CompletedTask;
        return EventProcessingResult.Completed;
    }
}

