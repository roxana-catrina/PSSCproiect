namespace Proiect.Domain.Workflows;

using Proiect.Domain.Models.Commands;
using Proiect.Domain.Models.Entities;
using Proiect.Domain.Models.Events;
using Proiect.Domain.Models.ValueObjects;
using Proiect.Domain.Operations;
using Proiect.Domain.Exceptions;

public class OrderProcessingWorkflow
{
    private readonly List<ActiveProduct> _products;

    public OrderProcessingWorkflow(List<ActiveProduct> products)
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
            var updatedProduct = product.WithStockQuantity(product.StockQuantity - item.Quantity);
            
            return new OrderLine(
                item.ProductId,
                product.Name,
                item.Quantity,
                item.UnitPrice
            );
        }).ToList().AsReadOnly();

        var order = new PendingOrder(command.CustomerName, command.CustomerEmail, command.DeliveryAddress, orderLines);
        var confirmedOrder = new ConfirmedOrder(order);

        await NotifyCustomerOperation.SendOrderConfirmation(confirmedOrder.CustomerEmail, confirmedOrder.OrderNumber.Value);

        return new OrderPlaced(
            confirmedOrder.Id,
            confirmedOrder.OrderNumber.Value,
            confirmedOrder.CustomerName,
            confirmedOrder.CustomerEmail,
            confirmedOrder.DeliveryAddress,
            command.Items.Select(i => new Models.Events.OrderItem(i.ProductId, i.Quantity, i.UnitPrice)).ToList(),
            confirmedOrder.TotalAmount,
            confirmedOrder.CreatedAt
        );
    }
}
