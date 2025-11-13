/*namespace Proiect.Controllers;

using Microsoft.AspNetCore.Mvc;
using Proiect.Domain.Models.Commands;
using Proiect.Domain.Models.ValueObjects;
using Proiect.Domain.Models.Entities;
using Proiect.Domain.Workflows;
using Proiect.Domain.Exceptions;
using Proiect.DTOs;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private static readonly List<Product> Products = new()
    {
        new Product("Laptop", "High-performance laptop", new Price(2500), 10),
        new Product("Mouse", "Wireless mouse", new Price(100), 50),
        new Product("Keyboard", "Mechanical keyboard", new Price(300), 30)
    };

    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
    {
        try
        {
            var deliveryAddress = new DeliveryAddress(
                request.DeliveryAddress.Street,
                request.DeliveryAddress.City,
                request.DeliveryAddress.PostalCode,
                request.DeliveryAddress.Country
            );

            var items = request.Items.Select(i => 
                new Domain.Models.Commands.OrderItem(i.ProductId, i.Quantity, i.UnitPrice)
            ).ToList();

            var command = new PlaceOrderCommand(
                request.CustomerName,
                request.CustomerEmail,
                deliveryAddress,
                items
            );

            var workflow = new OrderProcessingWorkflow(Products);
            var orderPlacedEvent = await workflow.ExecuteAsync(command);

            return Ok(new
            {
                success = true,
                orderId = orderPlacedEvent.OrderId,
                orderNumber = orderPlacedEvent.OrderNumber,
                totalAmount = orderPlacedEvent.TotalAmount,
                message = "Order placed successfully"
            });
        }
        catch (InsufficientStockException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (InvalidAddressException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred while processing your order", error = ex.Message });
        }
    }

    [HttpGet("products")]
    public IActionResult GetProducts()
    {
        var products = Products.Select(p => new
        {
            id = p.Id,
            name = p.Name,
            description = p.Description,
            price = p.Price.Amount,
            currency = p.Price.Currency,
            stockQuantity = p.StockQuantity,
            isAvailable = p.IsAvailable
        });

        return Ok(products);
    }
}
*/