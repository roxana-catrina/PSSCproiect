namespace Proiect.DTOs;

public record DeliveryAddressDto(
    string Street,
    string City,
    string PostalCode,
    string Country
);

public record OrderItemDto(
    string ProductId,
    int Quantity,
    decimal UnitPrice
);

public record PlaceOrderRequest(
    string CustomerName,
    string CustomerEmail,
    DeliveryAddressDto DeliveryAddress,
    List<OrderItemDto> Items
);

public record GenerateInvoiceRequest(
    string OrderId,
    decimal TotalAmount,
    string CustomerName,
    string CustomerEmail,
    string PaymentMethod = "CreditCard"
);

public record PickupPackageRequest(
    string OrderId,
    string? Awb,
    DateTime PickupDate,
    DeliveryAddressDto DeliveryAddress,
    string CustomerEmail,
    bool SimulateDelivery = false
);

