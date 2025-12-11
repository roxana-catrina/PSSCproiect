using Proiect.Domain.Models.Commands;
using Proiect.Domain.Workflows;
using static Proiect.Domain.Models.Events.PackageDeliveredEvent;
using Proiect.Domain.Models.Events;
using Proiect.Messaging.Events;
using Proiect.Messaging.Events.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Proiect.EventProcessor.Workers;

/// <summary>
/// Event handler for processing package commands
/// Executes ShippingWorkflow and publishes events
/// </summary>
internal class PackagesEventHandler : AbstractEventHandler<PickupPackageCommand>
{
    private readonly IServiceProvider _serviceProvider;

    public PackagesEventHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override string[] EventTypes => new string[] { typeof(PickupPackageCommand).Name };

    protected override async Task<EventProcessingResult> OnHandleAsync(PickupPackageCommand command)
    {
        using var scope = _serviceProvider.CreateScope();
        var workflow = scope.ServiceProvider.GetRequiredService<ShippingWorkflow>();
        var eventSender = scope.ServiceProvider.GetRequiredService<IEventSender>();

        // Execute workflow
        var workflowResult = workflow.Execute(
            command,
            generateAwb: () => $"AWB-{Guid.NewGuid().ToString()[..10].ToUpper()}",
            notifyCourier: (awb) => true,
            getRecipientName: (orderNumber) => "Customer"
        );

        // Handle result and publish event
        if (workflowResult is PackageDeliveredSucceededEvent successEvent)
        {
            await eventSender.SendAsync("package-events", new PackageDeliveredDto
            {
                OrderNumber = successEvent.OrderNumber.Value,
                TrackingNumber = successEvent.TrackingNumber.Value,
                DeliveredAt = successEvent.DeliveredAt,
                RecipientName = successEvent.RecipientName
            });

            Console.WriteLine($"Package {successEvent.TrackingNumber.Value} delivered successfully");
            return EventProcessingResult.Completed;
        }
        else if (workflowResult is PackageDeliveredFailedEvent failedEvent)
        {
            Console.WriteLine($"Package delivery failed: {string.Join(", ", failedEvent.Reasons)}");
            return EventProcessingResult.Failed;
        }

        return EventProcessingResult.Completed;
    }
}
