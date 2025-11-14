using Microsoft.AspNetCore.Mvc;
using Proiect.Domain.Models.Commands;
using Proiect.Domain.Workflows;
using Proiect.Domain.Models.Events;

namespace Proiect.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpPost]
    public IActionResult PlaceOrder([FromBody] PlaceOrderRequest request)
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

            var workflow = new OrderProcessingWorkflow();
            
            // Mock dependencies
            Func<string, int, bool> checkStockAvailability = (productName, quantity) => true; // Always available
            Func<string> generateOrderNumber = () => $"ORD-{DateTime.Now:yyyyMMdd}-{new Random().Next(1, 9999):D4}";
            
            var orderEvent = workflow.Execute(command, checkStockAvailability, generateOrderNumber);

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
                    message = "Order placed successfully"
                }),
                OrderPlacedEvent.OrderPlacedFailedEvent failure => BadRequest(new
                {
                    success = false,
                    errors = failure.Reasons,
                    message = "Failed to place order"
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

public record PlaceOrderRequest(
    string CustomerName,
    string CustomerEmail,
    string DeliveryStreet,
    string DeliveryCity,
    string DeliveryPostalCode,
    string DeliveryCountry,
    List<OrderItemRequest> Items);

public record OrderItemRequest(string ProductName, string Quantity, string UnitPrice);
