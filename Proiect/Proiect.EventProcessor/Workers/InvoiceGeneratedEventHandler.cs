using Proiect.Domain.Models.Events;
using Proiect.Messaging.Events;
using Proiect.Messaging.Events.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Proiect.EventProcessor.Workers;

/// <summary>
/// Event handler for processing InvoiceGenerated events
/// Listens to events published after invoices are successfully generated
/// </summary>
internal class InvoiceGeneratedEventHandler : AbstractEventHandler<InvoiceGeneratedDto>
{
    private readonly IServiceProvider _serviceProvider;

    public InvoiceGeneratedEventHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override string[] EventTypes => new[] { typeof(InvoiceGeneratedDto).Name };

    protected override async Task<EventProcessingResult> OnHandleAsync(InvoiceGeneratedDto eventData)
    {
        Console.WriteLine($"[InvoiceGeneratedEventHandler] Processing invoice: {eventData.InvoiceNumber}");
        Console.WriteLine($"  Order: {eventData.OrderNumber}");
        Console.WriteLine($"  Customer: {eventData.CustomerName}");
        Console.WriteLine($"  Total Amount: {eventData.TotalAmount:C}");
        Console.WriteLine($"  VAT Amount: {eventData.VatAmount:C}");
        Console.WriteLine($"  Total with VAT: {eventData.TotalWithVat:C}");
        Console.WriteLine($"  Generated At: {eventData.GeneratedAt:yyyy-MM-dd HH:mm:ss}");

        await Task.CompletedTask;
        return EventProcessingResult.Completed;
    }
}
