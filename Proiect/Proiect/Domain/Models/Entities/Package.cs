namespace Proiect.Domain.Models.Entities;

using Proiect.Domain.Models.ValueObjects;

public class Package
{
    public string Id { get; private set; }
    public string OrderId { get; private set; }
    public AWB AWB { get; private set; }
    public DeliveryAddress DeliveryAddress { get; private set; }
    public PackageStatus Status { get; private set; }
    public DateTime PreparedAt { get; private set; }
    public DateTime? PickedUpAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public string? ReceivedBy { get; private set; }

    public Package(string orderId, DeliveryAddress deliveryAddress)
    {
        Id = Guid.NewGuid().ToString();
        OrderId = orderId;
        AWB = AWB.Generate();
        DeliveryAddress = deliveryAddress;
        Status = PackageStatus.Prepared;
        PreparedAt = DateTime.UtcNow;
    }

    public void MarkAsPickedUp()
    {
        if (Status != PackageStatus.Prepared)
            throw new InvalidOperationException("Only prepared packages can be picked up");
        
        Status = PackageStatus.InTransit;
        PickedUpAt = DateTime.UtcNow;
    }

    public void MarkAsDelivered(string receivedBy)
    {
        if (Status != PackageStatus.InTransit)
            throw new InvalidOperationException("Only packages in transit can be delivered");
        
        Status = PackageStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        ReceivedBy = receivedBy;
    }
}

public enum PackageStatus
{
    Prepared,
    InTransit,
    Delivered,
    Returned
}

