namespace Proiect.Domain.Operations;

using Proiect.Domain.Models.Entities;

/// <summary>
/// Operation to validate and create a pending order from unvalidated order
/// Single responsibility: Order validation and creation
/// </summary>
public class ValidateOrderOperation : OrderOperation
{
    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    public ValidateOrderOperation()
    {
    }

    /// <summary>
    /// Validates and converts UnvalidatedOrder to PendingOrder
    /// </summary>
    protected override IOrder OnUnvalidated(UnvalidatedOrder order)
    {
        ValidateOrder(order);
        
        return new PendingOrder(
            order.CustomerName,
            order.CustomerEmail,
            order.DeliveryAddress,
            order.OrderLines
        );
    }

    /// <summary>
    /// Private helper method to validate order
    /// </summary>
    private static void ValidateOrder(UnvalidatedOrder order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));
        
        if (string.IsNullOrWhiteSpace(order.CustomerName))
            throw new ArgumentException("CustomerName cannot be empty", nameof(order));
        
        if (string.IsNullOrWhiteSpace(order.CustomerEmail))
            throw new ArgumentException("CustomerEmail cannot be empty", nameof(order));
        
        if (order.DeliveryAddress == null)
            throw new ArgumentException("DeliveryAddress cannot be null", nameof(order));
        
        if (order.OrderLines == null || !order.OrderLines.Any())
            throw new ArgumentException("OrderLines cannot be empty", nameof(order));
    }
}

