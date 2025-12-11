using Proiect.Domain.Models.ValueObjects;
using static Proiect.Domain.Models.Entities.Order;

namespace Proiect.Domain.Operations;

internal sealed class NotifyCustomerOperation : OrderOperation
{
    private readonly Func<string> _generateOrderNumber;
    
    internal NotifyCustomerOperation(Func<string> generateOrderNumber)
    {
        _generateOrderNumber = generateOrderNumber;
    }
    
    protected override IOrder OnValidated(ValidatedOrder order)
    {
        // Generate order number
        var orderNumberStr = _generateOrderNumber();
        
        if (!OrderNumber.TryParse(orderNumberStr, out var orderNumber) || orderNumber == null)
        {
            return new InvalidOrder(new[] { "Failed to generate valid order number" });
        }
        
        // Calculate total amount
        Price totalAmount = order.Items
            .Aggregate(Price.Zero, (acc, item) => acc + (item.UnitPrice * item.Quantity));
        
        return new ConfirmedOrder(
            orderNumber,
            order.CustomerName,
            order.CustomerEmail,
            order.DeliveryAddress,
            order.Items,
            totalAmount,
            DateTime.Now);
    }
}
