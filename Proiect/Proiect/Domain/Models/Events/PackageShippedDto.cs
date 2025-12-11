namespace Proiect.Domain.Models.Events;

/// <summary>
/// DTO for publishing PackageShipped events to message bus
/// Contains only serializable primitive types
/// </summary>
public class PackageShippedDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public DateTime ShippedAt { get; set; }
    public DeliveryAddressDto DeliveryAddress { get; set; } = new();
    public string CourierMessage { get; set; } = string.Empty;
}

