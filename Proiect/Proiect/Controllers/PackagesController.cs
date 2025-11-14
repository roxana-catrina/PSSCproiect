using Microsoft.AspNetCore.Mvc;
using Proiect.Domain.Models.Commands;
using Proiect.Domain.Workflows;
using Proiect.Domain.Models.Events;

namespace Proiect.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PackagesController : ControllerBase
{
    [HttpPost("pickup")]
    public IActionResult PickupPackage([FromBody] PickupPackageRequest request)
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

            var workflow = new ShippingWorkflow();
            
            // Mock dependencies
            Func<string> generateAwb = () => $"RO{DateTime.Now:yyyyMMddHHmm}{new Random().Next(10, 99):D2}";
            Func<string, bool> notifyCourier = (awb) => true; // Always successful
            
            var packageEvent = workflow.Execute(command, generateAwb, notifyCourier);

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
                    message = "Package shipped successfully"
                }),
                PackageDeliveredEvent.PackageDeliveredFailedEvent failure => BadRequest(new
                {
                    success = false,
                    errors = failure.Reasons,
                    message = "Failed to ship package"
                }),
                _ => StatusCode(500, new { success = false, message = "Unexpected error" })
            };
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred", error = ex.Message });
        }
    }
}

public record PickupPackageRequest(
    string OrderNumber,
    string DeliveryStreet,
    string DeliveryCity,
    string DeliveryPostalCode,
    string DeliveryCountry);
