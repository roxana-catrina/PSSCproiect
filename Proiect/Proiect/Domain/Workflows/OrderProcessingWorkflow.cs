namespace Proiect.Domain.Workflows;

using Proiect.Domain.Models.Commands;
using Proiect.Domain.Models.Entities;
using Proiect.Domain.Models.Events;
using Proiect.Domain.Models.ValueObjects;
using Proiect.Domain.Operations;
using Proiect.Domain.Exceptions;

public class OrderProcessingWorkflow
{
    private readonly List<Product> _products;

    public OrderProcessingWorkflow(List<Product> products)
    {
        _products = products;
    }

    public async Task<OrderPlaced> ExecuteAsync(PlaceOrderCommand command)
    {
        if (command.DeliveryAddress == null)
            throw new InvalidAddressException("Delivery address is required");

        var requestedQuantities = command.Items.ToDictionary(i => i.ProductId, i => i.Quantity);
        ValidateStockOperation.Execute(_products, requestedQuantities);

        var orderLines = command.Items.Select(item =>
        {
            var product = _products.First(p => p.Id == item.ProductId);
            product.ReserveStock(item.Quantity);
            
            return new OrderLine
            {
                ProductId = item.ProductId,
                ProductName = product.Name,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            };
        }).ToList();

        var order = new Order(command.CustomerName, command.CustomerEmail, command.DeliveryAddress, orderLines);
        order.Confirm();

        await NotifyCustomerOperation.SendOrderConfirmation(order.CustomerEmail, order.OrderNumber.Value);

        return new OrderPlaced(
            order.Id,
            order.OrderNumber.Value,
            order.CustomerName,
            order.CustomerEmail,
            order.DeliveryAddress,
            command.Items.Select(i => new Models.Events.OrderItem(i.ProductId, i.Quantity, i.UnitPrice)).ToList(),
            order.TotalAmount,
            order.CreatedAt
        );
    }
}

