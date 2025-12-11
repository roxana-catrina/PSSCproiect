namespace Proiect.Domain.Models.Commands;

public record PlaceOrderCommand(
    string CustomerName,
    string CustomerEmail,
    string DeliveryStreet,
    string DeliveryCity,
    string DeliveryPostalCode,
    string DeliveryCountry,
    IReadOnlyList<OrderItemCommand> Items,
    string? OrderNumber = null);

public record OrderItemCommand(
    string ProductName,
    string Quantity,
    string UnitPrice);
