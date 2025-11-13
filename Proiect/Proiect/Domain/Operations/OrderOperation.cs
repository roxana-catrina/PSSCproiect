namespace Proiect.Domain.Operations;

using Proiect.Domain.Models.Entities;

/// <summary>
/// Base class for order operations that transform order states
/// </summary>
public abstract class OrderOperation
{
    /// <summary>
    /// Transforms an order through its lifecycle states
    /// </summary>
    /// <param name="order">The order to transform</param>
    /// <returns>The transformed order</returns>
    public IOrder Transform(IOrder order)
    {
        return order switch
        {
            PendingOrder o => OnPending(o),
            ConfirmedOrder o => OnConfirmed(o),
            PaidOrder o => OnPaid(o),
            ShippedOrder o => OnShipped(o),
            DeliveredOrder o => OnDelivered(o),
            _ => throw new InvalidOperationException($"Unexpected order state: {order.GetType().Name}")
        };
    }

    /// <summary>
    /// Handles an order in the Pending state. Default is identity (returns same object).
    /// </summary>
    protected virtual IOrder OnPending(PendingOrder order) => order;

    /// <summary>
    /// Handles an order in the Confirmed state. Default is identity (returns same object).
    /// </summary>
    protected virtual IOrder OnConfirmed(ConfirmedOrder order) => order;

    /// <summary>
    /// Handles an order in the Paid state. Default is identity (returns same object).
    /// </summary>
    protected virtual IOrder OnPaid(PaidOrder order) => order;

    /// <summary>
    /// Handles an order in the Shipped state. Default is identity (returns same object).
    /// </summary>
    protected virtual IOrder OnShipped(ShippedOrder order) => order;

    /// <summary>
    /// Handles an order in the Delivered state. Default is identity (returns same object).
    /// </summary>
    protected virtual IOrder OnDelivered(DeliveredOrder order) => order;
}

/// <summary>
/// Base class for order operations that transform order states and return a specific result
/// </summary>
/// <typeparam name="TResult">The result type of the operation</typeparam>
public abstract class OrderOperation<TResult>
{
    /// <summary>
    /// Transforms an order through its lifecycle states and returns a result
    /// </summary>
    /// <param name="order">The order to transform</param>
    /// <returns>The result of the transformation</returns>
    public TResult Transform(IOrder order)
    {
        return order switch
        {
            PendingOrder o => OnPending(o),
            ConfirmedOrder o => OnConfirmed(o),
            PaidOrder o => OnPaid(o),
            ShippedOrder o => OnShipped(o),
            DeliveredOrder o => OnDelivered(o),
            _ => throw new InvalidOperationException($"Unexpected order state: {order.GetType().Name}")
        };
    }

    /// <summary>
    /// Handles an order in the Pending state and returns a result
    /// </summary>
    protected abstract TResult OnPending(PendingOrder order);

    /// <summary>
    /// Handles an order in the Confirmed state and returns a result
    /// </summary>
    protected abstract TResult OnConfirmed(ConfirmedOrder order);

    /// <summary>
    /// Handles an order in the Paid state and returns a result
    /// </summary>
    protected abstract TResult OnPaid(PaidOrder order);

    /// <summary>
    /// Handles an order in the Shipped state and returns a result
    /// </summary>
    protected abstract TResult OnShipped(ShippedOrder order);

    /// <summary>
    /// Handles an order in the Delivered state and returns a result
    /// </summary>
    protected abstract TResult OnDelivered(DeliveredOrder order);
}

