namespace Proiect.Controllers;

using Microsoft.AspNetCore.Mvc;
using Proiect.Domain.Models.Commands;
using Proiect.Domain.Models.ValueObjects;
using Proiect.Domain.Workflows;
using Proiect.DTOs;

[ApiController]
[Route("api/[controller]")]
public class PackagesController : ControllerBase
{
    [HttpPost("pickup")]
    public async Task<IActionResult> PickupPackage([FromBody] PickupPackageRequest request)
    {
        try
        {
            var command = new PickupPackageCommand(
                request.OrderId,
                request.Awb ?? string.Empty,
                request.PickupDate
            );

            var deliveryAddress = new DeliveryAddress(
                request.DeliveryAddress.Street,
                request.DeliveryAddress.City,
                request.DeliveryAddress.PostalCode,
                request.DeliveryAddress.Country
            );

            var workflow = new ShippingWorkflow();
            var (preparedEvent, deliveredEvent) = await workflow.ExecuteAsync(
                command,
                deliveryAddress,
                request.CustomerEmail,
                request.SimulateDelivery
            );

            var response = new
            {
                success = true,
                package = new
                {
                    packageId = preparedEvent.PackageId,
                    orderId = preparedEvent.OrderId,
                    awb = preparedEvent.AWB,
                    preparedAt = preparedEvent.PreparedAt
                },
                delivery = deliveredEvent != null ? new
                {
                    deliveredAt = deliveredEvent.DeliveredAt,
                    receivedBy = deliveredEvent.ReceivedBy
                } : null,
                message = deliveredEvent != null ? "Package prepared and delivered" : "Package prepared for shipping"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred while processing the package", error = ex.Message });
        }
    }
}
