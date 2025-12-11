using Microsoft.AspNetCore.Mvc;
using Proiect.Domain.Models.Commands;
using Proiect.Domain.Models.Events;
using Proiect.Domain.Workflows;
using Proiect.Messaging.Events;
using static Proiect.Domain.Models.Events.PackageDeliveredEvent;

namespace Proiect.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PackagesController : ControllerBase
{
    private readonly ILogger<PackagesController> _logger;
    private readonly ShippingWorkflow _shippingWorkflow;
    private readonly IEventSender _eventSender;

    public PackagesController(
        ILogger<PackagesController> logger,
        ShippingWorkflow shippingWorkflow,
        IEventSender eventSender)
    {
        _logger = logger;
        _shippingWorkflow = shippingWorkflow;
        _eventSender = eventSender;
    }

    [HttpPost]
    public async Task<IActionResult> PickupPackage([FromBody] PickupPackageRequest request)
    {
        try
        {
            var command = new PickupPackageCommand(
                request.OrderNumber,
                request.DeliveryStreet,
                request.DeliveryCity,
                request.DeliveryPostalCode,
                request.DeliveryCountry
            );

            // Execute workflow with dependencies
            IPackageDeliveredEvent workflowResult = _shippingWorkflow.Execute(
                command,
                generateAwb: () => $"AWB-{Guid.NewGuid().ToString()[..10].ToUpper()}",
                notifyCourier: (awb) => true, // Simulate courier notification
                getRecipientName: (orderNumber) => "Customer" // Simplified for now
            );

            // Handle workflow result
            IActionResult response = workflowResult switch
            {
                PackageDeliveredSucceededEvent @event => await PublishEvent(@event),
                PackageDeliveredFailedEvent @event => BadRequest(@event.Reasons),
                _ => throw new NotImplementedException()
            };

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing package pickup");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while processing the package",
                error = ex.Message
            });
        }
    }

    private async Task<IActionResult> PublishEvent(PackageDeliveredSucceededEvent successEvent)
    {
        await _eventSender.SendAsync("package-events", new PackageDeliveredDto
        {
            OrderNumber = successEvent.OrderNumber.Value,
            TrackingNumber = successEvent.TrackingNumber.Value,
            DeliveredAt = successEvent.DeliveredAt,
            RecipientName = successEvent.RecipientName
        });

        return Ok();
    }
}

public record PickupPackageRequest(
    string OrderNumber,
    string DeliveryStreet,
    string DeliveryCity,
    string DeliveryPostalCode,
    string DeliveryCountry
);
