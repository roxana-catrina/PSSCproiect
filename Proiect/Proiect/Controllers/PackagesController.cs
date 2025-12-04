using Microsoft.AspNetCore.Mvc;
using Proiect.Domain.Models.Commands;
using Proiect.Domain.Models.Events;
using Proiect.Data.Services;

namespace Proiect.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PackagesController : ControllerBase
{
    private readonly IWorkflowOrchestrationService _orchestrationService;

    public PackagesController(IWorkflowOrchestrationService orchestrationService)
    {
        _orchestrationService = orchestrationService;
    }

    [HttpPost("pickup")]
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

            // Folosește serviciul de orchestrare care încarcă/salvează starea din/în baza de date
            var packageEvent = await _orchestrationService.ProcessShippingAsync(command);

            return packageEvent switch
            {
                PackageDeliveredEvent.PackageDeliveredSucceededEvent success => Ok(new
                {
                    success = true,
                    package = new
                    {
                        orderNumber = success.OrderNumber.Value,
                        trackingNumber = success.TrackingNumber.Value,
                        recipientName = success.RecipientName,
                        deliveredAt = success.DeliveredAt
                    },
                    message = "Package shipped successfully and saved to database"
                }),

                PackageDeliveredEvent.PackageDeliveredFailedEvent failure => BadRequest(new
                {
                    success = false,
                    errors = failure.Reasons,
                    message = "Failed to ship package"
                }),

                _ => StatusCode(500, new { success = false, message = "Unknown error occurred" })
            };
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while processing the package",
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
    string DeliveryCountry
);
