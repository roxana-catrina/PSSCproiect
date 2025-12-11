using Proiect.Domain.Models.Events;
using Proiect.Domain.Models.Commands;
using Proiect.Domain.Workflows;
using Proiect.Messaging.Events;
using Proiect.Messaging.Events.Models;
using Proiect.Data.Services;
using System.Globalization;
using static Proiect.Domain.Models.Events.InvoiceGeneratedEvent;

namespace Proiect.EventProcessor.Workers;

/// <summary>
/// Event handler for processing OrderPlaced events
/// Listens to events published after orders are successfully placed
/// TRIGGERS: BillingWorkflow to generate invoice automatically
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
        Console.WriteLine($"\n[OrderPlacedEventHandler] ✅ Order received: {eventData.OrderNumber}");
        Console.WriteLine($"  Customer: {eventData.CustomerName} ({eventData.CustomerEmail})");
        Console.WriteLine($"  Total Amount: {eventData.TotalAmount:C}");
        Console.WriteLine($"  Delivery: {eventData.DeliveryAddress.Street}, {eventData.DeliveryAddress.City}");
        Console.WriteLine($"  Placed At: {eventData.PlacedAt:yyyy-MM-dd HH:mm:ss}");

        // 🔥 TRIGGER BILLING WORKFLOW automatically
        Console.WriteLine($"\n[OrderPlacedEventHandler] 🚀 Triggering BillingWorkflow for order {eventData.OrderNumber}...");
        
        using (var scope = _serviceProvider.CreateScope())
        {
            var billingWorkflow = scope.ServiceProvider.GetRequiredService<BillingWorkflow>();
            var eventSender = scope.ServiceProvider.GetRequiredService<IEventSender>();
            var invoiceStateService = scope.ServiceProvider.GetRequiredService<IInvoiceStateService>();

            // Create command from order data
            var command = new GenerateInvoiceCommand(
                eventData.OrderNumber,
                eventData.CustomerName,
                eventData.TotalAmount.ToString(CultureInfo.InvariantCulture)
            );

            // Execute billing workflow
            IInvoiceGeneratedEvent workflowResult = billingWorkflow.Execute(
                command,
                vatRate: 0.19m, // 19% VAT
                generateInvoiceNumber: () => $"INV-{eventData.OrderNumber}-{DateTime.UtcNow:yyyyMMdd}"
            );

            // Handle result
            if (workflowResult is InvoiceGeneratedSucceededEvent successEvent)
            {
                // Save to database using the state service
                await invoiceStateService.SaveInvoiceFromEventAsync(
                    successEvent.InvoiceNumber,
                    successEvent.OrderNumber.Value,
                    successEvent.CustomerName,
                    successEvent.TotalAmount.Value,
                    successEvent.VatAmount.Value,
                    successEvent.TotalWithVat.Value,
                    successEvent.GeneratedAt
                );

                Console.WriteLine($"[OrderPlacedEventHandler] 💾 Invoice {successEvent.InvoiceNumber} saved to database");

                // Publish InvoiceGeneratedDto event
                await eventSender.SendAsync("invoice-events", new InvoiceGeneratedDto
                {
                    InvoiceNumber = successEvent.InvoiceNumber,
                    OrderNumber = successEvent.OrderNumber.Value,
                    CustomerName = successEvent.CustomerName,
                    TotalAmount = successEvent.TotalAmount.Value,
                    VatAmount = successEvent.VatAmount.Value,
                    TotalWithVat = successEvent.TotalWithVat.Value,
                    GeneratedAt = successEvent.GeneratedAt
                });

                Console.WriteLine($"[OrderPlacedEventHandler] 📤 Invoice event published to invoice-events topic");
            }
            else if (workflowResult is InvoiceGeneratedFailedEvent failedEvent)
            {
                Console.WriteLine($"[OrderPlacedEventHandler] ❌ Invoice generation failed: {string.Join(", ", failedEvent.Reasons)}");
                return EventProcessingResult.Failed;
            }
        }

        return EventProcessingResult.Completed;
    }
}
