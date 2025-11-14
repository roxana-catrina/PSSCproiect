namespace Proiect.Domain.Models.Commands;

public record PlaceOrderCommand(
    string CustomerName,
    string CustomerEmail,
    string DeliveryStreet,
    string DeliveryCity,
    string DeliveryPostalCode,
    string DeliveryCountry,
    IReadOnlyList<OrderItemCommand> Items);

public record OrderItemCommand(
    string ProductName,
    string Quantity,
    string UnitPrice);
