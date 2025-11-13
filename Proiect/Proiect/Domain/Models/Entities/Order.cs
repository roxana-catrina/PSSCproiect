namespace Proiect.Domain.Models.Entities;

using Proiect.Domain.Models.ValueObjects;

public interface IOrder { }

public record PendingOrder(
    string Id,
    OrderNumber OrderNumber,
    string CustomerName,
    string CustomerEmail,
    DeliveryAddress DeliveryAddress,
    IReadOnlyCollection<OrderLine> OrderLines,
    decimal TotalAmount,
    DateTime CreatedAt
) : IOrder
{
    internal PendingOrder(string customerName, string customerEmail, DeliveryAddress deliveryAddress, IReadOnlyCollection<OrderLine> orderLines)
        : this(
            Guid.NewGuid().ToString(),
            OrderNumber.Generate(),
            customerName,
            customerEmail,
            deliveryAddress,
            orderLines,
            orderLines.Sum(ol => ol.TotalPrice),
            DateTime.UtcNow
        )
    { }
}

public record ConfirmedOrder(
    string Id,
    OrderNumber OrderNumber,
    string CustomerName,
    string CustomerEmail,
    DeliveryAddress DeliveryAddress,
    IReadOnlyCollection<OrderLine> OrderLines,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime ConfirmedAt
) : IOrder
{
    internal ConfirmedOrder(PendingOrder pendingOrder)
        : this(
            pendingOrder.Id,
            pendingOrder.OrderNumber,
            pendingOrder.CustomerName,
            pendingOrder.CustomerEmail,
            pendingOrder.DeliveryAddress,
            pendingOrder.OrderLines,
            pendingOrder.TotalAmount,
            pendingOrder.CreatedAt,
            DateTime.UtcNow
        )
    { }
}

public record PaidOrder(
    string Id,
    OrderNumber OrderNumber,
    string CustomerName,
    string CustomerEmail,
    DeliveryAddress DeliveryAddress,
    IReadOnlyCollection<OrderLine> OrderLines,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime ConfirmedAt,
    DateTime PaidAt
) : IOrder
{
    internal PaidOrder(ConfirmedOrder confirmedOrder)
        : this(
            confirmedOrder.Id,
            confirmedOrder.OrderNumber,
            confirmedOrder.CustomerName,
            confirmedOrder.CustomerEmail,
            confirmedOrder.DeliveryAddress,
            confirmedOrder.OrderLines,
            confirmedOrder.TotalAmount,
            confirmedOrder.CreatedAt,
            confirmedOrder.ConfirmedAt,
            DateTime.UtcNow
        )
    { }
}

public record ShippedOrder(
    string Id,
    OrderNumber OrderNumber,
    string CustomerName,
    string CustomerEmail,
    DeliveryAddress DeliveryAddress,
    IReadOnlyCollection<OrderLine> OrderLines,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime ConfirmedAt,
    DateTime PaidAt,
    DateTime ShippedAt
) : IOrder
{
    internal ShippedOrder(PaidOrder paidOrder)
        : this(
            paidOrder.Id,
            paidOrder.OrderNumber,
            paidOrder.CustomerName,
            paidOrder.CustomerEmail,
            paidOrder.DeliveryAddress,
            paidOrder.OrderLines,
            paidOrder.TotalAmount,
            paidOrder.CreatedAt,
            paidOrder.ConfirmedAt,
            paidOrder.PaidAt,
            DateTime.UtcNow
        )
    { }
}

public record DeliveredOrder(
    string Id,
    OrderNumber OrderNumber,
    string CustomerName,
    string CustomerEmail,
    DeliveryAddress DeliveryAddress,
    IReadOnlyCollection<OrderLine> OrderLines,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime ConfirmedAt,
    DateTime PaidAt,
    DateTime ShippedAt,
    DateTime DeliveredAt
) : IOrder
{
    internal DeliveredOrder(ShippedOrder shippedOrder)
        : this(
            shippedOrder.Id,
            shippedOrder.OrderNumber,
            shippedOrder.CustomerName,
            shippedOrder.CustomerEmail,
            shippedOrder.DeliveryAddress,
            shippedOrder.OrderLines,
            shippedOrder.TotalAmount,
            shippedOrder.CreatedAt,
            shippedOrder.ConfirmedAt,
            shippedOrder.PaidAt,
            shippedOrder.ShippedAt,
            DateTime.UtcNow
        )
    { }
}

public record CancelledOrder(
    string Id,
    OrderNumber OrderNumber,
    string CustomerName,
    string CustomerEmail,
    DeliveryAddress DeliveryAddress,
    IReadOnlyCollection<OrderLine> OrderLines,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime CancelledAt,
    string CancellationReason
) : IOrder
{
    internal CancelledOrder(PendingOrder pendingOrder, string cancellationReason)
        : this(
            pendingOrder.Id,
            pendingOrder.OrderNumber,
            pendingOrder.CustomerName,
            pendingOrder.CustomerEmail,
            pendingOrder.DeliveryAddress,
            pendingOrder.OrderLines,
            pendingOrder.TotalAmount,
            pendingOrder.CreatedAt,
            DateTime.UtcNow,
            cancellationReason
        )
    { }
}

public record InvalidOrder(
    string CustomerName,
    string CustomerEmail,
    DeliveryAddress DeliveryAddress,
    IReadOnlyCollection<OrderLine> OrderLines,
    IEnumerable<string> Reasons
) : IOrder
{
    internal InvalidOrder(string customerName, string customerEmail, DeliveryAddress deliveryAddress, IReadOnlyCollection<OrderLine> orderLines, params string[] reasons)
        : this(customerName, customerEmail, deliveryAddress, orderLines, reasons.ToList().AsReadOnly())
    { }
}

public record OrderLine(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice
)
{
    public decimal TotalPrice => Quantity * UnitPrice;
}
