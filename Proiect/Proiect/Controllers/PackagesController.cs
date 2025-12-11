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
    public async Task<IActionResult> PickupPackage(
        [FromBody] PickupPackageRequest request,
        [FromServices] Proiect.Data.Services.IPackageStateService packageStateService)
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
                generateAwb: () => $"RO{DateTime.UtcNow:yyMMddHHmm}", // Format: RO + 10 digits
                notifyCourier: (awb) => true, // Simulate courier notification
                getRecipientName: (orderNumber) => request.RecipientName ?? "Customer"
            );

            // Handle workflow result and save to database
            IActionResult response = workflowResult switch
            {
                PackageDeliveredSucceededEvent @event => await SaveAndPublishEvent(@event, packageStateService),
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

    private async Task<IActionResult> SaveAndPublishEvent(PackageDeliveredSucceededEvent successEvent, Proiect.Data.Services.IPackageStateService packageStateService)
    {
        try
        {
            // Save package to database
            var deliveredPackage = new Proiect.Domain.Models.Entities.Package.DeliveredPackage(
                successEvent.OrderNumber,
                successEvent.TrackingNumber,
                successEvent.DeliveredAt,
                successEvent.RecipientName
            );

            await packageStateService.SavePackageAsync(deliveredPackage);
            _logger.LogInformation($"Package {successEvent.TrackingNumber.Value} saved to database");

            // Publish event to Service Bus
            await _eventSender.SendAsync("package-events", new PackageDeliveredDto
            {
                OrderNumber = successEvent.OrderNumber.Value,
                TrackingNumber = successEvent.TrackingNumber.Value,
                DeliveredAt = successEvent.DeliveredAt,
                RecipientName = successEvent.RecipientName
            });

            _logger.LogInformation($"Package {successEvent.TrackingNumber.Value} event published");

            return Ok(new
            {
                success = true,
                trackingNumber = successEvent.TrackingNumber.Value,
                orderNumber = successEvent.OrderNumber.Value,
                message = "Package pickup processed successfully",
                deliveredAt = successEvent.DeliveredAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving package to database");
            return StatusCode(500, new
            {
                success = false,
                message = "Package processed but failed to save",
                error = ex.Message
            });
        }
    }
}

public record PickupPackageRequest(
    string OrderNumber,
    string DeliveryStreet,
    string DeliveryCity,
    string DeliveryPostalCode,
    string DeliveryCountry,
    string? RecipientName = null
);
