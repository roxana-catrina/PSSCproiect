using Microsoft.AspNetCore.Mvc;
using Proiect.Domain.Models.Commands;
using Proiect.Domain.Models.Events;
using Proiect.Data.Services;

namespace Proiect.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IWorkflowOrchestrationService _orchestrationService;

    public OrdersController(IWorkflowOrchestrationService orchestrationService)
    {
        _orchestrationService = orchestrationService;
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
    {
        try
        {
            var items = request.Items.Select(i => 
                new OrderItemCommand(i.ProductName, i.Quantity, i.UnitPrice)
            ).ToList();

            var command = new PlaceOrderCommand(
                request.CustomerName,
                request.CustomerEmail,
                request.DeliveryStreet,
                request.DeliveryCity,
                request.DeliveryPostalCode,
                request.DeliveryCountry,
                items
            );

            // Folosește serviciul de orchestrare care încarcă/salvează starea din/în baza de date
            var orderEvent = await _orchestrationService.ProcessOrderAsync(command);

            return orderEvent switch
            {
                OrderPlacedEvent.OrderPlacedSucceededEvent success => Ok(new
                {
                    success = true,
                    order = new
                    {
                        orderNumber = success.OrderNumber.Value,
                        customerName = success.CustomerName,
                        customerEmail = success.CustomerEmail,
                        totalAmount = success.TotalAmount.Value,
                        placedAt = success.PlacedAt
                    },
                    message = "Order placed successfully and saved to database"
                }),

                OrderPlacedEvent.OrderPlacedFailedEvent failure => BadRequest(new
                {
                    success = false,
                    errors = failure.Reasons,
                    message = "Order placement failed"
                }),

                _ => StatusCode(500, new { success = false, message = "Unknown error occurred" })
            };
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while processing the order",
                error = ex.Message
            });
        }
    }
}

public record PlaceOrderRequest(
    string CustomerName,
    string CustomerEmail,
    string DeliveryStreet,
    string DeliveryCity,
    string DeliveryPostalCode,
    string DeliveryCountry,
    List<OrderItemRequest> Items
);

public record OrderItemRequest(
    string ProductName,
    string Quantity,
    string UnitPrice
);
