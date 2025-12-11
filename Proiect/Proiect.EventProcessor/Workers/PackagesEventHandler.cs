using Proiect.Domain.Models.Commands;
using Proiect.Domain.Workflows;
using Proiect.Domain.Models.Events;
using Proiect.Messaging.Events;
using Proiect.Messaging.Events.Models;
using Microsoft.Extensions.DependencyInjection;
using static Proiect.Domain.Models.Events.PackageShippedEvent;

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

        // Execute workflow (only 2 parameters now)
        var workflowResult = workflow.Execute(
            command,
            generateAwb: () => $"RO{DateTime.UtcNow:yyMMddHHmm}",
            notifyCourier: (awb) => true
        );

        // Handle result and publish event
        if (workflowResult is PackageShippedSucceededEvent successEvent)
        {
            await eventSender.SendAsync("package-events", new PackageShippedDto
            {
                OrderNumber = successEvent.OrderNumber.Value,
                TrackingNumber = successEvent.TrackingNumber.Value,
                ShippedAt = successEvent.ShippedAt,
                DeliveryAddress = new Proiect.Domain.Models.Events.DeliveryAddressDto
                {
                    Street = successEvent.DeliveryAddress.Street,
                    City = successEvent.DeliveryAddress.City,
                    PostalCode = successEvent.DeliveryAddress.PostalCode,
                    Country = successEvent.DeliveryAddress.Country
                },
                CourierMessage = "The recipient will be contacted by the delivery man"
            });

            Console.WriteLine($"Package {successEvent.TrackingNumber.Value} shipped successfully");
            Console.WriteLine($"📞 The recipient will be contacted by the delivery man");
            return EventProcessingResult.Completed;
        }
        else if (workflowResult is PackageShippedFailedEvent failedEvent)
        {
            Console.WriteLine($"Package shipping failed: {string.Join(", ", failedEvent.Reasons)}");
            return EventProcessingResult.Failed;
        }

        return EventProcessingResult.Completed;
    }
}
