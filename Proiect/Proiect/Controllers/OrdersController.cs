using Microsoft.AspNetCore.Mvc;
using Proiect.Domain.Models.Commands;
using Proiect.Domain.Models.Events;
using Proiect.Domain.Workflows;
using Proiect.Messaging.Events;
using Proiect.Data.Services;
using static Proiect.Domain.Models.Events.OrderPlacedEvent;

namespace Proiect.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ILogger<OrdersController> _logger;
    private readonly OrderProcessingWorkflow _orderProcessingWorkflow;
    private readonly IEventSender _eventSender;

    public OrdersController(
        ILogger<OrdersController> logger,
        OrderProcessingWorkflow orderProcessingWorkflow,
        IEventSender eventSender)
    {
        _logger = logger;
        _orderProcessingWorkflow = orderProcessingWorkflow;
        _eventSender = eventSender;
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder(
        [FromBody] PlaceOrderRequest request,
        [FromServices] IOrderStateService orderStateService)
    {
        try
        {
            // Map request to command
            var items = request.Items
                .Select(i => new OrderItemCommand(i.ProductName, i.Quantity, i.UnitPrice))
                .ToList();

            var command = new PlaceOrderCommand(
                request.CustomerName,
                request.CustomerEmail,
                request.DeliveryStreet,
                request.DeliveryCity,
                request.DeliveryPostalCode,
                request.DeliveryCountry,
                items
            );

            // Execute workflow with dependencies
            IOrderPlacedEvent workflowResult = _orderProcessingWorkflow.Execute(
                command,
                checkStockAvailability: (productName, quantity) => 
                    orderStateService.CheckStockAvailabilityAsync(productName, quantity).Result,
                generateOrderNumber: () => $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}"
            );

            // Handle workflow result
            IActionResult response = workflowResult switch
            {
                OrderPlacedSucceededEvent @event => await PublishEvent(@event),
                OrderPlacedFailedEvent @event => BadRequest(@event.Reasons),
                _ => throw new NotImplementedException()
            };

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while processing the order",
                error = ex.Message
            });
        }
    }

    private async Task<IActionResult> PublishEvent(OrderPlacedSucceededEvent successEvent)
    {
        await _eventSender.SendAsync("order-events", new OrderPlacedDto
        {
            OrderNumber = successEvent.OrderNumber.Value,
            CustomerName = successEvent.CustomerName,
            CustomerEmail = successEvent.CustomerEmail,
            TotalAmount = successEvent.TotalAmount.Value,
            PlacedAt = successEvent.PlacedAt,
            DeliveryAddress = new DeliveryAddressDto
            {
                Street = successEvent.DeliveryStreet,
                City = successEvent.DeliveryCity,
                PostalCode = successEvent.DeliveryPostalCode,
                Country = successEvent.DeliveryCountry
            }
        });

        return Ok();
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
