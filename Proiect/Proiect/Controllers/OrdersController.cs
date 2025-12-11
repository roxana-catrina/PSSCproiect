using Microsoft.AspNetCore.Mvc;
using Proiect.Domain.Models.Commands;
using Proiect.Domain.Models.Events;
using Proiect.Domain.Workflows;
using Proiect.Messaging.Events;
using Proiect.Data.Services;
using Proiect.Domain.Models.ValueObjects;
using static Proiect.Domain.Models.Events.OrderPlacedEvent;
using static Proiect.Domain.Models.Entities.Order;

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

            // Handle workflow result and save to database
            IActionResult response = workflowResult switch
            {
                OrderPlacedSucceededEvent @event => await SaveAndPublishEvent(@event, orderStateService),
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

    private async Task<IActionResult> SaveAndPublishEvent(OrderPlacedSucceededEvent successEvent, IOrderStateService orderStateService)
    {
        try
        {
            // Save order to database
            var deliveryAddress = DeliveryAddress.TryParse(
                successEvent.DeliveryStreet,
                successEvent.DeliveryCity,
                successEvent.DeliveryPostalCode,
                successEvent.DeliveryCountry,
                out var address);

            if (deliveryAddress && address != null)
            {
                var confirmedOrder = new ConfirmedOrder(
                    successEvent.OrderNumber,
                    successEvent.CustomerName,
                    successEvent.CustomerEmail,
                    address,
                    new List<ValidatedOrderItem>(), // Items will be reconstructed
                    successEvent.TotalAmount,
                    successEvent.PlacedAt
                );

                await orderStateService.SaveOrderAsync(confirmedOrder);
                _logger.LogInformation($"Order {successEvent.OrderNumber.Value} saved to database");
            }

            // Publish event to Service Bus - using Events.DeliveryAddressDto
            await _eventSender.SendAsync("order-events", new OrderPlacedDto
            {
                OrderNumber = successEvent.OrderNumber.Value,
                CustomerName = successEvent.CustomerName,
                CustomerEmail = successEvent.CustomerEmail,
                TotalAmount = successEvent.TotalAmount.Value,
                PlacedAt = successEvent.PlacedAt,
                DeliveryAddress = new Proiect.Domain.Models.Events.DeliveryAddressDto
                {
                    Street = successEvent.DeliveryStreet,
                    City = successEvent.DeliveryCity,
                    PostalCode = successEvent.DeliveryPostalCode,
                    Country = successEvent.DeliveryCountry
                }
            });

            _logger.LogInformation($"Order {successEvent.OrderNumber.Value} event published");

            return Ok(new
            {
                success = true,
                orderNumber = successEvent.OrderNumber.Value,
                message = "Order placed successfully",
                totalAmount = successEvent.TotalAmount.Value
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving order to database");
            return StatusCode(500, new
            {
                success = false,
                message = "Order processed but failed to save",
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
