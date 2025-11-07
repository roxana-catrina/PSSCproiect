namespace Proiect.Domain.Models.Commands;

using Proiect.Domain.Models.ValueObjects;

public record PlaceOrderCommand(
    string CustomerName,
    string CustomerEmail,
    DeliveryAddress DeliveryAddress,
    List<OrderItem> Items
);

public record OrderItem(
    string ProductId,
    int Quantity,
    decimal UnitPrice
);

