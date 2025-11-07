namespace Proiect.Domain.Models.Events;

using Proiect.Domain.Models.ValueObjects;

public record OrderPlaced(
    string OrderId,
    string OrderNumber,
    string CustomerName,
    string CustomerEmail,
    DeliveryAddress DeliveryAddress,
    List<OrderItem> Items,
    decimal TotalAmount,
    DateTime PlacedAt
);

public record OrderItem(
    string ProductId,
    int Quantity,
    decimal UnitPrice
);

