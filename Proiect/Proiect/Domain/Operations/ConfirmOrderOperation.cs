namespace Proiect.Domain.Operations;

using Proiect.Domain.Models.Entities;

/// <summary>
/// Operation to confirm a pending order
/// Single responsibility: Order confirmation
/// </summary>
public class ConfirmOrderOperation : OrderOperation
{
    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    public ConfirmOrderOperation()
    {
    }

    /// <summary>
    /// Confirms a PendingOrder and converts to ConfirmedOrder
    /// </summary>
    protected override IOrder OnPending(PendingOrder order)
    {
        ValidateOrder(order);
        
        return new ConfirmedOrder(order);
    }

    /// <summary>
    /// Private helper method to validate order
    /// </summary>
    private static void ValidateOrder(PendingOrder order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));
    }
}

