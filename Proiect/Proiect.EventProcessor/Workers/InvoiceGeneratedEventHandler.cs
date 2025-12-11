using Proiect.Domain.Models.Events;
using Proiect.Domain.Models.Commands;
using Proiect.Domain.Workflows;
using Proiect.Messaging.Events;
using Proiect.Messaging.Events.Models;
using Proiect.Data.Services;
using static Proiect.Domain.Models.Events.PackageDeliveredEvent;

namespace Proiect.EventProcessor.Workers;

/// <summary>
/// Event handler for processing InvoiceGenerated events
/// Listens to events published after invoices are successfully generated
/// TRIGGERS: ShippingWorkflow to prepare package for delivery automatically
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
        Console.WriteLine($"\n[InvoiceGeneratedEventHandler] ✅ Invoice received: {eventData.InvoiceNumber}");
        Console.WriteLine($"  Order: {eventData.OrderNumber}");
        Console.WriteLine($"  Customer: {eventData.CustomerName}");
        Console.WriteLine($"  Total Amount: {eventData.TotalAmount:C}");
        Console.WriteLine($"  VAT Amount: {eventData.VatAmount:C}");
        Console.WriteLine($"  Total with VAT: {eventData.TotalWithVat:C}");
        Console.WriteLine($"  Generated At: {eventData.GeneratedAt:yyyy-MM-dd HH:mm:ss}");

        // 🔥 TRIGGER SHIPPING WORKFLOW automatically
        Console.WriteLine($"\n[InvoiceGeneratedEventHandler] 🚀 Triggering ShippingWorkflow for order {eventData.OrderNumber}...");
        
        using (var scope = _serviceProvider.CreateScope())
        {
            var shippingWorkflow = scope.ServiceProvider.GetRequiredService<ShippingWorkflow>();
            var eventSender = scope.ServiceProvider.GetRequiredService<IEventSender>();
            var packageStateService = scope.ServiceProvider.GetRequiredService<IPackageStateService>();
            var orderStateService = scope.ServiceProvider.GetRequiredService<IOrderStateService>();

            // Get order details for delivery address
            var order = await orderStateService.GetOrderByNumberAsync(eventData.OrderNumber);
            if (order == null)
            {
                Console.WriteLine($"[InvoiceGeneratedEventHandler] ❌ Order {eventData.OrderNumber} not found in database");
                return EventProcessingResult.Failed;
            }

            // Create command from order data
            var command = new PickupPackageCommand(
                eventData.OrderNumber,
                order.DeliveryAddress.Street,
                order.DeliveryAddress.City,
                order.DeliveryAddress.PostalCode,
                order.DeliveryAddress.Country
            );

            // Execute shipping workflow
            IPackageDeliveredEvent workflowResult = shippingWorkflow.Execute(
                command,
                generateAwb: () => $"RO{DateTime.UtcNow:yyMMddHHmm}", // Format: RO + 10 digits (yy=2, MM=2, dd=2, HH=2, mm=2)
                notifyCourier: _ => true, // Simulate courier notification
                getRecipientName: _ => eventData.CustomerName
            );

            // Handle result
            if (workflowResult is PackageDeliveredSucceededEvent successEvent)
            {
                // Save to database using the state service
                await packageStateService.SavePackageFromEventAsync(
                    successEvent.OrderNumber.Value,
                    successEvent.TrackingNumber.Value,
                    successEvent.DeliveredAt,
                    successEvent.RecipientName
                );

                Console.WriteLine($"[InvoiceGeneratedEventHandler] 💾 Package {successEvent.TrackingNumber.Value} saved to database");

                // Publish PackageDeliveredDto event
                await eventSender.SendAsync("package-events", new PackageDeliveredDto
                {
                    OrderNumber = successEvent.OrderNumber.Value,
                    TrackingNumber = successEvent.TrackingNumber.Value,
                    DeliveredAt = successEvent.DeliveredAt,
                    RecipientName = successEvent.RecipientName
                });

                Console.WriteLine($"[InvoiceGeneratedEventHandler] 📤 Package event published to package-events topic");
            }
            else if (workflowResult is PackageDeliveredFailedEvent failedEvent)
            {
                Console.WriteLine($"[InvoiceGeneratedEventHandler] ❌ Package delivery failed: {string.Join(", ", failedEvent.Reasons)}");
                return EventProcessingResult.Failed;
            }
        }

        return EventProcessingResult.Completed;
    }
}
