namespace Proiect.Domain.Models.Events;

/// <summary>
/// DTO for publishing PackageDelivered events to message bus
/// Contains only serializable primitive types
/// </summary>
public class PackageDeliveredDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public DateTime DeliveredAt { get; set; }
    public string RecipientName { get; set; } = string.Empty;
}

