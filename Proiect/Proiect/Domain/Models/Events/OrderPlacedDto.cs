namespace Proiect.Domain.Models.Events;

/// <summary>
/// DTO for publishing OrderPlaced events to message bus
/// Contains only serializable primitive types
/// </summary>
public class OrderPlacedDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime PlacedAt { get; set; }
    public DeliveryAddressDto DeliveryAddress { get; set; } = new();
}

public class DeliveryAddressDto
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

